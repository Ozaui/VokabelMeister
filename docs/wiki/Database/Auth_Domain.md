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

## Yazılmış Kod (A-03)
`User`, `RefreshToken` entity + `OtpPurpose` enum → `IPasswordService`, `ITokenService`,
`IOtpService`/`ILoginCompletionService` (paylaşılan OTP/giriş-tamamlama mantığı) → 13 Auth
Command+Handler'ı (MediatR CQRS, `Application/Features/Auth/`: register/verify-email/login
2-adım OTP/google/apple/refresh/logout/forgot-reset-password/delete-account) → `AuthController`
(13 endpoint, `IMediator.Send(command)` ile) — detay [[API_Sozlesmesi]] ve
`docs/REFERENCE/API_ENDPOINTS.md §3`, bkz. `API_YOL_HARITASI/A-03_auth-api.html`.

## QrLoginSessions (A-03.1 — planlı, henüz kod yok)
Steam benzeri "QR kod ile giriş": mobilde zaten giriş yapmış kullanıcı, web/masaüstünde gösterilen
QR'ı okutup onaylar. **Ayrı bir kimlik doğrulama sistemi değildir** — onaylanınca yukarıdaki
`ITokenService` çağrılır, `RefreshTokens`'a aynı şekilde yazılır. Alanlar: `QrTokenHash` (SHA-256,
`RefreshTokens.TokenHash` ile aynı desen), `PairingCode` (4 haneli, DB'den bağımsız relay/phishing
savunması — mobil onay ekranında gösterilip web ekranındakiyle gözle karşılaştırılır), `Status`
(`Pending→Scanned→Confirmed→Consumed` veya `Denied`/`Expired`). Detay → `DATABASE_SCHEMA/Auth.md`,
[[Guvenlik_Politikalari]].

## Apple Sosyal Giriş — Platformlar Arası Tutarlılık (not, kod değişikliği gerektirmiyor)
Apple `sub` (AppleId) client bazında (Bundle ID/Services ID) farklı üretilir. Mobil ve ileride
eklenecek web Apple girişi Apple Developer Console'da **gruplanmadan** (web Services ID'nin
Primary App ID'si mobil Bundle ID olarak seçilmeden) eklenirse, aynı kişi için iki ayrı hesap
açılır. Bu bir Apple Developer portal ayarıdır, `User`/`RefreshToken` şemasında hiçbir karşılığı
yok. Şu an web'de Apple girişi zaten **yok** (bkz. [[Sistem_Mimarisi]]); mobilde Apple ile kayıt
olan biri bugün web'e (a) `forgot-password` ile şifre belirleyip veya (b) QR ile giriş yapabilir.

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
