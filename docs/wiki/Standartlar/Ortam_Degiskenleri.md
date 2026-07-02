# Ortam Değişkenleri (ENV)

**Özet:** Hassas değerler (`appsettings.json` veya kaynak koda asla yazılmaz) ve ortama özgü ayarlar için ortam değişkeni listesi. ASP.NET Core'da `__` (çift alt çizgi) nested config ayırıcısıdır (`Jwt__SecretKey` → `IConfiguration["Jwt:SecretKey"]`). Dev: `appsettings.Development.json`/`launchSettings.json` (`.gitignore`'da); Prod: IIS sunucu ortam değişkenleri.
**Kütüphaneler:** —
**Bağlantılar:** [[WordLearner_API]] · [[Guvenlik_Politikalari]] · [[InfrastructureServiceExtensions]]

## Değişken Grupları

| Grup | Anahtar örneği | Not |
|------|-----------------|-----|
| Veritabanı | `ConnectionStrings__DefaultConnection` | Log tabloları aynı DB'yi kullanır, ek değişken gerekmez |
| JWT | `Jwt__SecretKey` (min 32 karakter, HMAC-SHA256), `Jwt__Issuer`, `Jwt__Audience`, `Jwt__RefreshTokenExpirationDays` | `openssl rand -base64 48` ile üretilir |
| Google Sign In | `Google__ClientId` | Yalnızca audience doğrulanır, client secret gerekmez |
| Apple Sign In | `Apple__BundleId` | JWKS `appleid.apple.com/auth/keys`'ten dinamik çekilir |
| SMTP Şifreleme | `AES_ENCRYPTION_KEY` (32 byte, Base64) | Kaybolursa DB'deki şifreli SMTP şifreleri çözülemez → [[Guvenlik_Politikalari]] §3.4 |
| Push (OneSignal) | `OneSignal__AppId`, `OneSignal__RestApiKey` | `RestApiKey` **yalnızca backend**, asla mobil koda girmez |
| Dosya Depolama | `FileStorage__UploadPath`, `FileStorage__BaseUrl` | `UseStaticFiles()` ile public servis edilir |
| CORS | `Cors__AllowedOrigins__0..2` | Expo (8081) · Admin/Vite (5173) · Web/Vite (5174) |

## Özet — Hangi Bilgi Nerede?
| Bilgi | Yer | Neden |
|-------|-----|-------|
| DB bağlantısı, JWT/AES anahtarları | ENV | Hassas / kaynak koda girmemeli |
| Google ClientId, Apple BundleId | ENV | Ortama göre değişir |
| OneSignal RestApiKey | ENV (yalnızca backend) | Asla mobil koda girmez |
| SMTP host/port/kullanıcı/şifre | **Database** (şifre AES) | Admin panelden yönetilir, `SmtpSettings` |
| Log verileri | **Database** (ana DB) | Ek değişken gerekmez |

## Güncelleme Kuralı
Yeni servis eklenince: (1) bu listeye bölüm ekle, (2) `TASK.md`'ye referans, (3) dev şablonu güncelle.
