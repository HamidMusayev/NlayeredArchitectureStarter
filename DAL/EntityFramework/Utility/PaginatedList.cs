using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Utility;

/// <summary>
///     EF Core-backed paged result wrapper. Extends <see cref="PaginationInfo" /> with the actual
///     item slice and a <see cref="Meta" /> sidecar for building <c>?page=N</c> links.
///     Use <see cref="CreateAsync" /> to materialise from an <see cref="IQueryable{T}" />.
///     Pass <c>pageIndex = 0</c> only when all rows are intentionally requested.
/// </summary>
public class PaginatedList<T> : PaginationInfo
{
    public PaginatedList(List<T> items, int totalCount, int pageIndex, int pageSize)
    {
        if (pageIndex != 0)
        {
            PageIndex = pageIndex;
            TotalRecordCount = totalCount;
            TotalPageCount = pageSize == 0 ? 1 : (totalCount + pageSize - 1) / pageSize;
        }
        else
        {
            // pageIndex == 0 is treated as "return all rows in one page".
            // Callers should pass a real page index in production code paths.
            PageIndex = 1;
            TotalRecordCount = items.Count;
            TotalPageCount = 1;
        }

        Items = items;

        Meta = new PageMeta(
            PageIndex,
            pageSize,
            TotalPageCount,
            TotalRecordCount,
            HasPreviousPage,
            HasNextPage,
            HasPreviousPage ? PageIndex - 1 : null,
            HasNextPage ? PageIndex + 1 : null);
    }

    public List<T> Items { get; set; }

    /// <summary>
    ///     Client-friendly pagination sidecar — use this for emitting <c>?page=N</c> links
    ///     without recomputing on the consumer. Mirrors and supersedes the legacy
    ///     <see cref="PaginationInfo" /> fields.
    /// </summary>
    public PageMeta Meta { get; }

    /// <summary>
    ///     Materializes a paged result from an <see cref="IQueryable{T}" />.
    ///     Pass <paramref name="pageIndex" /> = 0 only when you intentionally want every row;
    ///     otherwise pass a 1-based page index.
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();

        var items = pageIndex != 0
            ? await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync()
            : await source.ToListAsync();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}