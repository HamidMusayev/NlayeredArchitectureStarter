using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IUserRepository" />. Extends the generic surface with
///     email-uniqueness check, per-user PBKDF2 salt retrieval, and <see cref="UpdateUser" /> which
///     marks the entity <c>Modified</c> while explicitly excluding the <c>Password</c> and
///     <c>Salt</c> columns to prevent credential corruption on profile updates.
/// </summary>
public class UserRepository(DataContext dataContext) : GenericRepository<User>(dataContext), IUserRepository
{
    public async Task<string?> GetUserSaltAsync(string userEmail)
    {
        var user = await dataContext.Users.SingleOrDefaultAsync(m => m.Email == userEmail);
        return user?.Salt;
    }

    public Task<bool> IsUserExistAsync(string email, Guid? userId)
    {
        return dataContext.Users.AnyAsync(m => m.Email == email && m.Id != userId);
    }

    public void UpdateUser(User user)
    {
        dataContext.Entry(user).State = EntityState.Modified;
        dataContext.Entry(user).Property(m => m.Password).IsModified = false;
        dataContext.Entry(user).Property(m => m.Salt).IsModified = false;
    }
}