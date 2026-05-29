using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using File = ENTITIES.Entities.File;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IFileRepository" />. Delegates entirely to
///     <see cref="GenericRepository{TEntity}" /> — actual bytes are managed by
///     <c>IBlobStorage</c>; this repository tracks only the metadata row.
/// </summary>
public class FileRepository(DataContext dataContext)
    : GenericRepository<File>(dataContext), IFileRepository;