# GÜVENLİK POLİTİKALARI

Hedefler: Gizlilik, Bütünlük, Kullanılabilirlik. Uyum: OWASP Top 10, GDPR, KVKK.

## 1. Kimlik Doğrulama (JWT)

```
Access Token : 15 dakika  ·  Refresh Token: 7 gün  ·  Her refresh'te rotation
JWT: HMAC-SHA256, payload { sub, email, role, iat, exp, iss }
```
- **Algorithm Confusion önlemi:** Süresi dolmuş token okunurken `alg == HS256` doğrulanır (→ `REFERENCE/TECHNICAL_SPECIFICATIONS.md §5`).
- ASP.NET Identity KULLANILMAZ — JWT + şifre hashleme manuel.

### Şifre Kuralları
```
Min 12 karakter · ≥1 büyük · ≥1 küçük · ≥1 rakam · ≥1 özel (!@#$%^&*)
Hash: bcrypt work factor 12 (60 karakter çıktı, otomatik salt). Plaintext ASLA saklanmaz.
```

### Login Akışı (2 adımlı OTP / 2FA)
```
ADIM 1 — POST /auth/login { email, password }
   ├─ Rate limit: 5 hatalı/15dk → blok
   ├─ Timing attack önlemi: kullanıcı yoksa sahte BCrypt karşılaştırması (sabit süre)
   ├─ BCrypt doğrula (constant-time) + hesap durumu kontrolü
   ├─ OTP üret (6 hane, 5dk) → hash'i DB'ye (PendingOtpCodeHash), e-posta ile gönder
   └─ Yanıt: { message: "OTP gönderildi" }   ← token YOK
ADIM 2 — POST /auth/login/verify-otp { email, otpCode }
   ├─ OTP hash + süre + amaç (purpose) doğrula · 3 yanlış → kod geçersiz
   ├─ Grace period: hesap silinmiş ama 30 gün içindeyse otomatik kurtar (accountWasRecovered)
   ├─ IsAnonymized → 403 (kalıcı silindi)
   ├─ Access + Refresh token üret; refresh hash'i DB'ye; LastLoginAt güncelle
   └─ Yanıt: { accessToken, refreshToken, user, accountWasRecovered }
```

### Refresh (Token Family Pattern)
Eski refresh tek kullanımlık → kullanılınca geçersiz. Aynı family'den ikinci kullanım = **replay** →
tüm family iptal (`SecurityLog: TokenReplay`).

### 1.2 Sosyal Giriş (Google/Apple) — Platformlar Arası Tutarlılık

Google/Apple girişinde `PasswordHash` NULL kalır; `AuthProvider` ilgili değere set edilir, `GoogleId`/
`AppleId` benzersiz kullanıcı kimliğini tutar.

**Apple'a özgü risk:** Apple, kullanıcı kimliğini (`sub`) **client bazında** (Bundle ID/Services ID)
farklı üretir. Mobil uygulama (Bundle ID) ile ileride eklenecek web (Services ID) gruplanmazsa, aynı
gerçek kişi için Apple iki farklı `sub` döner → sistemde yanlışlıkla iki ayrı hesap açılır.
**Önlem (kod değil, Apple Developer Console ayarı):** Web için Services ID oluşturulurken
"Sign in with Apple → Configure" adımında mobil uygulamanın App ID'si **Primary App ID** olarak
seçilmeli (grouping) — bu adım atılmadan web'e Apple girişi asla eklenmemeli. Şu an
(`REFERENCE/ARCHITECTURE.md §1`) Apple girişi **kasıtlı olarak yalnızca mobilde** var; bu grouping
işlemi yalnızca web/masaüstüne Apple girişi eklendiğinde gerekli olacak, bugün yapılacak bir kod
değişikliği yok.

**Peki mobilde Apple ile kayıt olan biri bugün web'e nasıl girer?** İki yol:
1. `forgot-password` akışı (`§7`) `PasswordHash`'in önceden var olup olmadığına bakmaz — sosyal
   girişli bir hesap da OTP doğrulayıp yeni şifre belirleyebilir, ardından web'de e-posta+şifre ile
   girer. **Bu davranış kasıtlıdır** — ileride biri "AuthProvider != Local ise forgot-password'u
   reddet" gibi bir kısıtlama eklemeye kalkarsa bu köprü kırılır, eklenmemeli.
2. Aşağıdaki **QR Kod ile Giriş** — hiç şifre belirlemeden, zaten mobilde açık olan oturumla web'e girer.

### 1.3 QR Kod ile Giriş (Steam benzeri — "aynı token sistemine bağlanan yeni bir giriş yöntemi")

QR girişi ayrı bir kimlik doğrulama mekanizması **DEĞİLDİR** — yalnızca kullanıcının kimliğini
kanıtlama yöntemidir (şifre/OTP yerine "zaten mobilde giriş yapmış olmak"). Onaylandığında normal
login'deki **AYNI** `ITokenService`/`RefreshTokens` akışı çalışır (Token Family Pattern dahil).

```
ADIM 1 — POST /auth/qr/generate (Anonim, web/masaüstü çağırır)
   ├─ Rastgele token (RefreshToken üretimiyle aynı yöntem) + SHA-256 hash'i DB'ye
   ├─ 4 haneli PairingCode üretilir (kullanıcı gözle karşılaştırsın diye)
   ├─ ExpiresAt = +2 dakika · Rate limit: IP başına 20/saat
   └─ Yanıt: { qrToken, pairingCode, expiresIn: 120 }  (qrToken QR görsel/deep-link içine gömülür, DB'de yalnızca hash'i durur)

ADIM 2 — Mobil kamerayla QR'ı okur → POST /auth/qr/{token}/scan  [Authorize]
   ├─ Token hash + süre doğrula · Status Pending değilse 410 Gone
   ├─ Status → Scanned, UserId = mevcut JWT'deki kullanıcı, ScannedAt
   └─ Yanıt: { requesterDeviceInfo, requesterIp, pairingCode } ← mobil ekranda gösterilir, kullanıcı web ekranındakiyle KARŞILAŞTIRIR

ADIM 3a — Kullanıcı mobilde onaylar → POST /auth/qr/{token}/confirm  [Authorize]
   ├─ Bu QR'ın Status=Scanned + UserId = çağıran kullanıcı mı doğrula
   └─ Status → Confirmed, ConfirmedAt · SecurityLog: QrLoginConfirmed
ADIM 3b — Kullanıcı reddeder → POST /auth/qr/{token}/deny  [Authorize]
   └─ Status → Denied · SecurityLog: QrLoginDenied (tekrarlıysa phishing sinyali)

ADIM 4 — Web taraf ~2 saniyede bir GET /auth/qr/{token}/status (Anonim, polling)
   ├─ Status='Confirmed' İLK okunduğunda: normal login'deki AYNI TokenService ile access+refresh
   │  token üretilir, RefreshTokens tablosuna yazılır — QR girişi burada normal login'le birleşir,
   │  ayrı bir token mekanizması YOKTUR
   ├─ Bu okumadan sonra Status → Consumed (token'lar yalnızca BİR kez döner)
   └─ Consumed sonrası tekrar sorgulanırsa 410 Gone
```
**Güvenlik notları:**
- `QrTokenHash` DB sızıntısına karşı (ham token asla saklanmaz — `PasswordService.HashToken` ile aynı yöntem).
- `PairingCode`, DB sızıntısından bağımsız bir savunma hattı: saldırgan kendi ürettiği bir QR'ı
  kurbana okutup (relay/phishing saldırısı) oturumu ele geçirmeye çalışırsa, web ekranındaki kod
  mobildeki onay ekranında gösterilenle **eşleşmez** — kullanıcı fark edip reddeder.
- Onay **otomatik değildir**: tarama (scan) ile onay (confirm) iki ayrı adımdır; kullanıcı cihaz/IP
  bilgisini görüp bilinçli onaylar (Steam mobil uygulamasının kullandığı desen).
- `SecurityLog` yeni event tipleri: `QrLoginConfirmed`, `QrLoginDenied` (bkz. `§6`).

## 2. Yetkilendirme (RBAC)

İki rol: `User` (varsayılan, herkes) ve `Admin` (elle atanır). Hiçbir public endpoint rol yükseltemez.

```csharp
[Authorize(Roles = "Admin")] [HttpPost("/api/v1/words")]   // sistem kelimesi CRUD
[Authorize]                   [HttpGet("/api/v1/words")]    // okuma — tüm kullanıcılar
```
**Kaynak yetkisi:** Kullanıcı yalnızca kendi kaydına erişir — servis/repo `UserId` filtresi; başkasının
kaydı 404/403. (→ `REFERENCE/ARCHITECTURE.md §4`)

## 3. Şifreleme

### 3.1 Transit ve At Rest

- **Transit:** TLS 1.3 (min). HSTS: `max-age=31536000; includeSubDomains; preload`.
- **At rest:** MSSQL TDE (AES-256) önerilir. Şifreler bcrypt hash; düz metin yok.

### 3.2 SMTP Kimlik Bilgisi (AES-256)
SMTP şifresi **DB'de AES-256-CBC şifreli**; `appsettings.json`/kaynak koda **asla** yazılmaz.
```
SmtpSettings.PasswordEncrypted = Base64(IV + cipher)   // IV her şifrelemede rastgele 16 byte
Anahtar: AES_ENCRYPTION_KEY (32 byte, Base64) → ortam değişkeni (DB'de DEĞİL)
```
- Anahtar tam 32 byte değilse exception. `GET /admin/smtp-settings` şifreyi `***` maskeleyerek döner.
- `PUT` → `IEncryptionService.Encrypt()` ile şifreler. Endpoint `[Authorize(Roles="Admin")]`.

## 4. Girdi Doğrulama & Çıktı Kodlama

- **Sunucu tarafı doğrulama her zaman** (client doğrulamasına güvenme) — FluentValidation.
- **SQL injection:** Parametreli sorgu / EF Core LINQ. String birleştirme ile SQL **yasak**.
- **XSS:** Çıktı kodlama; JSON yanıtlar otomatik encode.
- **Rate limiting:** Login 5/15dk; OTP 3 yanlış → geçersiz; genel 100/dk (auth), 10/dk (anonim).

## 5. API Güvenlik Başlıkları (middleware)
```
X-Frame-Options: DENY
X-Content-Type-Options: nosniff
Referrer-Policy: strict-origin-when-cross-origin
Content-Security-Policy: default-src 'self'; ...
Permissions-Policy: geolocation=(), microphone=(), camera=()
```
**CORS:** Yalnızca tanımlı origin'ler (`Cors__AllowedOrigins`), `*` yok.

## 6. Loglama & İzleme

Üç DB tablosu (→ `DATABASE_SCHEMA/Loglama.md`), hepsi admin panelden görüntülenir (`GET /admin/logs/*`):

| Tablo | Kaynak | Ne loglanır |
|-------|--------|-------------|
| `ApplicationLog` | Serilog (`_logger`) + MSSqlServer sink | Hata/uyarı/info (teknik) — konsol + dosya + DB |
| `ActivityLog` | `IActivityLogger` servisi | Audit: login, register, kelime/kart oluştur-sil, rol değiştir, hesap dondur (old/new JSON) |
| `SecurityLog` | `ISecurityLogger` servisi | LoginFailed, OtpFailed, RateLimitHit, UnauthorizedAccess, TokenReplay, QrLoginConfirmed, QrLoginDenied |

**PII kuralı:** Loglarda ham e-posta saklanmaz → `SHA-256(email)` (EmailHash). Şifre/token asla loglanmaz.
Gerçek zamanlı uyarı gereken olaylar (çoklu başarısız giriş, admin erişimi) ayrıca Serilog warning'i tetikleyebilir.

## 7. Şifre Yönetimi

### Sıfırlama (OTP tabanlı)
```
ADIM 1 — POST /auth/forgot-password { email }
   └─ Kullanıcı yoksa bile 200 (e-posta numaralandırma saldırısını önler) · OTP 5dk
ADIM 2 — POST /auth/reset-password { email, otpCode, newPassword }
   ├─ OTP hash + süre + amaç doğrula · yeni şifre gücü
   ├─ BCrypt hash · OTP alanlarını temizle
   └─ Tüm refresh token'ları iptal (tüm cihazlardan çıkış) + "şifre değişti" e-postası
```
Şifre değiştirme (giriş gerekli): mevcut şifre doğrulanır, yenisi farklı olmalı, tüm cihazlardan çıkış.

## 8. Mobil Güvenlik
- Token: `expo-secure-store` (iOS Keychain / Android Keystore). Web: `localStorage` (CSP ile XSS azaltılır).
- SSL pinning (opsiyonel), jailbreak/root tespiti (opsiyonel).

## 9. Hesap Silme (GDPR/KVKK)
- Genel soft delete (kelime/kart/kategori) sorunsuz — PII yok.
- **Hesap silmede:** soft delete + 30 gün grace → `AccountCleanupBackgroundService` PII anonimleştirir:
  `Email→deleted_{id}@deleted.invalid`, ad→"Silindi", PasswordHash/GoogleId/AppleId→null,
  `OriginalEmailHash=SHA-256(email)` (silinen e-posta ile tekrar kaydı blokla), `IsAnonymized=true`.

## 10. Deployment Checklist
```
☐ HTTPS/TLS · güvenlik başlıkları · CORS tanımlı origin
☐ JWT/DB/AES anahtarları ortam değişkeninde (kodda değil — REFERENCE/ENV.md)
☐ Rate limiting açık · loglar PII içermiyor (hash)
☐ Dependency güvenlik taraması · DB backup (AES-256, test edilmiş)
☐ GDPR: anonimleştirme görevi prod'da çalışıyor · OriginalEmailHash blok testi
☐ Log saklama politikası dokümante (kaç gün ApplicationLog/SecurityLog)
```
