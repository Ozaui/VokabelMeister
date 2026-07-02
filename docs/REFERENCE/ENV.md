# Ortam Değişkenleri (ENV)

Uygulamanın ihtiyaç duyduğu tüm ortam değişkenleri. Dış servis sayısı kasıtlı düşük tutulur.

## Genel Kurallar

- Hassas değerler `appsettings.json` veya kaynak koduna **asla** yazılmaz.
- ASP.NET Core'da `__` (çift alt çizgi) nested config ayırıcısıdır: `Jwt__SecretKey` → `IConfiguration["Jwt:SecretKey"]`.
- Dev: `appsettings.Development.json` / `launchSettings.json` (ikisi de `.gitignore`'da).
- Prod: IIS sunucu ortam değişkenleri.
- **Yeni servis eklenince bu dosyayı güncelle** (§9).

## 1. Veritabanı
```
ConnectionStrings__DefaultConnection=Server=127.0.0.1,1433;Database=VokabelMeisterDB;User Id=sa;Password=...;TrustServerCertificate=True;
```
> **Not:** Log tabloları (`ApplicationLog`, `ActivityLog`, `SecurityLog`) aynı veritabanını kullanır —
> ek bağlantı/değişken **gerekmez**.

## 2. JWT
```
Jwt__SecretKey=en-az-32-karakter-rastgele-anahtar      # HMAC-SHA256, min 32 karakter
Jwt__Issuer=WordLearnerApp
Jwt__Audience=WordLearnerApp
Jwt__RefreshTokenExpirationDays=7
```
Üretme: `openssl rand -base64 48`

## 3. Google Sign In
```
Google__ClientId=123456789-abc.apps.googleusercontent.com
```
Google Cloud Console → APIs & Services → Credentials → OAuth 2.0 Client ID. Backend yalnızca
`GoogleJsonWebSignature.ValidateAsync` ile audience doğrular; client secret gerekmez.

## 4. Apple Sign In
```
Apple__BundleId=com.vokabelmeister.app
```
Apple Developer → Identifiers → Bundle ID. JWKS `https://appleid.apple.com/auth/keys`'ten dinamik çekilir.

## 5. SMTP Şifreleme Anahtarı (AES)
```
AES_ENCRYPTION_KEY=K5v2XmP9qR8nW3jL6tH1cB4yE7uA0oD+N2sF5gM=    # 32 byte, Base64
```
SMTP ayarları admin panel üzerinden DB'ye kaydedilir; şifre AES-256 ile DB'de şifreli tutulur.
Anahtar DB'de saklanamaz → daima ENV'de. Üretme: `openssl rand -base64 32`.
> **Kritik:** Kaybolursa DB'deki şifreli SMTP şifreleri çözülemez. Güvenli sakla.

## 6. OneSignal (Push)
```
OneSignal__AppId=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
OneSignal__RestApiKey=os_v2_app_xxxxxxxxxxxxxxxxxxxxx     # YALNIZCA backend — asla mobil koda girmez
```
OneSignal Dashboard → Settings → Keys & IDs. `AppId` mobilde de `.env`'de kullanılır (hassas değil);
`RestApiKey` yalnızca backend'de.

## 7. Dosya Depolama (yerel sunucu — avatar/görsel)
```
FileStorage__UploadPath=/var/app/uploads
FileStorage__BaseUrl=https://api.vokabelmeister.com/uploads
```
`UseStaticFiles()` ile public servis edilir. Dev: `wwwroot/uploads` + `https://localhost:7001/uploads`.

## 8. CORS
```
Cors__AllowedOrigins__0=http://localhost:8081     # Expo
Cors__AllowedOrigins__1=http://localhost:5173     # Admin panel
Cors__AllowedOrigins__2=http://localhost:5174     # Web app
```

## appsettings.Development.json Şablonu (`.gitignore`'da)
```json
{
  "ConnectionStrings": { "DefaultConnection": "Server=127.0.0.1,1433;Database=VokabelMeisterDB;User Id=sa;Password=...;TrustServerCertificate=True;" },
  "Jwt": { "SecretKey": "gelistirme-icin-en-az-32-karakter", "Issuer": "WordLearnerApp", "Audience": "WordLearnerApp", "RefreshTokenExpirationDays": "7" },
  "Google": { "ClientId": "..." },
  "Apple": { "BundleId": "com.vokabelmeister.app" },
  "OneSignal": { "AppId": "...", "RestApiKey": "..." },
  "FileStorage": { "UploadPath": "wwwroot/uploads", "BaseUrl": "https://localhost:7001/uploads" },
  "Cors": { "AllowedOrigins": ["http://localhost:8081", "http://localhost:5173", "http://localhost:5174"] }
}
```
`AES_ENCRYPTION_KEY` → `launchSettings.json` `environmentVariables` altına eklenir (commit edilmez).

## 9. Güncelleme Kuralı
Yeni servis eklenince: (1) buraya bölüm ekle, (2) `TASK.md`'ye `→ REFERENCE/ENV.md §X` referansı, (3) dev şablonu güncelle.

## Özet — Hangi Bilgi Nerede?

| Bilgi | Yer | Neden |
|-------|-----|-------|
| DB bağlantısı, JWT anahtarı, AES anahtarı | ENV | Hassas / kaynak koda girmemeli |
| Google ClientId, Apple BundleId | ENV | Ortama göre değişir |
| OneSignal RestApiKey | ENV (yalnızca backend) | Asla mobil koda girmez |
| Dosya yükleme yolu | ENV | Sunucuya göre değişir |
| SMTP host/port/kullanıcı/şifre | **Database** (şifre AES) | Admin panelden yönetilir |
| Log verileri | **Database** (ana DB) | Ek değişken gerekmez |
