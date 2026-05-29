using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

/// <summary>
///     Entity-specific repository for <see cref="User" /> records. Adds existence check
///     (<see cref="IsUserExistAsync" />), salt retrieval (<see cref="GetUserSaltAsync" />), and a
///     <see cref="UpdateUser" /> helper that explicitly excludes the <c>Password</c> and <c>Salt</c>
///     columns from the update statement to prevent accidental credential overwrites.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<bool> IsUserExistAsync(string email, Guid? userId);

    Task<string?> GetUserSaltAsync(string userEmail);

    void UpdateUser(User user);
}