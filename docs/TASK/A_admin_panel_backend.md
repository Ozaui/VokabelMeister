# FAZ A — Admin Panel Backend

> **Yöntem/standart:** Bu dosyadaki her task, `../TASK.md`'deki **⭐ Çalışma Yöntemi** ve
> **Her Parça İçin Döngü** kurallarına göre yazılır (dikey dilim, parça yazılır yazılmaz
> API Yol Haritası'na işlenir). O bölümler değişmez standarttır — burada tekrar edilmez,
> her zaman `../TASK.md`'ye bakılır.

### A-01 — Proje İskeleti ✅
**Referans:** REFERENCE/DEVELOPMENT_SETUP.md §3, REFERENCE/ENV.md
- [x] Solution + 4 proje (API, Application, Infrastructure, Domain) + Tests + referanslar (Domain ← Infra ← App ← API)
- [x] NuGet paketleri (REFERENCE/TECHNICAL_SPECIFICATIONS.md §1), `appsettings*.json`, `Program.cs` temel yapı

### A-02 — Ortak Altyapı ✅
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §4, §7
*(Feature entity'leri YOK — yalnızca her API'ın ihtiyaç duyduğu paylaşılan temel.)*
- [x] `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt, CreatedByUserId, UpdatedByUserId, DeletedByUserId)
- [x] ➜ **API Yol Haritası'na işle** 
- [x] `WordLearnerDbContext` (boş; `ApplyConfigurationsFromAssembly`, soft delete filter, `SaveChangesAsync` override)
- [x] ➜ **API Yol Haritası'na işle**
- [x] `EntityNotFoundException` (Repository<T>.SoftDeleteAsync'in bağımlılığı olduğu için Repository'den önce yazıldı)
- [x] ➜ **API Yol Haritası'na işle**
- [x] `IRepository<T>` + `Repository<T>` generic base + `AddInfrastructureServices()`
- [x] ➜ **API Yol Haritası'na işle**
- [x] **Birim testleri:** `RepositoryTests` + `EntityNotFoundExceptionTests` (in-memory DB ile CRUD + soft delete filtresi + exception mesaj formatı — sonraki tüm API'lar bunu kullanır)
- [x] ➜ **API Yol Haritası'na işle**
- [x] Ortak hata tipi: `ApiErrorResponse` (`ExceptionHandlingMiddleware`'in gerçek tüketicisi olduğu
      için burada yazıldı; `ApiResponse<T>`/`PagedResult<T>` hiçbir controller yokken spekülatif
      olarak yazılmıştı → YAGNI kuralına göre geri alındı, ilk gerçek controller'ın ihtiyaç duyduğu
      anda o task içinde yazılacak — bkz. `../TASK.md` "Spekülatif ortak tip yazılmaz" kuralı)
- [x] ➜ **API Yol Haritası'na işle**
- [x] Middleware: global exception, security headers, request/response log
- [x] ➜ **API Yol Haritası'na işle**
- [x] `Program.cs`: JWT auth, CORS, Serilog, FluentValidation, MediatR, AutoMapper kayıtları
- [x] ➜ **API Yol Haritası'na işle**

### A-03 — Auth API (User) ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §3, REFERENCE/SECURITY.md §2, REFERENCE/TECHNICAL_SPECIFICATIONS.md §5-6
**Frontend karşılığı:** B-02 (Admin — sade giriş+OTP), D-03 (Web — tam akış+Google), E-05 (Mobil — tam akış+Google+Apple)
> 🧩 Bu API'nin HTML sayfası yazılınca `frontendRefs`'e B-02/D-03/E-05'in dosyaları eklenir;
> o üç feature sayfasındaki `api` adımına da bu sayfaya `backendRef` eklenir (iki yönlü).
*Dikey dilim: `User` + `RefreshToken` entity → servisler → controller → yol haritası.*
- [x] **Entity:** `User`, `RefreshToken` + `OtpPurpose` enum + EF config + migration
- [x] ➜ **API Yol Haritası'na işle**
- [x] `IPasswordService` (BCrypt wf:12 + SHA-256 token hash)
- [x] ➜ **API Yol Haritası'na işle**
- [x] `ITokenService` (JWT access 15dk + refresh; algorithm-confusion önlemi)
- [x] ➜ **API Yol Haritası'na işle**
- [x] `IOtpService`/`OtpService` (OTP üretimi/doğrulanması/temizlenmesi — Register/Login/
      ResetPassword/AccountDeletion akışlarının paylaştığı ortak servis) ve
      `ILoginCompletionService`/`LoginCompletionService` (OTP/Google/Apple girişlerinin ortak son
      adımı: grace period kurtarma, giriş istatistikleri, token üretimi)
- [x] ➜ **API Yol Haritası'na işle**
- [x] 13 Auth Command+Handler'ı (MediatR CQRS, `Application/Features/Auth/`): `RegisterCommand`,
      `VerifyEmailCommand`, `ResendVerificationCommand`, `LoginCommand`, `VerifyLoginOtpCommand`,
      `LoginWithGoogleCommand`, `LoginWithAppleCommand`, `RefreshCommand`, `LogoutCommand`,
      `ForgotPasswordCommand`, `ResetPasswordCommand`, `RequestAccountDeletionCommand`,
      `ConfirmAccountDeletionCommand` — her biri kendi dosyasında Command+Handler birlikte
      (dikey dilim). `RefreshCommand`/`LogoutCommand` eskiden tek bir `RefreshRequest` DTO'sunu
      paylaşırdı; MediatR'da bir `IRequest<T>` tek dönüş tipine bağlı olduğundan (Refresh
      `AuthTokenResponse`, Logout dönüşsüz) ayrı Command'lara bölündüler.
- [x] ➜ **API Yol Haritası'na işle**
- [x] `IEmailService` (sözleşme) + `DevEmailService`, `IAppleTokenValidator`, tüm DTO/exception
- [x] ➜ **API Yol Haritası'na işle**
- [x] `AuthController` (13 endpoint, `IMediator.Send(command)` ile) + FluentValidation (Command
      tiplerine retarget edilmiş validator'lar) + rate limiting (genel 100/dk, 10/dk anonim —
      "login 5/15dk"/"OTP 3 yanlış" BAŞARISIZ deneme sayaçları SecurityLog'a bağımlı, A-04 sonrası eklenecek)
- [x] ➜ **API Yol Haritası'na işle**
- [x] **Birim testleri:** 13 Command Handler'ın her biri için ayrı test dosyası
      (`WordLearner.Tests/Features/Auth/`, 38 test — register, e-posta doğrulama, login 2-adım,
      Google/Apple + account linking, refresh/replay tespiti, logout sahiplik, forgot/reset,
      delete-account grace), `OtpServiceTests` (7 test) + `LoginCompletionServiceTests` (5 test —
      grace period kurtarma dahil, `WordLearner.Tests/Services/`), `JwtTokenServiceTests` (6 test —
      claim'ler, Algorithm Confusion), `PasswordServiceTests` (5 test — hash/verify/salt).
      Toplam 72/72 yeşil.
- [x] ➜ **API Yol Haritası'na işle**
> **Not:** Bu API'daki `SecurityLog` (LoginFailed/OtpFailed/RateLimitHit) entegrasyonu A-04'te
> loglama altyapısı hazır olduktan **sonra** eklenir — A-03 bu adıma kadar log'suz tamamlanmış sayılır,
> A-04 bitince ilgili Command Handler'lara (LoginCommandHandler, VerifyLoginOtpCommandHandler vb.)
> kısa bir entegrasyon dönüşü yapılır (tek istisna, kuralın bilinçli ihlali).
> **Not (lokalizasyon):** `MessageResponse` metinleri (ör. "OTP gönderildi.", "Hesabınız silindi...")
> şu an hardcode Türkçe — hata mesajlarının aksine (`ErrorMessages.Resolve` + `Accept-Language`)
> başarı mesajları dile göre çözülmüyor. Kullanıcı bu eksikliği fark etti (2026-07-07); kapsam
> büyüteceği için ayrı bir task olarak bırakıldı → bkz. **A-03.2** aşağıda.

### A-03.1 — QR Kod ile Giriş ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §3.1, REFERENCE/SECURITY.md §1.3, DATABASE_SCHEMA/Auth.md (`QrLoginSessions`)
**Frontend karşılığı:** D-03 (Web — QR ekranı, `qrcode.react` ile görselleştirme), E-05 (Mobil — kamera tarayıcı + onay ekranı, `expo-camera`)
> 🧩 Bu API'nin HTML sayfası yazılınca `frontendRefs`'e D-03/E-05'in dosyaları eklenir (iki yönlü).
> **A-03 tamamlandıktan sonra** yapılır çünkü `User`/`ITokenService`/`ILoginCompletionService`
> (özellikle token üretim mantığı) buna bağımlı — QR girişi **ayrı bir kimlik doğrulama sistemi
> değil**, onaylandığında A-03'te yazılan `ITokenService`'i çağıran yeni bir "kimliği kanıtlama
> yöntemi"dir (bkz. `SECURITY.md §1.3`).
> Admin panelde (Faz B) **yok** — yalnızca Web (D) ve Mobil (E).
- [x] **Entity:** `QrLoginSession` + `QrLoginStatus` enum + EF config + migration
- [x] ➜ **API Yol Haritası'na işle**
> ⚠️ **Kural güncellemesi (bkz. `TASK.md` "Bir API'ın Yazım Sırası" notu):** Aşağıdaki
> `IQrLoginService`/`QrLoginService` ifadesi eski desendir, **geçersiz**. A-03'ün retrofit'inden
> sonraki her task gibi bu da MediatR Command+Handler (+ koşullu AutoMapper Profile) deseniyle
> yazılır — `GenerateAsync/ScanAsync/ConfirmAsync/DenyAsync/GetStatusAsync` her biri ayrı bir
> `IRequest<TResponse>` Command'ı olur (`Application/Features/QrLogin/XxxCommand.cs`, Handler aynı
> dosyada); paylaşılan mantık varsa `IOtpService` örneğindeki gibi ayrı bir servise çıkarılır.
- [ ] `IQrLoginService` + `QrLoginService`: `GenerateAsync` (token+hash+pairingCode+expiry), `ScanAsync`
      (Pending→Scanned, UserId set), `ConfirmAsync`/`DenyAsync` (sahiplik kontrolü), `GetStatusAsync`
      (Confirmed→ITokenService ile token üret→Consumed'a geçir, tek seferlik döndür)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `QrLoginController`: `POST /auth/qr/generate` (Anonim), `GET /auth/qr/{token}/status` (Anonim,
      polling), `POST /auth/qr/{token}/scan` `/confirm` `/deny` ([Authorize]) + rate limiting (generate: IP başına 20/saat)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `QrLoginServiceTests` (mutlu yol, süresi dolmuş token, yanlış kullanıcı
      confirm denemesi 403, Consumed sonrası tekrar okuma 410, pairingCode üretimi)
- [ ] ➜ **API Yol Haritası'na işle**
> **Not:** `SecurityLog` (QrLoginConfirmed/QrLoginDenied) entegrasyonu A-03'teki auth akışlarıyla aynı
> sebeple A-04'ten sonra eklenir (bkz. A-03'ün notu).

### A-03.2 — Auth Başarı Mesajlarının Lokalizasyonu ⬜
**Referans:** REFERENCE/SECURITY.md, `ErrorMessages.cs` (mevcut hata mesajı lokalizasyon deseni)
> **Neden ayrı task:** A-03'ün 13 `MessageResponse` metni (ör. "OTP gönderildi.", "Hesabınız
> silindi...") şu an hardcode Türkçe — kullanıcı doğrudan görüyor (web/mobil bunu aynen
> gösteriyor), bu yüzden log/DB sabitleri gibi İngilizce kalması gereken bir şey DEĞİL, "istemci
> mesajı dile göre kalır" mimari kararının kapsamına giriyor. Hata mesajları zaten
> `ErrorMessages.Resolve(code, language)` + `RequestLanguageResolver` (`Accept-Language`) ile
> çözülüyor; başarı mesajları bu sisteme hiç girmemiş. MediatR CQRS refactor'u sırasında (2026-07-07)
> fark edildi, kapsamı büyüteceği için o refactor'dan ayrı bir task olarak bırakıldı.
- [ ] Her `MessageResponse`'a bir kod ekle (ör. `"OTP_SENT"`, `"ACCOUNT_DELETED"`) — `ErrorMessages.cs`
      deseniyle aynı: sabit dosyada dil→kod→metin sözlüğü (en az tr+de, İngilizce YAGNI ile ertelenebilir)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] 13 Command Handler'ın `MessageResponse` üretim noktalarını kod+`RequestLanguageResolver`
      çözümüne geçir (`ValidationFilter`'ın zaten yaptığı dil çözme mantığıyla aynı desen)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** ilgili handler testlerine dil parametresi verildiğinde doğru dilde
      mesaj döndüğünü doğrulayan senaryolar eklenir
- [ ] ➜ **API Yol Haritası'na işle**

### A-04 — Loglama Sistemi (Audit + Application + Security → DB) ⬜
**Referans:** REFERENCE/SECURITY.md §6, DATABASE_SCHEMA/Loglama.md
**Frontend karşılığı:** B-08 (Admin — Log Görüntüleme Paneli)
> 🧩 `frontendRefs` ↔ B-08 `backendRef` (iki yönlü).
> **Amaç:** Tüm loglar DB'de tutulur, **admin panelden görüntülenir** (A-07/B-08). Üç tablo:
> kim ne yaptı (activity), uygulama logu (Serilog), güvenlik olayları.
- [ ] **Entity:** `ActivityLog`, `ApplicationLog`, `SecurityLog` + `LogEventType` enum + EF config + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Serilog `Serilog.Sinks.MSSqlServer` → `ApplicationLog` (konsol + dosya + DB)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IActivityLogger` + `ActivityLogger` (eski/yeni JSON ile audit), `ISecurityLogger` + `SecurityLogger`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Repository'ler (sayfalı, filtreli): `IActivityLogRepository`, `IApplicationLogRepository`, `ISecurityLogRepository`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Entegrasyon: auth akışlarına security log (LoginFailed, OtpFailed, RateLimitHit), `GET /health`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `ActivityLoggerTests`, `SecurityLoggerTests` (DB context mock/in-memory)
- [ ] ➜ **API Yol Haritası'na işle**

### A-05 — Sistem Kelimesi API (Words) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §5
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi)
> 🧩 `frontendRefs` ↔ B-03 `backendRef` (iki yönlü).
- [ ] **Entity:** `Language`, `WordConcept`, `Word`, `WordDetail`, `WordExample` + EF config + migration
      + `Language` seed (`de`, `tr`) — bkz. `DATABASE_SCHEMA/Icerik.md` (çoklu dile açık şema:
      kategori/seviye `WordConcept` üzerinde, gramer `WordDetail.GrammarData` JSON'da dile göre değişir)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IWordService` + `WordService` (liste filtre+sayfa, detay, CRUD Admin — bir kelime tüm dilleriyle
      (`translations[]`) tek işlemde oluşturulur/güncellenir, duplikat 409 + `?force=true`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `WordController` (`[Authorize]` liste/detay, `[Authorize(Roles="Admin")]` CRUD)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `WordServiceTests` (liste filtre, duplikat 409 + force, CRUD yetki)
- [ ] ➜ **API Yol Haritası'na işle**

### A-06 — Kategori API (Categories) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §6
**Frontend karşılığı:** B-04 (Admin — Kategori Yönetimi), D-06 (Web — Kategoriler Sayfası), E-08 (Mobil — Kategoriler Ekranı)
> 🧩 `frontendRefs` ↔ B-04/D-06/E-08 `backendRef` (iki yönlü).
- [ ] **Entity:** `Category` (self-ref hiyerarşi), `CategoryTranslation` (dil başına ad), `WordCategory`
      ara tablo (`WordConceptId`↔`CategoryId`) + EF config + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ICategoryService` + `CategoryService` (hiyerarşik liste, kategoriye ait kelimeler, CRUD Admin)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Silme koruması (alt kategori/aktif kelime varsa 409), `CategoriesController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `CategoryServiceTests` (hiyerarşik liste, silme koruması 409)
- [ ] ➜ **API Yol Haritası'na işle**

### A-07 — Admin API (Kullanıcı Yönetimi + İstatistik + Log Görüntüleme) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §11
**Frontend karşılığı:** B-05 (Kullanıcı Yönetimi), B-06 (Paylaşım/İçerik Moderasyonu), B-07 (İstatistik Paneli), B-08 (Log Görüntüleme Paneli)
> 🧩 `frontendRefs` ↔ B-05/B-06/B-07/B-08 `backendRef` (iki yönlü — bu API'nin farklı endpoint'leri
> farklı admin sayfalarına dağılır).
- [ ] `IAdminService` + `AdminService`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Kullanıcı: liste/arama/detay, rol değiştir, hesap dondur/aktif (her işlem **ActivityLog**'a)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] İçerik moderasyonu (kart liste + silme), genel istatistik, toplu kelime import
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Log görüntüleme:** `GET /admin/logs/activity`, `/admin/logs/application`, `/admin/logs/security` (filtre+sayfa)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `AdminController` (`[Authorize(Roles="Admin")]`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `AdminServiceTests` (rol değiştir, dondur/aktif, import, log filtreleme)
- [ ] ➜ **API Yol Haritası'na işle**

### A-08 — Medya / Dosya Yükleme API ⬜
**Referans:** REFERENCE/ENV.md §7
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi formundaki görsel yükleme)
> 🧩 `frontendRefs` ↔ B-03 `backendRef` (iki yönlü).
- [ ] `IFileStorageService` + `LocalFileStorageService`, `Word.ImageUrl` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `MediaController` (`POST /media/images/upload`), `UseStaticFiles`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `FileStorageServiceTests` (boyut/uzantı doğrulama, benzersiz ad üretimi)
- [ ] ➜ **API Yol Haritası'na işle**

### A-09 — SMTP Ayarları API ⬜
**Referans:** REFERENCE/SECURITY.md §3.2, REFERENCE/ENV.md §5
**Frontend karşılığı:** B-09 (Admin — SMTP Ayarları Sayfası)
> 🧩 `frontendRefs` ↔ B-09 `backendRef` (iki yönlü).
> SMTP bilgileri DB'de AES-256 şifreli; admin panelden yönetilir, `appsettings.json`'da DEĞİL.
- [ ] **Entity:** `SmtpSettings` (Host, Port, EnableSsl, Username, **PasswordEncrypted**, FromEmail, FromName, UpdatedBy) + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IEncryptionService` + `AesEncryptionService` (AES-256-CBC, rastgele IV, anahtar `AES_ENCRYPTION_KEY`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ISmtpSettingsRepository`, `SmtpSettingsController` (Admin): `GET` (şifre `***`), `PUT`, `POST .../test`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `AesEncryptionServiceTests` (encrypt/decrypt round-trip, 32 byte anahtar kontrolü)
- [ ] ➜ **API Yol Haritası'na işle**

### A-10 — E-posta Servisi + Hesap Temizleme Görevi ⬜
**Referans:** REFERENCE/SECURITY.md §7
- [ ] `SmtpEmailService` (MailKit; SMTP'yi repo'dan alır, `Decrypt` ile çözer) + DI (dev→Dev, prod→Smtp)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] E-posta şablonları (doğrulama, login OTP, şifre sıfırlama, hesap silme onayı, şifre değişti, hesap kurtarıldı)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `AccountCleanupBackgroundService : IHostedService` (PII anonimleştirme, günde 1, 03:00 UTC)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `AccountCleanupServiceTests` (30 gün grace sonrası anonimleştirme, blok hash'i)
- [ ] ➜ **API Yol Haritası'na işle**
