using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

/// <summary>
///     Entity-specific repository for <see cref="Permission" /> records.
///     Inherits the full generic read/write surface; extend with permission-lookup helpers as needed.
/// </summary>
public interface IPermissionRepository : IGenericRepository<Permission>
{
}