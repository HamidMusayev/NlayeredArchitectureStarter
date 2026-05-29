using System.Linq.Expressions;
using CORE.Abstract;
using DAL.ElasticSearch;

namespace DAL.Search;

/// <summary>
///     Adapter from the unified <see cref="ISearchService{T}" /> contract onto the existing
///     <see cref="IElasticSearchService{T}" />. Selected when <c>SearchSettings.Provider</c> is
///     <c>Elastic</c>; <c>ElasticSearchSettings.IsEnabled</c> must also be on so the underlying
///     client is registered.
/// </summary>
public sealed class ElasticSearchSearchService<T>(IElasticSearchService<T> inner) : ISearchService<T> where T : class
{
    public Task<bool> AddOrUpdateAsync(T document, string indexName, CancellationToken ct = default)
    {
        return inner.AddToIndexAsync(document, indexName);
    }

    public Task<bool> AddOrUpdateRangeAsync(IEnumerable<T> documents, string indexName, CancellationToken ct = default)
    {
        return inner.AddRangeToIndexAsync(documents, indexName);
    }

    public Task<bool> DeleteAsync(string id, string indexName, CancellationToken ct = default)
    {
        // The existing IElasticSearchService surface only exposes whole-index DELETE.
        // Real per-document delete needs an extra method on that interface; flagging here
        // rather than silently no-op'ing.
        throw new NotSupportedException(
            "Per-document delete is not on the underlying IElasticSearchService<T> contract. " +
            "Extend it with DeleteDocumentAsync(string id, string indexName) when this is needed.");
    }

    public async Task<IReadOnlyList<T>> SearchAsync(
        Expression<Func<T, object>> field,
        string query,
        int take = 50,
        CancellationToken ct = default)
    {
        var docs = await inner.SearchDocumentsAsync(field, query);
        return docs.Take(take).ToList();
    }
}