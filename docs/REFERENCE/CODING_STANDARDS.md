# KODLAMA STANDARTLARI

> **Felsefe:** Bu proje junior eğitimi amaçlı yazılır. Her kod, 6 ay tecrübeli bir junior'ın
> **Türkçe** okuyup anlayabileceği şekilde açıklanır. Kod kendini anlatır + Türkçe yorumlar *neden*i anlatır.

## 1. Dil Kuralları

| Türkçe | İngilizce (convention) |
|--------|------------------------|
| Tüm kod yorumları (AMAÇ/NEDEN/NASIL), XML doc, MD dosyaları, API Yol Haritası | Method / class / property isimleri (C#), test metodu adları (§7.2), DB kolon adları (SQL), JS değişkenleri, **log mesajları (`_logger.Log*`), exception `.Message`'ları, hata `Code` sabitleri (ör. `INVALID_CREDENTIALS`)** |

> **NEDEN (A-03'te değişti):** Log/exception mesajları ve hata kodları **DB'ye ve geliştiriciye** giden
> içeriktir — bu proje tek bir gerçek dile (İngilizce) kilitlenmeli ki loglar/DB kayıtları tutarlı,
> aranabilir ve gelecekteki herhangi bir dilde çalışan bir geliştirici tarafından okunabilir olsun.
> Kod yorumları ise **geliştiriciye öğretim** amaçlı olduğu için Türkçe kalır — bu ikisi ayrı kanallardır.
>
> **İSTİSNA — istemciye (API yanıtı) giden mesaj:** `AppException.Code` veya FluentValidation
> `ErrorCode`'u, isteğin `Accept-Language`'ına göre `ErrorMessages` sözlüğünden (şu an tr+de)
> çözülüp kullanıcıya o dilde gösterilir (bkz. [[ErrorMessages]], `ValidationFilter`,
> `ExceptionHandlingMiddleware`). Yani: **kullanıcı ne dil seçtiyse onu görür, DB/log/geliştirici
> İngilizce görür.**

```csharp
// ✅ DOĞRU — yorum Türkçe, log/exception mesajı İngilizce
// AMAÇ: Kullanıcı girişini loglar.
_logger.LogInformation("User {UserId} logged in. IP: {Ip}", userId, ip);
throw new EntityNotFoundException($"User not found: Id={userId}");

// ❌ YANLIŞ — log/exception mesajı Türkçe (DB'ye/geliştiriciye Türkçe gitmemeli)
// _logger.LogInformation("Kullanıcı {UserId} giriş yaptı.", userId);
// throw new Exception("Kullanıcı bulunamadı");
```

## 2. Dosya Başı Bloğu (zorunlu)

Her dosyanın başında `AMAÇ / NEDEN / BAĞIMLILIKLAR`:

```csharp
/// <summary>
/// UserProgressService.cs
///
/// AMAÇ: Kullanıcının kelime öğrenme ilerlemesini yönetmek ve SRS (Spaced Repetition)
///       hesaplamalarını yapmak.
/// NEDEN: SRS olmadan kullanıcı kelimeleri unutur (Ebbinghaus unutma eğrisi). Her cevap
///        sonrası ilerleme kaydı ve sonraki tekrar zamanı hesaplanmalı.
/// BAĞIMLILIKLAR: IUserProgressRepository, ILogger, IMapper.
/// </summary>
```

## 3. Public Metot Bloğu (zorunlu)

Her public metodun üstünde `AMAÇ / NEDEN / NASIL` (+ param/return):

```csharp
/// <summary>
/// Kullanıcının bir kelime için ilerlemesini günceller.
/// </summary>
/// <param name="userId">Kullanıcı ID'si</param>
/// <param name="wordId">Kelime ID'si</param>
/// <param name="quality">Öz değerlendirme (0-5): 🔴0 🟠2 🟢4 🔵5</param>
/// <returns>Güncel ilerleme + kazanılan XP</returns>
///
/// NEDEN: Her öğrenme aktivitesinden sonra ilerleme kaydı zorunlu; SM-2 aralıkları
///        doğru cevaba göre uzar, yanlışta sıfırlanır.
/// NASIL: 1) Mevcut ilerlemeyi çek  2) İstatistikleri güncelle  3) SM-2 ile sonraki
///        review'i hesapla  4) LearningHistory ekle  5) XP/streak güncelle  6) Kaydet.
public async Task<ProgressDto> UpdateProgressAsync(int userId, int wordId, int quality,
    CancellationToken ct = default) { /* ... */ }
```

## 4. Kod İçi Yorumlar (karmaşık bloklarda)

Adım adım `// ADIM N:` + `// NEDEN:` ile:

```csharp
// ADIM 1: Girdi validasyonu — geçersiz veri işlenmez
if (userId <= 0 || wordId <= 0)
    throw new ArgumentException("IDs must be positive");

// ADIM 2: Mevcut ilerlemeyi çek — SM-2 için önceki durum gerekli
var progress = await _progressRepo.GetByUserAndWordAsync(userId, wordId, ct)
    ?? throw new EntityNotFoundException($"Progress not found: user {userId}, word {wordId}");

// ADIM 3: SM-2 ile sonraki tekrar zamanını hesapla (bkz. REFERENCE/TECHNICAL_SPECIFICATIONS.md §8)
var (interval, newLevel, newEF) = SrsCalculator.Calculate(
    progress.CurrentLevel, progress.RepetitionNumber, progress.EasinessFactor, quality);
progress.NextReviewAt = DateTime.UtcNow.AddDays(interval);
```

## 5. Katman Şablonları (kısa)

**Entity** — `AMAÇ` + alan başına tek satır Türkçe doc:
```csharp
/// <summary>AMAÇ: Sistem kelimesi SRS takibi. İLİŞKİ: N:1 User, N:1 Word.</summary>
public class UserProgress : BaseEntity
{
    /// <summary>Mastery seviyesi (0=hiç görülmedi .. 5=otomatik hatırlama).</summary>
    public int CurrentLevel { get; set; }
    /// <summary>SRS'nin hesapladığı sonraki tekrar zamanı.</summary>
    public DateTime NextReviewAt { get; set; }
}
```

**DTO** — neden Entity değil: hassas alan gizleme + sözleşme + sadece gerekli alanlar.

**Validator** — her kuralın üstünde `// NEDEN:`. `WithMessage` İngilizce (log/DB'ye gider),
`WithErrorCode` ile istemciye giden mesaj `ErrorMessages`'ten dile göre çözülür (bkz. §1):
```csharp
RuleFor(x => x.Password)
    .MinimumLength(12).WithMessage("Password must be at least 12 characters").WithErrorCode("PASSWORD_TOO_SHORT")   // NEDEN: brute-force direnci
    .Matches(@"[A-Z]").WithMessage("Must contain at least 1 uppercase letter").WithErrorCode("PASSWORD_MISSING_UPPERCASE");
```

**Repository** — async + CancellationToken + Include (N+1 önle) + soft delete filtresi.

**Controller** — ince katman: JWT'den userId al, servisi çağır, DTO döndür. İş mantığı **yok**.

## 6. Genel En İyi Pratikler

- `async/await` + `CancellationToken` her I/O metodunda.
- Null kontrolü / guard clauses; exception + İngilizce loglama (bkz. §1).
- SOLID, DRY, KISS. Repository sorgularında soft delete + (kişiselde) UserId filtresi.
- Parametreli sorgular (SQL injection yok); EF Core LINQ tercih.

## 7. Birim Test Yazım Standardı (zorunlu — Faz F'ye bırakılmaz)

> **Araçlar:** xUnit (test çalıştırıcı) + Moq (repo/dış servis mock) + FluentAssertions (okunabilir
> assertion) + `Microsoft.EntityFrameworkCore.InMemory` (yalnızca `Repository<T>` gibi DB'ye dokunan
> testlerde). Paketler → `REFERENCE/TECHNICAL_SPECIFICATIONS.md §1`. Proje: `backend/WordLearner.Tests`.

### 7.1 Dosya Konumu ve Adlandırma

```
WordLearner.Tests/
├── Services/        → XxxServiceTests.cs        (her servisin kendi test sınıfı)
├── Helpers/         → SrsCalculatorTests.cs vb.  (statik/yardımcı sınıflar)
└── Repositories/     → RepositoryTests.cs         (A-02'de bir kez, generic taban için)
```
**Kural:** `{TestEdilenSınıf}Tests.cs` — `AuthService` → `AuthServiceTests.cs`.

### 7.2 Test Metodu Adlandırma (zorunlu kalıp)

```
{Metot}_{Senaryo}_{BeklenenSonuç}
```
**NOT:** Yapı (3 bölüm) sabit, ama metot **adının kendisi İngilizce** yazılır — method/property
isimleri gibi test metodu adı da C# tarafında "kod kimliği" sayılır (§1'deki İngilizce sütunu).
Türkçe kalan tek şey, testin içindeki `// ARRANGE/ACT/ASSERT` yorumları ve `AMAÇ/NEDEN` doc-comment'i.
```csharp
// ✅ DOĞRU
UpdateProgressAsync_QualityIsLow_ResetsLevel
UpdateProgressAsync_QualityIsHigh_ExtendsInterval
Register_EmailAlreadyRegistered_ThrowsDuplicateException

// ❌ YANLIŞ — ne test ettiği isimden anlaşılmıyor
Test1
UpdateProgress_Test

// ❌ YANLIŞ (eski kural) — test adı Türkçe
UpdateProgressAsync_QualityDusukSe_SeviyeyiSifirlar
```

### 7.3 AAA Deseni (Arrange / Act / Assert) — her test bu 3 bölümden oluşur

Türkçe yorumla bölünür, **NEDEN** sadece Assert'te gerekiyorsa eklenir (beklenti açık değilse):

```csharp
/// <summary>
/// UpdateProgressAsync_QualityIsLow_ResetsLevel
///
/// AMAÇ: Kullanıcı bir kelimeyi "Bilmedim" (quality=0) olarak işaretlediğinde SM-2'nin
///       seviyeyi sıfırladığını doğrulamak.
/// NEDEN: SrsCalculator.Calculate, quality&lt;3 durumunda interval'i 1 güne, seviyeyi 0'a
///        çeker — bu davranış UpdateProgressAsync'in doğru çalışması için kritik.
/// </summary>
[Fact]
public async Task UpdateProgressAsync_QualityIsLow_ResetsLevel()
{
    // ARRANGE — sahte (mock) repository + var olan bir ilerleme kaydı hazırla
    var mockRepo = new Mock<IUserProgressRepository>();
    var mevcutIlerleme = new UserProgress { CurrentLevel = 3, RepetitionNumber = 2, EasinessFactor = 2.5m };
    mockRepo.Setup(r => r.GetByUserAndWordAsync(1, 5, default)).ReturnsAsync(mevcutIlerleme);
    var servis = new UserProgressService(mockRepo.Object, Mock.Of<ILogger<UserProgressService>>());

    // ACT — test edilen metodu, quality=0 ("Bilmedim") ile çağır
    var sonuc = await servis.UpdateProgressAsync(userId: 1, wordId: 5, quality: 0);

    // ASSERT — seviyenin sıfırlandığını ve interval'in 1 güne döndüğünü doğrula
    // NEDEN: SM-2 kuralı — yanlış cevapta mastery kaybolur, kullanıcı kelimeyi yeniden öğrenmeli.
    sonuc.CurrentLevel.Should().Be(0);
    mockRepo.Verify(r => r.UpdateAsync(It.Is<UserProgress>(p => p.IntervalDays == 1), default), Times.Once);
}
```

### 7.4 Mock Kuralları

- **Repository ve dış servisler (email, OneSignal, Google/Apple doğrulama) her zaman mock'lanır** —
  gerçek DB/HTTP çağrısı **unit** testte yapılmaz (bu, integration test'in işi → F-02).
- `Mock.Of<ILogger<T>>()` ile logger mock'lanır; log mesajı içeriği test edilmez (gürültü).
- Yalnızca `Repository<T>` taban sınıfı testinde gerçek **in-memory EF Core** kullanılır, çünkü
  orada test edilen şey zaten DB sorgu davranışıdır (soft delete filtresi, vs.).

### 7.5 Hangi Senaryolar Zorunlu (minimum kapsam)

Her public servis metodu için en az:
```
[ ] Mutlu yol (happy path) — beklenen girdi, beklenen çıktı
[ ] Bulunamadı durumu — EntityNotFoundException fırlatılıyor mu
[ ] Yetki/sahiplik ihlali — başkasının kaydına erişim 403/404 mü dönüyor (kişisel içerikte)
[ ] Sınır/uç durum — duplikat 409, geçersiz parametre, SM-2'de quality<3 vb. iş kuralına özgü dallar
```

### 7.6 Roadmap'e İşleme (API Yol Haritası — Test Alanı)

Her API'ın HTML sayfasında **ayrı bir "Test" sekmesi/alanı** olur (kod adımlarının yanında, son
sırada). Bu alana, kod blokları gibi **birebir kopya** test sınıfı eklenir + her test metodunun
altına şu 3 satırlık açıklama yazılır (junior'a *neden bu senaryo test edildi* anlatılır):

```
Test Adı     : UpdateProgressAsync_QualityIsLow_ResetsLevel
Ne Test Edildi: Quality=0 ("Bilmedim") cevabında SM-2'nin seviyeyi sıfırlaması
Neden Önemli : Yanlış cevapta mastery kaybı olmazsa kullanıcı kelimeyi öğrenmiş gibi görünür
               ama gerçekte unutmuştur (Ebbinghaus eğrisi) — SRS'in temel amacı bozulur.
```

`AMAÇ/NEDEN` zaten test dosyasının XML doc'unda var (§8.3); roadmap'e işlerken bu blok **kod ile
birlikte** kopyalanır, ayrıca özetlenmez — tekrar yazılan kod kuralı (`REFERENCE/CODING_STANDARDS.md`'nin
genel ilkesi) test dosyaları için de geçerlidir.

## 8. Her Dosya İçin Kontrol Listesi

```
[ ] Dosya başı: AMAÇ / NEDEN / BAĞIMLILIKLAR
[ ] Her public metot: AMAÇ / NEDEN / NASIL
[ ] Karmaşık bloklar: ADIM + NEDEN yorumları
[ ] Tüm yorum TÜRKÇE; log/exception .Message/Code sabitleri + method/class/property/test metodu adı İngilizce (§1)
[ ] Servis katmanı için birim test yazıldı (§7 standardına uygun: AAA + isimlendirme + mock) — Faz F'ye bırakılmaz
[ ] async/await + CancellationToken
[ ] Yazıldıkça API_YOL_HARITASI/ rehberine işlendi (kod alanı + test alanı, §7.6)
```

> **Sonuç:** Her kod bir hikâye anlatır. Junior 6 ay sonra okuduğunda *ne*, *neden*, *nasıl* sorularının
> cevabını **Türkçe** bulabilmeli.
