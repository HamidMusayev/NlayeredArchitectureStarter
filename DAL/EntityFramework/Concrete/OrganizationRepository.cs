using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Concrete;

/// <summary>
///     EF Core implementation of <see cref="IOrganizationRepository" />. Delegates entirely to
///     <see cref="GenericRepository{TEntity}" /> — extend with tree-traversal or hierarchical
///     queries when domain requirements arise.
/// </summary>
public class OrganizationRepository(DataContext dataContext)
    : GenericRepository<Organization>(dataContext), IOrganizationRepository;