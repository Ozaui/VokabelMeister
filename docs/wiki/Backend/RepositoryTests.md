# RepositoryTests

**Özet:** [[Repository]] generic taban sınıfının CRUD işlemlerini ve [[WordLearnerDbContext]]'teki global soft delete filtresini in-memory EF Core ile doğrulayan xUnit test sınıfı — A-02'nin son kalan "Birim testleri" adımı. Hiçbir feature entity'si yokken, yalnızca bu dosyaya özel bir `TestEntity`/`TestDbContext` çifti kullanılarak yazıldı.
**Kütüphaneler:** xUnit, FluentAssertions 6.12.0, Microsoft.EntityFrameworkCore.InMemory 9.0.0
**Bağlantılar:** [[Repository]] · [[IRepository]] · [[WordLearnerDbContext]] · [[BaseEntity]] · [[EntityNotFoundException]] · [[WordLearner_Tests]] · [[Kodlama_Standartlari]]

## Konum
`backend/WordLearner.Tests/Repositories/RepositoryTests.cs`

## Neden Test Yardımcıları (TestEntity / TestDbContext)
A-02 aşamasında Domain katmanında henüz hiçbir gerçek entity yok, dolayısıyla `Repository<T>`'yi
test etmek için gerçek bir feature entity beklenemez. Bu dosyaya özel iki yardımcı tanımlanmıştır:
- **`TestEntity : BaseEntity`** — testin ihtiyaç duyduğu minimal sahte entity (tek alan: `Name`).
- **`TestDbContext : WordLearnerDbContext`** — `OnModelCreating`'i override edip `TestEntity`'yi
  modele elle ekler (WordLearnerDbContext'te henüz DbSet yok), sonra `base.OnModelCreating`'i
  çağırarak gerçek soft delete filtresinin `TestEntity` üzerinde de çalışmasını sağlar.

Bu ikisi **yalnızca test projesinde** yaşar, production koduna hiç dokunulmaz.

## Kapsanan Senaryolar (7 test)
| Test | Ne Doğrular |
|------|--------------|
| `AddAsync_GecerliEntity_IdAtarVeKaydeder` | Mutlu yol — ekleme sonrası Id atanır |
| `GetByIdAsync_KayitVarsa_EntityDoner` | Mutlu yol — var olan kayıt getirilir |
| `GetByIdAsync_KayitYoksa_NullDoner` | Bulunamadı — `null` döner, exception fırlatılmaz |
| `GetAllAsync_SoftDeleteFiltresiAktifken_YalnizcaSilinmemisleriDoner` | **En kritik senaryo** — global query filter silinmiş kaydı listeden çıkarır |
| `UpdateAsync_MevcutEntity_UpdatedAtOtomatikGuncellenir` | `WordLearnerDbContext.SaveChangesAsync` override'ı `UpdatedAt`'i otomatik günceller |
| `SoftDeleteAsync_KayitVarsa_IsDeletedTrueYaparVeSorgudanGizler` | Soft delete fiziksel silmez, `IsDeleted`/`DeletedAt` set eder + sorgudan gizler |
| `SoftDeleteAsync_KayitYoksa_EntityNotFoundExceptionFirlatir` | Bulunamadı — [[EntityNotFoundException]] fırlatılır |

Bu kapsam, [[Kodlama_Standartlari]] §7.5'teki zorunlu minimum (mutlu yol / bulunamadı / uç durum)
ile birebir örtüşür; "yetki/sahiplik" senaryosu bu jenerik taban sınıfa uygulanamaz (kişisel
içerik olmadığından), sonraki feature repository testlerinde eklenecektir.

## Roadmap Durumu
[[API_Yol_Haritasi_Sistemi]]'ndeki A-02 sayfasına 7. adım (`tur: 'test'`) olarak işlendi;
`TASK.md`'de A-02'nin "Birim testleri" maddesi ✅.
