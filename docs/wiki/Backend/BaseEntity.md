# BaseEntity

**Özet:** [[WordLearner_Domain]] içinde tanımlı, tüm entity'lerin türeyeceği soyut taban sınıf — Id, oluşturma/güncelleme zaman damgaları, soft delete ve "kim yaptı" (audit actor) alanlarını tek yerden yönetir. `IsDeleted`/`DeletedAt` alanları [[WordLearnerDbContext]]'teki global query filter tarafından otomatik uygulanır, `UpdatedAt` ise [[Repository]]/`SaveChangesAsync` tarafından otomatik güncellenir. `CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId` [[Repository]]'nin `AddAsync`/`UpdateAsync`/`SoftDeleteAsync` metotlarına geçilen (opsiyonel) `userId` parametresiyle set edilir.
**Kütüphaneler:** Saf C# — dış bağımlılık yok
**Bağlantılar:** [[WordLearner_Domain]] · [[WordLearnerDbContext]] · [[Repository]] · [[IRepository]] · [[Veritabani_Semasi]] · [[Auth_Domain]] · [[Loglama_Domain]]

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
| `CreatedByUserId` | `int?` | Kaydı oluşturan kullanıcının Id'si — [[Repository]].`AddAsync` set eder |
| `UpdatedByUserId` | `int?` | Kaydı en son güncelleyen (soft delete dâhil) kullanıcının Id'si — `UpdateAsync`/`SoftDeleteAsync` set eder |
| `DeletedByUserId` | `int?` | Kaydı soft delete eden kullanıcının Id'si — `SoftDeleteAsync` set eder |

**Not (A-02, Auth öncesi):** Yukarıdaki üç "kim" alanı `int?` — [[Auth_Domain]] (A-03) tamamlanana
kadar servis katmanında henüz "mevcut kullanıcı" kavramı yok, dolayısıyla `userId` geçilmediği sürece
`null` kalır. FK (→ `Users(Id)`) da `User` entity'si yazılınca EF config ile bağlanacak. Bu alanlar,
tam geçmiş/diff bilgisini tutan [[Loglama_Domain]] (`ActivityLog`) ile **çakışmaz** — `ActivityLog`
her işlemin tam geçmişini (old/new JSON) tutar, `BaseEntity`'deki alanlar ise "şu an kaydın son hâlini
kim oluşturdu/güncelledi/sildi" sorusunu log'a JOIN atmadan hızlıca cevaplar (denormalize, tek en-son-değer önbelleği).

## Kullanan Yerler
Tüm gelecekteki feature entity'ler (`User`, `Word`, `Category`, `UserCard`, ...) bu sınıftan türeyecek
— tam liste → [[Veritabani_Semasi]]. `IRepository<T> where T : BaseEntity` kısıtı sayesinde
[[Repository]] her entity için tekrar yazılmadan çalışır.
