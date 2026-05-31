using DTO.Auth;
using ENTITIES.Entities;
using Riok.Mapperly.Abstractions;

namespace BLL.Mappers;

/// <summary>
///     Compile-time mapper for <c>Token</c>. Only the <c>LoginResponseDto</c> → entity direction
///     is needed — outbound mapping was dropped along with the dead <c>TokenService.GetAsync</c>
///     path. <c>UserId</c> is projected out of the nested user DTO via <see cref="MapPropertyAttribute" />.
///     <c>Jti</c> and <c>RefreshTokenHash</c> are set explicitly in <c>TokenService.IssueAsync</c> after
///     the map runs since the DTO doesn't carry them.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class TokenMapper
{
    [MapProperty($"{nameof(LoginResponseDto.User)}.{nameof(LoginResponseDto.User.Id)}", nameof(Token.UserId))]
    [MapperIgnoreTarget(nameof(Token.User))]
    public partial void UpdateEntity(LoginResponseDto source, Token target);
}