using Application.Specifications;

namespace Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TResult>> ListAsync<TResult>(IProjectionSpecification<T, TResult> spec, CancellationToken cancellationToken = default)
        where TResult : class;

    Task<TResult?> FirstOrDefaultAsync<TResult>(IProjectionSpecification<T, TResult> spec, CancellationToken cancellationToken = default)
        where TResult : class;

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Add(T entity);

    void Update(T entity);

    void Delete(T entity);
}
