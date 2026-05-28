namespace DAL.EntityFramework.GenericRepository;

/// <summary>
///     Aggregate of <see cref="IReadRepository{T}" /> and <see cref="IWriteRepository{T}" />.
///     Prefer depending on a narrower interface (read-only or write-only) where possible.
/// </summary>
public interface IGenericRepository<T> : IReadRepository<T>, IWriteRepository<T>
    where T : class;