using AutoMapper;
using DTO.Organization;
using ENTITIES.Entities;

namespace BLL.Mappers;

/// <summary>
///     AutoMapper profile for <c>Organization</c> entity ↔ DTO conversions.
///     Covers list (outbound), add, and update (inbound) DTOs.
/// </summary>
public class OrganizationMapper : Profile
{
    public OrganizationMapper()
    {
        CreateMap<Organization, OrganizationToListDto>();
        CreateMap<OrganizationToAddDto, Organization>();
        CreateMap<OrganizationToUpdateDto, Organization>();
    }
}