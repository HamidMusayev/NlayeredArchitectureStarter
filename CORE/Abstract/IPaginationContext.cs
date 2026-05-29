using DTO.Helper;

namespace CORE.Abstract;

/// <summary>
///     Resolves the current request's pagination parameters from a stable source. Default
///     <c>HttpPaginationContext</c> reads them from the query string using the names in
///     <see cref="CORE.Config.RequestSettings" />.
/// </summary>
public interface IPaginationContext
{
    PaginationDto GetPagination();
}