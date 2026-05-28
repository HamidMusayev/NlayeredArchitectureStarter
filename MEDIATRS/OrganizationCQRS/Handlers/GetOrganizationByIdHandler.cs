using AutoMapper;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DTO.Organization;
using DTO.Responses;
using MediatR;
using MEDIATRS.OrganizationCQRS.Queries;

namespace MEDIATRS.OrganizationCQRS.Handlers;

public class GetOrganizationByIdHandler(
    IOrganizationRepository organizationRepository,
    IMapper mapper) : IRequestHandler<GetOrganizationByIdQuery, IDataResult<OrganizationToListDto>>
{
    public async Task<IDataResult<OrganizationToListDto>> Handle(GetOrganizationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var data = await organizationRepository.GetAsync(e => e.Id == request.Id);
        if (data is null)
            return new ErrorDataResult<OrganizationToListDto>(Messages.DataNotFound.Translate());

        var result = mapper.Map<OrganizationToListDto>(data);

        return new SuccessDataResult<OrganizationToListDto>(result, Messages.Success.Translate());
    }
}