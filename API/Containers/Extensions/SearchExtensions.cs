using CORE.Abstract;
using CORE.Config;
using DAL.Search;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Containers.Extensions;

/// <summary>
///     Registers the open-generic <see cref="CORE.Abstract.ISearchService{T}" /> implementation
///     chosen by <c>SearchSettings.Provider</c>: <c>PostgresFts</c> (default, EF Core full-text
///     search) or <c>Elastic</c> (delegates to the existing Elasticsearch integration).
/// </summary>
public static class SearchExtensions
{
    public static IServiceCollection AddSearch(this IServiceCollection services, ConfigSettings config)
    {
        switch (config.SearchSettings.Provider)
        {
            case SearchProvider.Elastic:
                services.TryAddScoped(typeof(ISearchService<>), typeof(ElasticSearchSearchService<>));
                break;
            case SearchProvider.PostgresFts:
            default:
                services.TryAddScoped(typeof(ISearchService<>), typeof(PostgresFtsSearchService<>));
                break;
        }

        return services;
    }
}