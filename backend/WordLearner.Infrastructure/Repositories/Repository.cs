/// <summary>
/// Repository.cs
///
/// AMAÇ: IRepository&lt;T&gt; arayüzünün generic implementasyonu — tüm spesifik repository'lerin temel sınıfı.
/// NEDEN: Ortak CRUD kodunun tek yerde yazılması; türetilmiş sınıflar sadece özel sorguları ekler.
/// BAĞIMLILIKLAR: WordLearnerDbContext, BaseEntity (Domain), IRepository (Application)
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Common;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementasyonu.
///
/// AMAÇ: Tüm entity'ler için ortak CRUD operasyonlarını sağlamak.
/// NEDEN: Her repository'de aynı kodu tekrarlamamak — DRY prensibi.
/// NASIL: protected _db ve _set alanları türetilmiş sınıfların özel sorgular yazmasına izin verir.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WordLearnerDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(WordLearnerDbContext db)
    {
        _db  = db;
        _set = db.Set<T>();
    }

    /// <summary>
    /// AMAÇ: ID ile tek kayıt getirir.
    /// NEDEN: HasQueryFilter otomatik soft delete filtrelemesi yapar — IsDeleted=true görünmez.
    /// NASIL: FirstOrDefaultAsync ile Id eşleşmesi; birden fazla kayıt olamaz (PK).
    /// </summary>
    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    /// <summary>
    /// AMAÇ: Tüm aktif kayıtları getirir.
    /// NEDEN: Küçük tablolar (kategoriler, rozetler) için pratik listeleme.
    /// NASIL: HasQueryFilter zaten uygulanmış; ek filtre gerekmez.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Yeni entity ekler ve veritabanına kaydeder.
    /// NEDEN: Ekleme ve kaydetme atomik — yarım kayıt olmaz.
    /// NASIL: AddAsync + SaveChangesAsync; CreatedAt entity'de set edilmiş olmalı.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// AMAÇ: Mevcut entity'yi günceller.
    /// NEDEN: UpdatedAt DbContext.SaveChangesAsync override'ında otomatik güncellenir.
    /// NASIL: EF Core change tracking üzerinden güncelleme; entity tracked durumda olmalı.
    /// </summary>
    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// AMAÇ: Entity'yi soft delete yapar — fiziksel silme yoktur.
    /// NEDEN: Veri kaybını önlemek; audit trail korumak; HasQueryFilter ile otomatik gizlenir.
    /// NASIL: IsDeleted=true, DeletedAt=şimdi; UpdateAsync çağrılır.
    /// </summary>
    public virtual async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        // Kayıt bulunamazsa işlem yapılmaz — controller zaten 404 döner
        var entity = await GetByIdAsync(id, ct);
        if (entity is null) return;

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await UpdateAsync(entity, ct);
    }

    /// <summary>
    /// AMAÇ: Değişiklikleri veritabanına kaydeder — birden fazla işlemi tek transaction'da birleştirmek için.
    /// NEDEN: Servis katmanı bazen birden fazla entity değiştirip tek seferde kaydetmek ister.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
