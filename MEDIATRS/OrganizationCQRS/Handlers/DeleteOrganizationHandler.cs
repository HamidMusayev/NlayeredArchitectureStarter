using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.UnitOfWork;
using DTO.Responses;
using MediatR;
using MEDIATRS.OrganizationCQRS.Commands;

namespace MEDIATRS.OrganizationCQRS.Handlers;

public class DeleteOrganizationHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteOrganizationCommand, IResult>
{
    public async Task<IResult> Handle(DeleteOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        var data = await organizationRepository.GetAsync(e => e.Id == request.Id);
        if (data is null) return new ErrorResult(Messages.DataNotFound.Translate());

        organizationRepository.SoftDelete(data);

        await unitOfWork.CommitAsync(cancellationToken);

        return new SuccessResult(Messages.Success.Translate());
    }
}