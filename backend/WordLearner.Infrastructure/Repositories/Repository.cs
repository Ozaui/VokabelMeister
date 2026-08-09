using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Domain.Exceptions;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly WordLearnerDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(WordLearnerDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, int? userId = null, CancellationToken cancellationToken = default)
    {
        entity.CreatedByUserId = userId;
        entity.UpdatedByUserId = userId;
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(T entity, int? userId = null, CancellationToken cancellationToken = default)
    {
        entity.UpdatedByUserId = userId;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(int id, int? userId = null, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException($"{typeof(T).Name} not found: Id={id}");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserId = userId;
        entity.UpdatedByUserId = userId;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
