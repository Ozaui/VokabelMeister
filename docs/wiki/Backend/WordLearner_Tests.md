# WordLearner.Tests

**Özet:** xUnit birim test projesi; [[WordLearner_Application]] ve [[WordLearner_Infrastructure]]'a referans verir. A-02: [[RepositoryTests]] (in-memory EF Core ile generic [[Repository]] CRUD + soft delete filtresi + `UpdatedAt`/userId audit alanları, 10 test) ve `EntityNotFoundExceptionTests` (Type+key overload'ının mesaj formatını doğrular, 1 test). A-03: `PasswordServiceTests` (BCrypt/SHA-256, mock'suz — saf algoritma, 5 test), `JwtTokenServiceTests` (claim'ler + Algorithm Confusion önlemi, gerçek in-memory `IConfiguration` ile, 6 test), `AuthServiceTests` (13 akışın tamamı — timing attack, e-posta numaralandırma önlemi, Token Family replay tespiti, Google/Apple account linking, grace period kurtarma; repository/dış servisler Moq ile mock'lanır, 39 test). Toplam 61 test, hepsi yeşil.
**Kütüphaneler:** xUnit 2.9.2, xunit.runner.visualstudio 2.8.2, Microsoft.NET.Test.Sdk 17.12.0, coverlet.collector 6.0.2, `Moq` 4.20.70, `FluentAssertions` 6.12.0, `Microsoft.EntityFrameworkCore.InMemory` 9.0.0 (bkz. [[Kodlama_Standartlari]] §7, tam versiyon listesi [[Teknik_Ozellikler]] §1)
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Repository]] · [[RepositoryTests]] · [[Kodlama_Standartlari]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Tests.csproj` → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## Klasör Yapısı
```
Services/              → PasswordServiceTests.cs, JwtTokenServiceTests.cs, AuthServiceTests.cs ✅ (A-03)
Helpers/               → SrsCalculatorTests.cs vb. (henüz yok)
Repositories/          → RepositoryTests.cs   (generic taban için, A-02'de bir kez) ✅
Common/Exceptions/     → EntityNotFoundExceptionTests.cs (Type+key overload mesaj formatı) ✅
```

## Test Felsefesi
Testler Faz F'ye bırakılmaz — her API'nın servis katmanı bitince aynı task içinde birim testi
yazılır. İsimlendirme kalıbı: `{Metot}_{Senaryo}_{BeklenenSonuç}`. AAA deseni (Arrange/Act/Assert),
repository ve dış servisler her zaman mock'lanır. Detay → [[Kodlama_Standartlari]] §7.
