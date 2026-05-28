namespace DAL.EntityFramework.GenericRepository;

public interface IWriteRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<List<T>> AddRangeAsync(List<T> entities);

    T Update(T entity);
    List<T> UpdateRange(List<T> entities);

    void Delete(T entity);
    void SoftDelete(T entity);
}