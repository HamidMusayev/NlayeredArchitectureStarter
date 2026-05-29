namespace CORE.Config;

/// <summary>Query-string parameter names read by <c>HttpPaginationContext</c> for paging.</summary>
public record RequestSettings
{
    public required string PageIndex { get; set; }
    public required string PageSize { get; set; }
}