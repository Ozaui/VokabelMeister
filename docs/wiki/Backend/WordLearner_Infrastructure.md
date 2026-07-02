# WordLearner.Infrastructure

**Özet:** Veri erişim katmanı — EF Core DbContext, generic repository implementasyonu ve DI kayıt extension'ı burada yaşar. [[WordLearner_Application]] ve [[WordLearner_Domain]]'e bağımlıdır; feature repository'ler (`IUserRepository` vb.) henüz eklenmedi, yalnızca generic [[Repository]] mevcut.
**Kütüphaneler:** Microsoft.EntityFrameworkCore 9.0.0, Microsoft.EntityFrameworkCore.SqlServer 9.0.0, Microsoft.EntityFrameworkCore.Tools 9.0.0 — planlanan (henüz kurulmadı): `MailKit` 4.3.0 (SMTP, A-10)
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Domain]] · [[WordLearnerDbContext]] · [[Repository]] · [[InfrastructureServiceExtensions]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Infrastructure.csproj` → [[WordLearner_Domain]], [[WordLearner_Application]]

## Klasör Yapısı (mevcut)
```
Data/        → WordLearnerDbContext.cs
Extensions/  → InfrastructureServiceExtensions.cs
Repositories/→ Repository.cs
```

## Planlanan Genişleme
`Configurations/` (`IEntityTypeConfiguration<T>` sınıfları — `WordLearnerDbContext.OnModelCreating`
bunları assembly taramasıyla otomatik uygular), `Repositories/` altına feature repository'ler
(`UserRepository`, `WordRepository` ...), `Migrations/` (henüz hiç migration oluşturulmadı).

## Dosyalar
- [[WordLearnerDbContext]] — merkezi DbContext; soft delete global filter + `SaveChangesAsync` override
- [[Repository]] — `IRepository<T>`'nin EF Core tabanlı generic implementasyonu
- [[InfrastructureServiceExtensions]] — `AddInfrastructureServices()` DI extension metodu
