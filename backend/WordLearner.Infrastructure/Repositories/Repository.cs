using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class Repository<T> : IRepository<T>
    where T : BaseEntity
{
    protected readonly WordLearnerDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(WordLearnerDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await _set.ToListAsync(ct);

    public virtual async Task<T> AddAsync(
        T entity,
        int? userId = null,
        CancellationToken ct = default
    )
    {
        entity.CreatedByUserId = userId;
        entity.UpdatedByUserId = userId;
        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    // _set.Update() ÇAĞRILMAZ — entity her zaman bu Repository'den (GetByIdAsync vb.) alınmış,
    // yani DbContext tarafından zaten takip edilen bir örnektir; Update() tüm alanları Modified
    // işaretleyip UPDATE'i gereksiz genişletir. Detached bir entity ile çağrılırsa değişiklik sessizce kaybolur.
    public virtual async Task UpdateAsync(
        T entity,
        int? userId = null,
        CancellationToken ct = default
    )
    {
        entity.UpdatedByUserId = userId;
        await _db.SaveChangesAsync(ct);
    }

    public virtual async Task SoftDeleteAsync(
        int id,
        int? userId = null,
        CancellationToken ct = default
    )
    {
        var entity = await GetByIdAsync(id, ct) ?? throw new EntityNotFoundException(typeof(T), id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserId = userId;
        await UpdateAsync(entity, userId, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
