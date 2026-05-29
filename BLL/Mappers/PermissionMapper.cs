using AutoMapper;
using DTO.Permission;
using ENTITIES.Entities;

namespace BLL.Mappers;

/// <summary>
///     AutoMapper profile for <c>Permission</c> entity ↔ DTO conversions.
///     Covers add, update (inbound), and list (outbound) DTOs.
/// </summary>
public class PermissionMapper : Profile
{
    public PermissionMapper()
    {
        CreateMap<PermissionToAddDto, Permission>();
        CreateMap<PermissionToUpdateDto, Permission>();
        CreateMap<Permission, PermissionToListDto>();
    }
}