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
| `Task<T> AddAsync(T entity, ct)` | Yeni kayıt ekler, Id dolu hâliyle döner (201 Created için) |
| `Task UpdateAsync(T entity, ct)` | Günceller; `UpdatedAt` otomatik set edilir |
| `Task SoftDeleteAsync(int id, ct)` | Fiziksel silme yerine `IsDeleted=true, DeletedAt=UtcNow` |
| `Task SaveChangesAsync(ct)` | Birden fazla değişikliği toplu commit etmek için |

## Implementasyon
Tek somut implementasyon → [[Repository]] (EF Core tabanlı, [[WordLearnerDbContext]] kullanır).
Feature repository'ler (`IUserRepository` vb., henüz yazılmadı) bu arayüzü genişletecek.
