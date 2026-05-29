using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

/// <summary>
///     Entity-specific repository for <see cref="Role" /> records. Extends the generic surface with
///     <see cref="UpdateRole" /> (explicit EF state mark), <see cref="AddRoleAsync" /> (direct add),
///     and <see cref="ClearRolePermissionsAync" /> (load + clear the Permissions join collection —
///     caller commits via <c>IUnitOfWork</c>).
/// </summary>
public interface IRoleRepository : IGenericRepository<Role>
{
    Role UpdateRole(Role role);
    Task AddRoleAsync(Role role);
    Task ClearRolePermissionsAync(Guid roleId);
}