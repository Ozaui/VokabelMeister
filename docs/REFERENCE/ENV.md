# Ortam Değişkenleri (ENV)

> Genel kural (hassas değer asla appsettings/koda girmez, yeni servis → bu dosyayı güncelle) → `CLAUDE.md §1`.
> ASP.NET Core'da `__` = nested ayırıcı (`Jwt__SecretKey` → `IConfiguration["Jwt:SecretKey"]`).
> Dev: `appsettings.Development.json` + `launchSettings.json` (ikisi `.gitignore`'da). Prod: IIS ortam değişkenleri.

## 1. Veritabanı (log tabloları da aynı DB — ek değişken yok)
```
ConnectionStrings__DefaultConnection=Server=127.0.0.1,1433;Database=ZauselDB;User Id=sa;Password=...;TrustServerCertificate=True;
```

## 2. JWT
```
Jwt__SecretKey=en-az-32-karakter-rastgele        # HMAC-SHA256, min 32 karakter (openssl rand -base64 48)
Jwt__Issuer=ZauselApp
Jwt__Audience=ZauselApp
Jwt__RefreshTokenExpirationDays=7
```

## 3. Google Sign In
```
Google__ClientId=123456789-abc.apps.googleusercontent.com
```
Backend yalnızca `GoogleJsonWebSignature.ValidateAsync` ile audience doğrular; client secret gerekmez.

## 4. Apple Sign In
```
Apple__BundleId=com.zausel.app
```
JWKS `https://appleid.apple.com/auth/keys`'ten dinamik çekilir.
> **İleride web'e Apple eklenirse (şu an YOK):** `Apple__ServicesId` gerekir + Apple Console'da bu Services ID mobil Bundle ID'ye "Primary App ID" olarak gruplanmalı, yoksa aynı kişi için iki `sub` → iki hesap (`SECURITY.md §1.2`).

## 5. SMTP Şifreleme Anahtarı (AES)
```
AES_ENCRYPTION_KEY=Z0n/xH/HxOpxJfQO3s5qCMQBhW2yKd+Qo9jDESO5t8Q=    # 32 byte Base64 (openssl rand -base64 32)
```
SMTP şifresi DB'de AES-256 şifreli; anahtar DB'de saklanamaz → daima ENV. **Kaybolursa DB'deki SMTP şifreleri çözülemez.**

## 6. OneSignal (Push)
```
OneSignal__AppId=xxxxxxxx-...      # mobilde de kullanılır (hassas değil)
OneSignal__RestApiKey=os_v2_...    # YALNIZCA backend — asla mobil koda girmez
```

## 7. Dosya Depolama (avatar/görsel)
```
FileStorage__UploadPath=/var/app/uploads
FileStorage__BaseUrl=https://api.zausel.com/uploads
```
`UseStaticFiles()` ile public. Dev: `wwwroot/uploads` + `https://localhost:7001/uploads`.

## 8. CORS
```
Cors__AllowedOrigins__0=http://localhost:8081     # Expo
Cors__AllowedOrigins__1=http://localhost:5173     # Admin panel
Cors__AllowedOrigins__2=http://localhost:5174     # Web app
```

## 9. İlk Admin Hesabı (A-03.2)
```
INITIAL_ADMIN_EMAIL=admin@zausel.com
INITIAL_ADMIN_PASSWORD=...
```
`Program.cs` başlangıcında (`app.Run()`'dan önce) `AdminSeedService` bu ikisini okur — yalnızca o
e-posta hiç yoksa bir `Admin` kullanıcısı oluşturur (idempotent). Yalnızca geliştirme/staging'de
`appsettings.Development.json`'a yazılır; prod'da secret olarak verilir. İkisi de tanımlı değilse
seed sessizce atlanır (zorunlu değil). İlk girişten sonra şifrenin değiştirilmesi önerilir —
bu bir dokümantasyon notudur, kod zorlaması değil.

## appsettings.Development.json Şablonu (`.gitignore`'da)
```json
{
  "ConnectionStrings": { "DefaultConnection": "Server=127.0.0.1,1433;Database=ZauselDB;User Id=sa;Password=...;TrustServerCertificate=True;" },
  "Jwt": { "SecretKey": "gelistirme-icin-en-az-32-karakter", "Issuer": "ZauselApp", "Audience": "ZauselApp", "RefreshTokenExpirationDays": "7" },
  "Google": { "ClientId": "..." },
  "Apple": { "BundleId": "com.zausel.app" },
  "INITIAL_ADMIN_EMAIL": "admin@zausel.com",
  "INITIAL_ADMIN_PASSWORD": "...",
  "OneSignal": { "AppId": "...", "RestApiKey": "..." },
  "FileStorage": { "UploadPath": "wwwroot/uploads", "BaseUrl": "https://localhost:7001/uploads" },
  "Cors": { "AllowedOrigins": ["http://localhost:8081", "http://localhost:5173", "http://localhost:5174"] }
}
```
`AES_ENCRYPTION_KEY` → `launchSettings.json` `environmentVariables` altına (commit edilmez).

## Hangi Bilgi Nerede?

| Bilgi | Yer |
|-------|-----|
| DB bağlantısı, JWT/AES anahtarı, Google ClientId, Apple BundleId, OneSignal RestApiKey, dosya yolu | ENV |
| SMTP host/port/kullanıcı/şifre (şifre AES) | Database (admin panelden) |
| Log verileri | Database (ana DB) |
