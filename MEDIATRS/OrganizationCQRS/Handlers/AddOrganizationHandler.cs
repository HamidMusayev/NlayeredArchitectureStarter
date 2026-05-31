using BLL.Mappers;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Responses;
using ENTITIES.Entities;
using MediatR;
using MEDIATRS.OrganizationCQRS.Commands;

namespace MEDIATRS.OrganizationCQRS.Handlers;

/// <summary>
///     Handles <see cref="AddOrganizationCommand" />. Constructs the entity, layers the DTO
///     over it via Mapperly, persists, and commits.
/// </summary>
public class AddOrganizationHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork,
    OrganizationMapper organizationMapper) : IRequestHandler<AddOrganizationCommand, IResult>
{
    public async Task<IResult> Handle(AddOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new Organization
        {
            FullName = string.Empty,
            ShortName = string.Empty,
            Address = string.Empty,
            PhoneNumber = string.Empty,
            Tin = string.Empty,
            Email = string.Empty,
            Rekvizit = string.Empty
        };
        organizationMapper.UpdateEntity(request.Organization, entity);

        await organizationRepository.AddAsync(entity);
        await unitOfWork.CommitAsync(cancellationToken);

        return new SuccessResult(Messages.Success.Translate());
    }
}