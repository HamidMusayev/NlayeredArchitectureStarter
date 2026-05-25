using ENTITIES.Entities.Generic;

namespace ENTITIES.Entities;

public class Token : Auditable, IEntity
{
    public virtual required User User { get; set; }
    public Guid UserId { get; set; }
    public required string AccessToken { get; set; }
    public DateTimeOffset AccessTokenExpireDate { get; set; }
    public required string RefreshToken { get; set; }
    public DateTimeOffset RefreshTokenExpireDate { get; set; }
}