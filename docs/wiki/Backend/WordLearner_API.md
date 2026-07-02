# WordLearner.API

**Özet:** Solution'ın HTTP giriş noktası (composition root) — Controller'lar, middleware pipeline ve Swagger burada tanımlanır. Şu an yalnızca [[Program_cs]] mevcut; `Controllers/` klasörü boş, henüz hiçbir endpoint yazılmadı. `Microsoft.NET.Sdk.Web` SDK'sı kullanır, hedef framework `net9.0`.
**Kütüphaneler:** ASP.NET Core, Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0, Serilog.AspNetCore 8.0.1 (+ Console/File/MSSqlServer sink), Swashbuckle.AspNetCore 7.2.0
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Program_cs]] · [[Backend_Katmanli_Mimari]]

## Proje Referansları
`WordLearner.API.csproj` → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## NuGet Paketleri (kurulu, henüz tam kullanılmıyor)
- `Microsoft.AspNetCore.Authentication.JwtBearer` — JWT auth (A-03'te devreye girecek)
- `Serilog.AspNetCore` + `Serilog.Sinks.Console/File/MSSqlServer` — loglama (A-04'te devreye girecek)
- `Swashbuckle.AspNetCore` — Swagger/OpenAPI (aktif, `/swagger`)

## Dosyalar
| Dosya | Amaç |
|-------|------|
| [[Program_cs]] | Composition root — DI kaydı + middleware pipeline |
| `appsettings.json` | Genel config (gizli değer yok — bkz. [[Ortam_Degiskenleri]]) |
| `appsettings.Development.json` | Dev secrets (`.gitignore`'da) |
| `Controllers/` | **Boş** — ilk controller A-03 (`AuthController`) ile gelecek |
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
