using System.Linq.Expressions;

namespace DAL.ElasticSearch;

public interface IElasticSearchService<T> where T : class
{
    Task<bool> IndexExistsAsync(string indexName);
    Task<bool> CreateIndexAsync(string indexName);
    Task<bool> DeleteIndexAsync(string indexName);
    Task<bool> AddToIndexAsync(T document, string indexName);
    Task<bool> AddRangeToIndexAsync(IEnumerable<T> documents, string indexName);
    Task<IEnumerable<T>> SearchDocumentsAsync(Expression<Func<T, object>> field, string query);
}