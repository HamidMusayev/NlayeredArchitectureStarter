using CORE.Config;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace API.HealthChecks;

/// <summary>
///     Verifies the app's Postgres user can actually write — opens a transaction, creates a
///     session-scoped TEMP TABLE, inserts a row, then rolls back. Catches the failure mode
///     <c>AddNpgSql</c>'s plain <c>SELECT 1</c> misses: a primary that's been failed-over to a
///     read-only replica, or a connection routed to a hot-standby. The rollback leaves no
///     residue — the temp table is dropped at session close (which connection-pool reuse
///     handles via <c>DISCARD TEMP</c>).
/// </summary>
public sealed class PostgresWriteHealthCheck(ConfigSettings config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(config.ConnectionStrings.AppDb);
            await connection.OpenAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await using (var create = new NpgsqlCommand(
                             "CREATE TEMP TABLE IF NOT EXISTS _hc_probe (probed_at timestamptz NOT NULL)",
                             connection, transaction))
            {
                await create.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var insert = new NpgsqlCommand(
                             "INSERT INTO _hc_probe (probed_at) VALUES (now())",
                             connection, transaction))
            {
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.RollbackAsync(cancellationToken);

            return HealthCheckResult.Healthy("Postgres accepts writes.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres write probe failed.", ex);
        }
    }
}