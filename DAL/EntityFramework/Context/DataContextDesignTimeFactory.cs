using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DAL.EntityFramework.Context;

/// <summary>
///     Build-time factory consumed by <c>dotnet ef migrations …</c>. The CLI bootstraps the
///     full <c>API</c> host first and falls back to this factory only if that fails — which it
///     does because the API's <c>Program.cs</c> registers a lot of infrastructure that isn't
///     available at design time. This factory reads the connection string straight from
///     <c>API/appsettings.Development.json</c> and configures Npgsql, nothing else.
/// </summary>
public sealed class DataContextDesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        // The EF CLI runs from the startup project's directory (API/), so a relative
        // appsettings path works without further configuration.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", false)
            .Build();

        var connectionString = configuration["ConfigSettings:ConnectionStrings:AppDb"]
                               ?? throw new InvalidOperationException(
                                   "ConfigSettings:ConnectionStrings:AppDb missing from appsettings.Development.json");

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DataContext(options);
    }
}