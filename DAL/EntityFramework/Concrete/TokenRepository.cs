using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="ITokenRepository" />. Provides token validation
///     (<see cref="ITokenRepository.IsValid" />), active-token lookup for logout, refresh-token
///     lookup (with owning user eager-loaded for rotation), and family-wide revocation triggered
///     by token-reuse detection.
/// </summary>
public class TokenRepository(DataContext dataContext) : GenericRepository<Token>(dataContext), ITokenRepository
{
    public Task<bool> IsValid(string accessToken, string refreshToken)
    {
        return dataContext.Tokens.AnyAsync(m =>
            m.AccessToken == accessToken &&
            m.RefreshToken == refreshToken &&
            m.UsedAt == null &&
            !m.IsRevoked &&
            m.AccessTokenExpireDate > DateTime.UtcNow);
    }

    public Task<Token?> GetForValidationAsync(string accessToken, string refreshToken,
        CancellationToken ct = default)
    {
        return dataContext.Tokens.FirstOrDefaultAsync(m =>
            m.AccessToken == accessToken &&
            m.RefreshToken == refreshToken &&
            m.UsedAt == null &&
            !m.IsRevoked &&
            m.AccessTokenExpireDate > DateTime.UtcNow, ct);
    }

    public Task<List<Token>> GetActiveTokensAsync(string accessToken)
    {
        return dataContext.Tokens.Where(m => m.AccessToken == accessToken).ToListAsync();
    }

    public Task<Token?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return dataContext.Tokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken, ct);
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