using DTO.Common;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Utility;

/// <summary>
///     Bridge between EF Core's <see cref="IQueryable{T}" /> and the standard
///     <see cref="PagedResult{T}" /> envelope. One call computes the total + materializes the
///     page in two round-trips. Pass an already-ordered query — without an explicit
///     <c>OrderBy</c>, EF will warn and the page contents are undefined across calls.
/// </summary>
public static class QueryablePaginationExtensions
{
    public static async Task<PagedResult<T>> PageAsync<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var safePage = Math.Max(1, page);
        var total = await source.LongCountAsync(ct);

        var items = pageSize > 0
            ? await source.Skip((safePage - 1) * pageSize).Take(pageSize).ToListAsync(ct)
            : await source.ToListAsync(ct);

        return new PagedResult<T>(items, safePage, pageSize, total);
    }
}