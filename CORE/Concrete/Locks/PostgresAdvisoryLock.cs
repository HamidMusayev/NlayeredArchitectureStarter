using System.IO.Hashing;
using System.Text;
using CORE.Abstract;
using CORE.Config;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CORE.Concrete.Locks;

/// <summary>
///     <see cref="IDistributedLock" /> over PostgreSQL session-level advisory locks
///     (<c>pg_advisory_lock</c>). No extra infrastructure — reuses the primary database.
///     <para>
///         Each acquired handle holds an open <see cref="NpgsqlConnection" /> for the lock's
///         lifetime; dispose runs <c>pg_advisory_unlock</c> and closes the connection. The
///         resource name is hashed to <c>bigint</c> (Postgres advisory keys are integer).
///     </para>
///     <para>
///         Note: session-level locks die with the connection — if the holding process crashes the
///         lock disappears automatically; you can't end up with an orphaned advisory lock the way
///         you can with a forgotten Redis key.
///     </para>
/// </summary>
public sealed class PostgresAdvisoryLock(
    IOptions<ConnectionStrings> connectionOptions,
    IOptions<DistributedLockSettings> lockOptions) : IDistributedLock
{
    private readonly ConnectionStrings _connections = connectionOptions.Value;
    private readonly DistributedLockSettings _lockSettings = lockOptions.Value;

    public async Task<ILockHandle?> AcquireAsync(
        string resource,
        TimeSpan? wait = null,
        TimeSpan? lifetime = null,
        CancellationToken ct = default)
    {
        // lifetime is ignored — pg releases on session end. Pass-through to satisfy contract.
        var key = HashToBigint(resource);
        var connection = new NpgsqlConnection(_connections.AppDb);
        try
        {
            await connection.OpenAsync(ct);

            var deadline = DateTime.UtcNow +
                           (wait ?? TimeSpan.FromMilliseconds(_lockSettings.DefaultWaitMilliseconds));

            while (true)
            {
                await using (var cmd = new NpgsqlCommand("SELECT pg_try_advisory_lock(@k)", connection))
                {
                    cmd.Parameters.AddWithValue("k", key);
                    var ok = (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
                    if (ok) return new Handle(resource, connection, key);
                }

                if (DateTime.UtcNow >= deadline)
                {
                    await connection.DisposeAsync();
                    return null;
                }

                ct.ThrowIfCancellationRequested();
                await Task.Delay(50, ct);
            }
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    ///     Postgres advisory keys are <c>bigint</c>; we collapse the resource string into 64 bits
    ///     via xxHash64. Hash collisions are statistically negligible for normal namespaces.
    /// </summary>
    private static long HashToBigint(string resource)
    {
        var bytes = Encoding.UTF8.GetBytes(resource);
        return unchecked((long)XxHash64.HashToUInt64(bytes));
    }

    private sealed class Handle(string resource, NpgsqlConnection connection, long key) : ILockHandle
    {
        public string Resource => resource;

        public async ValueTask DisposeAsync()
        {
            try
            {
                await using var cmd = new NpgsqlCommand("SELECT pg_advisory_unlock(@k)", connection);
                cmd.Parameters.AddWithValue("k", key);
                await cmd.ExecuteScalarAsync();
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}