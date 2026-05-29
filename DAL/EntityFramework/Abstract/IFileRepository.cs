using DAL.EntityFramework.GenericRepository;
using File = ENTITIES.Entities.File;

namespace DAL.EntityFramework.Abstract;

/// <summary>
///     Entity-specific repository for <see cref="ENTITIES.Entities.File" /> metadata rows.
///     Inherits the full generic read/write surface; extend with domain-specific queries as needed.
/// </summary>
public interface IFileRepository : IGenericRepository<File>
{
}