using AutoMapper;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Responses;
using ENTITIES.Entities;
using MediatR;
using MEDIATRS.OrganizationCQRS.Commands;

namespace MEDIATRS.OrganizationCQRS.Handlers;

/// <summary>
///     Handles <see cref="AddOrganizationCommand" />. Maps the DTO to an entity, persists it
///     via the repository, and commits the unit of work.
/// </summary>
public class AddOrganizationHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<AddOrganizationCommand, IResult>
{
    public async Task<IResult> Handle(AddOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var mapped = mapper.Map<Organization>(request.Organization);
        await organizationRepository.AddAsync(mapped);

        await unitOfWork.CommitAsync(cancellationToken);

        return new SuccessResult(Messages.Success.Translate());
    }
}