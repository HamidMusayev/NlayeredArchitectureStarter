using AutoMapper;
using DTO.User;
using ENTITIES.Entities;

namespace BLL.Mappers;

/// <summary>
///     AutoMapper profile for <c>User</c> entity ↔ DTO conversions. Includes bidirectional
///     entity ↔ list DTO, add DTO → entity, and update DTO → entity mappings.
/// </summary>
public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<User, UserToListDto>();
        CreateMap<UserToAddDto, User>();
        CreateMap<UserToUpdateDto, User>();
        CreateMap<UserToListDto, User>();
    }
}