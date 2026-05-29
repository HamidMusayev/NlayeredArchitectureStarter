namespace CORE.Config;

/// <summary>
///     Selects the active <c>ISearchService&lt;T&gt;</c> implementation. Default is
///     <see cref="SearchProvider.PostgresFts" /> — uses the existing primary database so no
///     extra infrastructure is needed for "good enough" search. Swap to
///     <see cref="SearchProvider.Elastic" /> when you need stemming/synonyms/scoring at scale.
/// </summary>
public record SearchSettings
{
    public SearchProvider Provider { get; set; } = SearchProvider.PostgresFts;
}

public enum SearchProvider
{
    PostgresFts = 0,
    Elastic = 1
}