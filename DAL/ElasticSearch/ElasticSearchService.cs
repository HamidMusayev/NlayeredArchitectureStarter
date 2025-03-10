using System.Linq.Expressions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Transport;

namespace DAL.ElasticSearch;

public class ElasticSearchService<T> : IElasticSearchService<T> where T : class
{
    private readonly ElasticsearchClient _client;

    public ElasticSearchService(string uri, string defaultIndex, string username, string password)
    {
        var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex(defaultIndex)
            .Authentication(new BasicAuthentication(username, password));

        _client = new ElasticsearchClient(settings);
    }

    public async Task<bool> IndexExistsAsync(string indexName)
    {
        var response = await _client.Indices.ExistsAsync(indexName);
        return response.Exists;
    }

    public async Task<bool> CreateIndexAsync(string indexName)
    {
        var response = await _client.Indices.CreateAsync(indexName);
        return response.IsSuccess();
    }

    public async Task<bool> AddToIndexAsync(T document, string indexName)
    {
        var response = await _client.IndexAsync(document, idx => idx.Index(indexName));
        return response.IsSuccess();
    }

    public async Task<bool> AddRangeToIndexAsync(IEnumerable<T> documents, string indexName)
    {
        var bulkRequest = new BulkRequest(indexName)
        {
            Operations = documents.Select(d => new BulkIndexOperation<T>(d)).Cast<IBulkOperation>().ToList()
        };

        var response = await _client.BulkAsync(bulkRequest);
        return response.IsSuccess();
    }

    public async Task<bool> DeleteIndexAsync(string indexName)
    {
        var response = await _client.Indices.DeleteAsync(indexName);
        return response.IsSuccess();
    }

    public async Task<IEnumerable<T>> SearchDocumentsAsync(Expression<Func<T, object>> field, string query)
    {
        var response = await _client.SearchAsync<T>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(field)
                    .Query(query)
                )
            )
        );

        return response.Documents;
    }
}