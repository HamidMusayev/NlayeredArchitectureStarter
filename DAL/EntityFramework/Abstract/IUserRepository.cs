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

    /// <summary>
    ///     Returns the permission <c>Key</c> strings the user holds via their assigned role.
    ///     Empty list if the user has no role or the role has no permissions. Authoritative
    ///     source for <see cref="BLL.Abstract.IUserPermissionsCache" />.
    /// </summary>
    Task<List<string>> GetPermissionKeysAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    ///     Returns the ids of every user currently assigned <paramref name="roleId" />. Used by
    ///     <c>RoleService</c> to fan permission-cache invalidations when a role's permission set
    ///     changes.
    /// </summary>
    Task<List<Guid>> GetUserIdsByRoleAsync(Guid roleId, CancellationToken ct = default);
}