using BLL.Abstract;
using CORE.Abstract;
using CORE.Config;
using DAL.EntityFramework.Abstract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLL.Concrete;

/// <summary>
///     Default <see cref="IUserPermissionsCache" /> backed by the project's vendor-neutral
///     <see cref="ICacheService" /> (Memory or Redis per <c>CacheSettings.Provider</c>). Values
///     are <c>List&lt;string&gt;</c> — JSON-serialized by the cache layer. Keys are
///     <c>"perms:{userId:N}"</c>.
///     <para>
///         Backend failures don't fail the caller: <see cref="GetAsync" /> falls through to the
///         DB on errors and returns the fresh result without populating the cache.
///         <see cref="InvalidateAsync" /> swallows + logs.
///     </para>
/// </summary>
public sealed class UserPermissionsCache(
    ICacheService cache,
    IUserRepository userRepository,
    IOptions<CacheSettings> cacheOptions,
    ILogger<UserPermissionsCache> logger)
    : IUserPermissionsCache
{
    private readonly CacheSettings _cacheSettings = cacheOptions.Value;

    public async Task<IReadOnlySet<string>> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var key = Key(userId);
        var ttl = TimeSpan.FromMinutes(Math.Max(1, _cacheSettings.UserPermissionsTtlMinutes));

        try
        {
            var cached = await cache.GetAsync<List<string>>(key, ct);
            if (cached is not null) return cached.ToHashSet(StringComparer.Ordinal);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "User permissions cache GET failed for {UserId}; falling through to DB", userId);
        }

        var fresh = await userRepository.GetPermissionKeysAsync(userId, ct);

        try
        {
            await cache.SetAsync(key, fresh, ttl, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "User permissions cache SET failed for {UserId}", userId);
        }

        return fresh.ToHashSet(StringComparer.Ordinal);
    }

    public async Task InvalidateAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            await cache.RemoveAsync(Key(userId), ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "User permissions cache REMOVE failed for {UserId}", userId);
        }
    }

    private static string Key(Guid userId)
    {
        return "perms:" + userId.ToString("N");
    }
}