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
  **`_set.Update(entity)` ÇAĞRILMAZ (2026-07-11'de kaldırıldı):** entity bu Repository üzerinden
  önce `GetByIdAsync`/`GetByXxxAsync` ile alınmış (dolayısıyla zaten TAKİP EDİLEN) bir örnek olduğu
  için `_set.Update()` gereksizdi — TÜM property'leri (değişmemiş olanlar dahil) `Modified`
  işaretleyip UPDATE ifadesini gereksiz genişletiyordu. EF'in otomatik change tracking'i tek başına
  yeterli. **Kısıt:** `UpdateAsync`'e yalnızca bu Repository'den alınmış (tracked) entity geçilmeli;
  detached bir entity ile çağrılırsa değişiklikler sessizce kaybolur.
- `SoftDeleteAsync` → önce `GetByIdAsync`, bulunamazsa [[EntityNotFoundException]] fırlatır; bulunursa
  `IsDeleted=true, DeletedAt=UtcNow` (+ verilmişse `DeletedByUserId`) set edip `UpdateAsync(entity, userId)`
  çağırır (fiziksel silme **yok**) — bu yüzden soft delete aynı zamanda `UpdatedByUserId`'yi de günceller.
- `SaveChangesAsync` → toplu commit için ayrı metot; `AddAsync`/`UpdateAsync` zaten kendi içinde çağırır.
- `userId` parametresi üç metotta da opsiyonel (`int? userId = null`). **Güncelleme (2026-07-11):**
  A-03/A-03.1 ilk yazıldığında Auth/QrLogin Handler'larının HİÇBİRİ bu parametreyi fiilen
  geçmiyordu (kod denetiminde bulundu) — artık kaydın SAHİBİ kendi eylemiyle güncellediğinde
  o kullanıcının Id'si geçiliyor; yalnızca gerçek self-servis kayıt oluşturma ve sistemin
  otomatik geçişlerinde `null` kalıyor, bkz. [[Auth_Domain]], [[BaseEntity]].

## DI Kaydı
Generic `IRepository<T>` doğrudan kaydedilmedi (hâlâ A-02'deki gibi) — ama her feature repository
kendi somut arayüzüyle kayıtlı: [[InfrastructureServiceExtensions]]'ta
`services.AddScoped<IUserRepository, UserRepository>()` (ve `IRefreshTokenRepository`/
`IQrLoginSessionRepository`) A-03/A-03.1'de eklendi — her biri `Repository<T>`'yi miras alır.

## Test Kapsamı
[[RepositoryTests]] (in-memory EF Core, 10 test — CRUD + soft delete filtresi + `UpdatedAt`/`userId` audit alanları) — yazıldı, bkz. [[WordLearner_Tests]].
