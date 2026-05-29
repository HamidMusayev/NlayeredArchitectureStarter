using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Seeds;

/// <summary>
///     Orchestrates all EF Core model-builder seed data. Called from
///     <c>DataContext.OnModelCreating</c> to populate reference rows on first migration.
/// </summary>
public static class DataSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        RoleSeed.Seed(modelBuilder);
        UserSeed.Seed(modelBuilder);
    }
}