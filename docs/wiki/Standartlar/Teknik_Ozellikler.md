# Teknik Özellikler (NuGet/npm paketleri + referans kod)

**Özet:** `docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md`'nin wiki karşılığı — backend NuGet paket listesi (kurulu + planlanan), frontend npm paket komutları ve ileride yazılacak servislerin (JWT, Şifre, SRS, Serilog) **birebir referans kod örnekleri**. Bu dosya, ilgili task'a gelindiğinde kopyalanacak/uyarlanacak "altın kaynak" niteliğindedir.
**Kütüphaneler:** bkz. aşağıdaki tablolar
**Bağlantılar:** [[WordLearner_API]] · [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[WordLearner_Tests]] · [[Program_cs]] · [[SRS_Domain]] · [[Auth_Domain]] · [[Loglama_Domain]] · [[Gelistirme_Kurulumu]] · [[Kodlama_Standartlari]]

## 1. NuGet Paketleri (kurulu vs. planlanan)

| Proje | Paket | Durum |
|-------|-------|-------|
| API | `Microsoft.AspNetCore.Authentication.JwtBearer` 9.0.0, `Serilog.AspNetCore` 8.0.1 (+Console/File sink), `Swashbuckle.AspNetCore` 7.2.0 | ✅ kurulu ve kullanımda ([[WordLearner_API]]) |
| API | `Serilog.Sinks.MSSqlServer` 6.6.0 | ⚠️ **kurulu, henüz bağlı değil — hedef: A-04.** `Program.cs`'te tek bir `WriteTo.MSSqlServer(...)` çağrısı yok, yalnızca yorum var (bkz. TASK.md §5 "Kurulu-ama-kullanılmayan paket taraması", 2026-07-07'de MediatR/AutoMapper retrofit'i sırasında fark edildi). A-04'e başlarken bu satır kontrol edilip sink fiilen bağlanacak, aksi halde kaldırılacak. |
| Application | `MediatR` 12.1.1, `AutoMapper` 13.0.1, `FluentValidation` 11.9.2 (+DI extensions) | ✅ kurulu (A-02) — MediatR A-03'te 13 Auth Command+Handler, AutoMapper A-03'te [[AuthProfile]] ile ilk gerçek kullanımına kavuştu |
| Application | `BCrypt.Net-Next` 4.0.3, `System.IdentityModel.Tokens.Jwt` 7.1.0, `Google.Apis.Auth` 1.67.0 | ✅ kurulu ve kullanımda (A-03) |
| Infrastructure | `Microsoft.EntityFrameworkCore(.SqlServer/.Tools)` 9.0.0 | ✅ kurulu |
| Infrastructure | `MailKit` 4.3.0 (SMTP e-posta) | ⬜ henüz kurulmadı — A-10 |
| Tests | `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector` | ✅ kurulu (farklı versiyonlarla — bkz. [[WordLearner_Tests]]) |
| Tests | `Moq` 4.20.70, `FluentAssertions` 6.12.0, `Microsoft.EntityFrameworkCore.InMemory` 9.0.0 | ✅ kurulu ve kullanımda (A-02'nin `RepositoryTests`'iyle geldi; bu satır bayattı, 2026-07-07'de düzeltildi) |

## 2. npm Paketleri (frontend — henüz hiçbiri kurulmadı, klasörler yok)

```bash
# Web (/web) — Faz D
npm create vite@latest web -- --template react-ts
npm i @reduxjs/toolkit react-redux axios react-hook-form react-router-dom @react-oauth/google
npm i -D tailwindcss postcss autoprefixer

# Admin (/admin) — Faz B — Google/Apple YOK
npm create vite@latest admin -- --template react-ts
npm i @reduxjs/toolkit react-redux axios react-hook-form react-router-dom
npm i -D tailwindcss postcss autoprefixer

# Mobil (/mobile) — Faz E
npx create-expo-app mobile --template expo-template-blank-typescript
npm i @reduxjs/toolkit react-redux axios react-hook-form i18next react-i18next
npx expo install expo-secure-store expo-av expo-image-picker expo-apple-authentication
npx expo install @react-navigation/native @react-navigation/bottom-tabs @react-navigation/stack
npm i @react-native-google-signin/google-signin
```

## 3. JWT Token Servisi (A-03 ✅ — implement edildi)

`ITokenService`/`JwtTokenService`: `GenerateAccessToken(User)` (15dk, HMAC-SHA256, claims:
NameIdentifier/Email/Role/firstName), `GenerateRefreshToken()` (64 byte random, Base64, gün
sayısı `Jwt:RefreshTokenExpirationDays`'ten), `GetPrincipalFromExpiredToken(string)` —
**Algorithm Confusion önlemi:** doğrulanan token'ın `Header.Alg`'i `HmacSha256` değilse `null`
döner. Detay → [[Auth_Domain]], [[Guvenlik_Politikalari]].

## 4. Şifre Servisi (A-03 ✅ — implement edildi)

`IPasswordService`/`PasswordService`: `Hash(password)` → `BCrypt.Net.BCrypt.HashPassword(pw,
workFactor: 12)`, `Verify(password, hash)`, `HashToken(token)` → SHA-256 (refresh/OTP token
hash'lemek için — ham token asla DB'de saklanmaz).

## 5. SrsCalculator — SM-2 Algoritması (planlanan — C-03, henüz yok)

```csharp
public static class SrsCalculator
{
    // quality: 0-5 (öz değerlendirme) — 🔴0 Bilmedim · 🟠2 Zor · 🟢4 İyi · 🔵5 Çok Kolay
    public static (int intervalDays, int newLevel, decimal newEF) Calculate(
        int currentLevel, int repetitionNumber, decimal easinessFactor, int quality)
    {
        if (quality < 3)   // yanlış/çok zor → başa dön, EF düşer ama 1.3 altına inmez
            return (1, 0, Math.Max(1.3m, easinessFactor - 0.2m));

        int interval = repetitionNumber == 0 ? 1
                     : repetitionNumber == 1 ? 3
                     : (int)Math.Round((repetitionNumber - 1) * easinessFactor);

        decimal newEF = easinessFactor + (0.1m - (5 - quality) * (0.08m + (5 - quality) * 0.02m));
        newEF = Math.Max(1.3m, newEF);
        return (interval, Math.Min(currentLevel + 1, 5), newEF);
    }
}
```
Bu, [[SRS_Domain]]'deki `UserProgress`/`UserCardProgress` alanlarını doğrudan besleyen algoritmadır.

## 6. Serilog → ApplicationLog (DB sink, planlanan — A-04, henüz yok)

`builder.Host.UseSerilog(...)` ile `WriteTo.Console()` + `WriteTo.File(...)` +
`WriteTo.MSSqlServer(tableName: "ApplicationLog", AutoCreateSqlTable: false)`. `ActivityLog` ve
`SecurityLog` Serilog ile **değil**, özel `IActivityLogger`/`ISecurityLogger` servisleriyle
yazılır — detay [[Loglama_Domain]].

## 7. Program.cs — Hedef Yapılandırma (A-02 ✅ — tamamlandı)

Bu, [[Program_cs]]'in gerçek A-02 sonrası hâlidir — Serilog host, DbContext + Infrastructure/
Application servis kayıtları, FluentValidation, JWT Bearer authentication, CORS policy,
`SecurityHeadersMiddleware` + `ExceptionHandlingMiddleware`, `UseStaticFiles()` (avatar/görsel)
sırasıyla eklendi. Tam kod `docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md §10`'da; adım adım
gerekçesiyle [[Program_cs]] düğümünde.
