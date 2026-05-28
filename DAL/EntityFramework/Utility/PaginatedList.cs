using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Utility;

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
    }

    public List<T> Items { get; set; }

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