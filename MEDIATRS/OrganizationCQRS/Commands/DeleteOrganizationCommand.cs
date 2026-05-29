using DTO.Responses;
using MediatR;

namespace MEDIATRS.OrganizationCQRS.Commands;

/// <summary>
///     MediatR command that soft-deletes an <c>Organization</c> by its primary key.
///     Handled by <c>DeleteOrganizationHandler</c>.
/// </summary>
public record DeleteOrganizationCommand(Guid Id) : IRequest<IResult>;