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
| Kimlik doğrulama | `Users`, `RefreshTokens` | `DATABASE_SCHEMA/Auth.md` |
| Sistem içeriği | `Words`, `WordDetails`, `WordExamples`, `Categories`, `WordCategories` | `DATABASE_SCHEMA/Icerik.md` |
| Kişisel içerik | `UserCards`, `UserCardExamples`, `UserCategories`, ara tablolar | `DATABASE_SCHEMA/Kisisel_Icerik.md` |
| SRS / ilerleme | `UserProgress`, `UserCardProgress`, `LearningSessions`, `LearningHistory`, `Achievements` | `DATABASE_SCHEMA/SRS.md` |
| Sosyal | `Classes`, `ClassWords`, `ClassMemberships`, `Friendships`, `SharedContents` | `DATABASE_SCHEMA/Sosyal.md` |
| Loglama | `ActivityLog`, `ApplicationLog`, `SecurityLog` | `DATABASE_SCHEMA/Loglama.md` |
| Sistem ayarı | `SmtpSettings` | `DATABASE_SCHEMA/Sistem.md` |

## 1. İlişki Özeti (ERD)

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

---

## 5. Seed Data (başlangıç kategorileri)
```sql
INSERT INTO Categories (NameDE, NameTR, DisplayOrder, MinLevel, Color, Icon) VALUES
('Menschen','İnsanlar',1,'A1','#FF6B6B','people'), ('Familie','Aile',2,'A1','#FF8C42','family'),
('Essen','Yemek',3,'A1','#95E1D3','food'), ('Haus','Ev',4,'A1','#4ECDC4','house'),
('Schule','Okul',5,'A1','#AA96DA','school'), ('Zahlen','Sayılar',6,'A1','#FCBAD3','numbers'),
('Farben','Renkler',7,'A1','#A8EDEA','colors'), ('Zeit','Zaman',8,'A1','#FFD89B','time'),
('Körperteile','Vücut',9,'A1','#FB7D5B','body'), ('Tiere','Hayvanlar',10,'A1','#84DCC6','animal'),
('Arbeit','İş',11,'A2','#F38181','work'), ('Reisen','Seyahat',12,'A2','#C7CEEA','travel');
```

---

## 6. Önemli Kurallar (uygulama katmanında zorunlu)

1. **UserProgress** (sistem) ve **UserCardProgress** (kişisel) ayrı tablolardır.
2. **LearningHistory.WordId** ve **UserCardId** ikisi birden NULL olamaz (uygulama kontrolü).
3. **UserCard** yalnızca sahibi tarafından görülür → her sorguda `UserId` filtresi.
4. **Sistem kelimesi eşleşmesi:** UserCard.FrontText, Words.GermanWord ile eşleşir ve kullanıcının
   o kelime için `UserProgress` kaydı varsa, Mixed sınav oturumunda o UserCard **atlanır** (UserProgress önceliklidir).
5. **ClassWords** yalnızca sınıf üyelerine görünür: `WHERE ClassId IN (kullanıcının üye olduğu sınıflar)`.
6. **Log tabloları** değişmezdir (insert-only); soft delete yok, güncellenmez.
