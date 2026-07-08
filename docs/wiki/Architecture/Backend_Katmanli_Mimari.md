# Backend Katmanlı Mimari

**Özet:** Backend, klasik katmanlı (layered) mimariyle dört .NET projesine ayrılır ve bağımlılık yönü tek yönlüdür: `Domain ← Application ← Infrastructure ← API`. Her katman yalnızca kendi altındaki katmana referans verir; Domain hiçbir şeye bağımlı değildir. Çalışma yöntemi "dikey dilim"dir — bir API tüm katmanlarıyla bitirilip sonra diğerine geçilir. **A-03'te (17. INGEST) mimari kanonik desen değişti:** Controller → Servis Arayüzü/Servis yerine artık Controller → `IMediator.Send(command)` → MediatR Command+Handler (CQRS); "Servis Arayüzü/Servis" deseni tamamen terk edildi, yalnızca birden çok Handler'ın paylaştığı küçük yardımcı servisler (`OtpService`, `LoginCompletionService` gibi) kalıyor. Detay → `CLAUDE.md §3`.
**Kütüphaneler:** .NET 9, EF Core 9, ASP.NET Core, MediatR 12.1.1, AutoMapper 13.0.1, FluentValidation 11.9.2
**Bağlantılar:** [[WordLearner_API]] · [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[WordLearner_Domain]] · [[WordLearner_Tests]] · [[Gelistirme_Yol_Haritasi]] · [[Sistem_Mimarisi]] · [[Auth_Domain]]

## Katmanlar

```
WordLearner.API            → HTTP (Controllers, Middleware, Program.cs)
WordLearner.Application    → İş mantığı (Features/<Domain>/ — Command+Handler, DTOs, Validators,
                              Interfaces, Services — paylaşılan yardımcı servisler)
WordLearner.Infrastructure → Veri erişimi (DbContext, Repositories, Configurations, Serilog sink)
WordLearner.Domain         → Entities, Enums
```

Proje referansları (`.csproj`):
- [[WordLearner_API]] → [[WordLearner_Application]], [[WordLearner_Infrastructure]]
- [[WordLearner_Infrastructure]] → [[WordLearner_Application]], [[WordLearner_Domain]]
- [[WordLearner_Application]] → [[WordLearner_Domain]]
- [[WordLearner_Domain]] → (bağımsız)
- [[WordLearner_Tests]] → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## Dikey Dilim Çalışma Yöntemi

Bir API'ın tüm parçaları (Entity → EF Config → Migration → Command/Query → Validator → Repository →
Handler → Birim Test → Controller → DI kaydı) **tek task içinde** tamamlanır, ardından
`API_YOL_HARITASI/` rehberine işlenir. Katman katman ilerleme (önce tüm entity'ler, sonra tüm
DTO'lar) yasaktır. Handler'lar birbirini `_mediator.Send()` ile ASLA çağırmaz (döngüsel bağımlılık) —
paylaşılan mantık gerekiyorsa küçük bir servise çıkarılır. Detay ve gerekçe → `CLAUDE.md §3`,
[[Gelistirme_Yol_Haritasi]].

## Şu Ana Kadar Yazılan Kod

**A-01/A-02 ✅ (ortak altyapı):**
- [[Program_cs]] (API katmanı — tam yapılandırılmış: Serilog, JWT, CORS, MediatR/AutoMapper/FluentValidation, middleware, rate limiting)
- [[BaseEntity]] (Domain)
- [[IRepository]] / [[Repository]], [[WordLearnerDbContext]], [[InfrastructureServiceExtensions]] (Infrastructure)
- [[EntityNotFoundException]] / [[AppException]] / [[ErrorMessages]] / [[ApiErrorResponse]] (Application)
- [[Middleware]] — `ExceptionHandlingMiddleware`/`SecurityHeadersMiddleware`/`RequestResponseLoggingMiddleware`

**A-03 ✅ + A-03.1 ✅ (Auth + QR ile Giriş — ilk gerçek feature domain'i):**
- Domain: `User`, `RefreshToken`, `QrLoginSession` entity + `OtpPurpose`, `QrLoginStatus` enum (`Entities/Auth/`, `Enums/Auth/`)
- Infrastructure: `UserConfiguration`/`RefreshTokenConfiguration`/`QrLoginSessionConfiguration`, `UserRepository`/`RefreshTokenRepository`/`QrLoginSessionRepository`
- Application: 13 (Auth) + 5 (QrLogin) Command+Handler (`Features/Auth/`, `Features/QrLogin/`), `AuthProfile` (AutoMapper), paylaşılan servisler (`OtpService`, `LoginCompletionService`, `PasswordService`, `JwtTokenService`, `GoogleTokenValidator`, `AppleTokenValidator`), FluentValidation validator'ları (`Validators/Auth/`)
- API: `AuthController` (13 endpoint) + `QrLoginController` (5 endpoint), ikisi de yalnızca `IMediator.Send(command)` çağırır
- Test: 90 birim test (72 Auth + 18 QrLogin), hepsi yeşil
- Detay → [[Auth_Domain]], `API_YOL_HARITASI/A-03_auth-api.html`, `A-03.1_qr-login.html`

Word/Category/... (A-05+) henüz yazılmadı.
