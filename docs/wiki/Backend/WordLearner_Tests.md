# WordLearner.Tests

**Özet:** xUnit birim test projesi; [[WordLearner_Application]] ve [[WordLearner_Infrastructure]]'a referans verir. A-02: [[RepositoryTests]] (in-memory EF Core ile generic [[Repository]] CRUD + soft delete filtresi + `UpdatedAt`/userId audit alanları, 10 test) ve `EntityNotFoundExceptionTests` (Type+key overload'ının mesaj formatını doğrular, 1 test). A-03 ✅: `PasswordServiceTests` (BCrypt/SHA-256, mock'suz — saf algoritma, 5 test), `JwtTokenServiceTests` (claim'ler + Algorithm Confusion önlemi, gerçek in-memory `IConfiguration` ile, 6 test), `OtpServiceTests` (OTP üretimi/doğrulanması/temizlenmesi, 7 test) + `LoginCompletionServiceTests` (OTP/Google/Apple girişlerinin ortak son adımı, grace period kurtarma dahil, 5 test — MediatR CQRS refactor'ünde `AuthService`'ten çıkan paylaşılan servislerin testleri), `Features/Auth/` altında 13 akışın her biri için ayrı bir `*CommandHandlerTests` dosyası (eskiden tek `AuthServiceTests` dosyasıydı, 39 test — şimdi handler başına bölünmüş hâliyle 38 test, grace period/anonimleştirme senaryoları tekrar edilmek yerine `LoginCompletionServiceTests`'e taşındığı için) — A-03 toplamı 72 test. **A-03.1 ✅:** `Features/QrLogin/` altında 5 akışın her biri için ayrı `*CommandHandlerTests` dosyası (Generate/Scan/Confirm/Deny/GetStatus), 18 test. **A-03.2 ✅ (2026-07-11):** `SuccessMessages`/`ErrorMessages`'ın tr/de dil çözümlemesini doğrulamak için 7 handler testine (`MessageResponse.Code`/`Language` döndüren akışlar) `de` senaryosu eklendi — `Features/Auth/` 38→45 test, A-03 toplamı 72→79. **Denetim sonrası bugfix turu ✅ (2026-07-11, aynı gün):** QR ile giriş akışındaki 4 gerçek buga (rate-limit self-lockout, audit alanları boş kalıyordu, soft-delete/hesap-durumu kontrolü atlanıyordu, ham QR token exception mesajına sızıyordu) karşılık `Features/QrLogin/` altına 5 yeni regresyon testi eklendi (Confirm/Deny'e birer `TokenNotFound` testi — önceden hiç yoktu, GetQrLoginStatus'a grace-period kurtarma/inactive/anonymized senaryoları) — `Features/QrLogin/` 18→23 test. **Genel toplam 102 test, hepsi yeşil** (`dotnet test --logger trx` ile doğrulandı — sonuçlar `API_YOL_HARITASI/*.html` sayfalarına `adim.sonuclar` alanı olarak gömülü, bkz. [[API_Yol_Haritasi_Sistemi]]).
**Kütüphaneler:** xUnit 2.9.2, xunit.runner.visualstudio 2.8.2, Microsoft.NET.Test.Sdk 17.12.0, coverlet.collector 6.0.2, `Moq` 4.20.70, `FluentAssertions` 6.12.0, `Microsoft.EntityFrameworkCore.InMemory` 9.0.0 (bkz. [[Kodlama_Standartlari]] §7, tam versiyon listesi [[Teknik_Ozellikler]] §1)
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Repository]] · [[RepositoryTests]] · [[Kodlama_Standartlari]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]] · [[Auth_Domain]]

## Proje Referansları
`WordLearner.Tests.csproj` → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## Klasör Yapısı
```
Services/              → PasswordServiceTests.cs, JwtTokenServiceTests.cs,
                          OtpServiceTests.cs, LoginCompletionServiceTests.cs ✅ (A-03, 23 test)
Features/Auth/         → 13 *CommandHandlerTests.cs (RegisterCommandHandlerTests.cs vb.,
                          eski AuthServiceTests.cs'in yerine — A-03 MediatR refactor) ✅ (45 test,
                          A-03.2'de 7 handler'a tr/de dil senaryosu eklendi: 38→45)
Features/QrLogin/      → 5 *CommandHandlerTests.cs (GenerateQrLoginCommandHandlerTests.cs vb.) ✅ (A-03.1, 23 test —
                          18 orijinal + 2026-07-11 bugfix turunda 5 yeni regresyon testi)
Helpers/               → SrsCalculatorTests.cs vb. (henüz yok)
Repositories/          → RepositoryTests.cs   (generic taban için, A-02'de bir kez) ✅ (10 test)
Common/Exceptions/     → EntityNotFoundExceptionTests.cs (Type+key overload mesaj formatı) ✅ (1 test)
```

## Test Felsefesi
Testler Faz F'ye bırakılmaz — her API'nın servis katmanı bitince aynı task içinde birim testi
yazılır. İsimlendirme kalıbı: `{Metot}_{Senaryo}_{BeklenenSonuç}`. AAA deseni (Arrange/Act/Assert),
repository ve dış servisler her zaman mock'lanır. Detay → [[Kodlama_Standartlari]] §7.

**İsimlendirme düzeltmesi (2026-07-11):** `{Metot}` alanı, MediatR Handler'ları için gerçek C#
metot adı (`Handle`) değil, **domain fiili** anlamına gelir — `Handle_...` her testte tekrar edip
ayırt edici olmayacağı için. `Features/QrLogin/` (`Scan_...`, `Confirm_...` vb.) bunu baştan doğru
uyguluyordu; `Features/Auth/`'daki 13 dosya ise hâlâ pre-CQRS `AuthService` döneminden kalma
`LoginAsync_...`/`RegisterAsync_...` deseniyle yazılıydı ("Async" eki artık var olmayan eski servis
metodunun izi). 45 test metodu `Xxx_...` desenine hizalandı (ör. `LoginAsync_ValidCredentials_...`
→ `Login_ValidCredentials_...`) — davranış değişmedi, yalnızca isim.
