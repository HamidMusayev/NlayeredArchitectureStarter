using BLL.Mappers;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Responses;
using MediatR;
using MEDIATRS.OrganizationCQRS.Commands;

namespace MEDIATRS.OrganizationCQRS.Handlers;

/// <summary>
///     Handles <see cref="UpdateOrganizationCommand" />. Loads the tracked entity and layers
///     the DTO over it via Mapperly — keeps audit fields + LogoFileId intact instead of letting
///     the EF change tracker reset them to defaults.
/// </summary>
public class UpdateOrganizationHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork,
    OrganizationMapper organizationMapper) : IRequestHandler<UpdateOrganizationCommand, IResult>
{
    public async Task<IResult> Handle(UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await organizationRepository.GetAsync(o => o.Id == request.OrganizationId);
        if (existing is null) return new ErrorResult(Messages.DataNotFound.Translate());

        organizationMapper.UpdateEntity(request.Organization, existing);

        await unitOfWork.CommitAsync(cancellationToken);

        return new SuccessResult(Messages.Success.Translate());
    }
}