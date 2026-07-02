# Auth Domain (Users, RefreshTokens)

**Özet:** Kimlik doğrulama şemasının çekirdeği — `Users` tablosu hem profil hem öğrenme istatistikleri hem OTP durumunu tek satırda tutar; `RefreshTokens` Token Family Pattern ile replay saldırılarını tespit eder. Bu domain [[Gelistirme_Yol_Haritasi]]'nde **A-03 (Auth API)** task'ında yazılacak, henüz kod yok.
**Kütüphaneler:** BCrypt.Net-Next (şifre hash), JWT (Microsoft.AspNetCore.Authentication.JwtBearer), System.IdentityModel.Tokens.Jwt, Google.Apis.Auth
**Bağlantılar:** [[Veritabani_Semasi]] · [[Guvenlik_Politikalari]] · [[Roller_ve_Erisim]] · [[BaseEntity]] · [[Loglama_Domain]] · [[Teknik_Ozellikler]] · [[Gelistirme_Kurulumu]]

## Users
Tek satırda: kimlik (Email/PasswordHash/GoogleId/AppleId/AuthProvider), profil (FirstName/LastName/
DisplayName/AvatarUrl), öğrenme hedefleri (DailyWordGoal/DailyNewWordLimit), istatistikler
(CurrentLevel A1-C2/TotalXP/LifetimeXP/StreakDays), tek-set OTP alanları
(PendingOtpCodeHash/ExpiresAt/Purpose — `EmailVerification|LoginOtp|PasswordReset|AccountDeletion`),
hesap durumu (IsActive/IsEmailVerified/LastLoginAt/LoginCount), hesap silme
(ScheduledDeletionAt/IsAnonymized/OriginalEmailHash — bkz. [[Guvenlik_Politikalari]] §9), push
(OneSignalPlayerId), rol (`User|Admin` — bkz. [[Roller_ve_Erisim]]).

## RefreshTokens
`TokenHash` (SHA-256), `TokenFamily` (GUID — replay tespiti), `ExpiresAt`, `IsUsed`, `RevokedAt`,
`DeviceInfo`, `IpAddress`. Her refresh'te rotation yapılır; aynı family'den ikinci kullanım =
replay → tüm family iptal edilir + `SecurityLog: TokenReplay` (bkz. [[Loglama_Domain]]).

## BaseEntity Audit FK'si
[[BaseEntity]]'ye A-02'de eklenen `CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId` (`int?`)
alanları, `Users` tablosu bu task'ta yazılınca EF config ile `Users(Id)`'ye FK olarak bağlanacak
(muhtemel `ON DELETE SET NULL`, [[Veritabani_Semasi]]'ndeki `Activity/Security` log FK deseniyle
tutarlı). `User` entity'sinin kendisi de `BaseEntity`'den türediği için kendine referans (self-FK)
imkanı vardır — örn. bir admin başka bir kullanıcıyı oluşturursa `CreatedByUserId` o admin'in Id'si
olur; self-servis kayıtta `null` kalır.

## Planlanan Kod (A-03)
`User`, `RefreshToken` entity + `OtpPurpose` enum → `IPasswordService`, `ITokenService`,
`IAuthService` (register/verify-email/login 2-adım OTP/google/apple/refresh/logout/
forgot-reset-password/delete-account) → `AuthController` (13 endpoint) — detay [[API_Sozlesmesi]]
ve `docs/REFERENCE/API_ENDPOINTS.md §3`.

### Referans Kod (henüz yazılmadı, `docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md §5-6`'dan)
- **`ITokenService`/`JwtTokenService`:** `GenerateAccessToken(User)` (claims: NameIdentifier/
  Email/Role/firstName, 15dk, HMAC-SHA256), `GenerateRefreshToken()` (64 byte random → Base64),
  `GetPrincipalFromExpiredToken(token)` — Algorithm Confusion önlemi: `Header.Alg != HmacSha256`
  ise `null` döner.
- **`IPasswordService`/`PasswordService`:** `Hash()` → `BCrypt.HashPassword(pw, workFactor: 12)`,
  `Verify()`, `HashToken()` → SHA-256 (refresh/OTP hash'i).
- **Google/Apple doğrulama:** `GoogleJsonWebSignature.ValidateAsync(idToken)` (backend yalnızca
  audience doğrular, client secret gerekmez); Apple identity token JWKS ile doğrulanır.

Tam kod → [[Teknik_Ozellikler]] §3-4, kurulum komutları → [[Gelistirme_Kurulumu]].
