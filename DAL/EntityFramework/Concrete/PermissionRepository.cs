using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IPermissionRepository" />. Delegates entirely to
///     <see cref="GenericRepository{TEntity}" /> — extend with permission-lookup helpers as
///     domain requirements evolve.
/// </summary>
public class PermissionRepository(DataContext dataContext)
    : GenericRepository<Permission>(dataContext), IPermissionRepository;