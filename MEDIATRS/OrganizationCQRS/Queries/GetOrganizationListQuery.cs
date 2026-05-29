using DTO.Organization;
using DTO.Responses;
using MediatR;

namespace MEDIATRS.OrganizationCQRS.Queries;

/// <summary>
///     MediatR query that returns all non-deleted <c>Organization</c> records as a flat list.
///     Handled by <c>GetOrganizationListHandler</c>.
/// </summary>
public record GetOrganizationListQuery : IRequest<IDataResult<List<OrganizationToListDto>>>;