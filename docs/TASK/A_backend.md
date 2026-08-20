# FAZ A — Backend (`.NET 9 Web API`)

> **Yöntem/standart:** Bu dosyadaki her task, `../../CLAUDE.md` §3/§6 kurallarına göre yazılır
> (dikey dilim, MediatR CQRS, parça yazılır yazılmaz `AKADEMI/backend/`ye işlenir). O bölümler
> değişmez standarttır — burada tekrar edilmez, her zaman `../../CLAUDE.md`'ye bakılır.

> **2026-08-08 — Baştan yazım:** Önceki backend kodu (`backend/`) ve onu öğreten `AKADEMI/backend/`
> tamamen silindi (kullanıcı kararı) — git geçmişinde duruyor, kayıp değil. Backend artık "Admin
> Panel Backend" / "Kullanıcı Backend" diye ikiye ayrılmıyor — TEK, ortak bir backend, TEK bir faz
> olarak baştan tasarlandı. `docs/DATABASE_SCHEMA/` ve `docs/REFERENCE/` (API_ENDPOINTS,
> ARCHITECTURE, SECURITY, TECHNICAL_SPECIFICATIONS, ENV, GERMAN/TURKISH_LANGUAGE_FEATURES)
> korundu — aşağıdaki task'lar bunları blueprint olarak kullanır. **Aşağıdaki sıralama, eski
> A/C fazlarının iki bilinçli tasarım hatasını düzeltir:** (1) eski C-01 (`/users/me/statistics`)
> `UserProgress` yazılmadan önce geliyordu (yarım/anlamsız istatistik döndürüyordu) — burada
> **Kullanıcı Profil API**, SRS/İlerleme'den SONRAYA alındı, ilk günden gerçek veri döner;
> (2) eski A-07'nin "UserCard Moderasyonu" maddesi `UserCard` entity'si henüz yokken planlanmış,
> sonradan **A-07.1** retrofit'ine ertelenmişti — burada **Admin API**, Kişisel Kart API'sinden
> SONRAYA alındı, moderasyon ilk seferde tam yazılır, ayrı bir retrofit task'ı gerekmez.

> **2026-08-12 — Gözden geçirme notu:** Aşağıdaki 9 madde, tüm task listesi üzerinden yapılan bir
> denetimde tespit edildi ve ilgili task'lara işlendi. Her biri task'ın içinde `⚠️ [2026-08-12]`
> etiketiyle işaretli, böylece nereden geldikleri kaybolmuyor:
> 1. **İlk admin hesabı oluşturma eksik** (A-03 → yeni **A-03.2**)
> 2. **A-06 "orphan terfi" testi ile silme koruması çelişiyor** (A-06)
> 3. **`UserCardUserCategories`/`UserCardUserCategory` isim tutarsızlığı** (A-08, A-10)
> 4. **Süresi geçmiş token/session/OTP temizliği planlanmamış** (A-17)
> 5. **A-14 Paylaşım API'nin neyi paylaştığı belirsiz** (A-14)
> 6. **A-15 `ClassWord`'ün sistem kelimesiyle FK'i ve `UserProgress` yansıması belirsiz** (A-15)
> 7. **A-18 toplu import'ta kategori ataması yok** (A-18)
> 8. **Health check endpoint'i yok** (A-02)
> 9. **API versiyonlama stratejisi belirtilmemiş** (A-01)

### A-01 — Proje İskeleti ✅
**Referans:** REFERENCE/DEVELOPMENT_SETUP.md §3, REFERENCE/ENV.md
- [x] Solution + 4 proje (API, Application, Infrastructure, Domain) + Tests + referanslar (Domain ← Infra ← App ← API)
- [x] NuGet paketleri (REFERENCE/TECHNICAL_SPECIFICATIONS.md §1), `appsettings*.json`, `Program.cs` temel yapı
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-01_proje-iskeleti/` (2 bölüm)
- [x] ⚠️ **[2026-08-12] API versiyonlama kararı:** Route prefix `/api/v1/...` olarak baştan
      sabitlenir (`Program.cs`'te tek yerden `MapControllers` öncesi ayarlanır). Şimdilik tek versiyon
      var — `Asp.Versioning` gibi bir kütüphane eklenmez (YAGNI), yalnızca URL prefix'i ileride
      `v2` açılabilecek şekilde baştan konur. Bu karar geriye dönük A-03+ tüm controller'ları etkiler,
      bu yüzden A-01'e (ilk task) eklendi. → `RoutePrefixConvention` (`Zausel.API/Conventions/`,
      `IApplicationModelConvention`) tüm controller'lara `"api/v1"` önekini ekler; `Program.cs`'te
      `AddControllers(options => options.Conventions.Add(...))` ile TEK yerden bağlanır.
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-01_proje-iskeleti/02_route-versiyonlama.html`

### A-02 — Ortak Altyapı ✅
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §4, §7
*(Feature entity'leri YOK — yalnızca her API'ın ihtiyaç duyduğu paylaşılan temel.)*
> **A-02 sırasında düzeltilen 2 şey (kayıt altına alınmış kararlar):** (1) A-01'de proje referansları
> kurulurken `Zausel.Infrastructure` yalnızca `Zausel.Domain`'e referans veriyordu, Application'a
> değil — `Repository<T>`'nin `IRepository<T>`'yi implemente edebilmesi için bu eksik fark edilip
> düzeltildi (`DEVELOPMENT_SETUP.md` güncellendi). (2) `SECURITY.md §1.4`'teki "EntityNotFoundException
> için 404 yanıtı ex.Message (yalnızca Türkçe)" notu `CLAUDE.md §1`/`CODING_STANDARDS.md`'nin
> "exception .Message İngilizce" kuralıyla çelişiyordu — çözüldü: `.Message` her zaman İngilizce/log-only,
> istemciye giden metin her zaman `Code` + `ErrorMessages` sözlüğü üzerinden üretilir (`EntityNotFoundException`
> kendi `Code` alanını taşıyor, `AppException`'dan türemeden aynı ilkeyi uyguluyor).
- [x] `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt, CreatedByUserId, UpdatedByUserId, DeletedByUserId)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `ZauselDbContext` (boş; `ApplyConfigurationsFromAssembly`, soft delete filter, `SaveChangesAsync` override)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `EntityNotFoundException` (Repository<T>.SoftDeleteAsync'in bağımlılığı olduğu için Repository'den önce yazılır)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `IRepository<T>` + `Repository<T>` generic base + `AddInfrastructureServices()`
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `ApiErrorResponse` (`{ error: { code, message }, success }`) — ilk gerçek controller'dan önce
      spekülatif ortak DTO (`ApiResponse<T>`/`PagedResult<T>` vb.) **açılmaz**, her tip onu fiilen
      kullanan ilk task'ta yazılır (CLAUDE.md §3 YAGNI kuralı); istemciye giden hata metnini dile göre
      çözen `ErrorMessages` sözlüğü de bu adımda yazıldı (`Application/Common/ErrorMessages.cs`)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] Middleware: global exception handling, security headers, request/response log
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `Program.cs`: JWT auth, CORS, Serilog, FluentValidation, MediatR kayıtları (AutoMapper yalnızca
      koşullu — CLAUDE.md §3 "AutoMapper Profile yalnızca" kuralı, ilk gerçek Entity→DTO dönüşümünde eklenir)
- [x] **Birim testleri:** `RepositoryTests` + `EntityNotFoundExceptionTests` (in-memory DB, CRUD + soft delete filtresi + exception mesaj formatı)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] ⚠️ **[2026-08-12] Health check endpoint'i:** `GET /health` (`[AllowAnonymous]`, auth'tan ve
      versiyon prefix'inden BAĞIMSIZ — yani `/health`, `/api/v1/health` değil) — `AddHealthChecks()` +
      DB bağlantı kontrolü (`AddDbContextCheck<ZauselDbContext>`). Deployment/monitoring için
      gerekli, spekülatif değil — ilk günden ihtiyaç duyulur, bu yüzden A-02'ye (ortak altyapı) eklendi.
      → `MapHealthChecks("/health", ...)` minimal API endpoint'i olarak (Controller DEĞİL, bu yüzden
      `RoutePrefixConvention`'dan hiç etkilenmiyor) — özel `ResponseWriter` `{ status, databaseConnected,
      timestampUtc }` şeklini üretir, sağlıksızsa (DB bağlanamıyorsa) 503 döner (canlı doğrulandı).
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-02_ortak-altyapi/07_saglik-kontrolu.html`

### A-03 — Auth API ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §3, §3.1, REFERENCE/SECURITY.md §1.3/§2, REFERENCE/TECHNICAL_SPECIFICATIONS.md §5-6
**Frontend karşılığı:** B-02/B-02.1 (Admin — sade giriş+OTP+QR), C-03/C-03.1 (Web — tam akış+Google+QR), D-05/D-05.1 (Mobil — tam akış+Google+Apple+QR tarayıcı)
> Eski turda QR ile giriş / tema tercihi / dil tercihi / başarı mesajı lokalizasyonu dört ayrı
> "retrofit" task'ı olarak sonradan eklenmişti (kullanıcı ihtiyacı iterasyon sırasında ortaya
> çıkmıştı). Artık hepsi baştan biliniyor — bu task hepsini **ilk turda** kapsar, ayrı retrofit
> task'ı açılmaz.
> **A-03 sırasında verilen bir tasarım kararı:** `User`/`RefreshToken`, `BaseEntity`'den BİLİNÇLİ
> olarak türemiyor — gerçek DB şemaları (DATABASE_SCHEMA/Auth.md) BaseEntity'nin "kim yaptı"
> alanlarını (User) ya da soft-delete alanlarını (RefreshToken, kendi IsUsed/RevokedAt deseni var)
> taşımıyor. `QrLoginSession` ise BaseEntity'nin tüm alanlarını birebir taşıdığı için ondan türüyor.
> Bu, CLAUDE.md §1 "her tablo BaseEntity taşır (log tabloları hariç)" kuralına yeni, kayıt altına
> alınmış iki istisna daha ekliyor (User, RefreshToken).
- [x] **Entity:** `User` (Role/IsActive/CurrentLevel/**ThemePreference**[Light|Dark|System, CHECK
      constraint]/**LanguagePreference**[tr|de, CHECK constraint] dahil), `RefreshToken`,
      `QrLoginSession` + `OtpPurpose`/`QrLoginStatus` enum + EF config + migration
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `IPasswordService` (BCrypt wf:12 + SHA-256 token hash), `ITokenService` (JWT access 15dk +
      refresh, algorithm-confusion önlemi, claim'ler: NameIdentifier/Email/Role/firstName —
      Theme/LanguagePreference JWT'ye GİRMEZ, yalnızca yetki taşınır)
- [x] ➜ **AKADEMI/backend'ye işle**
> **OtpService/LoginCompletionService sırasında verilen iki tasarım kararı:** (1) SECURITY.md §1'deki
> "3 yanlış → kod geçersiz" kuralı denemeleri isteğe göre saymak için kalıcı bir sayaç gerektiriyordu —
> `Users.PendingOtpCodeAttempts` (`INT DEFAULT 0`) eklendi, `DATABASE_SCHEMA/Auth.md` ve
> `AddOtpAttemptsToUser` migration'ıyla senkron. (2) `LoginCompletionService`'in `IsAnonymized` hesabı
> reddetmesi için ilk gerçek `AppException` alt sınıfı gerekti — `Application/Common/Exceptions/AppException.cs`
> (Code + HttpStatusCode, SECURITY.md §1.4) ve `AccountAnonymizedException` bu adımda yazıldı,
> `ExceptionHandlingMiddleware` genel bir `catch (AppException)` koluyla güncellendi (gelecekteki tüm
> `AppException` alt sınıfları — ör. 13 Auth Handler'ın `InvalidCredentialsException`'ı — middleware'e
> dokunmadan otomatik yakalanır). Her iki servis de OtpPurpose/User'ı mutasyona uğratır, DB'ye YAZMAZ —
> persist çağıran Handler'ın işi (`ITokenService`/`IPasswordService` ile aynı "saf mantık" deseni).
- [x] `IOtpService`/`OtpService` (Register/Login/ResetPassword/AccountDeletion ortak OTP üretimi/
      doğrulaması), `ILoginCompletionService`/`LoginCompletionService` (OTP/Google/Apple/QR
      girişlerinin ortak son adımı: grace period kurtarma, token üretimi)
- [x] ➜ **AKADEMI/backend'ye işle**
> **13 Handler sırasında verilen tasarım kararları:** (1) `User`/`RefreshToken` BaseEntity'den
> türemediği için generic `IRepository<T>` kullanılamıyor — `IUserRepository`/`UserRepository` ve
> `IRefreshTokenRepository`/`RefreshTokenRepository` (Auth'a özel, dar arayüz; Update metodu YOK,
> EF change tracking + tek `SaveChangesAsync` yeterli, OtpService/LoginCompletionService'in "servis
> saf mantık taşır" deseniyle aynı). `ZauselDbContext`'e de bu adımda ilk kez `DbSet<User>`/
> `DbSet<RefreshToken>` eklendi (önceden yalnızca `ApplyConfigurationsFromAssembly` ile örtük
> tanımlıydı). (2) Task metninde yalnızca `IAppleTokenValidator` anılmış olsa da `IGoogleTokenValidator`
> da eklendi — `GoogleJsonWebSignature.ValidateAsync` statik bir çağrı, arkasına bir arayüz konmazsa
> `LoginWithGoogleCommandHandler` testi CODING_STANDARDS.md §6.4'ün "Google/Apple her zaman mock"
> kuralına uyamaz. (3) `IEmailService` A-20'nin 6 şablonunu (doğrulama/login OTP/şifre sıfırlama/
> şifre değişti/hesap silme onayı/hesap kurtarıldı) baştan tanımlıyor — A-03 yalnızca konsola loglayan
> `DevEmailService`'i sağlıyor. (4) Başarı mesajı gerektiren uçlar (Login adım 1, ResendVerification,
> VerifyEmail, Logout, ForgotPassword, ResetPassword, RequestAccountDeletion, ConfirmAccountDeletion)
> `MediatR.Unit` döner — hardcoded Türkçe metin YAZILMADI (CLAUDE.md §1), localize edilmiş `message`
> alanı `SuccessMessages.cs` + `AuthController` yazılınca (bu dosyanın birkaç madde altındaki ayrı
> checkbox'ları) eklenecek, `ErrorMessages`/`ExceptionHandlingMiddleware` ile simetrik.
- [x] 13 Auth Command+Handler (`Application/Features/Auth/`): Register, VerifyEmail,
      ResendVerification, Login, VerifyLoginOtp, LoginWithGoogle, LoginWithApple, Refresh, Logout,
      ForgotPassword, ResetPassword, RequestAccountDeletion, ConfirmAccountDeletion + `IEmailService`
      sözleşmesi + `DevEmailService` (gerçek SMTP gönderimi A-20'de) + `IAppleTokenValidator`
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03_auth-api/` 08-15. bölümler (repository
      katmanı, yeni istisnalar, e-posta/sosyal giriş servisleri, 13 Handler); `postman` slaytları
      henüz YOK — `AuthController` yazılana kadar bilerek eklenmiyor (STANDART.md kuralı, endpoint
      bir controller'a bağlanınca eklenir)
- [x] 5 QR Login Command+Handler (`Application/Features/QrLogin/`): Generate/Scan/Confirm/Deny/
      GetStatus (Confirmed'de `ILoginCompletionService` ile tek seferlik token) +
      `QrSessionGoneException`(410)/`QrSessionForbiddenException`(403)
> **QR Login sırasında verilen 2 tasarım kararı:** (1) `QrLoginSession` `BaseEntity`'den türese de
> token-hash'e göre arama generic `IRepository<T>`'de yok — `IUserRepository`/`IRefreshTokenRepository`
> ile aynı dar-arayüz deseni: `IQrLoginSessionRepository` (Update metodu yok, EF change tracking
> yeterli). (2) Süre dolumu yorumu iki yerde farklı davranır: Scan/Confirm/Deny (aksiyon uçları)
> süresi geçmiş bir session'ı sessizce `QrSessionGoneException`'a çevirir, DB'ye ayrıca yazmaz (zaten
> hiçbir mutasyon olmamış); `GetQrLoginStatusQuery` (polling ucu) ise `Expired`'ı gerçek, DB'ye
> yazılan bir durum olarak üretir — hem UI'a "süresi doldu" diye ayrı bir durum döner hem de A-17'nin
> `ExpiredTokenCleanupJob`'unun `Status` filtresine (Confirmed/Denied/Expired) veri sağlar.
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03_auth-api/` 16-20. bölümler (repository
      katmanı, 2 yeni istisna + 3 DTO, 5 Handler); `postman` slaytları henüz YOK — `QrLoginController`
      yazılana kadar bilerek eklenmiyor (STANDART.md kuralı, endpoint bir controller'a bağlanınca eklenir)
- [x] Başarı mesajları (`MessageResponse` döndüren Command'lar) — `ErrorMessages.cs` deseniyle
      simetrik bir `SuccessMessages.cs` (Code + `Accept-Language`'a göre tr/de çözümü), hardcode
      Türkçe metin **yazılmaz**
> **Not:** Code seçimi Handler'da DEĞİL, Controller'da yapılacak (Handler `Unit` dönmeye devam
> eder) — 10 Code tanımlandı: 8 Auth (`LOGIN_OTP_SENT`, `VERIFICATION_RESENT`, `EMAIL_VERIFIED`,
> `LOGGED_OUT`, `PASSWORD_RESET_OTP_SENT`, `PASSWORD_RESET`, `ACCOUNT_DELETION_OTP_SENT`,
> `ACCOUNT_DELETED`) + 2 QR (`QR_LOGIN_CONFIRMED`, `QR_LOGIN_DENIED`). `MessageResponse`
> (`Application/DTOs/MessageResponse.cs`) `ApiErrorResponse`'un başarı karşılığı — flat `DTOs/`
> klasöründe, Auth'a özel değil.
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03_auth-api/21_basari-mesajlari.html`
- [x] `AuthController` (13 endpoint) + `QrLoginController` (5 endpoint — task metnindeki "4" API_ENDPOINTS.md
      §3.1 tablosunun confirm+deny'yi TEK satırda göstermesinden kaynaklı bir sayım farkı, gerçekte
      5 ayrı action) + FluentValidation + rate limiting (100/dk general, 10/dk anonymous, 5/15dk
      login, 20/saat qrGenerate, 40/dk qrStatus)
> **Controller katmanı sırasında verilen tasarım kararları:** (1) "(ThemePreference/LanguagePreference
> dahil)" ibaresi bu task'ta YANLIŞ/eski bir kalıntıydı — A-03'ün hiçbir Command'ı bu iki alanı
> INPUT olarak almıyor (A-12'nin `PUT /users/me`'sine ait), FluentValidation bu ikisi için hiç kural
> içermiyor. (2) `ValidationBehavior` (MediatR pipeline) + `ApiErrorResponse.Details[]` (kullanıcıyla
> netleştirildi: FluentValidation birden fazla kural aynı anda başarısız olursa TÜMÜ döner, yalnızca
> `error.code`/`error.message` ilk kuralı taşımaya devam eder — geriye dönük uyumlu). (3) Request
> DTO'ları Command'ların TÜM alanlarını yansıtmaz — Language/UserId/IpAddress/DeviceInfo gibi
> HTTP-bağlamından gelen alanlar `ApiControllerBase`'den ayrıca eklenir (over-posting/JSON çakışması
> önlemi). (4) **Canlı testte bulunan bug:** `GenerateQrLoginCommand`'ın standart Base64 token'ı
> (`/`, `+` içerebilir) URL path segmentinde (`/auth/qr/{token}/...`) routing'i bozuyordu — gerçek
> sunucu + gerçek HTTP isteğiyle test edilene kadar fark edilmedi, URL-safe Base64'e (`-`/`_`,
> dolgusuz) çevrilerek düzeltildi. (5) Test sırasında `ZauselDB`'nin 2026-08-08'de silinen
> eski backend'in artığı olduğu keşfedildi (Words/Categories/SmtpSettings gibi artık kod tabanında
> olmayan tablolar + eksik migration geçmişi) — kullanıcı onayıyla DB sıfırlanıp güncel 2 migration
> temiz uygulandı.
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03_auth-api/` 22-38. bölümler (ValidationBehavior,
      Details[]/HttpContextExtensions, ApiControllerBase, 15 validator, ErrorMessages diff, rate
      limiting, 18 endpoint kod+postman, URL-safe token bugfix)
- [x] **Birim testleri:** 13+5 Command Handler testi, `OtpServiceTests`, `LoginCompletionServiceTests`,
      `JwtTokenServiceTests`, `PasswordServiceTests`
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03_auth-api/` 39-43. bölümler (39: AAA/mock/
      adlandırma/kapsam standardı — kullanıcı isteğiyle "nasıl yazılır" ayrı bir bölüm oldu; 40: 13
      Auth handler testi TEK dosyada, her Handler kendi kod slaytında art arda — kullanıcı isteğiyle
      temsili öğretim istisnası bu bölüm için BİLEREK kaldırıldı, 18 test dosyasının HEPSİ tam satır
      kapsamıyla işlendi; 41: 5 QR handler testi, aynı desen; 42: `dotnet test` ile çalıştırma/filtreleme;
      43: kapanış özet+sözlük). Süreçte 2 tur kalite sorunu bulunup düzeltildi: (1) `karsilastirma`/
      `sozluk` slayt şeması yanlış kullanılmıştı (`iyi`/`kotu` düz string yerine `{baslik,maddeler[]}`
      objesi, `tanim` yerine `aciklama` alanı) — motor kodundan doğru şema çıkarılıp düzeltildi;
      (2) "bu satır N kez geçiyor" notlarının bir kısmı yanlış sayıyordu ve birkaç satır hiç
      açıklanmamıştı (satirlar[] boşluğu, motor sessizce atlıyor) — bunu insan gözüyle tekrar tekrar
      bulmak yerine `window.MODULE`'ü gerçekten çalıştırıp kod bloğuyla satirlar[]'ı karşılaştıran
      iki script yazıldı (kapsam + sayım doğrulama), 43 dosyanın tamamı bu scriptlerden temiz geçene
      kadar iterasyon yapıldı.

### A-03.2 — İlk Admin Hesabı ✅ ⚠️ **[2026-08-12 — yeni task, tespit edilen boşluk]**
**Neden gerekli:** `AdminController` (A-18) ve tüm `[Authorize(Roles="Admin")]` uçları, sistemde
zaten bir Admin olmasını şart koşuyor. `UpdateUserRoleCommand` (A-18) bir kullanıcıyı Admin yapabilir
ama bunu çağırmak için de zaten Admin olmak gerekiyor — döngüsel bir bağımlılık. Register akışı
(A-03) varsayılan olarak yalnızca `User` rolüyle kayıt açıyor, hiçbir task ilk Admin'i oluşturmuyordu.
Bu task A-03'ten SONRA (User entity'si hazır), A-18'den ÖNCE (Admin uçları test edilebilsin diye) gelir.
- [x] `IAdminSeedService`/`AdminSeedService` (`Application/Interfaces/Services/`, `Application/Services/`
      — flat, CLAUDE.md §3 "paylaşılan mantık" deseni) — `Program.cs`'te `app.Run()`'dan önce bir
      scope içinde çağrılır, `IConfiguration`'dan okunan `INITIAL_ADMIN_EMAIL`/`INITIAL_ADMIN_PASSWORD`
      ile, yalnızca **o e-posta hiç yoksa** bir `User` (Role=Admin, IsActive=true, IsEmailVerified=true,
      EmailVerifiedAt=now) oluşturur — idempotent, her `Program.cs` başlangıcında güvenle tekrar
      çalışabilir; ikisi de tanımlı değilse veya e-posta formatı geçersizse (`MailAddress.TryCreate`)
      sessizce atlar
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03.2_ilk-admin-hesabi/` (1 bölüm)
- [x] `REFERENCE/ENV.md`'ye `INITIAL_ADMIN_EMAIL`/`INITIAL_ADMIN_PASSWORD` eklendi (§9) — yalnızca
      geliştirme/staging'de `appsettings.Development.json`'da (yerel dosyaya da eklendi, `.gitignore`'da);
      prod'da secret olarak verilir, ilk girişten sonra şifre değiştirilmesi önerilir — dokümantasyon
      notu, kod zorlaması değil
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03.2_ilk-admin-hesabi/` (1 bölüm)
- [x] **Birim testleri:** `AdminSeedServiceTests` (yoksa oluşturur, varsa dokunmaz/idempotent, env
      değişkenleri tanımsızsa/e-posta formatı geçersizse sessizce atlar) — 4/4 yeşil, tüm paket 99/99 yeşil
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-03.2_ilk-admin-hesabi/` (1 bölüm)

> **Not (2026-08-14):** Kod+ENV+test tamamlandı, canlı doğrulama yapıldı — gerçek DB'ye karşı `dotnet run`
> ile ilk çalıştırmada `admin@zausel.com` (Role=Admin) oluştuğu SQL loglarında görüldü
> (`INSERT INTO [Users]... Initial admin account seeded for admin@zausel.com`), ikinci
> çalıştırmada idempotent davrandığı doğrulandı (INSERT YOK, sessiz atlama — DB'de zaten var).
> Akademi işleme de tamamlandı — `AKADEMI/backend/A-03.2_ilk-admin-hesabi/` (1 bölüm: kavram,
> `IAdminSeedService`/`AdminSeedService` kod, `DependencyInjection.cs`/`Program.cs` kod-değişiklik,
> `AdminSeedServiceTests` — 4 senaryo, sözlük, özet). A-03'ün kök `AKADEMI/backend/index.html`
> kartı da bu görevle birlikte "tamamlandı" olarak nihai hale getirildi. Task ✅.

### A-04 — Loglama Sistemi ✅
**Referans:** REFERENCE/SECURITY.md §6, DATABASE_SCHEMA/Loglama.md
**Frontend karşılığı:** B-08 (Admin — Log Görüntüleme Paneli)
> `ActivityLog`/`SecurityLog`'un `UserId` FK'i `Users`'a bağlı (SET NULL) — bu yüzden A-03'ten
> SONRA gelir. A-03'ün handler'ları bu task bitene kadar loglama YAPMAZ; bu task'ın bir parçası
> olarak A-03'e (ve varsa QR akışına) **geriye dönük** `IActivityLogger`/`ISecurityLogger`
> çağrıları eklenir — tek seferlik, planlı bir entegrasyon adımı, sürpriz retrofit değil.
- [x] **Entity:** `ActivityLog`, `ApplicationLog`, `SecurityLog` + `LogEventType` enum + EF config +
      migration — hiçbiri `BaseEntity`'den türemez (insert-only, soft delete yok)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-04_loglama-sistemi/01_loglama-entity-katmani.html`
- [x] Serilog `Serilog.Sinks.MSSqlServer` → `ApplicationLogs` (konsol+dosya+DB), `RequestResponseLoggingMiddleware`
      — canlı doğrulama yapıldı (`dotnet run` + `/health` isteği sonrası tabloda gerçek satırlar görüldü)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-04_loglama-sistemi/02_serilog-mssql-sink.html`
- [x] `IActivityLogger`/`ActivityLogger` (OldValue/NewValue JSON diff), `ISecurityLogger`/`SecurityLogger`
      (e-posta `IPasswordService.HashToken` ile hash'lenip `EmailHash`'e yazılır — ham e-posta ASLA loglanmaz)
      — yazma-amaçlı `IActivityLogRepository`/`ISecurityLogRepository` de bu adımda açıldı (okuma/sayfalama
      bir sonraki adımda eklenecek)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-04_loglama-sistemi/03_activity-security-logger.html`
- [x] `IActivityLogRepository`/`IApplicationLogRepository`/`ISecurityLogRepository` (sayfalı, filtreli — `PagedResult<T>`'in ilk gerçek tüketicisi)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-04_loglama-sistemi/04_paged-repository.html`
- [x] **A-03 retrofit:** LoginFailed/OtpFailed(4 akış)/TokenReplay/RateLimitHit/QrLoginConfirmed/
      QrLoginDenied + PasswordReset/AccountDeletion başarı olayları — `SecurityLog.Detail` serbest
      metin DEĞİL bir Code (CLAUDE.md "İkinci istisna" — admin okurken KENDİ `Accept-Language`'ıyla çözülür)şimd.
      Canlı doğrulama yapıldı: yanlış şifreyle `/auth/login` isteği `SecurityLogs`'a doğru
      `EventType`/`EmailHash`/`IpAddress`/`Detail` ile yazıldı.
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-04_loglama-sistemi/05_a03-retrofit.html`
- [x] **Birim testleri:** `ActivityLoggerTests`, `SecurityLoggerTests` (Bölüm 3), 3 Repository testi (Bölüm 4),
      A-03 handler testlerine eklenen log-doğrulama senaryoları (Bölüm 5) — 109/109 tüm paket yeşil
- [x] ➜ **AKADEMI/backend'ye işle** — testler ilgili bölümlerin `kod` slaytlarında birlikte işlendi

### A-05 — Sistem Kelimesi API (Words) ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §5, §5.1, §5.2, REFERENCE/GERMAN_LANGUAGE_FEATURES.md §10, REFERENCE/TURKISH_LANGUAGE_FEATURES.md §9
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi)
> Dil listesi endpoint'i (eski A-05.1) ve Türkçe `vowelHarmony`/`possessive` zorunluluğu (eski
> A-05.2) artık baştan biliniyor — bu task ilk turda kapsar.
- [x] **Entity:** `Language` (`BaseEntity`'den TÜREMEZ — statik seed/referans tablosu, audit
      gerekmez) + seed (`de`, `tr`); `WordConcept`/`Word`/`WordDetail`/`WordExample` (`BaseEntity`'den
      türer) + EF config + migration — canlı doğrulama yapıldı (migration DB'ye uygulandı, `Languages`
      tablosunda `de`/`tr` seed satırları `sqlcmd` ile teyit edildi)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-05_sistem-kelimesi-api/01_word-content-entity-katmani.html`
- [x] `WordGrammarValidator` (FluentValidation, `LanguageId`'ye göre dile dispatch): **`de`**
      (Noun: gender+plural+4 hâl zorunlu; Verb: 18 çekim+auxiliary+pastParticiple+koşullu
      `separablePrefix`; Diğer: GrammarData NULL), **`tr`** (Noun: plural+6 hâl+**vowelHarmony**+
      **possessive**[6 kişi] zorunlu; Verb: verbRoot+negativeForm+30 çekim; Diğer: GrammarData
      NULL) — `consonantMutation` bilinçli olarak dışarıda bırakılır (yalnızca ileri bir quiz
      özelliğinde kullanılacak, YAGNI)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-05_sistem-kelimesi-api/02_word-grammar-validator.html`
- [x] `ILanguageRepository`/`IWordConceptRepository` + `GetLanguagesQuery` + 5 Command/Query
      (Create/Update/Delete/GetById/GetWords) — `translations[]` 1 veya 2 dil tek işlemde, duplikat
      409+`?force=true`, tek dilse kavram "eşleşmemiş" kalır — canlı doğrulama yapıldı (`dotnet run`
      + `/health`, DI çözümlemesi sağlıklı); süreçte `WordGrammarValidator`'ın `AddValidatorsFromAssembly`
      otomatik taramasına yakalanıp uygulamayı açılışta çökerttiği bulunup `Program.cs`'e filtre eklenerek düzeltildi
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-05_sistem-kelimesi-api/` 03-06. bölümler
- [x] `LanguagesController` (`GET /languages`, `[Authorize]`), `WordsController` (`[Authorize]`
      liste/detay, `[Authorize(Roles="Admin")]` CRUD)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-05_sistem-kelimesi-api/07_languages-ve-words-controller.html`

> **Not (2026-08-17):** Canlı doğrulama sırasında SQL Server'da hâlâ projenin eski adıyla açılmış
> `VokabelMeisterDB` bulundu, `appsettings.Development.json`'daki bağlantı dizesi zaten yeni ada
> (`ZauselDB`) güncel ama o isimde bir DB henüz hiç oluşturulmamıştı. Eski `VokabelMeisterDB`
> silindi (içinde korunacak veri yoktu), `dotnet ef database update` ile migration'lardan sıfırdan
> `ZauselDB` oluşturuldu (`de`/`tr` seed dahil). Ardından admin girişiyle (`/auth/login` +
> `[DEV EMAIL]` loguna düşen OTP) alınan bir JWT ile `GET /languages` ve `GET/POST/PUT/DELETE
> /words` ALTISI da gerçek HTTP istekleriyle çağrıldı — 200/201/204/400(WORD_DEFINITION_REQUIRED)/
> 404(ENTITY_NOT_FOUND) durumlarının hepsi beklenen şekilde gözlemlendi.

- [x] **Eşleştirme:** `GetUnmatchedWordConceptsQuery` (`languageId` bazlı + `suggestedMatchConceptId`
      — `Definition` virgülle ayrılmış çoklu karşılığı token'lara bölünüp aranır) + `PairWordConceptsCommand`
      (`primaryId` kazanır, tür/kategori çakışması bloklamaz — dilin doğası) — canlı doğrulama yapıldı
      (`de`-only "Anrufbeantworter" + `tr`-only "telesekreter" oluşturulup iki yönde de doğru
      `suggestedMatchConceptId` bulundu, `POST /words/pair` ikisini birleştirdi, self-pair 400
      `SAME_CONCEPT_PAIR_NOT_ALLOWED` ile engellendi)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-05_sistem-kelimesi-api/08_eslestirme.html`
- [x] **`IActivityLogger`:** `CREATE_WORD`/`UPDATE_WORD`/`DELETE_WORD`/`PAIR_WORD_CONCEPTS` —
      `ApiControllerBase.CurrentUserRole` eklendi (JWT `role` claim'i, `ActivityLog.ActorRole`
      kaynağı), `UPDATE`/`DELETE`/`PAIR`'da "önce oku sonra değiştir" deseniyle `OldValue`
      yakalanıyor — canlı doğrulama yapıldı (`ActivityLogs` tablosuna gerçek isteklerle 6 satır
      yazıldı, `PAIR_WORD_CONCEPTS`'in `OldValue`si iki kavramı da içeren JSON'u `sqlcmd` ile
      doğrulandı)
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-05_sistem-kelimesi-api/09_activity-logger.html`
- [x] **Birim testleri:** `WordGrammarValidatorTests` (her dil×tür, 30 test) + 5 Command/Query
      Handler testi (Create/Update/Delete/GetById/GetWords, 20 test) + eşleştirme testleri
      (GetUnmatchedWordConceptsQuery/PairWordConceptsCommand, 9 test) — 89 yeni test, tüm paket
      165/165 yeşil
- [x] ➜ **AKADEMI/backend'ye işle** — `AKADEMI/backend/A-05_sistem-kelimesi-api/10_birim-testleri.html`

> **A-05 TAMAMLANDI (2026-08-18).** Entity katmanı (Language statik seed + WordConcept/Word/
> WordDetail/WordExample) → WordGrammarValidator (de/tr × Noun/Verb, 22+13 hata kodu) →
> Repository katmanı (ILanguageRepository/IWordConceptRepository) → 5 CRUD Command/Query →
> LanguagesController/WordsController (canlı doğrulandı, proje adı değişikliği nedeniyle
> ZauselDB migration'dan sıfırdan oluşturuldu) → Eşleştirme (GetUnmatchedWordConceptsQuery
> +PairWordConceptsCommand, iki yönlü öneri algoritması, canlı doğrulandı) → IActivityLogger
> (4 Handler, ApiControllerBase.CurrentUserRole, canlı doğrulandı) → 89 birim testi. Süreçte
> AKADEMI motorunda gerçek bir bug bulunup düzeltildi (`kod-degisiklik` slaytlarında context
> satırlarının satirlar[] metin çakışmasıyla yanlışlıkla tıklanabilir hâle gelmesi —
> `engine/slides-engine.js`'e `satirIndex` konum-tabanlı eşleştirme eklendi) ve kalıcı bir
> denetim script'i yazıldı (`AKADEMI/backend/_scripts/audit-bolum.js`) — STANDART.md §3.1/§4'e
> işlendi, A-05'in TÜM 10 bölümü ve öncesindeki gözden kaçmış 48 içeriksiz açıklama script ile
> bulunup düzeltildi. Sıradaki: `A-06` (Kategori API).

### A-06 — Kategori API (Categories) ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §6
**Frontend karşılığı:** B-04 (Admin), C-06 (Web), D-08 (Mobil)
> ⚠️ **[2026-08-12] Silme koruması ↔ "orphan terfi" çelişkisi çözüldü:** Eski task metninde hem
> "çocuğu olan kategori silinemez" (`CategoryHasChildrenException`) hem de birim testi listesinde
> "orphan terfi" birlikte anılıyordu — bu iki davranış aynı anda var olamaz (çocuğu olan kategori
> hiç silinemiyorsa öksüz çocuk senaryosu oluşmaz). **Karar:** silme koruması KALIR (çocuğu olan
> kategori silinemez, 409) — bu daha güvenli ve DATABASE_SCHEMA.md'deki self-ref FK Restrict
> kısıtıyla tutarlı. "Orphan terfi" ifadesi kaldırıldı, yerine gerçekte test edilmesi gereken
> senaryo yazıldı: **kategori taşıma** (`UPDATE`'te `ParentCategoryId` değişimi — bir kategori başka
> bir üst kategorinin altına taşınabilir, çocukları kendisiyle birlikte gelir, döngü kontrolü burada devreye girer).
- [x] **Entity:** `Category` (self-ref hiyerarşi), `CategoryTranslation`, `WordCategory` ara tablo
      (`WordConceptId`↔`CategoryId` — kategori dilden bağımsız) + EF config (CHECK MinLevel/MaxLevel,
      self-ref FK Restrict) + migration (12 kategori + 24 çeviri seed, `DATABASE_SCHEMA.md` sırasıyla)
      — canlı doğrulama yapıldı (migration DB'ye uygulandı, `sqlcmd` ile 12/24 satır teyit edildi)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `ICategoryRepository` (hiyerarşik liste, `HasChildrenAsync`/`HasActiveWordsAsync`/`WouldCreateCycleAsync`)
      + 4 Command/Query (Create/Update/Delete/GetCategories) ⚠️ **[2026-08-15]** `GetCategoryWords`
      (`GET /categories/{id}/words`) kapsam dışı bırakıldı — `GET /words?categoryId=` (aşağıdaki madde)
      ile birebir aynı veriyi döndürüyordu, hiçbir frontend task'ı onu değil bunu kullanıyordu (YAGNI,
      kullanılmayan bir endpoint yazılmaz); `API_ENDPOINTS.md` §6'dan da kaldırıldı
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] Silme koruması (`CategoryHasChildrenException`/`CategoryHasActiveWordsException`/`CategoryParentCycleException`, 409/400), `CategoriesController` (4 endpoint)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] `GET /words`'e `categoryId` filtresi + Word DTO'larına `categories[]` alanı — ⚠️ **[2026-08-18
      kullanıcı kararı]** kapsam `CreateWordCommand`/`UpdateWordCommand`'a `categoryIds[]` yazma desteğiyle
      GENİŞLETİLDİ (task metninde açıkça yoktu, ama `API_ENDPOINTS.md §5`'in `POST /words` örneği zaten
      `categoryIds` gösteriyordu — yoksa `categories[]`/`categoryId` filtresi hiçbir zaman veri döndürmezdi).
      `WordCreateRequest`/`WordUpdateRequest.CategoryIds` → Handler `ICategoryRepository.AllExistAsync` ile
      doğrular → `IWordConceptRepository.ReplaceWordCategoriesAsync` (translations[] ile AYNI "tam değişim"
      deseni, WordCategory BaseEntity olmadığı için hard delete+insert)
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] **`IActivityLogger`:** `CREATE_CATEGORY`/`UPDATE_CATEGORY`/`DELETE_CATEGORY`
- [x] ➜ **AKADEMI/backend'ye işle**
- [x] **Birim testleri:** hiyerarşik liste, **kategori taşıma** (üst kategori değişimi + döngü koruması), silme koruması (çocuk/aktif kelime), `categoryId` filtresi — 22 yeni test (4 Create+6 Update+4 Delete+4 GetCategories Category testi, +2 Create Word+2 Update Word+1 GetWords categoryId wiring testi), tüm paket 187/187 yeşil
- [x] ➜ **AKADEMI/backend'ye işle**

> **A-06 TAMAMLANDI (2026-08-18).** `Category`(self-ref, BaseEntity)/`CategoryTranslation`(plain,
> `Language` gibi audit'siz)/`WordCategory`(plain, `UserCardCategories` gibi saf M:N bağı) → EF config
> (CHECK MinLevel/MaxLevel, self-ref FK Restrict, 12+24 seed `HasData`) → `ICategoryRepository` +
> `CategoryAggregate` → 4 Command/Query (Create/Update/Delete/GetCategories, hiyerarşi düz listeden
> Handler'da `ILookup` ile kurulur) → silme koruması + `CategoriesController` → `GET /words` categoryId
> filtresi + `categories[]` (+ kullanıcı kararıyla `categoryIds[]` yazma, A-05'in `WordConceptAggregate`'ine
> geriye dönük `Categories` alanı eklendi) → `IActivityLogger` (3 Handler) → 22 birim testi. Canlı
> doğrulama yapıldı: hiyerarşik liste (`GET /categories`, `includeWordCount`, `level` filtresi), alt
> kategori oluşturma+`children[]`'da görünmesi, silme koruması (409 `CATEGORY_HAS_CHILDREN` →
> 409 `CATEGORY_HAS_ACTIVE_WORDS` izole edilerek), kategori taşıma (`PUT`), döngü koruması (400
> `CATEGORY_PARENT_CYCLE`), `POST /words` `categoryIds` ile kelime oluşturma → `GET /words?categoryId=`
> filtresi ve `categories[]` alanı doğru döndü, `ActivityLogs`'a 3 aksiyonun tamamı doğru yazıldı — test
> verileri temizlendi (soft-delete edilen seed kategori 1 geri alındı). Sıradaki: `A-07` (Medya API).

### A-07 — Medya / Dosya Yükleme API ⬜
**Referans:** REFERENCE/ENV.md §7
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi formundaki görsel yükleme)
> `WordConcept.ImageUrl` alanı A-05'te zaten var — bu task yeni migration açmaz, yalnızca oraya
> yazılacak URL'i üreten yükleme uç noktasını yazar.
- [ ] `IFileStorageService`/`LocalFileStorageService` (uzantı jpg/jpeg/png/webp + 5 MB + **içerik**
      [magic bytes] doğrulaması — yalnızca uzantı kontrolü GÜVENSİZ, bir `.exe` `.png` adıyla
      yüklenebilir), `Guid` tabanlı benzersiz ad üretimi
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `MediaController` (`POST /media/images/upload`, `[Authorize(Roles="Admin")]`, `IFormFile?`
      nullable + `FileRequiredException` — eksik dosya projenin standart `ApiErrorResponse`
      şeklinde döner, ASP.NET'in ham hata şekli DEĞİL), `[RequestSizeLimit]`, `UseStaticFiles`
      (auth'tan ÖNCE, `/uploads` herkese açık)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **`IActivityLogger`:** `UPLOAD_MEDIA` (`EntityType=Word`)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** boyut/uzantı/içerik doğrulama (spoofing regresyonu dahil), benzersiz ad üretimi
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-08 — Kişisel Kategori API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §8
**Frontend karşılığı:** C-06 (Web — Kategoriler Sayfası, kişisel sekme), D-08 (Mobil — Kategoriler Ekranı)
> A-10'daki (Kişisel Kart) `UserCardUserCategory` ara tablosunun FK verdiği `UserCategory`
> entity'si önce hazır olmalı — dikey dilim bütünlüğü için Kişisel Kart'tan ÖNCE gelir.
> ⚠️ **[2026-08-12]** Ara tablo adı `UserCardUserCategory` (tekil) olarak standartlaştırıldı — A-10'daki
> entity listesiyle birebir eşleşsin diye (önceki sürümde burada yanlışlıkla çoğul `UserCardUserCategories` yazılıydı).
- [ ] **Entity:** `UserCategory` + migration, `IUserCategoryService`/`UserCategoryController` (yalnızca sahibi, `UserId` filtresi zorunlu)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **`IActivityLogger`:** `CREATE_USER_CATEGORY`/`UPDATE_USER_CATEGORY`/`DELETE_USER_CATEGORY`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** sahiplik filtresi, CRUD
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-09 — SRS / İlerleme API (UserProgress) ⬜
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §8
**Frontend karşılığı:** C-11 (Web — İlerleme Sayfası), D-13 (Mobil — İlerleme Ekranı); C-05/D-07 (Öğrenme/Sınav) bu API'nin sonuçlarını dolaylı kullanır (bkz. A-11)
> `POST /user-cards/learn-system-word` (A-10'da yazılacak) bu entity'yi (`UserProgress`) kullanır
> — bu yüzden Kişisel Kart API'sından ÖNCE bitirilmesi gerekir.
- [ ] **Entity:** `UserProgress`, `UserCardProgress` (`NextReviewAt` **nullable** — NULL=yeni kelime
      havuzu, + `ConsecutiveIncorrect`/`IsSuspended` leech alanları), `LearningHistory` (+
      `HintUsed`/`IsExtraPractice`/`MasteryBefore`/`MasteryAfter`), `Achievements`/`UserAchievements` + migration
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `SrsCalculator` (SM-2: interval, easiness factor, mastery 0-5 + `CalculateMastery` yüzdelik formülü)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** `SrsCalculatorTests` (quality<3 sıfırlama, EF alt sınır 1.3, interval hesapları, Mastery formülü)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `IProgressService`/`ProgressService` (XP, streak **yalnızca günlük yeni kelime hedefine
      bağlı**, Mastery bantları Zayıf/Orta/İyi 0-40/40-70/70-100, yeni kelime seçim sorgusu,
      leech tespiti `ConsecutiveIncorrect>=5` → Suspend/Reset/Continue), `ProgressController`
      (`GET /progress/summary`, `GET /progress/words`, `GET /progress/suspended`, leech-action endpoint'leri)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `IAchievementService`/`AchievementService` (seed: streak 3/7/30, kelime sayısı 50/200/500, ilk
      `CurrentLevel=5`, 100 kelime İyi bantta, hatasız oturum, leech kurtarma), `GET /achievements/me`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** `ProgressServiceTests` (XP/streak, `NextReviewAt`, bant eşikleri, leech), `AchievementServiceTests`
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-10 — Kişisel Kart API (UserCard) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §7
**Frontend karşılığı:** C-07 (Web — Kişisel Kartlar Sayfası), D-09 (Mobil — Kişisel Kartlar Ekranı)
- [ ] **Entity:** `UserCard`, `UserCardExample` + ara tablolar (`UserCardCategory`, `UserCardUserCategory`) + migration
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `IUserCardService`/`UserCardService` (liste/detay/CRUD — yalnızca sahibi), duplikat uyarısı
      (409+`?force=true`), sistem kelimesi eşleşme uyarısı (`suggestedSystemWordId`)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `POST /user-cards/learn-system-word` → `UserCard` değil **`UserProgress`** açar, `UserCardController`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] ⚠️ **[2026-08-15] Kart görseli yükleme eksikti:** `UserCards.ImageUrl` (DATABASE_SCHEMA/
      Kisisel_Icerik.md) alanı vardı ama onu dolduracak bir uç nokta hiç planlanmamıştı — A-07'nin
      `/media/images/upload`'ı `[Authorize(Roles="Admin")]`, sıradan bir `User` çağıramaz. A-13
      (Avatar) ile AYNI desen: `POST /user-cards/{id}/image` (`[Authorize]`, yalnızca sahibi, A-07'nin
      `IFileStorageService`'i yeniden kullanılır, eski görsel silinir)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **`IActivityLogger`:** `CREATE_USER_CARD`/`UPDATE_USER_CARD`/`DELETE_USER_CARD`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** sahiplik filtresi, duplikat 409, learn-system-word akışı, kart görseli
      yükleme (sahiplik kontrolü + eski dosyanın silindiği), audit çağrısı
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-11 — Öğrenme / Sınav API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §9
**Frontend karşılığı:** C-05 (Web — Öğrenme/Sınav Sayfası), D-07 (Mobil — Öğrenme/Sınav Ekranı)
- [ ] **Entity:** `LearningSession` (+ `TargetLanguageId` FK `Languages`, 6 `SessionType` — MultipleChoice/TranslationQuiz/ArticleQuiz/PluralQuiz/TrueFalse/Flashcard) + migration
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `ILearningSessionService`/`LearningSessionService` (başlat — `mode: New|Due|Band|Mixed` +
      zorunlu `targetLanguageId` [her oturum kendi yönünü seçer, `UserProgress`/`UserCardProgress`
      `WordId`'ye [dile özel] bağlı olduğu için iki yön bağımsız ilerler — şema değişikliği
      gerekmez], kelime havuzu yalnızca **eşleşmiş** `WordConcept`'lerden, her review sorusu için
      rastgele format ataması, ipucu→quality tavanı düşürme, "günde tek resmi review" kuralı
      [`IsExtraPractice`], tamamla/bırak/`repeat` [SM-2 güncellemeden tekrar])
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `LearningSessionController` (+ `GET /learning-history/today/learned`, `/today/tested`)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** Mixed dedup, SRS önceliği, rastgele format ataması, ipucu/TrueFalse tavanı, repeat'in SM-2'yi etkilememesi
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-12 — Kullanıcı Profil API (`/users/me`) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §4
**Frontend karşılığı:** B-01 (Admin — `languageSlice`/`themeSlice`'ın gerçek yazma ucu), C-12 (Web — Profil Sayfası), D-14 (Mobil — Profil Ekranı)
> A-09'dan (SRS) SONRA geliyor — `GET /users/me/statistics` ilk günden **gerçek** `UserProgress`
> verisi döner, boş/yarım bir istatistik uç noktası olarak başlamaz.
- [ ] `UserController`: `GET /users/me`, `PUT /users/me` (CurrentLevel, ThemePreference,
      LanguagePreference dahil — `RegisterCommand`'a girdi olarak EKLENMEZ, DB varsayılanı döner,
      gerçek seçim burada), `GET /users/me/statistics` (A-09'un `UserProgress`/`Achievements`'ından), `DELETE /users/me`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] ⚠️ **[2026-08-15] Şifre değiştirme (giriş gerekli) eksikti:** `ChangePasswordCommand`
      (`PUT /users/me/password`, `[Authorize]`, `{ currentPassword, newPassword }`) — mevcut
      `/auth/reset-password` OTP tabanlı ve **anonim** ("şifremi unuttum" akışı), giriş yapmış bir
      kullanıcının mevcut şifresini bilerek değiştirmesi için ayrı bir uç yoktu (Web C-12/Mobil D-14'ün
      `ChangePasswordModal`'ı bu endpoint'i varsayıyordu, hiç yazılmamıştı). `IPasswordService` ile
      `currentPassword` doğrulanır, farklıysa `InvalidCredentialsException`; başarılıysa
      `/auth/reset-password` ile AYNI davranış — tüm `RefreshTokens` iptal (tüm cihazlardan çıkış,
      Token Family Pattern) + A-20'nin mevcut "şifre değişti" e-posta şablonu gönderilir;
      `ISecurityLogger` mevcut `LogEventType.PasswordReset`'i yeniden kullanır (yeni enum değeri
      AÇILMAZ — Detail Code'u `PASSWORD_CHANGED` ile `PASSWORD_RESET`'ten ayrışır)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Admin panel bağlantısı:** `admin/src/store/slices/languageSlice.ts`/`themeSlice.ts` (B-01'de
      yalnızca `localStorage`'a yazıyordu) bu API'ye bağlanır — dil/tema değiştirildiğinde hem
      `localStorage` hem backend güncellenir, başka cihazda login'de `AuthUserDto`'dan senkron okunur
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** profil güncelleme, istatistik hesaplama, ThemePreference/LanguagePreference
      validasyonu, şifre değiştirme (yanlış mevcut şifre reddi, başarılı değişimde tüm oturumların
      iptali + e-posta gönderimi)
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-12.1 — Oturum/Cihaz Yönetimi ⬜ ⚠️ **[2026-08-15 — yeni task, kullanıcı isteği]**
**Referans:** REFERENCE/SECURITY.md (Token Family Pattern), DATABASE_SCHEMA/Auth.md `RefreshTokens`
**Frontend karşılığı:** C-12 (Web — Profil Sayfası, oturum listesi), D-14 (Mobil — Profil Ekranı, oturum listesi)
> Yeni entity/migration **gerekmez** — `RefreshTokens` (A-03) zaten `TokenFamily`/`DeviceInfo`/
> `IpAddress`/`ExpiresAt`/`IsUsed`/`RevokedAt` taşıyor; her aktif (süresi geçmemiş, iptal edilmemiş)
> `TokenFamily` bir "cihaz/oturum" olarak listelenir. "Tüm cihazlardan çıkış" mantığı zaten
> `ResetPasswordCommandHandler`'da (A-03, SECURITY.md) var — bu task aynı iptal mekanizmasını
> kullanıcıya seçmeli hale getirir (tek bir cihazı veya diğer tümünü iptal).
- [ ] `IRefreshTokenRepository`'ye `GetActiveSessionsForUserAsync` (`TokenFamily` bazlı, her family
      için en son kullanılan token TEK satır) — `GetActiveSessionsQuery` (`isCurrent` alanı, o anki
      isteğin token'ının family'siyle karşılaştırılarak işaretlenir)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `RevokeSessionCommand` (`{ tokenFamily }` → yalnızca **çağıranın kendi** `UserId`'sine ait bir
      family'yse tüm token'ları `RevokedAt` ile iptal edilir, başkasının family'si 404), `RevokeAllSessionsCommand`
      (`exceptCurrent: bool`, varsayılan `true` — "diğer tüm cihazlardan çıkış")
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `UserController`'a 3 endpoint: `GET /users/me/sessions`, `DELETE /users/me/sessions/{tokenFamily}`,
      `POST /users/me/sessions/revoke-all`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **`ISecurityLogger`:** oturum iptali `AdminAction` DEĞİL (kullanıcı kendi hesabında işlem
      yapıyor) — `LogEventType` enum'ına (A-04, Domain/Enums/Logging) yeni bir değer **`SessionRevoked`**
      eklenir, `DATABASE_SCHEMA/Loglama.md`'deki `CK_SecurityLog_EventType` listesi de güncellenir
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** aktif oturum listesi (family bazlı tekilleştirme, `isCurrent` işaretleme),
      tek oturum iptali (sahiplik kontrolü — başkasının family'si → 404), tümünü iptal
      (`exceptCurrent` true/false)
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-13 — Avatar Yükleme API ⬜
**Frontend karşılığı:** C-12 (Web — Profil Sayfası, avatar), D-14 (Mobil — Profil Ekranı, avatar)
- [ ] `POST /users/me/avatar` (multipart, max 5MB, jpg/png/webp, benzersiz ad, eski avatar silinir — A-07'nin `IFileStorageService`'i yeniden kullanılır)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** boyut/uzantı reddi, eski dosyanın silindiğinin doğrulanması
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-14 — Paylaşım API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §14
**Frontend karşılığı:** C-10 (Web — Paylaşım Linki Sayfası), D-12 (Mobil — Paylaşım Linki Ekranı)
> ⚠️ **[2026-08-12] Netleştirme:** `SharedContent` neyi paylaşıyor, task metninde belirsizdi.
> Karar: paylaşılabilen içerik **`UserCard` (tekil kart) ve `UserCategory` (kart koleksiyonu olarak
> kategori)** — sistem kelimeleri (`Word`/`WordConcept`) zaten herkese açık olduğu için paylaşıma
> gerek yok. `SharedContent.ContentType` enum'ı (`UserCard`|`UserCategory`) + `ContentId` (polymorphic
> FK, ilişkisel FK yerine — iki farklı tabloya işaret edebildiği için) bu ayrımı taşır.
- [ ] **Entity:** `SharedContent` (`ContentType` enum: UserCard|UserCategory, `ContentId`), `SharedContentImport` + migration
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `IShareService`/`ShareService` (UUID link, anonim önizleme, listene kopyala, sil — `ContentType`'a
      göre `UserCard` mı yoksa `UserCategory`'nin içindeki tüm kartlar mı kopyalanacağını dallandırır), `SharedContentController`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** link üretimi, `expiresAt` kontrolü, anonim önizleme, **her iki `ContentType` için** kopyalama
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-15 — Sınıf API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §12
**Frontend karşılığı:** C-08 (Web — Sınıf Sayfası), D-10 (Mobil — Sınıf Ekranı)
> ⚠️ **[2026-08-12] Netleştirme:** `ClassWord`'ün `Word`/`WordConcept`'e (A-05) FK'i olduğu ve
> `UserProgress`'e (A-09) OTOMATİK yansımadığı açıkça belirtildi (önceki sürümde belirsizdi).
- [ ] **Entity:** `Class`, `ClassMembership`, `ClassWord` (`WordConceptId` FK → A-05 `WordConcept`,
      sınıfa özel not/öncelik taşıyabilir ama kendi başına öğrenme kaydı DEĞİLDİR), `ClassCategory`,
      `ClassUserCategory` + migration
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `IClassService`/`ClassService` (oluştur+davet kodu, katıl, kategori ekle, istatistik, ayrıl/sil)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `IClassWordService`/`ClassWordService` (yalnızca sahibi ekler/düzenler/siler, üyeler görür — duplikat + sistem uyarısı), `ClassController`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Öğrenmeye bağlanma:** Bir öğrenci sınıf kelimesini "öğrenmeye" başladığında bu OTOMATİK
      olmaz — üye, A-10'daki `POST /user-cards/learn-system-word` uç noktasını (`ClassWord`'ün işaret
      ettiği `WordConceptId` ile) KENDİSİ çağırır, böylece `UserProgress` her zaman kullanıcının
      kendi bilinçli eylemiyle açılır (CLAUDE.md'nin "sürpriz yan etki yok" ilkesiyle tutarlı);
      `ClassController`'a bu akışı kolaylaştıran bir yardımcı endpoint (`POST /classes/{id}/words/{wordConceptId}/learn`,
      dahili olarak aynı `UserProgress` açma mantığını çağırır) eklenir
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** davet kodu, katılım, sahiplik, üye görünürlüğü, sınıf kelimesinden `UserProgress` açma akışı
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-16 — Arkadaş API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §13
**Frontend karşılığı:** C-09 (Web — Arkadaş Sayfası), D-11 (Mobil — Arkadaş Ekranı)
- [ ] **Entity:** `Friendship` + migration, `IFriendshipService`/`FriendshipService`, `FriendshipController`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** istek/kabul/reddet, self-friendship engeli
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-17 — Push Notification (OneSignal) + Bakım Görevleri ⬜
**Referans:** REFERENCE/ENV.md §6, REFERENCE/TECHNICAL_SPECIFICATIONS.md §1 (Hangfire)
**Frontend karşılığı:** D-14 (Mobil — Profil Ekranı, device token kaydı; Web'de push yok)
- [ ] `INotificationService`/`OneSignalNotificationService`, `User.OneSignalPlayerId` + migration, `PUT /users/me/device-token`
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] Hangfire (SQL Server storage, dashboard) + recurring job'lar: günlük hatırlatma (hedef
      tamamlanmadıysa), due hatırlatması (eşik geçince günde 1), streak riski (gün sonuna
      yaklaşırken hedef eksikse); achievement bildirimi event-driven (A-09'un `AchievementService`'i tetikleyince anlık)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] ⚠️ **[2026-08-12] Süresi geçmiş kayıt temizliği:** `ExpiredTokenCleanupJob` — Hangfire'a eklenen
      günlük recurring job, üç tabloyu temizler: (1) `RefreshTokens` — `ExpiresAt` geçmiş VE
      (`IsRevoked=true` OR `ExpiresAt` üzerinden 30+ gün geçmiş) kayıtlar hard-delete edilir (soft
      delete yok, zaten audit amaçlı tutulmuyorlar); (2) `QrLoginSessions` — `ExpiresAt` geçmiş ve
      `Status` terminal (Confirmed/Denied/Expired) olanlar hard-delete; (3) `Users.PendingOtp*` alanları
      — `PendingOtpExpiresAt` geçmiş kayıtlarda OTP alanları NULL'lanır (kullanıcı satırı silinmez).
      Bu iş A-17'ye eklendi çünkü zaten Hangfire altyapısını kuran task bu.
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** `OneSignalNotificationServiceTests` (HTTP client mock), `NotificationTriggerJobTests` (her tetikleyici koşulu), `ExpiredTokenCleanupJobTests` (üç tablo için ayrı senaryo)
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-18 — Admin API (Kullanıcı Yönetimi + İstatistik + Toplu Import + Log Görüntüleme + İçerik Moderasyonu) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §11, §11.1
**Frontend karşılığı:** B-05 (Kullanıcı Yönetimi), B-06 (İçerik Moderasyonu), B-07 (İstatistik Paneli), B-08 (Log Görüntüleme Paneli)
> A-10'dan (Kişisel Kart) SONRA geliyor — `UserCard` moderasyonu (liste/sil) ilk turda tam yazılır,
> eski turdaki gibi ayrı bir "A-07.1 ertelendi" retrofit'i açılmaz. A-03.2'den (İlk Admin) de SONRA
> gelir — bu uçların test edilebilmesi için önce en az bir Admin'in var olması gerekir.
- [ ] Kullanıcı yönetimi: `IUserRepository`'ye admin sorguları + `GetUsersQuery`/`GetUserByIdQuery`/
      `UpdateUserRoleCommand`/`UpdateUserStatusCommand` — her ikisi **hem** `IActivityLogger` **hem**
      `ISecurityLogger`'a (`AdminAction`) yazar; **self-lockout koruması**
      (`Id==UserId` → `SelfAdminActionNotAllowedException`, 400)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] İstatistik: `GetAdminStatisticsQuery` (toplam/aktif/dondurulmuş kullanıcı, toplam kelime/
      kategori/kişisel kart, kayıt grafiği) — `LoginsByDay` YAZILMAZ (SecurityLog'a yeni bir
      `LogEventType` gerektirir, ayrı/büyük bir task, burada spekülatif açılmaz — YAGNI)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] Toplu kelime import: `BulkImportWordsCommand` — her satır bağımsız tek dilli `WordConcept`
      (A-05'in `translations[]`'ının AKSİNE birleştirmez, eşleştirme A-05'in `pair` akışına
      bırakılır), A-05'in `WordGrammarValidator`'ı yeniden kullanılır, best-effort
      (`BulkImportResultDto.Results[]` satır bazlı hata raporu), TEK `BULK_IMPORT_WORDS` ActivityLog kaydı
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] ⚠️ **[2026-08-12] Import'ta kategori ataması:** `BulkImportWordsCommand`'ın satır formatına
      isteğe bağlı `categoryIds: Guid[]` alanı eklenir — verilirse A-06'nın `WordCategory` ara
      tablosuna aynı transaction içinde yazılır, verilmezse kelime kategorisiz kalır (bilinçli
      varsayılan, admin sonradan A-06 uçlarından atayabilir). Bu, önceki sürümde belirtilmemiş
      bilinçsiz bir boşluktu; artık açık bir tasarım kararı.
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] Log görüntüleme: `GetActivityLogsQuery`/`GetApplicationLogsQuery`/`GetSecurityLogsQuery`
      (filtre+sayfa) + `LogMessages.cs` (yalnızca `SecurityLog.Detail` Code→mesaj çözer,
      `ActivityLog.Action`/`OldValue`/`NewValue` sabit kalır/ham JSON döner — CLAUDE.md §1)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] İçerik moderasyonu: `GetUserCardsForModerationQuery` (tüm kullanıcıların kartları, filtre+sayfa),
      `DeleteUserCardAsAdminCommand` (`IActivityLogger` → `DELETE_USER_CARD`)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `AdminController` (`[Authorize(Roles="Admin")]`) — kullanıcı(4) + istatistik(1) + import(1) + log(3) + moderasyon(2) = 11 endpoint
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** her Command/Query Handler için ayrı test dosyası (self-lockout, best-effort
      import + **kategori ataması dahil**, log filtreleme, moderasyon liste+silme+audit)
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-19 — SMTP Ayarları API ⬜
**Referans:** REFERENCE/SECURITY.md §3.2, REFERENCE/ENV.md §5
**Frontend karşılığı:** B-09 (Admin — SMTP Ayarları Sayfası)
- [ ] **Entity:** `SmtpSettings` (`BaseEntity`'den türer — ayrı bir `UpdatedBy` alanı AÇILMAZ, `BaseEntity.UpdatedByUserId` yeterli) + migration
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `IEncryptionService`/`AesEncryptionService` (AES-256-CBC, rastgele IV, `AES_ENCRYPTION_KEY` tam 32 bayta çözüldüğü constructor'da doğrulanır)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `ISmtpSettingsRepository` (`OrderBy(Id)` ile deterministik okuma), `SmtpSettingsController`:
      `GET` (şifre `***` maskeli), `PUT` (upsert + "***" gönderilirse eski şifreyi koruma —
      hiç ayar yokken maskenin gerçek şifre sanılmaması için `SmtpPasswordRequiredException`),
      `POST .../test` (`ISmtpTestService`/`MailKitSmtpTestService`, MailKit ile gerçek bağlantı)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Loglama:** `PUT` hem `IActivityLogger` (`UPDATE_SMTP_SETTINGS`, şifre diff'ten hariç) hem `ISecurityLogger` (`AdminAction`)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** `AesEncryptionServiceTests` (round-trip, 32 byte kontrolü), 3 Command/Query Handler testi
- [ ] ➜ **AKADEMI/backend'ye işle**

### A-20 — E-posta Servisi + Hesap Temizleme Görevi ⬜
**Referans:** REFERENCE/SECURITY.md §7, §9
- [ ] E-posta şablonları (doğrulama, login OTP, şifre sıfırlama, hesap silme onayı, şifre değişti,
      hesap kurtarıldı) — `EmailTemplates.cs` (6 şablon × tr/de, ortak `Layout` + inline stil);
      `IEmailService`'in tüm metotları zorunlu `string? language` alır (A-03'te zaten böyle tasarlandı)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `SmtpEmailService` (MailKit, A-19'un şifreli SMTP ayarlarını HER gönderimde okur — önbellek
      YOK, admin panelden anlık değiştirilebilsin diye) — DI: dev→`DevEmailService`(A-03),
      prod→`SmtpEmailService`. **Kritik/bilgilendirme ayrımı:** OTP e-postaları gönderilemezse
      `EmailSendFailedException` (503) fırlatılır, bildirim e-postaları hata yutulup loglanır
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] `AccountCleanupBackgroundService : BackgroundService` (Hangfire varsa A-17'nin recurring
      job'larına eklenir, yoksa günde 1 `IHostedService`) — 30 gün grace sonrası anonimleştirme
      (`DisplayName`/`AvatarUrl`/`LastLoginIP`/`OneSignalPlayerId`/bekleyen OTP + `IsActive=false`),
      `OriginalEmailHash` GERÇEK adresten `Email` üzerine yazılmadan ÖNCE üretilir (tekrar kayıt
      engelinin tamamı bu sıraya bağlı), `IActivityLogger` → `ANONYMIZE_ACCOUNT` (`ActorRole=NULL`, `OldValue` YAZILMAZ)
- [ ] ➜ **AKADEMI/backend'ye işle**
- [ ] **Birim testleri:** `AccountCleanupServiceTests` (grace period, blok hash sırası, PII temizliği,
      çoklu kayıt), `EmailTemplatesTests`, `SmtpEmailServiceTests` (kritik/bilgilendirme ayrımı)
- [ ] ➜ **AKADEMI/backend'ye işle**