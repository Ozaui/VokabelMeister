# Repository&lt;T&gt;

**Özet:** [[WordLearner_Infrastructure]] içindeki, [[IRepository]]'nin EF Core tabanlı generic implementasyonu — tekrar eden CRUD kodunu tek sınıfta toplar; feature repository'ler yalnızca ek sorgular için bu sınıfı miras alacak (tüm metotlar `virtual`). [[WordLearnerDbContext]] ve `DbSet<T>` üzerinden çalışır.
**Kütüphaneler:** Microsoft.EntityFrameworkCore
**Bağlantılar:** [[IRepository]] · [[WordLearnerDbContext]] · [[BaseEntity]] · [[EntityNotFoundException]] · [[WordLearner_Infrastructure]]

## Konum
`backend/WordLearner.Infrastructure/Repositories/Repository.cs`

## Davranış Notları
- Constructor DI ile `WordLearnerDbContext` alır, `_set = db.Set<T>()` kısayolunu tutar.
- `GetByIdAsync`/`GetAllAsync` **virtual** — feature repository `Include()` ekleyerek N+1 önleyebilir.
- `AddAsync` → `_set.AddAsync` + `SaveChangesAsync`, entity'yi Id dolu döner.
- `SoftDeleteAsync` → önce `GetByIdAsync`, bulunamazsa [[EntityNotFoundException]] fırlatır; bulunursa
  `IsDeleted=true, DeletedAt=UtcNow` set edip `UpdateAsync` çağırır (fiziksel silme **yok**).
- `SaveChangesAsync` → toplu commit için ayrı metot; `AddAsync`/`UpdateAsync` zaten kendi içinde çağırır.

## DI Kaydı
Henüz generic olarak DI'a kaydedilmedi — [[InfrastructureServiceExtensions]] şu an yalnızca
`WordLearnerDbContext`'i kaydediyor; `services.AddScoped(typeof(IRepository<>), typeof(Repository<>))`
gibi bir satır A-03+'ta ilk feature repository ile birlikte eklenecek.

## Test Kapsamı
[[RepositoryTests]] (in-memory EF Core, 7 test — CRUD + soft delete filtresi) — yazıldı, bkz. [[WordLearner_Tests]].
