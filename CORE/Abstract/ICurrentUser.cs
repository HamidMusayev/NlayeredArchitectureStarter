namespace CORE.Abstract;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Role { get; }
}