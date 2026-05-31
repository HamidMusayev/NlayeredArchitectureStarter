namespace DTO.Common;

/// <summary>
///     Standard paged-result envelope for list endpoints. Returned from every BLL service that
///     hands back a slice of a larger collection. <c>HasNext</c> is pre-computed so the SPA can
///     enable a "Next" button without re-doing the math.
///     <para>
///         Page numbering is 1-based — page 1 is the first page. Use <c>PageSize = 0</c> only
///         when you intentionally want every row in a single page; prefer a real page size for
///         normal list endpoints.
///     </para>
/// </summary>
/// <param name="Items">The slice of items on this page.</param>
/// <param name="Page">1-based page index that was requested.</param>
/// <param name="PageSize">Items per page that was requested.</param>
/// <param name="Total">Total matching rows in the underlying source.</param>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total)
{
    /// <summary>True when there is at least one more page after this one.</summary>
    public bool HasNext => PageSize > 0 && (long)Page * PageSize < Total;

    /// <summary>True when this isn't the first page.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>Total page count given the requested <see cref="PageSize" />. Always 1 when <c>PageSize == 0</c>.</summary>
    public int TotalPages => PageSize == 0 ? 1 : (int)((Total + PageSize - 1) / PageSize);

    /// <summary>
    ///     Functor-style map across <see cref="Items" /> that preserves all pagination metadata.
    ///     Use when transforming entities → DTOs at the BLL boundary so you don't have to
    ///     re-pass <see cref="Page" />, <see cref="PageSize" />, and <see cref="Total" /> by hand.
    /// </summary>
    public PagedResult<TResult> Map<TResult>(Func<IReadOnlyList<T>, IReadOnlyList<TResult>> project)
    {
        return new PagedResult<TResult>(project(Items), Page, PageSize, Total);
    }
}