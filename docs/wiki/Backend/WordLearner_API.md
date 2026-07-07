# WordLearner.API

**Özet:** Solution'ın HTTP giriş noktası (composition root) — Controller'lar, middleware pipeline ve Swagger burada tanımlanır. [[Program_cs]] artık **A-02 itibarıyla tam yapılandırılmış** (JWT auth, CORS, Serilog, MediatR/AutoMapper/FluentValidation kayıtları); `Middleware/` klasöründe 3 sınıf var (bkz. [[Middleware]]). `Controllers/` klasöründe ilk (ve tek) controller `AuthController` (A-03, 13 endpoint) var — `IMediator.Send(command)` ile `Application/Features/Auth/`'taki Command Handler'ları çağırır, kendisi iş mantığı içermez. `Microsoft.NET.Sdk.Web` SDK'sı kullanır, hedef framework `net9.0`.
**Kütüphaneler:** ASP.NET Core, Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0 (aktif — JWT doğrulama kurulu), Serilog.AspNetCore 8.0.1 (+ Console/File sink aktif, MSSqlServer sink A-04'te), Swashbuckle.AspNetCore 7.2.0
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Program_cs]] · [[Middleware]] · [[Backend_Katmanli_Mimari]]

## Proje Referansları
`WordLearner.API.csproj` → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## NuGet Paketleri
- `Microsoft.AspNetCore.Authentication.JwtBearer` — JWT auth (**aktif**, A-03 login/refresh akışlarında gerçek kullanımda)
- `Serilog.AspNetCore` + `Serilog.Sinks.Console/File` — loglama (**aktif**); `Serilog.Sinks.MSSqlServer` paketi kurulu ama sink bağlantısı A-04'te
- `Swashbuckle.AspNetCore` — Swagger/OpenAPI (aktif, `/swagger`)

## Dosyalar
| Dosya | Amaç |
|-------|------|
| [[Program_cs]] | Composition root — DI kaydı + middleware pipeline |
| `Middleware/` | 3 sınıf: ExceptionHandling/SecurityHeaders/RequestResponseLogging (bkz. [[Middleware]]) |
| `appsettings.json` | Genel config (gizli değer yok — bkz. [[Ortam_Degiskenleri]]) |
| `appsettings.Development.json` | Dev secrets (`.gitignore`'da) |
| `Controllers/` | `AuthController.cs` (A-03, 13 endpoint, `IMediator` üzerinden çağırır) |
| `Filters/` | `ValidationFilter.cs` — global action filter, `IValidator<T>`'leri otomatik çalıştırır |
| `Common/` | `RequestLanguageResolver.cs` — `Accept-Language` çıkarma mantığı |
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
