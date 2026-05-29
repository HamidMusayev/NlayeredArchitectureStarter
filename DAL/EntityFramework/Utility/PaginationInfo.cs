namespace DAL.EntityFramework.Utility;

/// <summary>
///     Lightweight pagination metadata returned alongside any paged query — current page,
///     total record / page counts, and prev/next existence flags.
/// </summary>
public class PaginationInfo
{
    public int PageIndex { get; set; }

    public int TotalPageCount { get; set; }

    public int TotalRecordCount { get; set; }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPageCount;
}