# VERİTABANI ŞEMASI — INDEX

> MS SQL Server. Genel kurallar (BaseEntity alanları, soft delete + UserId filtresi zorunluluğu) → `CLAUDE.md §1`.
> Tam `CREATE TABLE` SQL'leri domain başına ayrı dosyalarda. Bir task'ta **yalnızca ilgili domain dosyasını oku.**

> **BaseEntity notu:** `CreatedByUserId/UpdatedByUserId/DeletedByUserId` (int?, FK→Users) A-02'de eklendi;
> `Users` A-03'te yazılana kadar FK constraint yok, `userId` geçilmedikçe null kalır. Bazı tablolarda
> ad-hoc tanımlıydı (`Words.CreatedBy`, `SmtpSettings.UpdatedByUserId`, `ClassWords.CreatedBy`) — o tablolar
> kodlanırken (A-03+) ad-hoc kolonlar kaldırılıp BaseEntity standardıyla birleştirilir.

## Domain Haritası

| Domain | Ana Tablolar | Dosya |
|--------|--------------|-------|
| Kimlik doğrulama | `Users`, `RefreshTokens`, `QrLoginSessions` | `DATABASE_SCHEMA/Auth.md` |
| Sistem içeriği | `Languages`, `WordConcepts`, `Words`, `WordDetails`, `WordExamples`, `Categories`, `CategoryTranslations`, `WordCategories` | `DATABASE_SCHEMA/Icerik.md` |
| Kişisel içerik | `UserCards`, `UserCardExamples`, `UserCategories`, ara tablolar | `DATABASE_SCHEMA/Kisisel_Icerik.md` |
| SRS / ilerleme | `UserProgress`, `UserCardProgress`, `LearningSessions`, `LearningHistory`, `Achievements` | `DATABASE_SCHEMA/SRS.md` |
| Sosyal | `Classes`, `ClassWords`, `ClassMemberships`, `Friendships`, `SharedContents` | `DATABASE_SCHEMA/Sosyal.md` |
| Loglama | `ActivityLog`, `ApplicationLog`, `SecurityLog` | `DATABASE_SCHEMA/Loglama.md` |
| Sistem ayarı | `SmtpSettings` | `DATABASE_SCHEMA/Sistem.md` |

## İlişki Özeti (ERD)

```
Users ─┬─ RefreshTokens (1:N)
       ├─ QrLoginSessions (1:N, SET NULL)
       ├─ UserProgress (1:N) ── Words
       ├─ UserCards (1:N) ─┬─ UserCardProgress / UserCardExamples (1:N)
       │                   ├─ UserCardCategories (M:N) ── Categories
       │                   └─ UserCardUserCategories (M:N) ── UserCategories
       ├─ UserCategories · LearningSessions/History (1:N)
       └─ Classes(owner) · ClassMemberships · Friendships · SharedContents · loglar(SET NULL)

Languages ─┬─ Words (1:N) · CategoryTranslations (1:N)
WordConcepts ─┬─ Words (1:N, her biri farklı dilde) ── WordDetails (1:1) ── WordExamples (1:N)
              └─ WordCategories (M:N) ── Categories ── CategoryTranslations (1:N)
Classes ─ ClassWords (1:N) [yalnızca üyeler, kendi ad-hoc DE/TR alanlarıyla]
Categories ─ self-ref (ParentCategoryId)
```

## Seed Data (başlangıç kategorileri)

Önce dilden bağımsız `Categories`, sonra her biri için `de`+`tr` adları `CategoryTranslations`'a (İngilizce eklenince tek satırlık ek INSERT, şema değişmez).

```sql
INSERT INTO Categories (DisplayOrder, MinLevel, Color, Icon) VALUES
(1,'A1','#FF6B6B','people'), (2,'A1','#FF8C42','family'), (3,'A1','#95E1D3','food'),
(4,'A1','#4ECDC4','house'), (5,'A1','#AA96DA','school'), (6,'A1','#FCBAD3','numbers'),
(7,'A1','#A8EDEA','colors'), (8,'A1','#FFD89B','time'), (9,'A1','#FB7D5B','body'),
(10,'A1','#84DCC6','animal'), (11,'A2','#F38181','work'), (12,'A2','#C7CEEA','travel');

-- LanguageId 1='de', 2='tr'
INSERT INTO CategoryTranslations (CategoryId, LanguageId, Name) VALUES
(1,1,'Menschen'),(1,2,'İnsanlar'),(2,1,'Familie'),(2,2,'Aile'),(3,1,'Essen'),(3,2,'Yemek'),
(4,1,'Haus'),(4,2,'Ev'),(5,1,'Schule'),(5,2,'Okul'),(6,1,'Zahlen'),(6,2,'Sayılar'),
(7,1,'Farben'),(7,2,'Renkler'),(8,1,'Zeit'),(8,2,'Zaman'),(9,1,'Körperteile'),(9,2,'Vücut'),
(10,1,'Tiere'),(10,2,'Hayvanlar'),(11,1,'Arbeit'),(11,2,'İş'),(12,1,'Reisen'),(12,2,'Seyahat');
```

## Uygulama Katmanı Kuralları (zorunlu)

1. `UserProgress` (sistem) ve `UserCardProgress` (kişisel) **ayrı** tablolar.
2. `LearningHistory.WordId` ve `UserCardId` ikisi birden NULL olamaz (uygulama kontrolü).
3. `UserCard` yalnızca sahibince görülür → her sorguda `UserId` filtresi.
4. **Sistem kelimesi eşleşmesi:** `UserCard.FrontText`, `Words.Text` ile (aynı LanguageId) eşleşir ve kullanıcının o `WordConcept` için `UserProgress` kaydı varsa, Mixed sınavda o UserCard **atlanır** (UserProgress önceliklidir). Karşılaştırılacak LanguageId (kullanıcının hedef dili) C-fazında netleşir.
5. `ClassWords` yalnızca üyelere: `WHERE ClassId IN (kullanıcının üye sınıfları)`.
6. Log tabloları insert-only.
