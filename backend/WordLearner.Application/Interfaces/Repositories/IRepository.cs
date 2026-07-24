using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IRepository<T>
    where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

    // userId null kalabilir — self-servis kayıtta (RegisterCommand, GenerateQrLoginCommand vb.)
    // kaydı oluşturan aktör henüz kendi Id'sine sahip değildir.
    Task<T> AddAsync(T entity, int? userId = null, CancellationToken ct = default);

    Task UpdateAsync(T entity, int? userId = null, CancellationToken ct = default);
    Task SoftDeleteAsync(int id, int? userId = null, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
