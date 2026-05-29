using AutoMapper;
using DTO.Role;
using ENTITIES.Entities;

namespace BLL.Mappers;

/// <summary>
///     AutoMapper profile for <c>Role</c> entity ↔ DTO conversions. Covers the full list DTO
///     (bidirectional), the FK projection DTO, and add/update inbound DTOs.
/// </summary>
public class RoleMapper : Profile
{
    public RoleMapper()
    {
        CreateMap<Role, RoleToListDto>().ReverseMap();
        CreateMap<Role, RoleToFkDto>();
        CreateMap<RoleToAddDto, Role>();
        CreateMap<RoleToUpdateDto, Role>();
    }
}