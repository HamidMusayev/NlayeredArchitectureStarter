using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Abstract;

/// <summary>
///     Entity-specific repository for <see cref="Organization" /> records.
///     Inherits the full generic read/write surface; extend with tree-traversal helpers as needed.
/// </summary>
public interface IOrganizationRepository : IGenericRepository<Organization>
{
}