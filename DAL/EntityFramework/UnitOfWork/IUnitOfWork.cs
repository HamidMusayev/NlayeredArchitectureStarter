using DAL.EntityFramework.Abstract;

namespace DAL.EntityFramework.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    public IFileRepository FileRepository { get; set; }
    public IOrganizationRepository OrganizationRepository { get; set; }
    public IPermissionRepository PermissionRepository { get; set; }
    public IRoleRepository RoleRepository { get; set; }
    public ITokenRepository TokenRepository { get; set; }
    public IUserRepository UserRepository { get; set; }

    public Task CommitAsync();
}