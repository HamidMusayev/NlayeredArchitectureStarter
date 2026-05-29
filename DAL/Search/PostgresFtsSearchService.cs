using System.Linq.Expressions;
using CORE.Abstract;
using DAL.EntityFramework.Context;
using ENTITIES.Entities.Generic;
using Microsoft.EntityFrameworkCore;

namespace DAL.Search;

/// <summary>
///     Default <see cref="ISearchService{T}" /> — runs against the primary <see cref="DataContext" />
///     so "good enough" search needs no extra infrastructure. Indexing methods are no-ops because
///     the rows already live in Postgres.
///     <para>
///         <b>SearchAsync</b> pulls up to <c>take * 4</c> matching candidates with an EF
///         <c>LIKE</c> predicate (Npgsql translates to <c>ILIKE</c>) and then trims with an
///         in-memory <c>Contains</c>. For datasets where this becomes too slow, subclass and
///         override <see cref="SearchAsync" /> with an entity-specific query that uses
///         <c>EF.Functions.ToTsVector / PhraseToTsQuery</c> and a GIN index on the column.
///         Or switch <c>SearchSettings.Provider</c> to <c>Elastic</c>.
///     </para>
/// </summary>
public sealed class PostgresFtsSearchService<T>(DataContext context) : ISearchService<T> where T : Auditable
{
    public Task<bool> AddOrUpdateAsync(T document, string indexName, CancellationToken ct = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> AddOrUpdateRangeAsync(IEnumerable<T> documents, string indexName, CancellationToken ct = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string id, string indexName, CancellationToken ct = default)
    {
        return Task.FromResult(true);
    }

    public async Task<IReadOnlyList<T>> SearchAsync(
        Expression<Func<T, object>> field,
        string query,
        int take = 50,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        // Build a LIKE-pushable predicate from the user's field selector:
        //   row => EF.Functions.ILike(((string)field(row)), $"%{query}%")
        var param = field.Parameters[0];
        var asString = Expression.Convert(field.Body, typeof(string));

        var likeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
            nameof(NpgsqlDbFunctionsExtensions.ILike),
            [typeof(DbFunctions), typeof(string), typeof(string)])!;

        var efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        var pattern = Expression.Constant($"%{query}%");
        var ilikeCall = Expression.Call(likeMethod, efFunctions, asString, pattern);
        var predicate = Expression.Lambda<Func<T, bool>>(ilikeCall, param);

        var candidates = await context.Set<T>()
            .Where(predicate)
            .Take(take)
            .ToListAsync(ct);

        return candidates;
    }
}