# BaseEntity

**Özet:** [[WordLearner_Domain]] içinde tanımlı, tüm entity'lerin türeyeceği soyut taban sınıf — Id, oluşturma/güncelleme zaman damgaları ve soft delete alanlarını tek yerden yönetir. `IsDeleted`/`DeletedAt` alanları [[WordLearnerDbContext]]'teki global query filter tarafından otomatik uygulanır, `UpdatedAt` ise [[Repository]]/`SaveChangesAsync` tarafından otomatik güncellenir.
**Kütüphaneler:** Saf C# — dış bağımlılık yok
**Bağlantılar:** [[WordLearner_Domain]] · [[WordLearnerDbContext]] · [[Repository]] · [[IRepository]] · [[Veritabani_Semasi]]

## Konum
`backend/WordLearner.Domain/Entities/BaseEntity.cs`

## Alanlar
| Alan | Tip | Amaç |
|------|-----|------|
| `Id` | `int` | Birincil anahtar — EF Core otomatik PK olarak tanır |
| `CreatedAt` | `DateTime` (UTC) | Audit trail, varsayılan `DateTime.UtcNow` |
| `UpdatedAt` | `DateTime` (UTC) | [[Repository]].`UpdateAsync` / [[WordLearnerDbContext]].`SaveChangesAsync` her çağrıda otomatik günceller |
| `IsDeleted` | `bool` | Soft delete bayrağı — global query filter bunu kullanır |
| `DeletedAt` | `DateTime?` | Soft delete anı; hesap silme grace period (A-10) gibi süre bazlı işlemlerde kullanılır |

## Kullanan Yerler
Tüm gelecekteki feature entity'ler (`User`, `Word`, `Category`, `UserCard`, ...) bu sınıftan türeyecek
— tam liste → [[Veritabani_Semasi]]. `IRepository<T> where T : BaseEntity` kısıtı sayesinde
[[Repository]] her entity için tekrar yazılmadan çalışır.
