using System.Linq.Expressions;

namespace DAL.EntityFramework.GenericRepository;

public interface IReadRepository<T> where T : class
{
    Task<List<T>> GetListAsync(Expression<Func<T, bool>>? filter = null, bool ignoreQueryFilters = false);
    Task<T?> GetAsync(Expression<Func<T, bool>> filter, bool ignoreQueryFilters = false);
    IQueryable<T> GetList(Expression<Func<T, bool>>? filter = null, bool ignoreQueryFilters = false);

    Task<T?> GetAsNoTrackingAsync(Expression<Func<T, bool>> filter);
    IQueryable<T> GetAsNoTrackingList(Expression<Func<T, bool>>? filter = null);

    Task<int> CountAsync(Expression<Func<T, bool>> filter, bool ignoreQueryFilters = false);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter, bool ignoreQueryFilters = false);
    Task<bool> AllAsync(Expression<Func<T, bool>> filter, bool ignoreQueryFilters = false);

    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> filter, bool ignoreQueryFilters = false);
    Task<T?> SingleAsync(Expression<Func<T, bool>> filter, bool ignoreQueryFilters = false);
    Task<T?> FirstAsync(Expression<Func<T, bool>> filter, bool ignoreQueryFilters = false);
}