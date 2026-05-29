using CORE.Config;
using DAL.MongoDb;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Extensions;

/// <summary>
///     Registers the MongoDB client. Always-on once wired — if a project does not need MongoDB,
///     delete this file, the call to <c>AddMongoDb</c> in <c>Program.cs</c>, the
///     <see cref="MongoDbSettings" /> record, and the corresponding appsettings block.
/// </summary>
public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        services.TryAddSingleton<IMongoDbService, MongoDbService>();
        return services;
    }
}
