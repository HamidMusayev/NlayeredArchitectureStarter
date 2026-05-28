using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

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