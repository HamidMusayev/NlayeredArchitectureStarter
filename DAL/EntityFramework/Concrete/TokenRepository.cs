using CORE.Concrete.Cache;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="ITokenRepository" />. Per-request validation keys on
///     the JWT's <c>jti</c> claim — the full access token never appears in this layer.
///     <para>
///         <see cref="IsValid" /> and <see cref="GetForValidationAsync" /> run on every
///         authenticated request that misses the introspection cache, so both go through
///         <c>EF.CompileAsyncQuery</c> — EF parses the query tree once at startup instead of
///         every call. Family-revoke and dead-letter listings stay regular LINQ because they
///         run during write-rare paths.
///     </para>
/// </summary>
public class TokenRepository(DataContext dataContext) : GenericRepository<Token>(dataContext), ITokenRepository
{
    // Compiled hot-path queries. Predicate body is parsed + planned once at type init; subsequent
    // invocations skip the EF query-translation work entirely.
    private static readonly Func<DataContext, Guid, string, Task<bool>> CompiledIsValid =
        EF.CompileAsyncQuery((DataContext db, Guid jti, string hash) =>
            db.Tokens.Any(m =>
                m.Jti == jti &&
                m.RefreshTokenHash == hash &&
                m.UsedAt == null &&
                !m.IsRevoked &&
                m.AccessTokenExpireDate > DateTime.UtcNow));

    private static readonly Func<DataContext, Guid, string, Task<Token?>> CompiledGetForValidation =
        EF.CompileAsyncQuery((DataContext db, Guid jti, string hash) =>
            db.Tokens.FirstOrDefault(m =>
                m.Jti == jti &&
                m.RefreshTokenHash == hash &&
                m.UsedAt == null &&
                !m.IsRevoked &&
                m.AccessTokenExpireDate > DateTime.UtcNow));

    public Task<bool> IsValid(Guid jti, string refreshToken)
    {
        return CompiledIsValid(dataContext, jti, TokenIntrospectionCache.Hash(refreshToken));
    }

    public Task<Token?> GetForValidationAsync(Guid jti, string refreshToken,
        CancellationToken ct = default)
    {
        // CompileAsyncQuery's scalar overload doesn't accept a CancellationToken parameter —
        // throw early instead of silently ignoring a requested cancellation.
        ct.ThrowIfCancellationRequested();
        return CompiledGetForValidation(dataContext, jti, TokenIntrospectionCache.Hash(refreshToken));
    }

    public Task<List<Token>> GetActiveTokensByJtiAsync(Guid jti)
    {
        return dataContext.Tokens.Where(m => m.Jti == jti).ToListAsync();
    }

    public Task<Token?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default)
    {
        return dataContext.Tokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.RefreshTokenHash == refreshTokenHash, ct);
    }

    public async Task<List<Token>> RevokeFamilyAsync(Guid familyId, CancellationToken ct = default)
    {
        var rows = await dataContext.Tokens
            .Where(t => t.FamilyId == familyId && !t.IsRevoked)
            .ToListAsync(ct);
        rows.ForEach(t => t.IsRevoked = true);
        return rows;
    }
}