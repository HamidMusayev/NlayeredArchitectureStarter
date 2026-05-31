using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;
using ENTITIES.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IUserRepository" />. Extends the generic surface with
///     email-uniqueness check, per-user PBKDF2 salt retrieval, and <see cref="UpdateUser" /> which
///     marks the entity <c>Modified</c> while explicitly excluding the <c>Password</c> and
///     <c>Salt</c> columns to prevent credential corruption on profile updates.
///     <para>
///         <see cref="GetUserSaltAsync" /> and <see cref="GetPermissionKeysAsync" /> run on hot
///         per-request paths (login and authorization checks respectively), so both go through
///         <c>EF.CompileAsyncQuery</c> — query translation happens once at type init, not per call.
///     </para>
/// </summary>
public class UserRepository(DataContext dataContext) : GenericRepository<User>(dataContext), IUserRepository
{
    // Project to the single column we care about — no entity hydration, no change tracker.
    private static readonly Func<DataContext, string, Task<string?>> CompiledGetSaltByEmail =
        EF.CompileAsyncQuery((DataContext db, string email) =>
            db.Users.Where(u => u.Email == email).Select(u => (string?)u.Salt).FirstOrDefault());

    // Streaming variant — caller materializes via ToListAsync below.
    private static readonly Func<DataContext, Guid, IAsyncEnumerable<string>> CompiledGetPermissionKeys =
        EF.CompileAsyncQuery((DataContext db, Guid userId) =>
            db.Users
                .Where(u => u.Id == userId && u.Role != null)
                .SelectMany(u => u.Role!.Permissions.Select(p => p.Key))
                .Distinct());

    public Task<string?> GetUserSaltAsync(string userEmail)
    {
        return CompiledGetSaltByEmail(dataContext, userEmail);
    }

    public Task<bool> IsUserExistAsync(string email, Guid? userId)
    {
        return dataContext.Users.AnyAsync(m => m.Email == email && m.Id != userId);
    }

    public void UpdateUser(User user)
    {
        dataContext.Entry(user).State = EntityState.Modified;
        dataContext.Entry(user).Property(m => m.Password).IsModified = false;
        dataContext.Entry(user).Property(m => m.Salt).IsModified = false;
    }

    public async Task<List<string>> GetPermissionKeysAsync(Guid userId, CancellationToken ct = default)
    {
        var keys = new List<string>();
        await foreach (var key in CompiledGetPermissionKeys(dataContext, userId).WithCancellation(ct))
            keys.Add(key);
        return keys;
    }

    public Task<List<Guid>> GetUserIdsByRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        // Caller hands a raw Guid (from Role.Id, which is still the Auditable.Id Guid). Wrap to
        // RoleId for the EF query so the value converter can do its thing.
        var typed = new RoleId(roleId);
        return dataContext.Users
            .Where(u => u.RoleId == typed)
            .Select(u => u.Id)
            .ToListAsync(ct);
    }
}