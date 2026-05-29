using System.Linq.Expressions;

namespace CORE.Abstract;

/// <summary>
///     Engine-agnostic full-text search surface. Implementations live in their respective DAL
///     projects (Postgres FTS, Elasticsearch, Meilisearch). Application code depends on this
///     interface; the active engine is chosen via <see cref="CORE.Config.SearchSettings.Provider" />.
/// </summary>
/// <typeparam name="T">
///     Indexed document type. Engines may have different requirements
///     (e.g. Elasticsearch wants a no-arg constructor for deserialization).
/// </typeparam>
public interface ISearchService<T> where T : class
{
    /// <summary>
    ///     Adds or updates a single document. Index name is engine-specific (Postgres maps it
    ///     to a tsvector-indexed table; Elasticsearch maps it to an index name).
    /// </summary>
    Task<bool> AddOrUpdateAsync(T document, string indexName, CancellationToken ct = default);

    /// <summary>
    ///     Bulk add/update. Engines that support native bulk APIs use them; others fall back
    ///     to per-item operations.
    /// </summary>
    Task<bool> AddOrUpdateRangeAsync(IEnumerable<T> documents, string indexName, CancellationToken ct = default);

    /// <summary>
    ///     Removes a document by id (string for engine portability). No-op if missing.
    /// </summary>
    Task<bool> DeleteAsync(string id, string indexName, CancellationToken ct = default);

    /// <summary>
    ///     Free-text search on a single field. <paramref name="field" /> selects the property
    ///     to query; <paramref name="query" /> is the user input.
    /// </summary>
    Task<IReadOnlyList<T>> SearchAsync(Expression<Func<T, object>> field, string query, int take = 50,
        CancellationToken ct = default);
}