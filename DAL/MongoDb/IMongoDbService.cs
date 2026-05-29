using System.Linq.Expressions;
using MongoDB.Driver;

namespace DAL.MongoDb;

/// <summary>
///     Thin MongoDB driver abstraction. Provides raw collection access (<see cref="GetCollection{T}" />)
///     plus convenience helpers for the common CRUD operations. Call <see cref="ChangeDatabase" /> to
///     switch the active database at runtime without rebuilding the client.
/// </summary>
public interface IMongoDbService
{
    void ChangeDatabase(string database);
    IMongoCollection<T> GetCollection<T>(string collectionName);
    Task<IEnumerable<T>> GetAll<T>(string collectionName);
    Task<T> GetById<T>(string collectionName, string id);
    Task<IEnumerable<T>> GetByFilter<T>(string collectionName, Expression<Func<T, bool>> filterExpression);
    Task Insert<T>(string collectionName, T document);
    Task Update<T>(string collectionName, string id, T document);
    Task Delete<T>(string collectionName, string id);
}