# Veritabanı Şeması (genel bakış)

**Özet:** MS SQL Server üzerinde planlanan tam şema `docs/DATABASE_SCHEMA.md` (index — ERD, seed data,
genel kurallar) + `docs/DATABASE_SCHEMA/` klasöründeki domain dosyalarında (`Auth.md`, `Icerik.md`,
`Kisisel_Icerik.md`, `SRS.md`, `Sosyal.md`, `Loglama.md`, `Sistem.md` — tam `CREATE TABLE` SQL'leri)
tanımlı — bu düğüm onu domain'lere bölünmüş şekilde bilgi grafiğine bağlar. Henüz **hiçbir migration
oluşturulmadı**; tüm tablolar tasarım aşamasında. Tüm tablolar (log tabloları hariç) [[BaseEntity]] alanlarını (Id/CreatedAt/UpdatedAt/IsDeleted/DeletedAt + `CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId`) taşır ve soft delete + (kişisel içerikte) `UserId` filtresi zorunludur.
**Kütüphaneler:** MS SQL Server, EF Core 9 (Code-First migrations, henüz kullanılmadı)
**Bağlantılar:** [[BaseEntity]] · [[WordLearnerDbContext]] · [[Auth_Domain]] · [[Icerik_Domain]] · [[Kisisel_Icerik_Domain]] · [[SRS_Domain]] · [[Sosyal_Domain]] · [[Loglama_Domain]] · [[Roller_ve_Erisim]]

## Domain Haritası

| Domain | Ana Tablolar | Wiki Düğümü |
|--------|--------------|-------------|
| Kimlik doğrulama | `Users`, `RefreshTokens` | [[Auth_Domain]] |
| Sistem içeriği | `Words`, `WordDetails`, `WordExamples`, `Categories`, `WordCategories` | [[Icerik_Domain]] |
| Kişisel içerik | `UserCards`, `UserCardExamples`, `UserCategories`, ara tablolar | [[Kisisel_Icerik_Domain]] |
| SRS / ilerleme | `UserProgress`, `UserCardProgress`, `LearningSessions`, `LearningHistory`, `Achievements` | [[SRS_Domain]] |
| Sosyal | `Classes`, `ClassWords`, `ClassMemberships`, `Friendships`, `SharedContents` | [[Sosyal_Domain]] |
| Loglama | `ActivityLog`, `ApplicationLog`, `SecurityLog` | [[Loglama_Domain]] |
| Sistem ayarı | `SmtpSettings` | [[Guvenlik_Politikalari]] (AES şifreleme bağlamında) |

## İlişki Özeti (ERD)

```
Users ─┬─ RefreshTokens (1:N)
       ├─ UserProgress (1:N) ── Words
       ├─ UserCards (1:N) ─┬─ UserCardProgress (1:N)
       │                   ├─ UserCardExamples (1:N)
       │                   ├─ UserCardCategories (M:N) ── Categories
       │                   └─ UserCardUserCategories (M:N) ── UserCategories
       ├─ UserCategories (1:N)
       ├─ LearningSessions / LearningHistory (1:N)
       ├─ Classes (owner) / ClassMemberships / Friendships / SharedContents
       └─ Activity/Security loglarda UserId (1:N, SET NULL)

Words ─┬─ WordDetails (1:1) ── WordExamples (1:N) ── WordCategories (M:N) ── Categories
Classes ─ ClassWords (1:N) [yalnızca sınıf üyeleri] · ClassCategories / ClassUserCategories (M:N)
Categories ─ self-ref (ParentCategoryId)
```

## Genel Kurallar (uygulama katmanında zorunlu)

1. `UserProgress` (sistem) ve `UserCardProgress` (kişisel) **ayrı** tablolardır.
2. `LearningHistory.WordId` ve `UserCardId` ikisi birden NULL olamaz (uygulama kontrolü).
3. `UserCard` yalnızca sahibi tarafından görülür → her sorguda `UserId` filtresi zorunlu.
4. **Sistem kelimesi eşleşmesi:** `UserCard.FrontText`, `Words.GermanWord` ile eşleşir ve kullanıcının
   o kelime için `UserProgress` kaydı varsa, Mixed sınav oturumunda o `UserCard` **atlanır**.
5. `ClassWords` yalnızca sınıf üyelerine görünür.
6. Log tabloları değişmezdir (insert-only); soft delete yok, güncellenmez → [[Loglama_Domain]].
7. `CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId` artık [[BaseEntity]]'de standart — A-03+'ta
   yazılacak tablolarda `docs/DATABASE_SCHEMA/`'daki ad-hoc `CreatedBy`/`UpdatedBy` kolonları
   (`Words` → `Icerik.md`, `SmtpSettings` → `Sistem.md`, `ClassWords` → `Sosyal.md`) bu standart
   alanlarla birleştirilmeli, ayrıca tutulmamalı.

## Seed Data
12 başlangıç kategorisi (`Menschen/İnsanlar`, `Familie/Aile`, `Essen/Yemek`, ... A1/A2 seviye) —
tam liste `docs/DATABASE_SCHEMA.md §5`.
