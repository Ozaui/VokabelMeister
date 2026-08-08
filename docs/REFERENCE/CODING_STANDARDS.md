# KODLAMA STANDARTLARI

> Dil kuralı özeti (Türkçe yorum / İngilizce kod-log-code) → `CLAUDE.md §1`. Bu dosya: yorum standardı + birim test standardı.
> **Felsefe:** Kod kendini anlatır (isimlendirme); yorum yalnızca kodun anlatamadığı *neden*i, kısa ve gerektiğinde anlatır.
> **Not:** Bu standart yalnızca kaynak kod (`.cs`/`.ts`/`.tsx`) için geçerli. `AKADEMI/backend/` öğretim materyali kendi ayrıntılı `aciklama`/`neden`/`olmasaydi` formatını korur (`AKADEMI/backend/STANDART.md`).

## 1. Dil Kuralı — Örnek

```csharp
// ✅ DOĞRU — yorum Türkçe, log/exception mesajı İngilizce
_logger.LogInformation("User {UserId} logged in. IP: {Ip}", userId, ip);
throw new EntityNotFoundException($"User not found: Id={userId}");

// ❌ YANLIŞ — log/exception mesajı Türkçe
// _logger.LogInformation("Kullanıcı {UserId} giriş yaptı.", userId);
```
İstemciye giden mesaj `Accept-Language`'a göre `ErrorMessages` sözlüğünden çözülür (`SECURITY.md §1.4`); DB/log daima İngilizce.

## 2. Yorum Satırları — Ne Zaman, Nasıl

- **Zorunlu dosya-başı/method-başı blok yok.** Dosya adı + sınıf/metot adı zaten AMAÇ'ı taşıyor; her dosyaya/metoda şablon yorum eklemek gürültü.
- Yorum yalnızca kodun kendisinin anlatamadığı şeyi anlatır: gizli bir kısıt, iki yer arasında senkron kalması gereken bir sözleşme, "böyle değil de bilerek şöyle yaptım" kararı, non-obvious bir edge case.
- **Kısa:** genelde tek satır, nadiren iki. Paragraf hâlinde blok yorum yazılmaz — anlatılacak şey birkaç satıra sığmıyorsa muhtemelen bir yardımcı metot veya daha iyi bir isim asıl çözüm.
- Ne yaptığını değil (kod zaten gösteriyor), **neden** öyle yaptığını anlat.

```csharp
// ✅ DOĞRU — kısa, NEDEN'e odaklı, sadece non-obvious kısım
// GetSmtpSettingsQueryHandler'daki aynı isimli sabitle değer olarak eşleşmeli
private const string MaskedPassword = "***";

// ❌ YANLIŞ — paragraf, AMAÇ/NEDEN/NASIL şablonu, kodun zaten söylediğini tekrarlıyor
// ─────────────────────────────────────────────
// UpdateSmtpSettingsCommand.cs
// AMAÇ: PUT /admin/smtp-settings — SMTP ayarlarını kaydeder...
// NEDEN: SMTP ayarları CLAUDE.md "Kimlik & güvenlik"nin kapsadığı...
// NASIL: 1) mevcut kaydı çek 2) şifre maskesi kontrolü 3) upsert 4) logla
// ─────────────────────────────────────────────
```

## 3. Karmaşık Bloklar

Gerçekten çok adımlı bir akış varsa (ör. SM-2 hesaplama) kod kendi akışıyla anlatır; adım numaralama yorumu (`// ADIM N:`) yalnızca akış kodda göze çarpmıyorsa, istisnai olarak eklenir — varsayılan değildir.

## 4. Katman Şablonları (kısa)

- **Entity:** alan adı açıklayıcıysa yorum yok; yalnızca non-obvious bir alan varsa (ör. birimi/kısıtı isimden anlaşılmayan) tek satır.
- **DTO:** neden Entity değil — hassas alan gizleme + sözleşme + sadece gerekli alanlar (bu, kod incelemesinde/PR'da konuşulur; dosyada uzun yorum gerekmez).
- **Validator:** kural ismi genelde kendini anlatır; yalnızca eşik/regex'in NEDEN'i non-obvious ise kısa bir satır. `WithMessage` İngilizce (log/DB'ye gider), `WithErrorCode` ile istemciye giden mesaj dile göre çözülür:
  ```csharp
  RuleFor(x => x.Password)
      .MinimumLength(12).WithMessage("Password must be at least 12 characters").WithErrorCode("PASSWORD_TOO_SHORT")
      .Matches(@"[A-Z]").WithMessage("Must contain at least 1 uppercase letter").WithErrorCode("PASSWORD_MISSING_UPPERCASE");
  ```
- **Repository:** async + CancellationToken + Include (N+1 önle) + soft delete filtresi.
- **Controller/Handler:** Controller ince (yalnızca `_mediator.Send`); iş mantığı Handler'da.

## 5. Genel Pratikler

`async/await`+`CancellationToken` her I/O'da · guard clause + İngilizce loglama · SOLID/DRY/KISS · soft delete + (kişiselde) UserId filtresi · parametreli sorgu / EF LINQ.

## 6. Birim Test Standardı (zorunlu — Faz E'ye bırakılmaz)

**Araçlar:** xUnit + Moq + FluentAssertions + `EFCore.InMemory` (yalnızca `Repository<T>` gibi DB'ye dokunan testlerde). Proje: `WordLearner.Tests`.

**6.1 Konum/adlandırma:** `Tests/{Services|Features|Helpers|Repositories}/`. `{TestEdilenSınıf}Tests.cs`.

**6.2 Metot adı (İngilizce):** `{Metot}_{Senaryo}_{BeklenenSonuç}`
```
✅ UpdateProgressAsync_QualityIsLow_ResetsLevel · Register_EmailAlreadyRegistered_ThrowsDuplicateException
❌ Test1 · UpdateProgress_Test · (Türkçe ad)
```

**6.3 AAA deseni** — her test ARRANGE/ACT/ASSERT (Türkçe yorumla bölünür); NEDEN yalnızca beklenti açık değilse Assert'te.
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

**6.4 Mock kuralları:** Repository + dış servisler (email, OneSignal, Google/Apple) her zaman mock. `Mock.Of<ILogger<T>>()`; log içeriği test edilmez. `IMapper` mock'lanmaz — gerçek Profile'dan kurulur. Gerçek in-memory EF yalnızca `Repository<T>` testinde.

**6.5 Minimum kapsam** her public metot için: happy path · bulunamadı (EntityNotFoundException) · yetki/sahiplik ihlali (403/404, kişiselde) · sınır/uç durum (duplikat 409, quality<3 vb.).

**6.6 Roadmap'e işleme:** Her API'ın HTML sayfasında ayrı "Test" alanı; test sınıfı birebir kopya + her metoda 3 satır:
```
Test Adı      : UpdateProgressAsync_QualityIsLow_ResetsLevel
Ne Test Edildi: Quality=0'da SM-2'nin seviyeyi sıfırlaması
Neden Önemli  : Yanlışta mastery kaybı olmazsa kullanıcı öğrenmiş görünür ama unutmuştur (SRS bozulur).
```

## 7. Dosya Kontrol Listesi

```
[ ] Şablon dosya-başı/method-başı yorum bloğu YOK
[ ] Yorum yalnızca non-obvious NEDEN'i anlatıyor, kısa (1-2 satır), Türkçe
[ ] log/exception/Code + method/class/property/test adı İngilizce
[ ] Handler/servis birim testi yazıldı (§7)
[ ] async/await + CancellationToken
[ ] Yazıldıkça roadmap'e işlendi (kod + test alanı)
```
