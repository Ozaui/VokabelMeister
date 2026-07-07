# WordLearner.Tests

**Özet:** xUnit birim test projesi; [[WordLearner_Application]] ve [[WordLearner_Infrastructure]]'a referans verir. A-02: [[RepositoryTests]] (in-memory EF Core ile generic [[Repository]] CRUD + soft delete filtresi + `UpdatedAt`/userId audit alanları, 10 test) ve `EntityNotFoundExceptionTests` (Type+key overload'ının mesaj formatını doğrular, 1 test). A-03: `PasswordServiceTests` (BCrypt/SHA-256, mock'suz — saf algoritma, 5 test), `JwtTokenServiceTests` (claim'ler + Algorithm Confusion önlemi, gerçek in-memory `IConfiguration` ile, 6 test), `OtpServiceTests` (OTP üretimi/doğrulanması/temizlenmesi, 7 test) + `LoginCompletionServiceTests` (OTP/Google/Apple girişlerinin ortak son adımı, grace period kurtarma dahil, 5 test — MediatR CQRS refactor'ünde `AuthService`'ten çıkan paylaşılan servislerin testleri), `Features/Auth/` altında 13 akışın her biri için ayrı bir `*CommandHandlerTests` dosyası (eskiden tek `AuthServiceTests` dosyasıydı, 39 test — şimdi handler başına bölünmüş hâliyle 38 test, grace period/anonimleştirme senaryoları tekrar edilmek yerine `LoginCompletionServiceTests`'e taşındığı için). Toplam 72 test, hepsi yeşil.
**Kütüphaneler:** xUnit 2.9.2, xunit.runner.visualstudio 2.8.2, Microsoft.NET.Test.Sdk 17.12.0, coverlet.collector 6.0.2, `Moq` 4.20.70, `FluentAssertions` 6.12.0, `Microsoft.EntityFrameworkCore.InMemory` 9.0.0 (bkz. [[Kodlama_Standartlari]] §7, tam versiyon listesi [[Teknik_Ozellikler]] §1)
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Repository]] · [[RepositoryTests]] · [[Kodlama_Standartlari]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Tests.csproj` → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## Klasör Yapısı
```
Services/              → PasswordServiceTests.cs, JwtTokenServiceTests.cs,
                          OtpServiceTests.cs, LoginCompletionServiceTests.cs ✅ (A-03)
Features/Auth/         → 13 *CommandHandlerTests.cs (RegisterCommandHandlerTests.cs vb.,
                          eski AuthServiceTests.cs'in yerine — A-03 MediatR refactor) ✅
Helpers/               → SrsCalculatorTests.cs vb. (henüz yok)
Repositories/          → RepositoryTests.cs   (generic taban için, A-02'de bir kez) ✅
Common/Exceptions/     → EntityNotFoundExceptionTests.cs (Type+key overload mesaj formatı) ✅
```

## Test Felsefesi
Testler Faz F'ye bırakılmaz — her API'nın servis katmanı bitince aynı task içinde birim testi
yazılır. İsimlendirme kalıbı: `{Metot}_{Senaryo}_{BeklenenSonuç}`. AAA deseni (Arrange/Act/Assert),
repository ve dış servisler her zaman mock'lanır. Detay → [[Kodlama_Standartlari]] §7.
