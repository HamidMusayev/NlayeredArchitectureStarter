using DTO.Organization;
using DTO.Responses;
using MediatR;

namespace MEDIATRS.OrganizationCQRS.Commands;

/// <summary>
///     MediatR command that creates a new <c>Organization</c> record from the supplied DTO.
///     Handled by <c>AddOrganizationHandler</c>.
/// </summary>
public record AddOrganizationCommand(OrganizationToAddDto Organization) : IRequest<IResult>;