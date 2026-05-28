using DTO.Helper;

namespace CORE.Abstract;

public interface IPaginationContext
{
    PaginationDto GetPagination();
}