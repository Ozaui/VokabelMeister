# VERİTABANI ŞEMASI

> MS SQL Server. Tüm tablolar `BaseEntity` alanlarını (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt,
> CreatedByUserId, UpdatedByUserId, DeletedByUserId) taşır (log tabloları hariç — onlarda soft delete
> yok). Son üç alan (`CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId`, `int?`, FK → `Users(Id)`)
> A-02'de `BaseEntity`'ye eklendi; `Users` tablosu A-03'te yazılana kadar kod tarafında FK constraint
> yok, `Repository<T>.AddAsync/UpdateAsync/SoftDeleteAsync`'e `userId` geçilmediği sürece null kalır.
> Bu üç alan, aşağıdaki bazı tablolarda (`Words.CreatedBy/UpdatedBy`, `SmtpSettings.UpdatedByUserId`,
> `ClassWords.CreatedBy`) ayrıca ad-hoc kolon olarak tanımlıydı — o tablolar kodlanırken (A-03+)
> bu ad-hoc kolonlar kaldırılıp `BaseEntity`'nin standart alanlarıyla birleştirilmeli. Repository
> sorgularında **soft delete filtresi** ve kişisel içerikte **UserId filtresi** zorunludur.
>
> **Bu dosya artık bir INDEX'tir.** Tabloların tam `CREATE TABLE` SQL'i domain'e göre ayrı dosyalarda
> (aşağıdaki harita). Bir task'ta yalnızca ilgili domain dosyasını oku, hepsini değil.

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

## 1. İlişki Özeti (ERD)

```
Users ─┬─ RefreshTokens (1:N)
       ├─ QrLoginSessions (1:N, SET NULL — taranmadan önce UserId boş olabilir)
       ├─ UserProgress (1:N) ── Words
       ├─ UserCards (1:N) ─┬─ UserCardProgress (1:N)
       │                   ├─ UserCardExamples (1:N)
       │                   ├─ UserCardCategories (M:N) ── Categories
       │                   └─ UserCardUserCategories (M:N) ── UserCategories
       ├─ UserCategories (1:N)
       ├─ LearningSessions / LearningHistory (1:N)
       ├─ Classes (owner) / ClassMemberships / Friendships / SharedContents
       └─ Activity/Security loglarda UserId (1:N, SET NULL)

Languages ─┬─ Words (1:N) · CategoryTranslations (1:N)
WordConcepts ─┬─ Words (1:N, her biri farklı Languages'e bağlı) ── WordDetails (1:1) ── WordExamples (1:N)
              └─ WordCategories (M:N) ── Categories ── CategoryTranslations (1:N)
Classes ─ ClassWords (1:N) [yalnızca sınıf üyeleri, kendi ad-hoc DE/TR alanlarıyla — sistem sözlüğüne bağlı değil]
Categories ─ self-ref (ParentCategoryId)
```

---

## 5. Seed Data (başlangıç kategorileri)
Önce dilden bağımsız `Categories` çekirdeği, sonra her biri için `de`+`tr` adları
`CategoryTranslations`'a eklenir (İngilizce eklendiğinde tek satırlık ek `INSERT`, şema değişmez):
```sql
INSERT INTO Categories (DisplayOrder, MinLevel, Color, Icon) VALUES
(1,'A1','#FF6B6B','people'), (2,'A1','#FF8C42','family'), (3,'A1','#95E1D3','food'),
(4,'A1','#4ECDC4','house'), (5,'A1','#AA96DA','school'), (6,'A1','#FCBAD3','numbers'),
(7,'A1','#A8EDEA','colors'), (8,'A1','#FFD89B','time'), (9,'A1','#FB7D5B','body'),
(10,'A1','#84DCC6','animal'), (11,'A2','#F38181','work'), (12,'A2','#C7CEEA','travel');

-- LanguageId 1='de', 2='tr' (bkz. Icerik.md Languages seed)
INSERT INTO CategoryTranslations (CategoryId, LanguageId, Name) VALUES
(1,1,'Menschen'), (1,2,'İnsanlar'), (2,1,'Familie'), (2,2,'Aile'), (3,1,'Essen'), (3,2,'Yemek'),
(4,1,'Haus'), (4,2,'Ev'), (5,1,'Schule'), (5,2,'Okul'), (6,1,'Zahlen'), (6,2,'Sayılar'),
(7,1,'Farben'), (7,2,'Renkler'), (8,1,'Zeit'), (8,2,'Zaman'), (9,1,'Körperteile'), (9,2,'Vücut'),
(10,1,'Tiere'), (10,2,'Hayvanlar'), (11,1,'Arbeit'), (11,2,'İş'), (12,1,'Reisen'), (12,2,'Seyahat');
```

---

## 6. Önemli Kurallar (uygulama katmanında zorunlu)

1. **UserProgress** (sistem) ve **UserCardProgress** (kişisel) ayrı tablolardır.
2. **LearningHistory.WordId** ve **UserCardId** ikisi birden NULL olamaz (uygulama kontrolü).
3. **UserCard** yalnızca sahibi tarafından görülür → her sorguda `UserId` filtresi.
4. **Sistem kelimesi eşleşmesi:** UserCard.FrontText, Words.Text ile (aynı LanguageId'de) eşleşir ve
   kullanıcının o kelimenin WordConcept'i için `UserProgress` kaydı varsa, Mixed sınav oturumunda o
   UserCard **atlanır** (UserProgress önceliklidir). Hangi LanguageId ile karşılaştırılacağı (kullanıcının
   hedef dili) C-fazında (SRS) netleştirilecek — bkz. `Icerik.md`.
5. **ClassWords** yalnızca sınıf üyelerine görünür: `WHERE ClassId IN (kullanıcının üye olduğu sınıflar)`.
6. **Log tabloları** değişmezdir (insert-only); soft delete yok, güncellenmez.
