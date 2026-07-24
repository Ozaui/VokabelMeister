# WordLearner.API

**Özet:** Solution'ın HTTP giriş noktası (composition root) — Controller'lar, middleware pipeline ve Swagger burada tanımlanır. [[Program_cs]] **A-04 itibarıyla tam yapılandırılmış** (JWT auth, CORS, Serilog konsol+dosya+DB sink, MediatR/AutoMapper/FluentValidation kayıtları) + A-03.1'de eklenen IP-partitioned `qrGenerate` rate limit policy'si + A-04'te eklenen `RateLimiterOptions.OnRejected` (RateLimitHit→SecurityLog); `Middleware/` klasöründe 3 sınıf var (bkz. [[Middleware]]), `RequestResponseLoggingMiddleware` A-04'te `LogContext.PushProperty` ile RequestPath/UserId zenginleştirmesi kazandı. `Controllers/` klasöründe beş controller var: `AuthController` (A-03 ✅, 13 endpoint), `QrLoginController` (A-03.1 ✅, 5 endpoint, `/auth/qr/*`), `HealthController` (A-04 ✅, `GET /health` — MediatR DIŞINDA, doğrudan `WordLearnerDbContext.CanConnectAsync`, bilinçli CQRS sapması), `WordsController` (A-05 ✅, 7 endpoint, projedeki **İLK** `[Authorize(Roles="Admin")]` kullanımı — CRUD + eşleştirme `/words/unmatched`/`/words/pair`) ve `CategoriesController` (A-06 ✅, hiyerarşi+çoklu dil+kelime eşleştirme) — hepsi `IMediator.Send(command)` ile `Application/Features/`'taki Command Handler'ları çağırır, iş mantığı içermez. `Logging/` klasörü (A-04) `ApplicationLogColumnOptions.cs` barındırır. `Microsoft.NET.Sdk.Web` SDK'sı kullanır, hedef framework `net9.0`.
**Kütüphaneler:** ASP.NET Core, Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0 (aktif — JWT doğrulama kurulu), Serilog.AspNetCore 8.0.1 (+ Console/File/MSSqlServer sink hepsi aktif — A-04), Swashbuckle.AspNetCore 7.2.0
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Program_cs]] · [[Middleware]] · [[Backend_Katmanli_Mimari]] · [[Icerik_Domain]]

## Proje Referansları
`WordLearner.API.csproj` → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## NuGet Paketleri
- `Microsoft.AspNetCore.Authentication.JwtBearer` — JWT auth (**aktif**, A-03 login/refresh akışlarında gerçek kullanımda)
- `Serilog.AspNetCore` + `Serilog.Sinks.Console/File/MSSqlServer` — loglama (**hepsi aktif**, A-04)
- `Swashbuckle.AspNetCore` — Swagger/OpenAPI (aktif, `/swagger`)

## Dosyalar
| Dosya | Amaç |
|-------|------|
| [[Program_cs]] | Composition root — DI kaydı + middleware pipeline |
| `Middleware/` | 3 sınıf: ExceptionHandling/SecurityHeaders/RequestResponseLogging (bkz. [[Middleware]]) |
| `appsettings.json` | Genel config (gizli değer yok — bkz. [[Ortam_Degiskenleri]]) |
| `appsettings.Development.json` | Dev secrets (`.gitignore`'da) |
| `Controllers/` | `AuthController.cs` (A-03, 13 endpoint), `QrLoginController.cs` (A-03.1, 5 endpoint), `HealthController.cs` (A-04), `WordsController.cs` (A-05, 7 endpoint, Admin-only), `CategoriesController.cs` (A-06) — hepsi `IMediator` üzerinden çağırır |
| `Filters/` | `ValidationFilter.cs` — global action filter, `IValidator<T>`'leri otomatik çalıştırır |
| `Common/` | `RequestLanguageResolver.cs` — `Accept-Language` çıkarma mantığı |
| `Logging/` | `ApplicationLogColumnOptions.cs` (A-04) — Serilog MSSqlServer sink'in kolon eşlemesi |
| `Properties/launchSettings.json` | Dev başlatma profili |

## appsettings.json Yapılandırma Blokları
- `Logging` — Default: Information, `Microsoft.AspNetCore`: Warning
- `ConnectionStrings:DefaultConnection` — boş, gerçek değer [[Ortam_Degiskenleri]]'nden gelir
- `Jwt` — Issuer/Audience/ExpirationMinutes(15)/RefreshTokenExpirationDays(7) (SecretKey burada YOK)
- `Cors:AllowedOrigins` — `localhost:8081` (Expo) · `:5173` (Admin/Vite) · `:5174` (Web/Vite)

## Dev Çalıştırma Profilleri (`Properties/launchSettings.json`, `.gitignore`'da — commit edilmez)
- `http` profili → `http://localhost:5001`, tarayıcı otomatik `/swagger`'a açılır
- `https` profili → `https://localhost:7001` + `http://localhost:5001`
- İkisi de `ASPNETCORE_ENVIRONMENT=Development` set eder; `https` profili ayrıca
  `AES_ENCRYPTION_KEY`'i env var olarak taşır (bkz. [[Ortam_Degiskenleri]] §5) — gerçek değer
  yalnızca yerel makinede, git'e girmez.
