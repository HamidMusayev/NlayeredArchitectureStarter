using AutoMapper;
using DTO.Auth;
using DTO.Token;
using ENTITIES.Entities;

namespace BLL.Mappers;

/// <summary>
///     AutoMapper profile for <c>Token</c> entity ↔ DTO conversions. Maps the entity to the
///     list DTO and maps <c>LoginResponseDto</c> → entity, extracting <c>UserId</c> from the
///     nested user DTO and suppressing the navigation property to avoid double-mapping.
/// </summary>
public class TokenMapper : Profile
{
    public TokenMapper()
    {
        CreateMap<Token, TokenToListDto>();
        CreateMap<LoginResponseDto, Token>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}