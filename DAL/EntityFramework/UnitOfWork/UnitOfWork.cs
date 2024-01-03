using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;

namespace DAL.EntityFramework.UnitOfWork;

public sealed class UnitOfWork(
    DataContext dataContext,
    IFileRepository fileRepository,
    IOrganizationRepository organizationRepository,
    IPermissionRepository permissionRepository,
    IRoleRepository roleRepository,
    ITokenRepository tokenRepository,
    IUserRepository userRepository)
    : IUnitOfWork
{
    private bool _isDisposed;

    public IFileRepository FileRepository { get; set; } = fileRepository;
    public IOrganizationRepository OrganizationRepository { get; set; } = organizationRepository;
    public IPermissionRepository PermissionRepository { get; set; } = permissionRepository;
    public IRoleRepository RoleRepository { get; set; } = roleRepository;
    public ITokenRepository TokenRepository { get; set; } = tokenRepository;
    public IUserRepository UserRepository { get; set; } = userRepository;

    public async Task CommitAsync()
    {
        await dataContext.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing) dataContext.Dispose();
    }

    private async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing) await dataContext.DisposeAsync();
    }
}