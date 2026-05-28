using AutoMapper;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Responses;
using ENTITIES.Entities;
using MediatR;
using MEDIATRS.OrganizationCQRS.Commands;

namespace MEDIATRS.OrganizationCQRS.Handlers;

public class UpdateOrganizationHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateOrganizationCommand, IResult>
{
    public async Task<IResult> Handle(UpdateOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var old = await organizationRepository.GetAsNoTrackingAsync(u => u.Id == request.OrganizationId);
        if (old is null) return new ErrorResult(Messages.DataNotFound.Translate());

        var mapped = mapper.Map<Organization>(request.Organization);

        mapped.Id = request.OrganizationId;
        mapped.LogoFileId = old.LogoFileId;

        organizationRepository.Update(mapped);
        await unitOfWork.CommitAsync(cancellationToken);

        return new SuccessResult(Messages.Success.Translate());
    }
}