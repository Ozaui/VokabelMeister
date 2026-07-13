# FAZ A — Admin Panel Backend

> **Yöntem/standart:** Bu dosyadaki her task, `../../CLAUDE.md` §3/§6 kurallarına göre yazılır
> (dikey dilim, parça yazılır yazılmaz `BACKEND_AKADEMI/`ye işlenir). O bölümler değişmez
> standarttır — burada tekrar edilmez, her zaman `../../CLAUDE.md`'ye bakılır.

### A-01 — Proje İskeleti ✅
**Referans:** REFERENCE/DEVELOPMENT_SETUP.md §3, REFERENCE/ENV.md
- [x] Solution + 4 proje (API, Application, Infrastructure, Domain) + Tests + referanslar (Domain ← Infra ← App ← API)
- [x] NuGet paketleri (REFERENCE/TECHNICAL_SPECIFICATIONS.md §1), `appsettings*.json`, `Program.cs` temel yapı

### A-02 — Ortak Altyapı ✅
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §4, §7
*(Feature entity'leri YOK — yalnızca her API'ın ihtiyaç duyduğu paylaşılan temel.)*
- [x] `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt, CreatedByUserId, UpdatedByUserId, DeletedByUserId)
- [x] ➜ **BACKEND_AKADEMI'ye işle** 
- [x] `WordLearnerDbContext` (boş; `ApplyConfigurationsFromAssembly`, soft delete filter, `SaveChangesAsync` override)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `EntityNotFoundException` (Repository<T>.SoftDeleteAsync'in bağımlılığı olduğu için Repository'den önce yazıldı)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `IRepository<T>` + `Repository<T>` generic base + `AddInfrastructureServices()`
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] **Birim testleri:** `RepositoryTests` + `EntityNotFoundExceptionTests` (in-memory DB ile CRUD + soft delete filtresi + exception mesaj formatı — sonraki tüm API'lar bunu kullanır)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] Ortak hata tipi: `ApiErrorResponse` (`ExceptionHandlingMiddleware`'in gerçek tüketicisi olduğu
      için burada yazıldı; `ApiResponse<T>`/`PagedResult<T>` hiçbir controller yokken spekülatif
      olarak yazılmıştı → YAGNI kuralına göre geri alındı, ilk gerçek controller'ın ihtiyaç duyduğu
      anda o task içinde yazılacak — bkz. `../TASK.md` "Spekülatif ortak tip yazılmaz" kuralı)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] Middleware: global exception, security headers, request/response log
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `Program.cs`: JWT auth, CORS, Serilog, FluentValidation, MediatR, AutoMapper kayıtları
- [x] ➜ **BACKEND_AKADEMI'ye işle**

### A-03 — Auth API (User) ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §3, REFERENCE/SECURITY.md §2, REFERENCE/TECHNICAL_SPECIFICATIONS.md §5-6
**Frontend karşılığı:** B-02 (Admin — sade giriş+OTP), D-03 (Web — tam akış+Google), E-05 (Mobil — tam akış+Google+Apple)
*Dikey dilim: `User` + `RefreshToken` entity → servisler → controller → yol haritası.*
- [x] **Entity:** `User`, `RefreshToken` + `OtpPurpose` enum + EF config + migration
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `IPasswordService` (BCrypt wf:12 + SHA-256 token hash)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `ITokenService` (JWT access 15dk + refresh; algorithm-confusion önlemi)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `IOtpService`/`OtpService` (OTP üretimi/doğrulanması/temizlenmesi — Register/Login/
      ResetPassword/AccountDeletion akışlarının paylaştığı ortak servis) ve
      `ILoginCompletionService`/`LoginCompletionService` (OTP/Google/Apple girişlerinin ortak son
      adımı: grace period kurtarma, giriş istatistikleri, token üretimi)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] 13 Auth Command+Handler'ı (MediatR CQRS, `Application/Features/Auth/`): `RegisterCommand`,
      `VerifyEmailCommand`, `ResendVerificationCommand`, `LoginCommand`, `VerifyLoginOtpCommand`,
      `LoginWithGoogleCommand`, `LoginWithAppleCommand`, `RefreshCommand`, `LogoutCommand`,
      `ForgotPasswordCommand`, `ResetPasswordCommand`, `RequestAccountDeletionCommand`,
      `ConfirmAccountDeletionCommand` — her biri kendi dosyasında Command+Handler birlikte
      (dikey dilim). `RefreshCommand`/`LogoutCommand` eskiden tek bir `RefreshRequest` DTO'sunu
      paylaşırdı; MediatR'da bir `IRequest<T>` tek dönüş tipine bağlı olduğundan (Refresh
      `AuthTokenResponse`, Logout dönüşsüz) ayrı Command'lara bölündüler.
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `IEmailService` (sözleşme) + `DevEmailService`, `IAppleTokenValidator`, tüm DTO/exception
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `AuthController` (13 endpoint, `IMediator.Send(command)` ile) + FluentValidation (Command
      tiplerine retarget edilmiş validator'lar) + rate limiting (genel 100/dk, 10/dk anonim —
      "login 5/15dk"/"OTP 3 yanlış" BAŞARISIZ deneme sayaçları SecurityLog'a bağımlı, A-04 sonrası eklenecek)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] **Birim testleri:** 13 Command Handler'ın her biri için ayrı test dosyası
      (`WordLearner.Tests/Features/Auth/`, 38 test — register, e-posta doğrulama, login 2-adım,
      Google/Apple + account linking, refresh/replay tespiti, logout sahiplik, forgot/reset,
      delete-account grace), `OtpServiceTests` (7 test) + `LoginCompletionServiceTests` (5 test —
      grace period kurtarma dahil, `WordLearner.Tests/Services/`), `JwtTokenServiceTests` (6 test —
      claim'ler, Algorithm Confusion), `PasswordServiceTests` (5 test — hash/verify/salt).
      Toplam 72/72 yeşil.
- [x] ➜ **BACKEND_AKADEMI'ye işle**
> **Not:** Bu API'daki `SecurityLog` (LoginFailed/OtpFailed/RateLimitHit) entegrasyonu A-04'te
> loglama altyapısı hazır olduktan **sonra** eklenir — A-03 bu adıma kadar log'suz tamamlanmış sayılır,
> A-04 bitince ilgili Command Handler'lara (LoginCommandHandler, VerifyLoginOtpCommandHandler vb.)
> kısa bir entegrasyon dönüşü yapılır (tek istisna, kuralın bilinçli ihlali).
> **Not (lokalizasyon):** `MessageResponse` metinleri (ör. "OTP gönderildi.", "Hesabınız silindi...")
> şu an hardcode Türkçe — hata mesajlarının aksine (`ErrorMessages.Resolve` + `Accept-Language`)
> başarı mesajları dile göre çözülmüyor. Kullanıcı bu eksikliği fark etti (2026-07-07); kapsam
> büyüteceği için ayrı bir task olarak bırakıldı → bkz. **A-03.2** aşağıda.

### A-03.1 — QR Kod ile Giriş ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §3.1, REFERENCE/SECURITY.md §1.3, DATABASE_SCHEMA/Auth.md (`QrLoginSessions`)
**Frontend karşılığı:** D-03 (Web — QR ekranı, `qrcode.react` ile görselleştirme), E-05 (Mobil — kamera tarayıcı + onay ekranı, `expo-camera`)
> **A-03 tamamlandıktan sonra** yapılır çünkü `User`/`ITokenService`/`ILoginCompletionService`
> (özellikle token üretim mantığı) buna bağımlı — QR girişi **ayrı bir kimlik doğrulama sistemi
> değil**, onaylandığında A-03'te yazılan `ITokenService`'i çağıran yeni bir "kimliği kanıtlama
> yöntemi"dir (bkz. `SECURITY.md §1.3`).
> Admin panelde (Faz B) **yok** — yalnızca Web (D) ve Mobil (E).
- [x] **Entity:** `QrLoginSession` + `QrLoginStatus` enum + EF config + migration
- [x] ➜ **BACKEND_AKADEMI'ye işle**
> ⚠️ **Kural güncellemesi (bkz. `TASK.md` "Bir API'ın Yazım Sırası" notu):** Aşağıdaki
> `IQrLoginService`/`QrLoginService` ifadesi eski desendir, **geçersiz**. A-03'ün retrofit'inden
> sonraki her task gibi bu da MediatR Command+Handler (+ koşullu AutoMapper Profile) deseniyle
> yazılır — `GenerateAsync/ScanAsync/ConfirmAsync/DenyAsync/GetStatusAsync` her biri ayrı bir
> `IRequest<TResponse>` Command'ı olur (`Application/Features/QrLogin/XxxCommand.cs`, Handler aynı
> dosyada); paylaşılan mantık varsa `IOtpService` örneğindeki gibi ayrı bir servise çıkarılır.
- [x] 5 MediatR Command+Handler (`Application/Features/QrLogin/`): `GenerateQrLoginCommand`
      (token+hash+pairingCode+expiry), `ScanQrLoginCommand` (Pending→Scanned, UserId set),
      `ConfirmQrLoginCommand`/`DenyQrLoginCommand` (sahiplik kontrolü), `GetQrLoginStatusCommand`
      (Confirmed→ILoginCompletionService ile token üret→Consumed'a geçir, tek seferlik döndür) +
      paylaşılan `QrLoginSessionExpiryExtensions` (lazy expire) + `IQrLoginSessionRepository`/
      `QrLoginSessionRepository` + `QrSessionGoneException`(410)/`QrSessionForbiddenException`(403)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `QrLoginController`: `POST /auth/qr/generate` (Anonim), `GET /auth/qr/{token}/status` (Anonim,
      polling), `POST /auth/qr/{token}/scan` `/confirm` `/deny` ([Authorize]) + rate limiting (generate: IP başına 20/saat, partitioned)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] **Birim testleri:** 5 dosya, 18 test (`WordLearner.Tests/Features/QrLogin/`) — mutlu yol, süresi
      dolmuş token, yanlış kullanıcı confirm/deny denemesi 403, Consumed sonrası tekrar okuma 410,
      Expired'ın 410 DEĞİL 200 dönmesi, pairingCode/token üretimi. Toplam 90/90 yeşil (A-03 72 + A-03.1 18).
- [x] ➜ **BACKEND_AKADEMI'ye işle**
> **Not:** `SecurityLog` (QrLoginConfirmed/QrLoginDenied) entegrasyonu A-03'teki auth akışlarıyla aynı
> sebeple A-04'ten sonra eklenir (bkz. A-03'ün notu).

### A-03.2 — Auth Başarı Mesajlarının Lokalizasyonu ✅
**Referans:** REFERENCE/SECURITY.md, `ErrorMessages.cs` (mevcut hata mesajı lokalizasyon deseni)
> **Neden ayrı task:** A-03'ün 13 `MessageResponse` metni (ör. "OTP gönderildi.", "Hesabınız
> silindi...") şu an hardcode Türkçe — kullanıcı doğrudan görüyor (web/mobil bunu aynen
> gösteriyor), bu yüzden log/DB sabitleri gibi İngilizce kalması gereken bir şey DEĞİL, "istemci
> mesajı dile göre kalır" mimari kararının kapsamına giriyor. Hata mesajları zaten
> `ErrorMessages.Resolve(code, language)` + `RequestLanguageResolver` (`Accept-Language`) ile
> çözülüyor; başarı mesajları bu sisteme hiç girmemiş. MediatR CQRS refactor'u sırasında (2026-07-07)
> fark edildi, kapsamı büyüteceği için o refactor'dan ayrı bir task olarak bırakıldı.
- [x] Her `MessageResponse`'a bir kod ekle (ör. `"OTP_SENT"`, `"ACCOUNT_DELETED"`) — `ErrorMessages.cs`
      deseniyle aynı: sabit dosyada dil→kod→metin sözlüğü (en az tr+de, İngilizce YAGNI ile ertelenebilir)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] 13 Command Handler'ın `MessageResponse` üretim noktalarını kod+`RequestLanguageResolver`
      çözümüne geçir (`ValidationFilter`'ın zaten yaptığı dil çözme mantığıyla aynı desen)
> **Not:** Kapsam, `MessageResponse` DÖNDÜREN 7 Command'a (`LoginCommand`, `ResendVerificationCommand`,
> `ResetPasswordCommand`, `ConfirmAccountDeletionCommand`, `RequestAccountDeletionCommand`,
> `VerifyEmailCommand`, `ForgotPasswordCommand`) daraldı — 13'ün geri kalan 6'sı (`RegisterCommand`,
> `VerifyLoginOtpCommand`, `LoginWithGoogleCommand`, `LoginWithAppleCommand`, `RefreshCommand`,
> `LogoutCommand`) `AuthTokenResponse`/`RegisterResponse` döndürüyor ya da (Logout) hiç gövde
> döndürmüyor — dile göre değişen bir metin taşımadıkları için lokalize edilecek bir şeyleri yok.
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] **Birim testleri:** ilgili handler testlerine dil parametresi verildiğinde doğru dilde
      mesaj döndüğünü doğrulayan senaryolar eklenir — 7 handler test dosyasına birer
      `Xxx_GermanLanguage_ReturnsGermanMessage` testi eklendi (7 yeni test, `Code`+`Message`
      ikisi de doğrulanıyor). Toplam 97/97 yeşil (A-03 72 + A-03.1 18 + A-03.2 7).
- [x] ➜ **BACKEND_AKADEMI'ye işle**

### A-03.3 — Tema Tercihi (ThemePreference) ✅
**Referans:** `DATABASE_SCHEMA/Auth.md` (Users), `wiki/Database/Auth_Domain.md`
> **Neden ayrı task:** A-03 zaten ✅ tamamlanmıştı — bu bir davranış değişikliği/retrofit değil,
> saf bir DB+DTO eklentisi (A-03.2'nin izlediği desenle aynı: küçük-orta ölçekli, kendi
> entity/controller'ı yok). Kullanıcı admin panel tasarım kararları sırasında sistemde hiç tema
> (dark/light/system) kavramı olmadığını fark etti. **Kritik mimari karar:** `CurrentLevel` (A1-C2)
> zaten "kayıt sırasında sorulan tercih" kategorisinde bire bir emsal — ama `RegisterCommand`'a
> hiç girmiyor (kayıt anonim, henüz token/onboarding yok), DB varsayılanı döner, gerçek seçim
> kayıt sonrası ilk-login-sonrası onboarding'de (`LevelSelectPage`, `PUT /users/me` — C-01)
> yapılıyor. `ThemePreference` bu deseni birebir takip eder — `RegisterCommand`'a **girdi
> eklenmedi**, yalnızca çıkış DTO'ları (`RegisterResponse`/`AuthUserDto`) genişledi. JWT'ye hiç
> girmez (`JwtTokenService` claim'leri yalnızca yetki taşır — NameIdentifier/Email/Role/firstName).
- [x] **Entity + Config + Migration:** `Users.ThemePreference NVARCHAR(10) DEFAULT 'System'` +
      `CK_Users_ThemePreference` (`CurrentLevel` ile aynı iki-katmanlı savunma deseni) —
      `User.cs`, `UserConfiguration.cs`, `AddUserThemePreference` migration
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`A-03.3_tema-tercihi.html`)
- [x] **DTO:** `RegisterResponse` ve `AuthUserDto`'ya `ThemePreference` alanı (AutoMapper otomatik
      map eder, `AuthProfile.cs` değişmedi)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] **Birim testleri:** `RegisterCommandHandlerTests`e 1 yeni test (`Register_NewEmail_
      ReturnsDefaultSystemThemePreference`) + `AuthUserDto`'nun 3. parametre alması nedeniyle 4
      mevcut test dosyasında (`LoginWithGoogle/AppleCommandHandlerTests`, `GetQrLoginStatusCommandHandlerTests`,
      `VerifyLoginOtpCommandHandlerTests`) çağrı noktaları senkronize edildi (davranış değişmedi)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
> **Not:** Gerçek toplama (kullanıcının temayı SEÇMESİ) `LevelSelectPage`/`LevelSelectScreen` ile
> aynı onboarding anında, gelecekteki `PUT /users/me` (C-01, henüz yazılmadı) ile yapılacak — bugün
> kodlanmadı (YAGNI), yalnızca not bırakıldı (bkz. `C_kullanici_backend.md` C-01).

### A-04 — Loglama Sistemi (Audit + Application + Security → DB) ⬜
**Referans:** REFERENCE/SECURITY.md §6, DATABASE_SCHEMA/Loglama.md
**Frontend karşılığı:** B-08 (Admin — Log Görüntüleme Paneli)
> **Amaç:** Tüm loglar DB'de tutulur, **admin panelden görüntülenir** (A-07/B-08). Üç tablo:
> kim ne yaptı (activity), uygulama logu (Serilog), güvenlik olayları.
- [ ] **Entity:** `ActivityLog`, `ApplicationLog`, `SecurityLog` + `LogEventType` enum + EF config + migration
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] Serilog `Serilog.Sinks.MSSqlServer` → `ApplicationLog` (konsol + dosya + DB)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `IActivityLogger` + `ActivityLogger` (eski/yeni JSON ile audit), `ISecurityLogger` + `SecurityLogger`
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] Repository'ler (sayfalı, filtreli): `IActivityLogRepository`, `IApplicationLogRepository`, `ISecurityLogRepository`
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] Entegrasyon: auth akışlarına security log (LoginFailed, OtpFailed, RateLimitHit), `GET /health`
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `ActivityLoggerTests`, `SecurityLoggerTests` (DB context mock/in-memory)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-05 — Sistem Kelimesi API (Words) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §5
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi)
- [ ] **Entity:** `Language`, `WordConcept`, `Word`, `WordDetail`, `WordExample` + EF config + migration
      + `Language` seed (`de`, `tr`) — bkz. `DATABASE_SCHEMA/Icerik.md` (çoklu dile açık şema:
      kategori/seviye `WordConcept` üzerinde, gramer `WordDetail.GrammarData` JSON'da dile göre değişir)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `IWordService` + `WordService` (liste filtre+sayfa, detay, CRUD Admin — bir kelime tüm dilleriyle
      (`translations[]`) tek işlemde oluşturulur/güncellenir, duplikat 409 + `?force=true`)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `WordController` (`[Authorize]` liste/detay, `[Authorize(Roles="Admin")]` CRUD)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `WordServiceTests` (liste filtre, duplikat 409 + force, CRUD yetki)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-06 — Kategori API (Categories) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §6
**Frontend karşılığı:** B-04 (Admin — Kategori Yönetimi), D-06 (Web — Kategoriler Sayfası), E-08 (Mobil — Kategoriler Ekranı)
- [ ] **Entity:** `Category` (self-ref hiyerarşi), `CategoryTranslation` (dil başına ad), `WordCategory`
      ara tablo (`WordConceptId`↔`CategoryId`) + EF config + migration
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `ICategoryService` + `CategoryService` (hiyerarşik liste, kategoriye ait kelimeler, CRUD Admin)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] Silme koruması (alt kategori/aktif kelime varsa 409), `CategoriesController`
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `CategoryServiceTests` (hiyerarşik liste, silme koruması 409)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-07 — Admin API (Kullanıcı Yönetimi + İstatistik + Log Görüntüleme) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §11
**Frontend karşılığı:** B-05 (Kullanıcı Yönetimi), B-06 (Paylaşım/İçerik Moderasyonu), B-07 (İstatistik Paneli), B-08 (Log Görüntüleme Paneli)
- [ ] `IAdminService` + `AdminService`
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] Kullanıcı: liste/arama/detay, rol değiştir, hesap dondur/aktif (her işlem **ActivityLog**'a)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] İçerik moderasyonu (kart liste + silme), genel istatistik, toplu kelime import
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Log görüntüleme:** `GET /admin/logs/activity`, `/admin/logs/application`, `/admin/logs/security` (filtre+sayfa)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `AdminController` (`[Authorize(Roles="Admin")]`)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `AdminServiceTests` (rol değiştir, dondur/aktif, import, log filtreleme)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-08 — Medya / Dosya Yükleme API ⬜
**Referans:** REFERENCE/ENV.md §7
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi formundaki görsel yükleme)
- [ ] `IFileStorageService` + `LocalFileStorageService`, `Word.ImageUrl` + migration
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `MediaController` (`POST /media/images/upload`), `UseStaticFiles`
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `FileStorageServiceTests` (boyut/uzantı doğrulama, benzersiz ad üretimi)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-09 — SMTP Ayarları API ⬜
**Referans:** REFERENCE/SECURITY.md §3.2, REFERENCE/ENV.md §5
**Frontend karşılığı:** B-09 (Admin — SMTP Ayarları Sayfası)
> SMTP bilgileri DB'de AES-256 şifreli; admin panelden yönetilir, `appsettings.json`'da DEĞİL.
- [ ] **Entity:** `SmtpSettings` (Host, Port, EnableSsl, Username, **PasswordEncrypted**, FromEmail, FromName, UpdatedBy) + migration
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `IEncryptionService` + `AesEncryptionService` (AES-256-CBC, rastgele IV, anahtar `AES_ENCRYPTION_KEY`)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `ISmtpSettingsRepository`, `SmtpSettingsController` (Admin): `GET` (şifre `***`), `PUT`, `POST .../test`
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `AesEncryptionServiceTests` (encrypt/decrypt round-trip, 32 byte anahtar kontrolü)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-10 — E-posta Servisi + Hesap Temizleme Görevi ⬜
**Referans:** REFERENCE/SECURITY.md §7
- [ ] `SmtpEmailService` (MailKit; SMTP'yi repo'dan alır, `Decrypt` ile çözer) + DI (dev→Dev, prod→Smtp)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] E-posta şablonları (doğrulama, login OTP, şifre sıfırlama, hesap silme onayı, şifre değişti, hesap kurtarıldı)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `AccountCleanupBackgroundService : IHostedService` (PII anonimleştirme, günde 1, 03:00 UTC)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `AccountCleanupServiceTests` (30 gün grace sonrası anonimleştirme, blok hash'i)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
