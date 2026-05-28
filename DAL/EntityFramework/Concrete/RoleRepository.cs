using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

public class RoleRepository(DataContext dataContext) : GenericRepository<Role>(dataContext), IRoleRepository
{
    public Role UpdateRole(Role role)
    {
        dataContext.Roles.Update(role);
        return role;
    }

    public async Task ClearRolePermissionsAync(Guid roleId)
    {
        var entity = await dataContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        entity?.Permissions.Clear();
        // Commit is owned by IUnitOfWork; the caller (RoleService.UpdateAsync) commits once.
    }

    public async Task AddRoleAsync(Role role)
    {
        await dataContext.Roles.AddAsync(role);
    }
}