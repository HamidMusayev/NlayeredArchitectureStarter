using BLL.Mappers;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DTO.Organization;
using DTO.Responses;
using MediatR;
using MEDIATRS.OrganizationCQRS.Queries;

namespace MEDIATRS.OrganizationCQRS.Handlers;

/// <summary>
///     Handles <see cref="GetOrganizationByIdQuery" />. Fetches the entity by ID, maps to
///     <c>OrganizationToListDto</c>, and returns <c>ErrorDataResult</c> when not found.
/// </summary>
public class GetOrganizationByIdHandler(
    IOrganizationRepository organizationRepository,
    OrganizationMapper organizationMapper)
    : IRequestHandler<GetOrganizationByIdQuery, IDataResult<OrganizationToListDto>>
{
    public async Task<IDataResult<OrganizationToListDto>> Handle(GetOrganizationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var data = await organizationRepository.GetAsync(e => e.Id == request.Id);
        if (data is null)
            return new ErrorDataResult<OrganizationToListDto>(Messages.DataNotFound.Translate());

        return new SuccessDataResult<OrganizationToListDto>(organizationMapper.ToListDto(data),
            Messages.Success.Translate());
    }
}