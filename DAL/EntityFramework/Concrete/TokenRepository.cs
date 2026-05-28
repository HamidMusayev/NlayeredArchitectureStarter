using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

public class TokenRepository(DataContext dataContext) : GenericRepository<Token>(dataContext), ITokenRepository
{
    public Task<bool> IsValid(string accessToken, string refreshToken)
    {
        return dataContext.Tokens.AnyAsync(m =>
            m.AccessToken == accessToken &&
            m.RefreshToken == refreshToken &&
            m.AccessTokenExpireDate > DateTime.UtcNow);
    }

    public Task<List<Token>> GetActiveTokensAsync(string accessToken)
    {
        return dataContext.Tokens.Where(m => m.AccessToken == accessToken).ToListAsync();
    }
}