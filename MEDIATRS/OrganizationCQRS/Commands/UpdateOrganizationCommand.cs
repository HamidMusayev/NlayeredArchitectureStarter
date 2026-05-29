using DTO.Organization;
using DTO.Responses;
using MediatR;

namespace MEDIATRS.OrganizationCQRS.Commands;

/// <summary>
///     MediatR command that updates an existing <c>Organization</c> by its primary key.
///     The existing <c>LogoFileId</c> is preserved (not overwritten) by the handler.
///     Handled by <c>UpdateOrganizationHandler</c>.
/// </summary>
public record UpdateOrganizationCommand(Guid OrganizationId, OrganizationToUpdateDto Organization) : IRequest<IResult>;