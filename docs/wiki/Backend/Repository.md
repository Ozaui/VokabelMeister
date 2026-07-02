# Repository&lt;T&gt;

**Özet:** [[WordLearner_Infrastructure]] içindeki, [[IRepository]]'nin EF Core tabanlı generic implementasyonu — tekrar eden CRUD kodunu tek sınıfta toplar; feature repository'ler yalnızca ek sorgular için bu sınıfı miras alacak (tüm metotlar `virtual`). [[WordLearnerDbContext]] ve `DbSet<T>` üzerinden çalışır.
**Kütüphaneler:** Microsoft.EntityFrameworkCore
**Bağlantılar:** [[IRepository]] · [[WordLearnerDbContext]] · [[BaseEntity]] · [[EntityNotFoundException]] · [[WordLearner_Infrastructure]]

## Konum
`backend/WordLearner.Infrastructure/Repositories/Repository.cs`

## Davranış Notları
- Constructor DI ile `WordLearnerDbContext` alır, `_set = db.Set<T>()` kısayolunu tutar.
- `GetByIdAsync`/`GetAllAsync` **virtual** — feature repository `Include()` ekleyerek N+1 önleyebilir.
- `AddAsync` → `_set.AddAsync` + `SaveChangesAsync`, entity'yi Id dolu döner; `userId` verilirse
  [[BaseEntity]].`CreatedByUserId`/`UpdatedByUserId`'ye aynı değeri yazar.
- `UpdateAsync` → `userId` verilirse `UpdatedByUserId`'ye yazar, sonra `SaveChangesAsync` çağırır.
- `SoftDeleteAsync` → önce `GetByIdAsync`, bulunamazsa [[EntityNotFoundException]] fırlatır; bulunursa
  `IsDeleted=true, DeletedAt=UtcNow` (+ verilmişse `DeletedByUserId`) set edip `UpdateAsync(entity, userId)`
  çağırır (fiziksel silme **yok**) — bu yüzden soft delete aynı zamanda `UpdatedByUserId`'yi de günceller.
- `SaveChangesAsync` → toplu commit için ayrı metot; `AddAsync`/`UpdateAsync` zaten kendi içinde çağırır.
- `userId` parametresi üç metotta da opsiyonel (`int? userId = null`) — A-03 (Auth) öncesi servis
  katmanı geçmediği için `null` kalır, bkz. [[BaseEntity]].

## DI Kaydı
Henüz generic olarak DI'a kaydedilmedi — [[InfrastructureServiceExtensions]] şu an yalnızca
`WordLearnerDbContext`'i kaydediyor; `services.AddScoped(typeof(IRepository<>), typeof(Repository<>))`
gibi bir satır A-03+'ta ilk feature repository ile birlikte eklenecek.

## Test Kapsamı
[[RepositoryTests]] (in-memory EF Core, 9 test — CRUD + soft delete filtresi + `userId` audit alanları) — yazıldı, bkz. [[WordLearner_Tests]].
