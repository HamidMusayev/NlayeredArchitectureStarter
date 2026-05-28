using CORE.Concrete;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Seeds;

public class UserSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var hasher = new Pbkdf2PasswordHasher();
        var salt = hasher.GenerateSalt();
        var pass = hasher.Hash("testtest", salt);
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.NewGuid(),
                Username = "Test",
                Email = "test@test.tst",
                Password = pass,
                ContactNumber = "",
                RoleId = null,
                Salt = salt
            }
        );
    }
}