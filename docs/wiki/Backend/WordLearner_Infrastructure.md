# WordLearner.Infrastructure

**Özet:** Veri erişim katmanı — EF Core DbContext, generic repository implementasyonu, domain'e özgü `IEntityTypeConfiguration<T>` sınıfları, Serilog MSSqlServer sink (A-04) ve DI kayıt extension'ı burada yaşar. [[WordLearner_Application]] ve [[WordLearner_Domain]]'e bağımlıdır. Generic [[Repository]] yanında feature repository'ler mevcut: Auth (`UserRepository`, `RefreshTokenRepository`, `QrLoginSessionRepository`), Logging (`ActivityLogRepository`, `ApplicationLogRepository`, `SecurityLogRepository` — A-04), Words (`WordConceptRepository` — aggregate root, `LanguageRepository`), Categories (`CategoryRepository` — A-06).
**Kütüphaneler:** Microsoft.EntityFrameworkCore 9.0.0, Microsoft.EntityFrameworkCore.SqlServer 9.0.0, Microsoft.EntityFrameworkCore.Tools 9.0.0, Serilog.Sinks.MSSqlServer (A-04, `ApplicationLog` tablosuna yazar) — planlanan (henüz kurulmadı): `MailKit` 4.3.0 (SMTP, A-10)
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Domain]] · [[WordLearnerDbContext]] · [[Repository]] · [[InfrastructureServiceExtensions]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]] · [[Auth_Domain]] · [[Loglama_Domain]] · [[Icerik_Domain]]

## Proje Referansları
`WordLearner.Infrastructure.csproj` → [[WordLearner_Domain]], [[WordLearner_Application]]

## Klasör Yapısı (mevcut, A-06 itibarıyla)
```
Data/
  WordLearnerDbContext.cs
  Configurations/
    Auth/
      UserConfiguration.cs
      RefreshTokenConfiguration.cs
      QrLoginSessionConfiguration.cs
    Logging/                            ← A-04
      ActivityLogConfiguration.cs
      ApplicationLogConfiguration.cs
      SecurityLogConfiguration.cs
    Words/                              ← A-05
      LanguageConfiguration.cs
      WordConceptConfiguration.cs
      WordConfiguration.cs
      WordDetailConfiguration.cs
      WordExampleConfiguration.cs
    Categories/                         ← A-06
      CategoryConfiguration.cs
      CategoryTranslationConfiguration.cs
      WordCategoryConfiguration.cs
  Migrations/   → dotnet ef migrations add ile üretilir, alt klasörlenmez (tarihsel sırayla düz durur)
                  (AddUserAndRefreshToken → AddQrLoginSessions → FixHashColumnMaxLength →
                  AddUserThemePreference → AddLoggingTables → AddWordsSchema → AddCategoriesSchema)
Extensions/    → InfrastructureServiceExtensions.cs
Repositories/  → Repository.cs (generic), UserRepository.cs, RefreshTokenRepository.cs,
                 QrLoginSessionRepository.cs, ActivityLogRepository.cs, ApplicationLogRepository.cs,
                 SecurityLogRepository.cs, WordConceptRepository.cs, LanguageRepository.cs,
                 CategoryRepository.cs
```
**Kural:** `Configurations/` de `Domain/Entities`'teki gibi domain başına alt klasör alır
(`Configurations/<Domain>/`) — bkz. `wiki/Standartlar/Kodlama_Standartlari.md` "Klasör
Organizasyonu". `Repositories/` bilinçli olarak flat kalıyor (`Application/Services`/
`Interfaces/Services` ile aynı YAGNI kararı, `CLAUDE.md §5`).

## Dosyalar
- [[WordLearnerDbContext]] — merkezi DbContext; soft delete global filter + `SaveChangesAsync` override
- [[Repository]] — `IRepository<T>`'nin EF Core tabanlı generic implementasyonu
- [[InfrastructureServiceExtensions]] — `AddInfrastructureServices()` DI extension metodu
- `Configurations/Auth/*` — bkz. [[Auth_Domain]]
- `Configurations/Logging/*`, `Repositories/ActivityLogRepository.cs`/`ApplicationLogRepository.cs`/
  `SecurityLogRepository.cs` — bkz. [[Loglama_Domain]]
- `Configurations/Words/*`, `Configurations/Categories/*`, `Repositories/WordConceptRepository.cs`/
  `LanguageRepository.cs`/`CategoryRepository.cs` — bkz. [[Icerik_Domain]]
