# Güvenlik Politikaları

**Özet:** OWASP Top 10 + GDPR/KVKK uyumlu bir güvenlik modeli — JWT tabanlı 2 adımlı OTP girişi, Token Family Pattern ile refresh replay tespiti, bcrypt (wf:12) şifre hash'i, AES-256 ile SMTP kimlik bilgisi şifreleme ve PII'siz loglama (e-posta yerine SHA-256 hash). ASP.NET Identity **kullanılmıyor** — her şey manuel yazılıyor.
**Kütüphaneler:** Microsoft.AspNetCore.Authentication.JwtBearer (✅ kurulu ve kullanımda), BCrypt.Net-Next (✅ kurulu ve kullanımda, A-03), AES-256-CBC (planlı `IEncryptionService` — SMTP şifreleme A-10'da yazılacak, henüz yok)
**Bağlantılar:** [[Auth_Domain]] · [[Roller_ve_Erisim]] · [[Loglama_Domain]] · [[Ortam_Degiskenleri]] · [[API_Sozlesmesi]]

## Kimlik Doğrulama (JWT)
```
Access Token : 15 dakika  ·  Refresh Token: 7 gün  ·  Her refresh'te rotation
JWT: HMAC-SHA256, payload { sub, email, role, iat, exp, iss }
```
Algorithm Confusion önlemi: süresi dolmuş token okunurken `alg == HS256` doğrulanır.

### Şifre Kuralları
Min 12 karakter, ≥1 büyük/küçük/rakam/özel karakter. Hash: bcrypt work factor 12 (60 karakter,
otomatik salt). Plaintext asla saklanmaz.

### Login Akışı (2 adımlı OTP)
Adım 1 (`/auth/login`): şifre doğrula (timing-attack önlemi: kullanıcı yoksa sahte bcrypt
karşılaştırması) → OTP üret (6 hane, 5dk) → e-posta gönder → token **yok**.
Adım 2 (`/auth/login/verify-otp`): OTP doğrula (3 yanlış → geçersiz) → grace period kontrolü
(30 gün içinde silinmişse otomatik kurtar) → access+refresh token üret.

### Refresh (Token Family Pattern)
Eski refresh tek kullanımlık; aynı family'den ikinci kullanım = **replay** → tüm family iptal +
`SecurityLog: TokenReplay` (bkz. [[Loglama_Domain]]).

### Sosyal Giriş — Apple Platformlar Arası Tutarlılık (not)
Apple `sub` client bazlı üretilir; web'e Apple girişi eklenirse Apple Developer Console'da web
Services ID'si mobil Bundle ID'ye **gruplanmalı** (Primary App ID), yoksa aynı kişi için iki ayrı
hesap açılır. Bugün web'de Apple girişi yok, bu yüzden şimdilik yapılacak kod yok — detay [[Auth_Domain]].

### QR Kod ile Giriş (Steam benzeri, A-03.1 — ✅ tamamlandı)
Ayrı bir kimlik doğrulama sistemi **değil** — zaten mobilde giriş yapmış kullanıcının web/masaüstü
oturumunu onaylamasıdır; onaylanınca yukarıdaki **aynı** `ILoginCompletionService.CompleteLoginAsync`
çağrılır, `RefreshTokens`'a aynı şekilde yazılır. `QrTokenHash` (SHA-256, ham token DB'de tutulmaz)
+ `PairingCode` (4 haneli, kullanıcı gözle karşılaştırır — relay/phishing savunması, DB
sızıntısından bağımsız). Süre: 2dk, rate limit: `generate` IP başına 20/saat, `status` (polling)
IP başına 40/dk — ikisi de IP-partitioned (paylaşımlı anonim/authenticated bütçesini kullanmaz;
2026-07-11'de `status` polling'in paylaşımlı 10/dk bütçesini tüketip TÜM anonim trafiği kilitlediği
bulunup düzeltildi). `SecurityLog: QrLoginConfirmed/QrLoginDenied` (A-04'te SecurityLog yazılınca
bağlanacak — bkz. [[Loglama_Domain]]). Detay → [[Auth_Domain]].

## Yetkilendirme (RBAC)
İki rol: `User`/`Admin`. `[Authorize(Roles="Admin")]` sistem içeriği CRUD'unda, `[Authorize]`
okuma/genel erişimde. Kaynak yetkisi: `UserId` filtresiyle — detay [[Roller_ve_Erisim]].

## Şifreleme
- **Transit:** TLS 1.3 min, HSTS.
- **At rest:** MSSQL TDE önerilir; şifreler bcrypt.
- **SMTP kimlik bilgisi (§3.4):** `SmtpSettings.PasswordEncrypted = Base64(IV + AES-256-CBC cipher)`,
  anahtar `AES_ENCRYPTION_KEY` (32 byte) → **ortam değişkeni**, DB'de değil (bkz. [[Ortam_Degiskenleri]]).
  `GET /admin/smtp-settings` şifreyi `***` maskeler.

## Girdi Doğrulama & Rate Limiting
FluentValidation (sunucu tarafı her zaman) · parametreli sorgu/EF Core LINQ (SQL injection yasak) ·
JSON otomatik encode (XSS) · Login 5/15dk, OTP 3 yanlış, genel 100/dk (auth)/10/dk (anonim).

## Güvenlik Başlıkları (middleware, [[Program_cs]]'e eklendi ✅ — A-02, `SecurityHeadersMiddleware`)
```
X-Frame-Options: DENY
X-Content-Type-Options: nosniff
Referrer-Policy: strict-origin-when-cross-origin
Content-Security-Policy: default-src 'self'; ...
Permissions-Policy: geolocation=(), microphone=(), camera=()
```
CORS: yalnızca tanımlı origin'ler, `*` yok.

## Loglama & PII Kuralı
Üç tablo → [[Loglama_Domain]]. Loglarda ham e-posta **asla** saklanmaz — `SHA-256(email)`.
Şifre/token asla loglanmaz.

## Hesap Silme (GDPR/KVKK)
Soft delete + 30 gün grace → `AccountCleanupBackgroundService` (A-10) PII anonimleştirir:
`Email→deleted_{id}@deleted.invalid`, ad→"Silindi", `OriginalEmailHash` ile aynı e-postayla
tekrar kayıt bloklanır, `IsAnonymized=true`.
