using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, int? userId = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, int? userId = null, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, int? userId = null, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
