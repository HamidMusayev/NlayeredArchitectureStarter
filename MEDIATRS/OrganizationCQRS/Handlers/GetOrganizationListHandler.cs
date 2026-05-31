using BLL.Mappers;
using CORE.Localization;
using DAL.EntityFramework.Abstract;
using DTO.Organization;
using DTO.Responses;
using MediatR;
using MEDIATRS.OrganizationCQRS.Queries;

namespace MEDIATRS.OrganizationCQRS.Handlers;

/// <summary>
///     Handles <see cref="GetOrganizationListQuery" />. Retrieves all non-deleted organization rows
///     and maps them to a list of <c>OrganizationToListDto</c>.
/// </summary>
public class GetOrganizationListHandler(
    IOrganizationRepository organizationRepository,
    OrganizationMapper organizationMapper)
    : IRequestHandler<GetOrganizationListQuery, IDataResult<List<OrganizationToListDto>>>
{
    public async Task<IDataResult<List<OrganizationToListDto>>> Handle(
        GetOrganizationListQuery request,
        CancellationToken cancellationToken)
    {
        var data = await organizationRepository.GetListAsync();
        return new SuccessDataResult<List<OrganizationToListDto>>(
            organizationMapper.ToListDtos(data), Messages.Success.Translate());
    }
}