# Yazılan Kodlar Ne İşe Yarıyor? — Junior Rehberi

> Bu döküman, TASK-001 ile TASK-004 arasında yazılan her yapıyı sıfırdan açıklar.  
> 🎯 işareti = Mülakatta sorulabilir soru

---

## 1. Neden 4 Proje? — Clean Architecture

Projeyi açtığında 4 farklı klasör görürsün:

```
WordLearner.Domain
WordLearner.Application
WordLearner.Infrastructure
WordLearner.API
```

Bunun adı **Clean Architecture** (veya N-Katmanlı Mimari). Temel fikir şu:

> "Her katman sadece kendi işini bilir. Dışarıdaki katmanlar içeridekine bağımlıdır; içeridekiler dışarıdakileri tanımaz."

Düşün ki bir restoran açtın:

| Restoran | Kod |
|----------|-----|
| Menü ve tarifler | Domain |
| Aşçıların iş akışı | Application |
| Mutfak ekipmanları, depo | Infrastructure |
| Garson, sipariş alma | API |

Mutfak ekipmanı değişse (SQL Server → PostgreSQL), aşçılar yine aynı tarifleri kullanır. Garson da aynı şekilde sipariş alır. Sadece ekipman değişti.

---

### 🎯 Mülakat Sorusu
**"Clean Architecture nedir? Neden kullanırsın?"**

Cevap özeti:
- Her katman tek bir sorumluluk taşır (SRP — Single Responsibility Principle)
- Dış katmanlar değiştiğinde iç katmanlar etkilenmez
- Test yazmak kolaylaşır (veritabanı olmadan test edebilirsin)
- Büyüyen ekiplerde farklı kişiler farklı katmanlarda çalışabilir

---

## 2. Domain Katmanı — Projenin Kalbi

**Dosyalar:** `WordLearner.Domain/Entities/`

Bu katman **saf C# sınıfları** içerir. Veritabanını bilmez. API'yi bilmez. Sadece "bu uygulamada neler var?" sorusunu cevaplar.

### BaseEntity — Temel Sınıf

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

Her entity'nin ortak özellikleri burada. `User`, `Word`, `Category` hepsi buradan miras alır.

**Neden `abstract`?** Çünkü `BaseEntity` kendi başına bir nesne değil. Sadece şablon.

**`IsDeleted` ne işe yarıyor?**  
Buna **Soft Delete** denir. Veritabanından gerçekten silmek yerine `IsDeleted = true` yaparsın. Kayıt hâlâ var ama sorgulamalarda görünmez.

---

### 🎯 Mülakat Sorusu
**"Soft Delete nedir? Neden hard delete yerine kullanırsın?"**

Cevap özeti:
- Hard delete: `DELETE FROM Users WHERE Id = 5` — kayıt gider, geri gelemez
- Soft delete: `UPDATE Users SET IsDeleted = 1 WHERE Id = 5` — kayıt görünmez ama var
- Neden: Yanlış silme geri alınabilir, audit (denetim) kaydı tutulur, başka tablolardaki ilişkiler kopmaz

---

### Entity Nedir?

Entity, veritabanındaki bir tabloyu temsil eden C# sınıfıdır. Örneğin `User` sınıfı → `Users` tablosu.

```csharp
public class User : BaseEntity
{
    public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string FirstName { get; set; }
    // ...
}
```

Sınıftaki her `property` → tablodaki bir kolon.

---

### Navigation Property Nedir?

```csharp
public class User : BaseEntity
{
    // ...
    public ICollection<UserCard> UserCards { get; set; }
}
```

`UserCards` bir kolondur ama veritabanında ayrı bir alan yoktur. Bu EF Core'un ilişkileri anlamasını sağlar. "Bu kullanıcının kartlarını istiyorum" dediğinde EF Core JOIN sorgusunu otomatik yazar.

---

## 3. Infrastructure Katmanı — Teknik Kirli İş

**Dosyalar:** `WordLearner.Infrastructure/Data/`

Burada veritabanıyla ilgili her şey var. Domain bunu bilmez, bilmek zorunda değil.

---

### DbContext — Veritabanı Kapısı

```csharp
public class WordLearnerDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Word> Words { get; set; }
    // ...
}
```

**DbContext** EF Core'un kalbi. Veritabanına açılan pencere. `DbSet<T>` ise her tablo için bir kapı.

`_db.Users.ToListAsync()` dediğinde EF Core otomatik olarak `SELECT * FROM Users` SQL'i üretir ve çalıştırır.

---

### 🎯 Mülakat Sorusu
**"ORM nedir? EF Core ne işe yarar?"**

Cevap özeti:
- ORM (Object-Relational Mapper): Nesne dünyası (C# sınıfları) ile ilişkisel veritabanı arasında çevirmen
- EF Core: .NET'in resmi ORM kütüphanesi
- Avantaj: SQL yazmadan veritabanı sorgusu yapabilirsin; C# kodu → SQL'e çevrilir
- LINQ sorguları EF Core tarafından SQL'e dönüştürülür: `_db.Users.Where(u => u.IsActive)` → `SELECT * FROM Users WHERE IsActive = 1`

---

### Fluent API — Tablo Yapısını Kodla Tanımlama

`Data/Configurations/` klasöründeki dosyalar. Her entity için ayrı bir konfigürasyon sınıfı var.

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(254);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
```

**Neden Data Annotation yerine Fluent API?**

| Data Annotation | Fluent API |
|----------------|-----------|
| `[Required]` sınıfın içine yazılır | Ayrı dosyada yazılır |
| Entity sınıfı kalabalıklaşır | Entity sınıfı temiz kalır |
| Karmaşık kurallar yazılamaz | Her türlü kural yazılabilir |

Bizde Entity sınıfları Domain katmanında, konfigürasyonlar Infrastructure katmanında. Böylece Domain, Infrastructure'dan habersiz kalıyor.

---

### 🎯 Mülakat Sorusu
**"EF Core'da Fluent API ile Data Annotation arasındaki fark nedir?"**

Cevap özetti:
- Data Annotation: `[Required]`, `[MaxLength(50)]` gibi attribute'lar doğrudan entity üzerine
- Fluent API: `IEntityTypeConfiguration<T>` sınıflarında, daha güçlü ve ayrıştırılmış
- Clean Architecture'da Fluent API tercih edilir çünkü entity sınıfları "altyapı bilgisi" taşımaz

---

### HasQueryFilter — Global Soft Delete Filtresi

```csharp
builder.HasQueryFilter(u => !u.IsDeleted);
```

Bu satır **bir kez yazılır** ama tüm `Users` sorgularına otomatik eklenir.  
`_db.Users.ToListAsync()` yazdığında EF Core ürettiği SQL şudur:
```sql
SELECT * FROM Users WHERE IsDeleted = 0
```

Silindi mi? Sorguya girmiyor. Her yere `WHERE IsDeleted = 0` yazmak zorunda kalmıyorsun.

---

### Migration — Veritabanı Versiyonlama

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Migration** C# entity'lerindeki değişikliği SQL'e çeviren dosya. 

Nasıl çalışır:
1. Entity'ye yeni bir alan ekledin → `dotnet ef migrations add YeniAlan`
2. EF Core farkı görür, SQL üretir: `ALTER TABLE Users ADD YeniAlan NVARCHAR(100)`
3. `database update` ile SQL veritabanına uygulanır

**Neden?** Her geliştirici farklı bir şey eklerse veritabanı farklı yapılarda olur. Migration bu değişiklikleri sıralı ve tekrarlanabilir yapar — Git'te versiyonlanır.

---

### Seed Data — Başlangıç Verisi

```csharp
builder.HasData(
    new Category { Id = 1, NameDE = "Menschen", NameTR = "İnsanlar" },
    new Category { Id = 2, NameDE = "Familie", NameTR = "Aile" }
);
```

Uygulama sıfırdan kurulduğunda veritabanında başlangıç verisinin hazır olması için. Bizde 12 başlangıç kategorisi var. `database update` komutuyla otomatik eklendi.

---

### Cascade Delete Problemi

Biz şu hatayla karşılaştık:

```
Introducing FOREIGN KEY constraint may cause cycles or multiple cascade paths
```

**Ne demek?**

Diyelim ki `User` silindiğinde:
- Yol 1: `User` → `UserCard` → `UserCardProgress` (CASCADE ile silinir)
- Yol 2: `User` → `UserCardProgress` (doğrudan CASCADE ile silinir)

SQL Server iki farklı yoldan aynı tabloya CASCADE ulaşıyor → Hata veriyor.

**Çözüm:** Bir yolu `NoAction` yaptık. Yani `User` → `UserCardProgress` ilişkisini CASCADE yapmadık; zaten `UserCard` CASCADE ile `UserCardProgress`'i temizleyecek.

---

### 🎯 Mülakat Sorusu
**"Cascade Delete nedir? SQL Server'da neden sorun çıkarabilir?"**

Cevap özeti:
- Cascade Delete: Üst kayıt silindiğinde alt kayıtlar otomatik silinir
- SQL Server `cycles or multiple cascade paths` hatası: Aynı tabloya birden fazla CASCADE yolu varsa SQL Server buna izin vermez
- Çözüm: Bazı FK'ları `NoAction` veya `Restrict` olarak tanımlamak

---

## 4. Application Katmanı — İş Mantığı Sözleşmeleri

**Dosyalar:** `WordLearner.Application/Interfaces/Repositories/`

Bu katmanda şu an sadece **interface'ler** var. Asıl iş mantığı servisleri TASK-005'te gelecek.

---

### Interface Nedir? Neden Kullanılır?

```csharp
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}
```

Interface bir **sözleşme**. "Bu işi yapacak olan, bu metodları sağlamak zorunda" der.

Kimse `IUserRepository`'nin içini bilmek zorunda değil. Servis katmanı sadece şunu bilir: "bana `GetByEmailAsync` verecek biri lazım."

**Neden?**
- Değiştirilebilirlik: EF Core yerine başka bir veritabanı kütüphanesi kullansak, sadece implementasyonu değiştiririz; servise dokunmayız
- Test edilebilirlik: Gerçek veritabanı yerine sahte (mock) bir implementasyon geçirebilirsin

---

### 🎯 Mülakat Sorusu
**"Interface ile Abstract Class arasındaki fark nedir?"**

Cevap özeti:
- Interface: sadece sözleşme, kod yok (default impl hariç), çoklu miras yapılabilir
- Abstract Class: hem sözleşme hem kısmi implementasyon, tek miras
- Ne zaman interface: "Bu nesne bu işi yapabilir mi?" → birden fazla sınıf aynı sözleşmeye uyacaksa
- Ne zaman abstract class: "Bu nesne bu türden mi?" → ortak kod paylaşılacaksa

---

## 5. Repository Pattern — Veri Erişim Katmanı

**Dosyalar:** `WordLearner.Infrastructure/Repositories/`

---

### Neden Repository Pattern?

Servis katmanını düşün. Login akışı:

1. E-posta ile kullanıcıyı bul
2. Şifre doğru mu?
3. JWT üret
4. Döndür

Bu adımlarda "kullanıcıyı bul" için `_db.Users.FirstOrDefaultAsync(...)` yazmak gerekiyor.

Bunu direkt servise yazsaydın:
- Tüm EF Core kodu servislere dağılır
- Test etmek için gerçek veritabanı lazım
- Aynı sorguyu 5 yerde yazarsan, değişince 5 yeri güncelliyorsun

Repository Pattern ile:
```csharp
// Servis sadece bunu biliyor:
var user = await _userRepository.GetByEmailAsync(email);
```

Veritabanı nasıl sorgulandığı servisin umurunda değil.

---

### Generic Repository

```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    public Task<T?> GetByIdAsync(int id) { ... }
    public Task<T> AddAsync(T entity) { ... }
    public Task UpdateAsync(T entity) { ... }
    public Task SoftDeleteAsync(int id) { ... }
}
```

**`where T : BaseEntity`** ne demek?  
"T yalnızca BaseEntity'den türeyen bir sınıf olabilir." `Repository<string>` yapamazsın. Ama `Repository<User>` yapabilirsin çünkü `User : BaseEntity`.

Bu **Generic Constraint** (generic kısıt) olarak bilinir.

---

### Spesifik Repository

Generic repository genel CRUD'u veriyor. Ama bazı özel sorgular lazım:

```csharp
public class UserRepository : Repository<User>, IUserRepository
{
    public Task<User?> GetByEmailAsync(string email)
        => _set.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
}
```

`Repository<User>`'dan hem CRUD metodlarını miras aldı hem de kendi özel sorgusunu ekledi.

---

### 🎯 Mülakat Sorusu
**"Repository Pattern nedir? Avantajları nelerdir?"**

Cevap özeti:
- Veri erişim kodunu tek bir yerden yönetmek için kullanılan tasarım deseni
- Avantajlar:
  - Servis katmanı veritabanını bilmez — bağımlılık azalır
  - Aynı sorgu tek yerde — değişince bir yeri güncelliyorsun
  - Test'te mock repository geçirebilirsin — veritabanı gerekmez
- Dezavantaj: Ek katman ekler, küçük projelerde gereksiz olabilir

---

## 6. Dependency Injection — Bağımlılık Enjeksiyonu

**Dosya:** `InfrastructureServiceExtensions.cs`

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Bu satır şunu söylüyor:  
"Birisi `IUserRepository` istediğinde, ona `UserRepository` ver."

---

### Nasıl Çalışır?

Servis şöyle yazılır:

```csharp
public class AuthService
{
    private readonly IUserRepository _userRepo;

    public AuthService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }
}
```

`AuthService` nesnesi oluşturulurken .NET otomatik olarak `UserRepository`'yi bulup verir. Kimse `new UserRepository()` yazmak zorunda değil.

---

### Lifetime Farkları

| Lifetime | Ne zaman kullanılır |
|----------|-------------------|
| `AddSingleton` | Uygulama boyunca tek instance — loglama, config |
| `AddScoped` | Her HTTP isteği için yeni instance — repository, service |
| `AddTransient` | Her inject'te yeni instance — küçük, durumsuz işler |

Repository'ler `Scoped` — her istek kendi instance'ını alır, aynı istek içinde aynı instance paylaşılır. DbContext de Scoped, transaction tutarlılığı korunuyor.

---

### 🎯 Mülakat Sorusu
**"Dependency Injection nedir? Singleton, Scoped ve Transient arasındaki fark nedir?"**

Cevap özetti:
- DI: Bir nesnenin ihtiyaç duyduğu bağımlılıkları dışarıdan (container'dan) alması
- Neden: Gevşek bağlı (loosely coupled) kod → test edilebilir, değiştirilebilir
- Singleton: Tüm uygulama boyunca tek nesne
- Scoped: Her HTTP isteği için tek nesne (istek içinde paylaşılır)
- Transient: Her inject'te yeni nesne

---

## 7. SRS — SM-2 Algoritması

**Dosyalar:** `UserProgress.cs`, `UserCardProgress.cs`

SM-2 (SuperMemo 2) bir **Spaced Repetition** algoritması. Kelime kartlarını ne zaman tekrar göstereceğini hesaplar.

Temel mantık:
- Kolay hatırladın → uzun süre sonra göster
- Zor hatırladın → kısa süre sonra göster

```
Level 0 → 1 gün sonra
Level 1 → 3 gün sonra
Level 2 → 7 gün sonra
Level 3 → 14 gün sonra
Level 4 → 30 gün sonra
Level 5 → 60 gün sonra (Mastery)
```

**EasinessFactor (Kolaylık Çarpanı):** Kart ne kadar kolay hatırlanıyor? Başlangıç: 2.5. Yanlış cevap verince düşer → kartı daha sık gösterir.

---

## 8. Token Family Pattern — Güvenlik

**Dosyalar:** `RefreshToken.cs`, `RefreshTokenRepository.cs`

JWT token 15 dakikada bir süresi doluyor. Kullanıcıyı her 15 dakikada şifre girmeye zorlamak saçma. Bunun için **Refresh Token** var.

Ama saldırgan refresh token'ı çalıp kullanırsa?

**Token Family Pattern:**
1. Her refresh işleminde eski token kullanılmış (`IsUsed=true`) sayılır, yeni token aynı `TokenFamily` GUID'ini alır
2. Eğer kullanılmış (IsUsed=true) bir token tekrar kullanılmaya çalışılırsa — bu çalınmış demek!
3. O aileye ait TÜM token'lar iptal edilir → Saldırgan da meşru kullanıcı da atılır
4. Kullanıcı yeniden giriş yapmak zorunda kalır

---

### 🎯 Mülakat Sorusu
**"JWT nedir? Access Token ve Refresh Token neden ayrı kullanılır?"**

Cevap özeti:
- JWT: Sunucuya gidip gelmeden doğrulanabilen imzalı token
- Access Token: Kısa ömürlü (15 dk) — çalınsa bile kısa sürede geçersizleşir
- Refresh Token: Uzun ömürlü (7 gün) — sadece yeni Access Token almak için kullanılır, veritabanında saklanır
- Refresh Token veritabanında olduğu için istendiğinde iptal edilebilir; Access Token edilemez

---

## Özet Tablo

| Kavram | Dosya/Katman | .NET Adı |
|--------|-------------|----------|
| Proje yapısı | 4 proje | Clean Architecture |
| Temel sınıf | `BaseEntity.cs` | Abstract Class |
| Veri modelleri | `Entities/` | Entity / Domain Model |
| Veritabanı bağlantısı | `WordLearnerDbContext` | DbContext (EF Core) |
| Tablo konfigürasyonu | `*Configuration.cs` | IEntityTypeConfiguration (Fluent API) |
| Veritabanı değişiklikleri | `Migrations/` | EF Core Migration |
| Başlangıç verisi | `HasData(...)` | Seed Data |
| Silmeden saklama | `IsDeleted` | Soft Delete |
| Veri erişim soyutlaması | `IRepository<T>` | Repository Pattern |
| Servis sözleşmesi | `Interface` | Interface / Contract |
| Bağımlılık yönetimi | `AddScoped<I, C>()` | Dependency Injection (IoC Container) |
| Tekrar öğrenme algoritması | `UserProgress` | Spaced Repetition (SM-2) |
| Token güvenliği | `RefreshToken` | Token Family Pattern |

---

> **Not:** Bu döküman projeyle birlikte güncellenir. Yeni task tamamlandıkça buraya eklenir.
