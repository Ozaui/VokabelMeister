# IRepository&lt;T&gt;

**Özet:** [[WordLearner_Application]] içinde tanımlı generic CRUD sözleşmesi — servis katmanı somut [[Repository]] implementasyonuna değil bu arayüze bağımlıdır (bağımlılığı tersine çevirme), böylece testlerde mock repository enjekte edilebilir. `T : BaseEntity` kısıtı taşır.
**Kütüphaneler:** Saf C# — yalnızca [[WordLearner_Domain]]'e bağımlı
**Bağlantılar:** [[BaseEntity]] · [[Repository]] · [[WordLearner_Application]] · [[EntityNotFoundException]] · [[Kodlama_Standartlari]]

## Konum
`backend/WordLearner.Application/Interfaces/Repositories/IRepository.cs`

## Metotlar
| Metot | Amaç |
|-------|------|
| `Task<T?> GetByIdAsync(int id, ct)` | Id'ye göre tek kayıt; bulunamazsa `null` — servis katmanı null kontrolüyle [[EntityNotFoundException]] fırlatır |
| `Task<IEnumerable<T>> GetAllAsync(ct)` | Soft delete filtresi aktifken tüm kayıtlar |
| `Task<T> AddAsync(T entity, int? userId = null, ct)` | Yeni kayıt ekler, Id dolu hâliyle döner (201 Created için); `userId` verilirse [[BaseEntity]].`CreatedByUserId`/`UpdatedByUserId`'ye yazılır |
| `Task UpdateAsync(T entity, int? userId = null, ct)` | Günceller; `UpdatedAt` otomatik set edilir; `userId` verilirse `UpdatedByUserId`'ye yazılır |
| `Task SoftDeleteAsync(int id, int? userId = null, ct)` | Fiziksel silme yerine `IsDeleted=true, DeletedAt=UtcNow`; `userId` verilirse `DeletedByUserId`/`UpdatedByUserId`'ye yazılır |
| `Task SaveChangesAsync(ct)` | Birden fazla değişikliği toplu commit etmek için |

**Not:** `userId` parametresi opsiyonel (`= null`) — [[Auth_Domain]] (A-03) tamamlanana kadar
çağıran taraf hiç geçmez, alanlar `null` kalır; A-03 sonrası servis katmanı JWT'den okuduğu
`userId`'yi bu parametreye geçirmeye başlayacak.

## Implementasyon
Tek somut implementasyon → [[Repository]] (EF Core tabanlı, [[WordLearnerDbContext]] kullanır).
Feature repository'ler (`IUserRepository` vb., henüz yazılmadı) bu arayüzü genişletecek.
