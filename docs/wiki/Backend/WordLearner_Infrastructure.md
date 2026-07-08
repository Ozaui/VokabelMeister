# WordLearner.Infrastructure

**Özet:** Veri erişim katmanı — EF Core DbContext, generic repository implementasyonu, domain'e özgü `IEntityTypeConfiguration<T>` sınıfları ve DI kayıt extension'ı burada yaşar. [[WordLearner_Application]] ve [[WordLearner_Domain]]'e bağımlıdır. Generic [[Repository]] yanında feature repository'ler (`UserRepository`, `RefreshTokenRepository`, `QrLoginSessionRepository` — A-03.1) da mevcut.
**Kütüphaneler:** Microsoft.EntityFrameworkCore 9.0.0, Microsoft.EntityFrameworkCore.SqlServer 9.0.0, Microsoft.EntityFrameworkCore.Tools 9.0.0 — planlanan (henüz kurulmadı): `MailKit` 4.3.0 (SMTP, A-10)
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Domain]] · [[WordLearnerDbContext]] · [[Repository]] · [[InfrastructureServiceExtensions]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]] · [[Auth_Domain]]

## Proje Referansları
`WordLearner.Infrastructure.csproj` → [[WordLearner_Domain]], [[WordLearner_Application]]

## Klasör Yapısı (mevcut, 2026-07-07 itibarıyla Configurations/ domain alt klasörlerine geçildi)
```
Data/
  WordLearnerDbContext.cs
  Configurations/
    Auth/
      UserConfiguration.cs
      RefreshTokenConfiguration.cs
      QrLoginSessionConfiguration.cs
  Migrations/   → dotnet ef migrations add ile üretilir, alt klasörlenmez (tarihsel sırayla düz durur)
Extensions/    → InfrastructureServiceExtensions.cs
Repositories/  → Repository.cs (generic), UserRepository.cs, RefreshTokenRepository.cs,
                 QrLoginSessionRepository.cs (A-03.1)
```
**Kural:** `Configurations/` de `Domain/Entities`'teki gibi domain başına alt klasör alır
(`Configurations/<Domain>/`) — bkz. `wiki/Standartlar/Kodlama_Standartlari.md` "Klasör
Organizasyonu". `Repositories/` şimdilik flat kalıyor (dosya sayısı az); büyürse aynı kurala tabi
olacak.

## Planlanan Genişleme
Yeni her domain (Vocabulary, SRS, ...) kendi `Configurations/<Domain>/` klasörünü ve feature
repository'lerini (`WordRepository` vb.) ekler.

## Dosyalar
- [[WordLearnerDbContext]] — merkezi DbContext; soft delete global filter + `SaveChangesAsync` override
- [[Repository]] — `IRepository<T>`'nin EF Core tabanlı generic implementasyonu
- [[InfrastructureServiceExtensions]] — `AddInfrastructureServices()` DI extension metodu
- `Configurations/Auth/*` — bkz. [[Auth_Domain]]
