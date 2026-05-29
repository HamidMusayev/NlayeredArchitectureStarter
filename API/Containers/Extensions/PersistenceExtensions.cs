using CORE.Config;
using DAL.ElasticSearch;
using DAL.EntityFramework.Concrete;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.UnitOfWork;
using DAL.MongoDb;
using DTO.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Containers.Extensions;

/// <summary>
///     Storage backbone: EF Core <see cref="DataContext" />, the <see cref="IUnitOfWork" />, the
///     DAL repository scan, and the optional MongoDB / Elasticsearch clients. Each optional
///     store is gated by its <c>IsEnabled</c> flag — derived projects keep what they use and
///     turn off the rest by config.
/// </summary>
public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, ConfigSettings config)
    {
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(config.ConnectionStrings.AppDb));

        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.Scan(scan => scan
            .FromAssemblies(typeof(UserRepository).Assembly)
            .AddClasses(classes => classes.InNamespaces("DAL.EntityFramework.Concrete"))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        if (config.MongoDbSettings.IsEnabled)
            services.TryAddSingleton<IMongoDbService, MongoDbService>();

        if (config.ElasticSearchSettings.IsEnabled)
            services.TryAddScoped<IElasticSearchService<UserToListDto>>(_ =>
                new ElasticSearchService<UserToListDto>(
                    config.ElasticSearchSettings.Connection,
                    config.ElasticSearchSettings.DefaultIndex,
                    config.ElasticSearchSettings.Username,
                    config.ElasticSearchSettings.Password));

        return services;
    }
}