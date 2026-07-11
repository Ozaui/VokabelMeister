# Auth Domain (Users, RefreshTokens)

**Özet:** Kimlik doğrulama şemasının çekirdeği — `Users` tablosu hem profil hem öğrenme istatistikleri hem OTP durumunu tek satırda tutar; `RefreshTokens` Token Family Pattern ile replay saldırılarını tespit eder; `QrLoginSessions` (A-03.1) aynı token akışını QR ile tetikler. **A-03 ✅ ve A-03.1 ✅ tamamlandı** — bu domain artık gerçek kodda mevcut.
**Kütüphaneler:** BCrypt.Net-Next 4.0.3 (şifre hash, aktif), Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0 (aktif), System.IdentityModel.Tokens.Jwt 7.1.0 (aktif), Google.Apis.Auth 1.67.0 (aktif)
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

**Düzeltme (2026-07-11):** A-03/A-03.1 ilk yazıldığında bu alanlar hiçbir Auth/QrLogin
Handler'ında fiilen doldurulmuyordu (22 `AddAsync`/`UpdateAsync` çağrısının tamamı `userId`
parametresini boş bırakıyordu — kod denetiminde bulundu). Artık kural netleşti ve uygulandı:
kaydın SAHİBİ kendi eylemiyle güncelliyorsa (LoginCommand'ın OTP üretmesi, ResetPasswordCommand,
QR Scan/Confirm/Deny/GetStatus vb.) o kullanıcının Id'si geçiliyor; yalnızca gerçek self-servis
KAYIT OLUŞTURMA (RegisterCommand, LoginWithGoogle/AppleCommand'ın yeni-kullanıcı dalı,
GenerateQrLoginCommand) ve sistemin kendiliğinden yaptığı geçişlerde (ör. QR oturumunun otomatik
Expired'a çevrilmesi) `null` kalıyor. Detay → `IRepository.cs` NEDEN yorumu.

## Yazılmış Kod (A-03 ✅ + A-03.1 ✅ — ikisi de tamamlandı)
`User`, `RefreshToken` entity + `OtpPurpose` enum → `IPasswordService`, `ITokenService`,
`IOtpService`/`ILoginCompletionService` (paylaşılan OTP/giriş-tamamlama mantığı) → 13 Auth
Command+Handler'ı (MediatR CQRS, `Application/Features/Auth/`: register/verify-email/login
2-adım OTP/google/apple/refresh/logout/forgot-reset-password/delete-account) → `AuthController`
(13 endpoint, `IMediator.Send(command)` ile) — detay [[API_Sozlesmesi]] ve
`docs/REFERENCE/API_ENDPOINTS.md §3`, bkz. `API_YOL_HARITASI/A-03_auth-api.html`.

## QrLoginSessions (A-03.1 ✅ tamamlandı)
Steam benzeri "QR kod ile giriş": mobilde zaten giriş yapmış kullanıcı, web/masaüstünde gösterilen
QR'ı okutup onaylar. **Ayrı bir kimlik doğrulama sistemi değildir** — onaylanınca yukarıdaki
**aynı** `ILoginCompletionService.CompleteLoginAsync` çağrılır, `RefreshTokens`'a aynı şekilde
yazılır. Alanlar: `QrTokenHash` (SHA-256, `RefreshTokens.TokenHash` ile aynı desen), `PairingCode`
(4 haneli, DB'den bağımsız relay/phishing savunması — mobil onay ekranında gösterilip web
ekranındakiyle gözle karşılaştırılır), `Status` (`Pending→Scanned→Confirmed→Consumed` veya
`Denied`/`Expired`).
**Yazılmış kod:** `IQrLoginSessionRepository`/`QrLoginSessionRepository`, `QrSessionGoneException`
(410)/`QrSessionForbiddenException`(403) (ikisi de [[AppException]]'dan türer), paylaşılan
`QrLoginSessionExpiryExtensions` (lazy expire — ayrı temizlik job'ı yok), 5 MediatR Command+Handler
(`Application/Features/QrLogin/`: Generate/Scan/Confirm/Deny/GetStatus), `QrLoginController`
(5 endpoint, `/auth/qr/*` — `AuthController`'dan ayrı, çünkü Admin panelde bu akış yok), IP-partitioned
`qrGenerate` rate limit policy'si (20/saat). **Tasarım kararı:** `RequesterIp`/`RequesterDeviceInfo`
yalnızca `generate` adımında (web'in isteğinden) yazılır, `scan`'de değil — mobil ekranda "seni
İSTEYEN taraf" gösterilip kullanıcı gözle doğrular (relay/phishing önlemi). 23 birim test (18
orijinal + 2026-07-11 bugfix turunda 5 yeni). Detay →
`DATABASE_SCHEMA/Auth.md`, [[Guvenlik_Politikalari]], `API_YOL_HARITASI/A-03.1_qr-login.html`.

**Bugfix turu (2026-07-11, kod denetimi sonrası):** Dört gerçek sorun düzeltildi — (1)
`GET /auth/qr/{token}/status` (web'in ~2sn'de bir sorguladığı polling endpoint'i) paylaşımlı
`"anonymous"` rate-limit'ini (10/dk, TÜM anonim trafik ortak) kullanıyordu; bu polling hızı
(~30/dk) o bütçeyi saniyeler içinde tüketip register/login/forgot-password dahil TÜM anonim
trafiği kilitliyordu — yeni IP-partitioned `"qrStatus"` policy'si (40/dk/IP) eklendi (bkz.
[[Guvenlik_Politikalari]] "Güvenlik Başlıkları" altındaki rate limit notu, `REFERENCE/SECURITY.md
§4`). (2) `GetQrLoginStatusCommand` kullanıcıyı soft-delete filtresi UYGULANARAK (`GetByIdAsync`)
buluyordu, oysa normal login `GetByEmailAsync` ile filtresiz buluyor — hesabını yeni silmiş bir
kullanıcı QR ile giriş tamamlamaya çalışınca anlamsız bir 404 alıyordu; `IUserRepository`'ye
`GetByIdIncludingDeletedAsync` eklendi, artık `ILoginCompletionService.CompleteLoginAsync`'in
grace-period kurtarma mantığına normal login ile aynı şekilde ulaşıyor. Ayrıca diğer giriş
yollarıyla (Login/Google/Apple) parite için `IsActive` kontrolü eklendi. (3) Scan/Confirm/Deny/
GetStatus'ta oturum bulunamadığında fırlatılan `EntityNotFoundException`'ın mesajına ham QR
token'ı (bir secret) gömülüyordu — artık hash'i gömülüyor.

**Kod kalitesi turu (2026-07-11, aynı gün, ayrı denetim):** Üç küçük düzeltme daha: (1)
`PendingOtpCodeHash`/`OriginalEmailHash`/`TokenHash`/`QrTokenHash` kolonları `MaxLength(88)` idi —
`PasswordService.HashToken` (SHA-256→Base64) her zaman sabit **44** karakter üretir, 88 muhtemelen
SHA-512 ile karışıklıktan kaynaklanan bir hataydı (veri kaybı yoktu, yalnızca şema kendi verisini
yanlış belgeliyordu) — `FixHashColumnMaxLength` migration'ıyla düzeltildi. (2) `ConfirmQrLoginCommandHandler`/
`DenyQrLoginCommandHandler` neredeyse birebir kopya kod taşıyordu (hash arama + expiry + Scanned +
sahiplik kontrolü) — ortak mantık `QrLoginSessionOwnershipHelper.LoadScannedOwnedSessionAsync`'e
çıkarıldı (`QrLoginSessionExpiryExtensions`'ın yanına, aynı "internal static helper" deseniyle;
DI değişmedi, yalnızca handler gövdeleri sadeleşti). (3) `Repository<T>.UpdateAsync`'teki gereksiz
`_set.Update(entity)` çağrısı kaldırıldı (entity zaten tracked, bkz. [[Repository]]).

## Apple Sosyal Giriş — Platformlar Arası Tutarlılık (not, kod değişikliği gerektirmiyor)
Apple `sub` (AppleId) client bazında (Bundle ID/Services ID) farklı üretilir. Mobil ve ileride
eklenecek web Apple girişi Apple Developer Console'da **gruplanmadan** (web Services ID'nin
Primary App ID'si mobil Bundle ID olarak seçilmeden) eklenirse, aynı kişi için iki ayrı hesap
açılır. Bu bir Apple Developer portal ayarıdır, `User`/`RefreshToken` şemasında hiçbir karşılığı
yok. Şu an web'de Apple girişi zaten **yok** (bkz. [[Sistem_Mimarisi]]); mobilde Apple ile kayıt
olan biri bugün web'e (a) `forgot-password` ile şifre belirleyip veya (b) QR ile giriş yapabilir.

### Referans Kod (yazıldı — A-03, `docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md §5-6`'dan)
- **`ITokenService`/`JwtTokenService`:** `GenerateAccessToken(User)` (claims: NameIdentifier/
  Email/Role/firstName, 15dk, HMAC-SHA256), `GenerateRefreshToken()` (64 byte random → Base64),
  `GetPrincipalFromExpiredToken(token)` — Algorithm Confusion önlemi: `Header.Alg != HmacSha256`
  ise `null` döner.
- **`IPasswordService`/`PasswordService`:** `Hash()` → `BCrypt.HashPassword(pw, workFactor: 12)`,
  `Verify()`, `HashToken()` → SHA-256 (refresh/OTP hash'i).
- **Google/Apple doğrulama:** `GoogleJsonWebSignature.ValidateAsync(idToken)` (backend yalnızca
  audience doğrular, client secret gerekmez); Apple identity token JWKS ile doğrulanır.

Tam kod → [[Teknik_Ozellikler]] §3-4, kurulum komutları → [[Gelistirme_Kurulumu]].
