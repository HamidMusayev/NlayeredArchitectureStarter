using DTO.Organization;
using DTO.Responses;
using MediatR;

namespace MEDIATRS.OrganizationCQRS.Queries;

/// <summary>
///     MediatR query that fetches a single <c>Organization</c> by its primary key.
///     Returns <c>ErrorDataResult</c> when the record does not exist.
///     Handled by <c>GetOrganizationByIdHandler</c>.
/// </summary>
public record GetOrganizationByIdQuery(Guid Id) : IRequest<IDataResult<OrganizationToListDto>>;