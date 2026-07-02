// ─────────────────────────────────────────────────────────────────────────────
// Repository.cs
//
// AMAÇ: IRepository<T> arayüzünün EF Core tabanlı generic implementasyonu.
// NEDEN: Tekrar eden CRUD kodunu tek sınıfta toplar; feature repository'ler
//        yalnızca ek sorgular için bu sınıfı miras alır, temel işlemleri yeniden yazmaz.
// BAĞIMLILIKLAR: EF Core, WordLearnerDbContext, IRepository<T>, BaseEntity,
//                EntityNotFoundException (Application katmanından).
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WordLearnerDbContext _db;
    protected readonly DbSet<T> _set;

    // AMAÇ: DbContext ve DbSet referanslarını DI aracılığıyla alır.
    // NEDEN: _set kısayolu sayesinde alt sınıflar her seferinde _db.Set<T>() yazmak zorunda kalmaz.
    public Repository(WordLearnerDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    // AMAÇ: Soft delete filtresi aktifken Id'ye göre kayıt getirir.
    // NEDEN: virtual — feature repository gerektiğinde Include() ekleyerek override edebilir.
    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    // AMAÇ: Filtresiz tüm (silinmemiş) kayıtları belleğe yükler.
    // NEDEN: virtual — feature repository sayfalama veya projeksiyon için override edebilir.
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    // AMAÇ: Yeni entity'yi DB'ye ekler ve Id'si dolu hâliyle geri döner.
    // NEDEN: userId verilirse CreatedByUserId/UpdatedByUserId set edilir (kim oluşturdu).
    public virtual async Task<T> AddAsync(T entity, int? userId = null, CancellationToken ct = default)
    {
        entity.CreatedByUserId = userId;
        entity.UpdatedByUserId = userId;
        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    // AMAÇ: Mevcut entity'yi günceller. UpdatedAt WordLearnerDbContext.SaveChangesAsync'te otomatik set edilir.
    // NEDEN: userId verilirse UpdatedByUserId set edilir (kim güncelledi).
    public virtual async Task UpdateAsync(T entity, int? userId = null, CancellationToken ct = default)
    {
        entity.UpdatedByUserId = userId;
        _set.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    // AMAÇ: Kaydı fiziksel silmek yerine IsDeleted ve DeletedAt alanlarını set eder.
    // NEDEN: Fiziksel silme geri alınamaz; soft delete ile veri kaybı olmaz,
    //        admin silinmiş kaydı görmek istediğinde IgnoreQueryFilters() kullanır.
    //        userId verilirse DeletedByUserId set edilir (kim sildi).
    public virtual async Task SoftDeleteAsync(int id, int? userId = null, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException($"{typeof(T).Name} bulunamadı: Id={id}");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserId = userId;
        await UpdateAsync(entity, userId, ct);
    }

    // AMAÇ: Birden fazla değişikliği toplu olarak DB'ye yazar.
    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
