namespace DTO.Helper;

/// <summary>
///     Standard pagination input — bound from query string by the
///     <c>HttpPaginationContext</c> resolver. Page numbers are 1-based; <c>PageIndex = 0</c>
///     is a deliberate "return everything" sentinel handled by <c>PaginatedList&lt;T&gt;</c>.
/// </summary>
public record PaginationDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}