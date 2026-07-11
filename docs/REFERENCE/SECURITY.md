# GÜVENLİK POLİTİKALARI

> Hedefler: Gizlilik, Bütünlük, Kullanılabilirlik. Uyum: OWASP Top 10, GDPR, KVKK. Roller → `CLAUDE.md §1`.

## 1. Kimlik Doğrulama (JWT)

```
Access 15dk · Refresh 7gün · her refresh'te rotation
JWT: HMAC-SHA256, payload { sub, email, role, iat, exp, iss }
```
- **Algorithm Confusion önlemi:** süresi dolmuş token okunurken `alg == HS256` doğrulanır (`TECHNICAL_SPECIFICATIONS.md §5`).
- ASP.NET Identity kullanılmaz.

**Şifre:** Min 12 karakter · ≥1 büyük/küçük/rakam/özel (`!@#$%^&*`). Hash bcrypt wf=12 (60 karakter, otomatik salt). Plaintext asla saklanmaz.

**Login (2 adımlı OTP):**
```
ADIM 1 — POST /auth/login { email, password }
   ├─ Rate limit 5 hatalı/15dk → blok
   ├─ Timing attack önlemi: kullanıcı yoksa sahte BCrypt karşılaştırması (sabit süre)
   ├─ BCrypt doğrula + hesap durumu kontrolü
   ├─ OTP (6 hane, 5dk) → hash DB'ye (PendingOtpCodeHash), e-posta ile gönder
   └─ Yanıt: { message: "OTP gönderildi" }   ← token YOK
ADIM 2 — POST /auth/login/verify-otp { email, otpCode }
   ├─ OTP hash+süre+purpose doğrula · 3 yanlış → kod geçersiz
   ├─ Grace period: silinmiş ama 30 gün içindeyse otomatik kurtar (accountWasRecovered)
   ├─ IsAnonymized → 403 (kalıcı silindi)
   ├─ Access+Refresh üret; refresh hash DB'ye; LastLoginAt güncelle
   └─ Yanıt: { accessToken, refreshToken, user, accountWasRecovered }
```

**Refresh (Token Family Pattern):** Eski refresh tek kullanımlık. Aynı family'den ikinci kullanım = replay → tüm family iptal (`SecurityLog: TokenReplay`).

### 1.2 Sosyal Giriş (Google/Apple)

`PasswordHash` NULL, `AuthProvider` set, `GoogleId`/`AppleId` benzersiz kimlik tutar.

**Apple riski:** Apple `sub`'ı client bazında (Bundle ID/Services ID) farklı üretir. Mobil (Bundle ID) ile ileride eklenecek web (Services ID) gruplanmazsa aynı kişi için iki `sub` → iki hesap. **Önlem (kod değil, Apple Console ayarı):** web Services ID'sinde mobil App ID'si "Primary App ID" seçilmeli. Şu an Apple **yalnızca mobilde** (kasıtlı); bu grouping yalnızca web'e Apple eklendiğinde gerekli.

**Mobilde Apple ile kayıt olan web'e nasıl girer?** (1) `forgot-password` — `PasswordHash`'in var olup olmadığına bakmaz, sosyal hesap da OTP'yle şifre belirleyebilir (**kasıtlı**, "AuthProvider != Local ise reddet" kısıtı eklenmemeli). (2) QR ile giriş (aşağıda).

### 1.3 QR Kod ile Giriş (Steam benzeri)

Ayrı bir kimlik doğrulama mekanizması **değildir** — kimliği kanıtlama yöntemidir ("zaten mobilde giriş yapmış olmak"). Onaylanınca normal login'deki **AYNI** `ITokenService`/`RefreshTokens` akışı çalışır.

```
ADIM 1 — POST /auth/qr/generate (Anonim, web çağırır)
   ├─ Rastgele token + SHA-256 hash DB'ye · 4 haneli PairingCode · ExpiresAt=+2dk · Rate limit 20/saat/IP
   └─ Yanıt: { qrToken, pairingCode, expiresIn: 120 }  (qrToken QR/deep-link içinde, DB'de yalnızca hash)
ADIM 2 — Mobil okur → POST /auth/qr/{token}/scan [Authorize]
   ├─ Hash+süre doğrula · Status≠Pending → 410 Gone
   ├─ Status→Scanned, UserId=mevcut JWT
   └─ Yanıt: { requesterDeviceInfo, requesterIp, pairingCode } ← mobil ekranda gösterilir, web ile KARŞILAŞTIRILIR
ADIM 3a — POST /auth/qr/{token}/confirm [Authorize] → Status=Scanned+UserId doğrula → Confirmed · SecurityLog: QrLoginConfirmed
ADIM 3b — POST /auth/qr/{token}/deny [Authorize] → Denied · SecurityLog: QrLoginDenied
ADIM 4 — Web ~2sn'de GET /auth/qr/{token}/status (Anonim, polling)
   ├─ 'Confirmed' İLK okunduğunda: AYNI TokenService ile access+refresh üretilir, RefreshTokens'a yazılır
   ├─ Sonra Status→Consumed (token'lar yalnızca BİR kez döner)
   └─ Consumed sonrası → 410 Gone
```
**Güvenlik:** `QrTokenHash` DB sızıntısına karşı (ham token saklanmaz). `PairingCode` relay/phishing'e karşı (web'deki kod mobildekiyle eşleşmezse kullanıcı reddeder). Onay otomatik değil: scan≠confirm, kullanıcı cihaz/IP görüp bilinçli onaylar.

### 1.4 Hata Mesajlarında Dil Desteği (Code + Sözlük)

Bilinen iş hataları tek taban sınıftan türer: `AppException` — yalnızca `Code` taşır (ör. `INVALID_CREDENTIALS`), mesajı içinde sabitlemez.
```
1. throw new InvalidCredentialsException()  (Code=INVALID_CREDENTIALS)
2. ExceptionHandlingMiddleware → HTTP kodunu Code'a göre eşler
3. Accept-Language'dan dili çıkar (ör. "en-US"→"en")
4. ErrorMessages.Resolve(code, dil) → tr/de metin (yoksa tr'ye düşer; yeni dil = sözlüğe sütun)
5. İstemciye: { "error": { "code": "...", "message": "<dile göre>" } }
```
- `AppException.Message` (.NET `.Message`) → yalnızca log/geliştirici, daima Türkçe, istemciye gitmez.
- `ErrorMessages.Resolve()` → istemciye giden, dile göre değişen metin.
- `EntityNotFoundException` bilinçli olarak `AppException`'dan türemez (mesajı dinamik veri içerir); 404 için `ex.Message` (yalnızca Türkçe).
- Yeni dil = `ErrorMessages.cs` sözlüğüne sütun; exception sınıflarına dokunulmaz.

## 2. Yetkilendirme (RBAC)

İki rol (`CLAUDE.md §1`). Sistem CRUD `[Authorize(Roles="Admin")]`, okuma `[Authorize]`. Kaynak yetkisi: `UserId` filtresi, başkasının kaydı 404/403.

## 3. Şifreleme

**Transit:** TLS 1.3 min. HSTS `max-age=31536000; includeSubDomains; preload`. **At rest:** MSSQL TDE (AES-256) önerilir; şifreler bcrypt.

**3.2 SMTP (AES-256-CBC):** SMTP şifresi DB'de `Base64(IV + cipher)` (IV her seferinde rastgele 16 byte); appsettings/kaynak koda asla yazılmaz. Anahtar `AES_ENCRYPTION_KEY` (32 byte, Base64) → ENV (DB'de değil). Anahtar ≠32 byte → exception. `GET /admin/smtp-settings` şifreyi `***` maskeler. `PUT` → `IEncryptionService.Encrypt()`. Endpoint `[Authorize(Roles="Admin")]`.

## 4. Girdi Doğrulama & Çıktı Kodlama

Sunucu tarafı doğrulama her zaman (FluentValidation). SQL injection: parametreli / EF LINQ (string birleştirme yasak). XSS: JSON otomatik encode. Rate limiting: Login 5/15dk; OTP 3 yanlış→geçersiz; genel 100/dk (auth), 10/dk (anonim). QR akışı iki AYRI, IP-partitioned policy kullanır (paylaşımlı "anonim" 10/dk bütçesini kullanmaz — bkz. §1.3): `qrGenerate` 20/saat/IP, `qrStatus` 40/dk/IP (web'in ~2sn'de bir sorguladığı polling endpoint'i için — paylaşımlı bütçe kullansaydı bu polling tek başına tüm anonim trafiği kilitlerdi).

## 5. API Güvenlik Başlıkları (middleware)

`X-Frame-Options: DENY` · `X-Content-Type-Options: nosniff` · `Referrer-Policy: strict-origin-when-cross-origin` · `Content-Security-Policy: default-src 'self'; ...` · `Permissions-Policy: geolocation=(), microphone=(), camera=()`. **CORS:** yalnızca tanımlı origin'ler, `*` yok.

## 6. Loglama & İzleme

Üç DB tablosu (`DATABASE_SCHEMA/Loglama.md`), hepsi `GET /admin/logs/*`:

| Tablo | Kaynak | Ne |
|-------|--------|----|
| `ApplicationLog` | Serilog + MSSqlServer | Hata/uyarı/info |
| `ActivityLog` | `IActivityLogger` | login, register, kelime/kart oluştur-sil, rol değiştir (old/new JSON) |
| `SecurityLog` | `ISecurityLogger` | LoginFailed, OtpFailed, RateLimitHit, UnauthorizedAccess, TokenReplay, QrLoginConfirmed/Denied |

**PII:** ham e-posta yerine `SHA-256(email)`; şifre/token asla loglanmaz.

## 7. Şifre Yönetimi

**Sıfırlama (OTP):**
```
ADIM 1 — POST /auth/forgot-password { email } → kullanıcı yoksa bile 200 (enumerasyon önlemi) · OTP 5dk
ADIM 2 — POST /auth/reset-password { email, otpCode, newPassword }
   ├─ OTP doğrula + şifre gücü · BCrypt hash · OTP temizle
   └─ Tüm refresh token iptal (tüm cihazlardan çıkış) + "şifre değişti" e-postası
```
Şifre değiştirme (giriş gerekli): mevcut şifre doğrulanır, yenisi farklı olmalı, tüm cihazlardan çıkış.

## 8. Mobil Güvenlik

Token: `expo-secure-store` (Keychain/Keystore). Web: `localStorage` (CSP ile XSS azaltılır). SSL pinning + jailbreak/root tespiti opsiyonel.

## 9. Hesap Silme (GDPR/KVKK)

Genel soft delete (kelime/kart/kategori) sorunsuz — PII yok. **Hesap silme:** soft delete + 30 gün grace → `AccountCleanupBackgroundService` anonimleştirir: `Email→deleted_{id}@deleted.invalid`, ad→"Silindi", `PasswordHash/GoogleId/AppleId→null`, `OriginalEmailHash=SHA-256(email)` (tekrar kaydı blokla), `IsAnonymized=true`.

## 10. Deployment Checklist

```
☐ HTTPS/TLS · güvenlik başlıkları · CORS tanımlı origin
☐ JWT/DB/AES anahtarları ENV'de (kodda değil)
☐ Rate limiting açık · loglar PII içermiyor (hash)
☐ Dependency güvenlik taraması · DB backup (AES-256, test edilmiş)
☐ GDPR: anonimleştirme prod'da çalışıyor · OriginalEmailHash blok testi
☐ Log saklama politikası dokümante
```
