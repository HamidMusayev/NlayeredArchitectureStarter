using System.Linq.Expressions;

namespace DAL.ElasticSearch;

/// <summary>
///     Low-level Elasticsearch operations — index lifecycle (create / delete / exists) and document
///     ingestion / search for a single generic document type. Used directly for index management and
///     consumed by <c>ElasticSearchSearchService&lt;T&gt;</c> via the higher-level
///     <c>ISearchService&lt;T&gt;</c> abstraction.
/// </summary>
public interface IElasticSearchService<T> where T : class
{
    Task<bool> IndexExistsAsync(string indexName);
    Task<bool> CreateIndexAsync(string indexName);
    Task<bool> DeleteIndexAsync(string indexName);
    Task<bool> AddToIndexAsync(T document, string indexName);
    Task<bool> AddRangeToIndexAsync(IEnumerable<T> documents, string indexName);
    Task<IEnumerable<T>> SearchDocumentsAsync(Expression<Func<T, object>> field, string query);
}