/// <summary>
/// IRepository.cs
///
/// AMAÇ: Tüm entity repository'leri için ortak CRUD sözleşmesini tanımlar.
/// NEDEN: Generic interface, her entity için tekrar tekrar aynı metodları yazmaktan kurtarır.
/// BAĞIMLILIKLAR: BaseEntity (Domain)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository arayüzü — tüm spesifik repository'lerin temel sözleşmesi.
///
/// AMAÇ: CRUD operasyonlarını tek bir sözleşmede toplamak.
/// NEDEN: Application katmanı Infrastructure'ı doğrudan bilmez; bu interface üzerinden konuşur.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// AMAÇ: ID ile tek kayıt getirir.
    /// NEDEN: En temel okuma operasyonu — soft delete filter otomatik uygulanır.
    /// NASIL: HasQueryFilter nedeniyle IsDeleted=true kayıtlar sorguya girmez.
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Tüm aktif kayıtları getirir.
    /// NEDEN: Küçük tablolar için listeleme; büyük tablolarda türetilmiş repository sayfalama ekler.
    /// NASIL: Soft delete filter otomatik uygulanır.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Yeni kayıt ekler ve kaydeder.
    /// NEDEN: Ekleme ve kaydetme her zaman birlikte yapılır — yarım kalan işlem olmaz.
    /// NASIL: SaveChangesAsync otomatik çağrılır.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Mevcut kaydı günceller.
    /// NEDEN: UpdatedAt otomatik DbContext.SaveChangesAsync override'ında güncellenir.
    /// NASIL: Entity tracked durumda olmalı.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kaydı soft delete yapar — fiziksel silme yoktur.
    /// NEDEN: Veri kaybını önlemek ve audit trail korumak için.
    /// NASIL: IsDeleted=true, DeletedAt=şimdi yapılır; sorgu filtresinden otomatik düşer.
    /// </summary>
    Task SoftDeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Bağımsız kaydetme — birden fazla işlemi tek transaction'da birleştirmek için.
    /// NEDEN: Servis katmanı bazen birden fazla entity'yi değiştirip tek seferde kaydetmek ister.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
