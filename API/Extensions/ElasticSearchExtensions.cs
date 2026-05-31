using CORE.Config;
using DAL.ElasticSearch;
using DTO.User;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Registers the Elasticsearch client typed for <see cref="UserToListDto" />. Always-on once
///     wired — if a project does not need Elasticsearch, delete this file, the call to
///     <c>AddElasticSearch</c> in <c>Program.cs</c>, the <see cref="ElasticSearchSettings" />
///     record, and the corresponding appsettings block.
/// </summary>
public static class ElasticSearchExtensions
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services, ConfigSettings config)
    {
        services.TryAddScoped<IElasticSearchService<UserToListDto>>(_ =>
            new ElasticSearchService<UserToListDto>(
                config.ElasticSearchSettings.Connection,
                config.ElasticSearchSettings.DefaultIndex,
                config.ElasticSearchSettings.Username,
                config.ElasticSearchSettings.Password));

        return services;
    }
}