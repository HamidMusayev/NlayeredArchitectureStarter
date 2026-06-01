using CORE.Config;
using DAL.ElasticSearch;
using DTO.User;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace API.Extensions;

/// <summary>
///     Registers the Elasticsearch client typed for <see cref="UserToListDto" />. Always-on once
///     wired — if a project does not need Elasticsearch, delete this file, the call to
///     <c>AddElasticSearch</c> in <c>Program.cs</c>, the <see cref="ElasticSearchSettings" />
///     record, and the corresponding appsettings block.
/// </summary>
public static class ElasticSearchExtensions
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services)
    {
        services.TryAddScoped<IElasticSearchService<UserToListDto>>(sp =>
        {
            var s = sp.GetRequiredService<IOptions<ElasticSearchSettings>>().Value;
            return new ElasticSearchService<UserToListDto>(s.Connection, s.DefaultIndex, s.Username, s.Password);
        });

        return services;
    }
}