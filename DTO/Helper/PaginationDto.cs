namespace DTO.Helper;

/// <summary>
///     Standard pagination input — bound from query string by the
///     <c>HttpPaginationContext</c> resolver. Page numbers are 1-based. Pass <c>PageSize = 0</c>
///     to <c>IQueryable.PageAsync</c> to return every row in one page (intentional opt-in only;
///     prefer a real page size for normal list endpoints).
/// </summary>
public record PaginationDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}