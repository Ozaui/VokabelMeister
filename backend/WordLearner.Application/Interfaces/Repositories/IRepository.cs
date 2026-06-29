// ─────────────────────────────────────────────────────────────────────────────
// IRepository.cs
//
// AMAÇ: Tüm repository'lerin uygulaması gereken generic CRUD sözleşmesi.
// NEDEN: Servis katmanı somut repository implementasyonuna değil bu arayüze bağımlıdır;
//        böylece testlerde mock repository kolayca enjekte edilebilir (bağımlılığı tersine çevirme).
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.BaseEntity.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    // AMAÇ: Birincil anahtara göre tek kayıt getirir. Bulunamazsa null döner.
    // NEDEN: Nullable dönüş tipi ile "bulunamadı" durumu açıkça ifade edilir;
    //        servis katmanı null kontrolü yaparak EntityNotFoundException fırlatır.
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    // AMAÇ: Soft delete filtresi aktifken tüm kayıtları getirir.
    // NEDEN: Genel liste işlemleri için temel metot; özel filtreler için
    //        feature repository'ler bu interface'i genişletir.
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

    // AMAÇ: Yeni kayıt ekler ve eklenen entity'yi (Id dolu hâliyle) döner.
    // NEDEN: Dönüş değeri sayesinde controller, oluşturulan kaydın Id'sini
    //        hemen alıp 201 Created response'una ekleyebilir.
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    // AMAÇ: Mevcut kaydı günceller. UpdatedAt WordLearnerDbContext.SaveChangesAsync'te otomatik set edilir.
    Task UpdateAsync(T entity, CancellationToken ct = default);

    // AMAÇ: Kaydı fiziksel olarak silmek yerine IsDeleted=true, DeletedAt=UtcNow yapar (soft delete).
    // NEDEN: Silinen veriler DB'de tutulur; audit trail korunur; admin gerektiğinde geri yükleyebilir.
    Task SoftDeleteAsync(int id, CancellationToken ct = default);

    // AMAÇ: Birden fazla değişikliği tek seferde kaydetmek gerektiğinde kullanılır.
    // NEDEN: AddAsync/UpdateAsync zaten SaveChanges çağırır; bu metot yalnızca
    //        birden fazla işlemi toplu commit etmek için vardır.
    Task SaveChangesAsync(CancellationToken ct = default);
}
