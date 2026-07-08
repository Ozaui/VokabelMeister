# KODLAMA STANDARTLARI

> Dil kuralı özeti (Türkçe yorum / İngilizce kod-log-code) → `CLAUDE.md §1`. Bu dosya: yorum şablonları + birim test standardı.
> **Felsefe:** Junior eğitimi. Kod kendini anlatır; Türkçe yorumlar *neden*i anlatır.

## 1. Dil Kuralı — Örnek

```csharp
// ✅ DOĞRU — yorum Türkçe, log/exception mesajı İngilizce
// AMAÇ: Kullanıcı girişini loglar.
_logger.LogInformation("User {UserId} logged in. IP: {Ip}", userId, ip);
throw new EntityNotFoundException($"User not found: Id={userId}");

// ❌ YANLIŞ — log/exception mesajı Türkçe
// _logger.LogInformation("Kullanıcı {UserId} giriş yaptı.", userId);
```
İstemciye giden mesaj `Accept-Language`'a göre `ErrorMessages` sözlüğünden çözülür (`SECURITY.md §1.4`); DB/log daima İngilizce.

## 2. Dosya Başı Bloğu (zorunlu)

```csharp
/// <summary>
/// UserProgressService.cs
/// AMAÇ: Kullanıcının SRS ilerlemesini yönetmek.
/// NEDEN: SRS olmadan kullanıcı unutur (Ebbinghaus). Her cevap sonrası kayıt + sonraki tekrar zamanı.
/// BAĞIMLILIKLAR: IUserProgressRepository, ILogger, IMapper.
/// </summary>
```

## 3. Public Metot Bloğu (zorunlu)

```csharp
/// <summary>Kullanıcının bir kelime için ilerlemesini günceller.</summary>
/// <param name="quality">Öz değerlendirme (0-5): 🔴0 🟠2 🟢4 🔵5</param>
/// <returns>Güncel ilerleme + kazanılan XP</returns>
/// NEDEN: Her aktiviteden sonra ilerleme zorunlu; SM-2 aralıkları doğruda uzar, yanlışta sıfırlanır.
/// NASIL: 1) İlerlemeyi çek 2) İstatistik güncelle 3) SM-2 hesapla 4) LearningHistory ekle 5) XP/streak 6) Kaydet.
public async Task<ProgressDto> UpdateProgressAsync(int userId, int wordId, int quality, CancellationToken ct = default) { }
```

## 4. Karmaşık Bloklar

`// ADIM N:` + `// NEDEN:` ile adım adım açıkla (guard clause, mevcut durumu çekme, SM-2 çağrısı vb.).

## 5. Katman Şablonları (kısa)

- **Entity:** `AMAÇ` + alan başına tek satır Türkçe doc.
- **DTO:** neden Entity değil — hassas alan gizleme + sözleşme + sadece gerekli alanlar.
- **Validator:** her kuralın üstünde `// NEDEN:`. `WithMessage` İngilizce (log/DB'ye gider), `WithErrorCode` ile istemciye giden mesaj dile göre çözülür:
  ```csharp
  RuleFor(x => x.Password)
      .MinimumLength(12).WithMessage("Password must be at least 12 characters").WithErrorCode("PASSWORD_TOO_SHORT")
      .Matches(@"[A-Z]").WithMessage("Must contain at least 1 uppercase letter").WithErrorCode("PASSWORD_MISSING_UPPERCASE");
  ```
- **Repository:** async + CancellationToken + Include (N+1 önle) + soft delete filtresi.
- **Controller/Handler:** Controller ince (yalnızca `_mediator.Send`); iş mantığı Handler'da.

## 6. Genel Pratikler

`async/await`+`CancellationToken` her I/O'da · guard clause + İngilizce loglama · SOLID/DRY/KISS · soft delete + (kişiselde) UserId filtresi · parametreli sorgu / EF LINQ.

## 7. Birim Test Standardı (zorunlu — Faz F'ye bırakılmaz)

**Araçlar:** xUnit + Moq + FluentAssertions + `EFCore.InMemory` (yalnızca `Repository<T>` gibi DB'ye dokunan testlerde). Proje: `WordLearner.Tests`.

**7.1 Konum/adlandırma:** `Tests/{Services|Features|Helpers|Repositories}/`. `{TestEdilenSınıf}Tests.cs`.

**7.2 Metot adı (İngilizce):** `{Metot}_{Senaryo}_{BeklenenSonuç}`
```
✅ UpdateProgressAsync_QualityIsLow_ResetsLevel · Register_EmailAlreadyRegistered_ThrowsDuplicateException
❌ Test1 · UpdateProgress_Test · (Türkçe ad)
```

**7.3 AAA deseni** — her test ARRANGE/ACT/ASSERT (Türkçe yorumla bölünür); NEDEN yalnızca beklenti açık değilse Assert'te. XML doc'ta AMAÇ/NEDEN.
```csharp
[Fact]
public async Task UpdateProgressAsync_QualityIsLow_ResetsLevel()
{
    // ARRANGE — mock repo + mevcut ilerleme
    var mockRepo = new Mock<IUserProgressRepository>();
    var mevcut = new UserProgress { CurrentLevel = 3, RepetitionNumber = 2, EasinessFactor = 2.5m };
    mockRepo.Setup(r => r.GetByUserAndWordAsync(1, 5, default)).ReturnsAsync(mevcut);
    var servis = new UserProgressService(mockRepo.Object, Mock.Of<ILogger<UserProgressService>>());
    // ACT — quality=0 ("Bilmedim")
    var sonuc = await servis.UpdateProgressAsync(userId: 1, wordId: 5, quality: 0);
    // ASSERT — seviye sıfırlandı, interval 1 güne döndü
    sonuc.CurrentLevel.Should().Be(0);
    mockRepo.Verify(r => r.UpdateAsync(It.Is<UserProgress>(p => p.IntervalDays == 1), default), Times.Once);
}
```

**7.4 Mock kuralları:** Repository + dış servisler (email, OneSignal, Google/Apple) her zaman mock. `Mock.Of<ILogger<T>>()`; log içeriği test edilmez. `IMapper` mock'lanmaz — gerçek Profile'dan kurulur. Gerçek in-memory EF yalnızca `Repository<T>` testinde.

**7.5 Minimum kapsam** her public metot için: happy path · bulunamadı (EntityNotFoundException) · yetki/sahiplik ihlali (403/404, kişiselde) · sınır/uç durum (duplikat 409, quality<3 vb.).

**7.6 Roadmap'e işleme:** Her API'ın HTML sayfasında ayrı "Test" alanı; test sınıfı birebir kopya + her metoda 3 satır:
```
Test Adı      : UpdateProgressAsync_QualityIsLow_ResetsLevel
Ne Test Edildi: Quality=0'da SM-2'nin seviyeyi sıfırlaması
Neden Önemli  : Yanlışta mastery kaybı olmazsa kullanıcı öğrenmiş görünür ama unutmuştur (SRS bozulur).
```

## 8. Dosya Kontrol Listesi

```
[ ] Dosya başı: AMAÇ/NEDEN/BAĞIMLILIKLAR
[ ] Her public metot: AMAÇ/NEDEN/NASIL
[ ] Karmaşık bloklar: ADIM + NEDEN
[ ] Yorum Türkçe; log/exception/Code + method/class/property/test adı İngilizce
[ ] Handler/servis birim testi yazıldı (§7)
[ ] async/await + CancellationToken
[ ] Yazıldıkça roadmap'e işlendi (kod + test alanı)
```
