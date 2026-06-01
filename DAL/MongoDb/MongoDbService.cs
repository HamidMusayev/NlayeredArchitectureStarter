using System.Linq.Expressions;
using CORE.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DAL.MongoDb;

/// <summary>
///     Default <see cref="IMongoDbService" /> implementation backed by the official MongoDB .NET driver.
///     Connection string and default database name are read from <c>MongoDbSettings</c> at startup.
///     The internal <c>IMongoClient</c> is a long-lived singleton; <c>IMongoDatabase</c> is replaced
///     by <see cref="ChangeDatabase" /> when needed.
/// </summary>
public class MongoDbService : IMongoDbService
{
    private readonly IMongoClient _client;
    private IMongoDatabase _database;

    public MongoDbService(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        _client = new MongoClient(settings.Connection);
        _database = _client.GetDatabase(settings.Database);
    }

    public void ChangeDatabase(string database)
    {
        _database = _client.GetDatabase(database);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public async Task<IEnumerable<T>> GetAll<T>(string collectionName)
    {
        var collection = GetCollection<T>(collectionName);
        var documents = await collection.Find(_ => true).ToListAsync();
        return documents;
    }

    public async Task<T> GetById<T>(string collectionName, string id)
    {
        var collection = GetCollection<T>(collectionName);
        var filter = Builders<T>.Filter.Eq("_id", id);
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        return document;
    }

    public async Task<IEnumerable<T>> GetByFilter<T>(string collectionName, Expression<Func<T, bool>> filterExpression)
    {
        var collection = GetCollection<T>(collectionName);
        var documents = await collection.Find(filterExpression).ToListAsync();
        return documents;
    }

    public async Task Insert<T>(string collectionName, T document)
    {
        var collection = GetCollection<T>(collectionName);
        await collection.InsertOneAsync(document);
    }

    public async Task Update<T>(string collectionName, string id, T document)
    {
        var collection = GetCollection<T>(collectionName);
        var filter = Builders<T>.Filter.Eq("_id", id);
        await collection.ReplaceOneAsync(filter, document);
    }

    public async Task Delete<T>(string collectionName, string id)
    {
        var collection = GetCollection<T>(collectionName);
        var filter = Builders<T>.Filter.Eq("_id", id);
        await collection.DeleteOneAsync(filter);
    }
}