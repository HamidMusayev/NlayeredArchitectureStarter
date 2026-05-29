using System.Linq.Expressions;
using DAL.EntityFramework.Context;
using ENTITIES.Entities.Generic;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.GenericRepository;

/// <summary>
///     Default <see cref="IGenericRepository{TEntity}" /> backed by <see cref="DataContext" />.
///     Covers all standard CRUD operations including <c>SoftDelete</c> (which requires the entity
///     to derive from <see cref="Auditable" />). Reads honour EF Core global query filters by default;
///     pass <c>ignoreQueryFilters = true</c> to bypass soft-delete / tenant isolation.
/// </summary>
public class GenericRepository<TEntity>(DataContext ctx) : IGenericRepository<TEntity>
    where TEntity : class
{
    // ---- writes ----

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await ctx.Set<TEntity>().AddAsync(entity);
        return entity;
    }

    public async Task<List<TEntity>> AddRangeAsync(List<TEntity> entities)
    {
        await ctx.Set<TEntity>().AddRangeAsync(entities);
        return entities;
    }

    public TEntity Update(TEntity entity)
    {
        ctx.Set<TEntity>().Update(entity);
        return entity;
    }

    public List<TEntity> UpdateRange(List<TEntity> entities)
    {
        ctx.Set<TEntity>().UpdateRange(entities);
        return entities;
    }

    public void Delete(TEntity entity)
    {
        ctx.Set<TEntity>().Remove(entity);
    }

    public void SoftDelete(TEntity entity)
    {
        if (entity is not Auditable auditable)
            throw new InvalidOperationException(
                $"Type {typeof(TEntity).Name} cannot be soft-deleted; it does not inherit {nameof(Auditable)}.");

        if (auditable.IsDeleted)
            throw new InvalidOperationException("Entity is already soft-deleted.");

        auditable.IsDeleted = true;
        ctx.Set<TEntity>().Update(entity);
    }

    // ---- reads ----

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters).FirstOrDefaultAsync(filter);
    }

    public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? filter = null,
        bool ignoreQueryFilters = false)
    {
        return (filter is null ? Query(ignoreQueryFilters) : Query(ignoreQueryFilters).Where(filter)).ToListAsync();
    }

    public IQueryable<TEntity> GetList(Expression<Func<TEntity, bool>>? filter = null, bool ignoreQueryFilters = false)
    {
        return filter is null ? Query(ignoreQueryFilters) : Query(ignoreQueryFilters).Where(filter);
    }

    public Task<TEntity?> GetAsNoTrackingAsync(Expression<Func<TEntity, bool>> filter)
    {
        return ctx.Set<TEntity>().AsNoTracking().SingleOrDefaultAsync(filter);
    }

    public IQueryable<TEntity> GetAsNoTrackingList(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter is null
            ? ctx.Set<TEntity>().AsNoTracking()
            : ctx.Set<TEntity>().AsNoTracking().Where(filter);
    }

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> filter, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters).CountAsync(filter);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters).AnyAsync(filter);
    }

    public Task<bool> AllAsync(Expression<Func<TEntity, bool>> filter, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters).AllAsync(filter);
    }

    public Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> filter, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters).SingleOrDefaultAsync(filter);
    }

    public Task<TEntity?> SingleAsync(Expression<Func<TEntity, bool>> filter, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters).SingleAsync(filter)!;
    }

    public Task<TEntity?> FirstAsync(Expression<Func<TEntity, bool>> filter, bool ignoreQueryFilters = false)
    {
        return Query(ignoreQueryFilters).FirstAsync(filter)!;
    }

    private IQueryable<TEntity> Query(bool ignoreQueryFilters)
    {
        return ignoreQueryFilters ? ctx.Set<TEntity>().IgnoreQueryFilters() : ctx.Set<TEntity>();
    }
}