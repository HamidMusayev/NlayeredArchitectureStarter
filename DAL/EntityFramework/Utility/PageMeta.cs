namespace DAL.EntityFramework.Utility;

/// <summary>
///     Pagination sidecar with client-friendly link cues. Lives next to the legacy
///     <see cref="PaginationInfo" /> fields rather than replacing them so existing API
///     consumers don't break.
/// </summary>
public record PageMeta(
    int PageIndex,
    int PageSize,
    int TotalPageCount,
    int TotalRecordCount,
    bool HasPrev,
    bool HasNext,
    int? PrevPageIndex,
    int? NextPageIndex);