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

### A-04 — Loglama Sistemi (Audit + Application + Security → DB) ✅
**Referans:** REFERENCE/SECURITY.md §6, DATABASE_SCHEMA/Loglama.md
**Frontend karşılığı:** B-08 (Admin — Log Görüntüleme Paneli)
> **Amaç:** Tüm loglar DB'de tutulur, **admin panelden görüntülenir** (A-07/B-08). Üç tablo:
> kim ne yaptı (activity), uygulama logu (Serilog), güvenlik olayları.
- [x] **Entity:** `ActivityLog`, `ApplicationLog`, `SecurityLog` + `LogEventType` enum + EF config + migration
      (`AddLoggingTables`) — hiçbiri `BaseEntity`den türemiyor (insert-only, soft delete yok)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] Serilog `Serilog.Sinks.MSSqlServer` → `ApplicationLogs` (konsol + dosya + DB) — `AutoCreateSqlTable=false`,
      `ApplicationLogColumnOptions.cs` sink şemasını migration'ın gerçek şemasıyla eşler;
      `RequestResponseLoggingMiddleware` `LogContext.PushProperty` ile RequestPath/UserId ekler
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] `IActivityLogger` + `ActivityLogger` (OldValue/NewValue JSON ile audit), `ISecurityLogger` + `SecurityLogger`
      (e-posta `IPasswordService.HashToken` ile hash'lenerek EmailHash'e yazılır — PII kuralı)
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] Repository'ler (sayfalı, filtreli): `IActivityLogRepository`, `IApplicationLogRepository`, `ISecurityLogRepository`
      — `PagedResult<T>` (A-02'de YAGNI ile silinmişti) bu görevde gerçek ilk tüketicisiyle yeniden yazıldı
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] Entegrasyon: auth akışlarına security log — **LoginFailed** (Login, ConfirmAccountDeletion),
      **OtpFailed** (VerifyLoginOtp/VerifyEmail/ResetPassword/ConfirmAccountDeletion — 4 akış),
      **TokenReplay** (Refresh), **RateLimitHit** (`RateLimiterOptions.OnRejected`, tüm policy'ler),
      **QrLoginConfirmed/QrLoginDenied** (A-03.1'den beri bekleyen not kapatıldı), artı 2 BAŞARI olayı
      (**PasswordReset**, **AccountDeletion**) — toplam 8 Handler değişti. `GET /health` (MediatR dışı,
      doğrudan `DbContext.CanConnectAsync`, bilinçli CQRS sapması — YAGNI).
      **Mimari karar:** `SecurityLog.Detail`/`ActivityLog.OldValue`/`NewValue` serbest metin değil bir
      **Code** (admin panel de bir istemci, tr/de çözümü A-07'de admin okurken yapılacak — bkz.
      `CLAUDE.md` "İkinci istisna").
- [x] ➜ **BACKEND_AKADEMI'ye işle**
- [x] **Birim testleri:** `ActivityLoggerTests` (3), `SecurityLoggerTests` (3, e-posta hash'lemesi dahil),
      `ActivityLogRepositoryTests`/`SecurityLogRepositoryTests`/`ApplicationLogRepositoryTests`
      (in-memory EF Core, filtre+sayfalama), 8 Handler test dosyasına eklenen 11 yeni senaryo.
      Toplam 144/144 yeşil.
- [x] ➜ **BACKEND_AKADEMI'ye işle**

### A-05 — Sistem Kelimesi API (Words) ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §5
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi)
- [x] **Entity:** `Language`, `WordConcept`, `Word`, `WordDetail`, `WordExample` + EF config + migration
      + `Language` seed (`de`, `tr`) — bkz. `DATABASE_SCHEMA/Icerik.md` (çoklu dile açık şema:
      kategori/seviye `WordConcept` üzerinde, gramer `WordDetail.GrammarData` JSON'da dile göre değişir).
      **Karar:** `Language` `BaseEntity`'den TÜREMEZ (statik seed/referans tablosu, audit gerekmez);
      `WordConcept`/`Word`/`WordDetail`/`WordExample` hepsi `BaseEntity`'den türer (tutarlılık —
      `Icerik.md`'nin SQL taslağı `WordDetails`/`WordExamples` için `IsDeleted` göstermese de
      CLAUDE.md'nin genel kuralı önceliklidir). Migration: `AddWordsSchema`, gerçek DB'ye uygulandı,
      `Languages` seed (`de`,`tr`) doğrulandı, mevcut 144 test hâlâ yeşil.
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`BACKEND_AKADEMI/A-05_sistem-kelimesi-api/` — 3 bölüm: neden
      WordConcept, Entity+EF Config, Migration+Seed+DbSet'ler; görev tamamlanmadığı için kök
      `index.html`'e kart henüz eklenmedi, yalnızca A-04'ün son bölümüyle zincir bağlandı)
- [x] `WordGrammarValidator` (FluentValidation) — önce `LanguageId`'ye göre dile dispatch, sonra o dilin
      `PartOfSpeech` matrisini uygular. **`de` dalı** (`GERMAN_LANGUAGE_FEATURES.md §10`): Noun:
      gender+plural+4 hâl zorunlu, fiil alanları yasak; Verb: 18 çekim+auxiliary+pastParticiple
      zorunlu, `separablePrefix` yalnızca `isSeparableVerb=true` iken; Diğer: GrammarData tamamen NULL.
      **`tr` dalı** (`TURKISH_LANGUAGE_FEATURES.md §9`): Noun: plural+6 hâl zorunlu, fiil alanları
      yasak; Verb: verbRoot+negativeForm+30 çekim zorunlu; Diğer: GrammarData tamamen NULL. **İki
      dilde de** bileşik kelime notu (`WordDetails.Notes`) koşullu — `de`'de yalnızca Noun'da,
      `tr`'de hem Noun hem Verb'de olabilir (dile göre farklı, ortaklaştırılmaz). A-07 toplu import
      da bu validator'ı kullanır. **Uygulama:** `Application/Validators/Words/WordGrammarValidator.cs`
      — `AbstractValidator<WordGrammarInput>` (Command'a bağlı değil, bağımsız/tekrar kullanılabilir
      bir tip), `WordGrammarInput(LanguageCode, PartOfSpeech, GrammarDataJson)` + System.Text.Json ile
      alan bazlı kontrol; yalnızca dokümanların Zorunlu/Koşullu/Yasak listelerindeki alanlar
      doğrulanır (TR'nin §9 matrisinde geçmeyen possessive/vowelHarmony/pluralForm/consonantMutation
      zorunlu tutulmaz). 22 yeni `ErrorMessages.cs` kodu (tr/de). **Birim testleri:**
      `WordGrammarValidatorTests` (23 test — her dil×tür kombinasyonu, DE `isSeparableVerb`
      çapraz kontrolü, geçersiz JSON, desteklenmeyen dil). Toplam 167/167 yeşil (144 + 23).
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`BACKEND_AKADEMI/A-05_sistem-kelimesi-api/04_word-grammar-
      validator.html` — 6 slayt, her satır tek tek açıklandı: alan sabitleri, constructor/Custom
      rule, EnumerateFailures dağıtımı, ValidateGerman/ValidateTurkish, JsonElement yardımcı
      metotları)
- [x] `IWordConceptRepository`/`WordConceptRepository` + `ILanguageRepository`/`LanguageRepository`
      (sayfalı liste, tüm dilleriyle detay, duplikat kontrolü, kavram+diller birlikte soft delete)
      + 5 MediatR Command/Query (`Application/Features/Words/`): `CreateWordCommand`,
      `UpdateWordCommand`, `DeleteWordCommand`, `GetWordByIdQuery`, `GetWordsQuery` — paylaşılan
      `WordEntityBuilder` (girdi→entity ağacı) ve `WordConceptDtoBuilder` (entity ağacı→DTO,
      koşullu AutoMapper kuralına göre elle inşa edildi) + `DuplicateWordException` (409,
      `WORD_TEXT_ALREADY_EXISTS`) + `CreateWordCommandValidator`/`UpdateWordCommandValidator`
      (FluentValidation, `WordGrammarValidator`'ı her `translations[]` öğesi için çağırır).
      `translations[]` 1 veya 2 dil tek işlemde oluşturulur/güncellenir, duplikat 409 + `?force=true`;
      1 dilse kavram "eşleşmemiş" kalır, `PUT` ile eksik dil eklenerek eşleştirilebilir —
      bkz. `Icerik.md` "Eşleştirme"
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`05_repository-katmani.html`, `06_command-handlerlari.html`)
- [x] `WordsController` (`[Authorize]` liste/detay, `[Authorize(Roles="Admin")]` CRUD — projedeki
      İLK `[Authorize(Roles="Admin")]` kullanımı)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`07_controller-validator-testler.html`)
- [x] **`IActivityLogger` entegrasyonu** (A-04'te yazıldı — bkz. `Loglama_Domain.md`, `Action` kolonunun
      örnek değeri zaten `CREATE_WORD` idi): `CREATE_WORD`/`UPDATE_WORD`/`DELETE_WORD` üç Command
      Handler'da (`EntityType=WordConcept`, `OldValue`/`NewValue` JSON diff) — `PAIR_WORD_CONCEPTS`
      aşağıdaki "Eşleştirme" adımıyla birlikte eklenecek, henüz YOK.
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`06_command-handlerlari.html` içindeki Handler kod slaytlarına işlendi)
- [x] **Birim testleri:** `CreateWordCommandHandlerTests` (5 — tek/iki dil, duplikat 409 + force
      bypass, bilinmeyen dil 404, CREATE_WORD audit), `UpdateWordCommandHandlerTests` (5 — mevcut
      çeviri güncelleme, eşleştirme ile ikinci dil ekleme, duplikat 409, 404, UPDATE_WORD audit),
      `DeleteWordCommandHandlerTests` (3 — soft delete çağrısı, 404, DELETE_WORD audit),
      `GetWordByIdQueryHandlerTests` (2), `GetWordsQueryHandlerTests` (1 — filtre/sayfa iletimi).
      Toplam 184/184 yeşil (167 + 17).
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`07_controller-validator-testler.html`)
- [x] **Eşleştirme:** `GetUnmatchedWordConceptsQuery` (`languageId` bazlı, tek dilli kavram listesi +
      `suggestedMatchConceptId` — karşı dilin `Definition`↔`Text` örtüşmesiyle önerilen aday;
      `Definition` virgülle ayrılmış çoklu karşılık içerebildiği için [ör. "ama, fakat, ancak"]
      **token'lara bölünüp her biri ayrı denenir**, tek string olarak denenmez — yoksa hiç
      eşleşme bulunmaz. Çoklu eşanlamlıdan yalnızca biri eşleştirilir, kalanı eşleşmemiş kalabilir
      — bilinçli kabul edilen sınırlama, bkz. `Icerik.md` "Eşleştirme") + `PairWordConceptsCommand`
      (`primaryId`+`otherConceptId` → 2. dilin `Words`'ünü taşı, boş kalan kavramı sil;
      `primaryId` = admin'in işlemi başlattığı taraf; **bloklayıcı hata yok** —
      `PartOfSpeech`/`Category`/`DifficultyLevel` çakışsa bile `primaryId`'ninki sessizce kazanır,
      çünkü diller arası tür kayması dilin doğası, veri hatası değil) — paylaşılan
      `WordMatchSuggestionResolver` (iki yönlü Definition↔Text arama) + `IWordConceptRepository`'ye
      3 yeni metot (`GetUnmatchedPagedAsync`/`GetUnmatchedOtherLanguagePoolAsync`/`PairAsync`) +
      `PairWordConceptsCommandValidator` (`otherConceptId != primaryId`, `SAME_CONCEPT_PAIR_NOT_ALLOWED`) —
      `Icerik.md` "Eşleştirme" bölümü tek doğruluk kaynağı
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`08_esleztirme-repository-query-command.html`)
- [x] `WordsController`'a eşleştirme endpoint'leri (`GET /words/unmatched`, `POST /words/pair`) —
      `docs/REFERENCE/API_ENDPOINTS.md`/`Icerik.md`'deki eski `/word-concepts/...` taslağı bu gerçek
      rotaya göre güncellendi (ayrı bir controller açılmadı, YAGNI)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`09_esleztirme-controller-postman-testler.html`)
- [x] **`IActivityLogger` entegrasyonu:** `PAIR_WORD_CONCEPTS` (`EntityType=WordConcept`,
      `OldValue`=birleşmeden önce primary+other, `NewValue`=birleşmiş sonuç + `MergedConceptId`)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`09_esleztirme-controller-postman-testler.html`)
- [x] **Birim testleri:** eşleştirme mutlu yol + `PartOfSpeech`/`Category` çakışmasında sessizce
      `primaryId`'nin kazanması (hata fırlatılmaması), `suggestedMatchConceptId` üretimi
      (çoklu `Definition` token'lara bölünerek denenmesi + ters yön dahil), primary/other 404,
      `IActivityLogger` çağrısı doğru `Action`/`EntityType` ile — 9 yeni test (2 dosya),
      toplam 193/193 yeşil (184 + 9)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`09_esleztirme-controller-postman-testler.html` + kapanış `10_ozet-sozluk.html`)

### A-06 — Kategori API (Categories) ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §6
**Frontend karşılığı:** B-04 (Admin — Kategori Yönetimi), D-06 (Web — Kategoriler Sayfası), E-08 (Mobil — Kategoriler Ekranı)
> **Not (A-05'ten sonra düzeltme):** Bu listenin ilk hâli "ICategoryService/CategoryService" deseninden
> bahsediyordu — CLAUDE.md §3'e göre bu desen artık TERK EDİLDİ (kanonik desen MediatR Command+Handler,
> A-05'te uygulandığı gibi). Aşağıdaki maddeler buna göre güncellendi.
- [x] **Entity:** `Category` (self-ref hiyerarşi), `CategoryTranslation` (dil başına ad), `WordCategory`
      ara tablo (`WordConceptId`↔`CategoryId`, `Word` DEĞİL — kategori dilden bağımsız bir kavram
      özelliği) + EF config (CHECK constraint MinLevel/MaxLevel, self-ref FK Restrict, 2 UNIQUE index)
      + migration `AddCategoriesSchema` (12 kategori + 24 çeviri HasData seed, DATABASE_SCHEMA.md'deki
      sırayla birebir), gerçek DB'ye uygulandı, mevcut 193 test hâlâ yeşil. `WordConcept.cs`'e saf
      ekleme: `WordCategories` navigasyonu (A-06 sonunda Word DTO'larına `categories[]` eklenmesi için).
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`BACKEND_AKADEMI/A-06_kategori-api/` — 3 bölüm: neden 3 tablo,
      Entity+EF Config, Migration+DbContext; A-05'in son bölümüyle zincir bağlandı)
- [x] `ICategoryRepository`/`CategoryRepository` (hiyerarşik liste — bellekte level filtresi, kategoriye
      ait kelimeler `IWordConceptRepository.GetPagedAsync`'in categoryId parametresi üzerinden, silme
      koruması sorguları `HasChildrenAsync`/`HasActiveWordsAsync`, döngü koruması `WouldCreateCycleAsync`)
      + MediatR Command/Query (`Application/Features/Categories/`): `CreateCategoryCommand`,
      `UpdateCategoryCommand`, `DeleteCategoryCommand`, `GetCategoriesQuery`, `GetCategoryWordsQuery`
      + `CategoryDtoBuilder` (elle yazılan, AutoMapper DEĞİL — A-05'teki WordConceptDtoBuilder kararıyla
      aynı gerekçe) — A-05'teki `IWordConceptRepository`/5 Command-Query deseniyle birebir.
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`04_repository-katmani.html`, `05_command-handlerlari.html`)
- [x] Silme koruması (alt kategori→`CategoryHasChildrenException`, aktif kelime→`CategoryHasActiveWordsException`,
      döngü→`CategoryParentCycleException`, hepsi 409/400), `CategoriesController` (API_ENDPOINTS.md §6'daki
      5 endpoint, [Authorize]/[Authorize(Roles="Admin")] ayrımı A-05 ile birebir)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`06_controller-validator-testler.html`)
- [x] **`IActivityLogger` entegrasyonu** (A-04): `CREATE_CATEGORY`/`UPDATE_CATEGORY`/`DELETE_CATEGORY`
      (`EntityType=Category`, `OldValue`/`NewValue` JSON diff)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (aynı bölümde işlendi)
- [x] **Birim testleri:** `Tests/Features/Categories/` (5 dosya, 21 test — hiyerarşik liste + orphan
      terfi, silme koruması 409×2, döngü koruması 400×2, `IActivityLogger` çağrısı doğru `Action` ile).
      **Sıra dışı ek:** A-05'in `GET /words` borcu da kapatıldı — `categoryId` filtresi + `categories[]`
      alanı (`WordDtos.cs`/`WordConceptDtoBuilder.cs`/`CreateWordCommand`/`UpdateWordCommand`/
      `GetWordsQuery`/`WordsController` güncellendi, A-05 testlerine 5 yeni test eklendi). **Ayrıca A-06'nın
      kendi kod denetiminde (bu oturumda, çok-agent'lı bir inceleme ile) 2 gerçek hata bulunup düzeltildi:**
      (1) `UpdateWordCommand`/`UpdateCategoryCommand`'da audit log'un `oldValue.Translations`'ı tembel
      (deferred) LINQ yüzünden mutasyon SONRASI değeri yazıyordu — `.ToList()` ile materyalize edildi,
      regresyon testi eklendi; (2) `CreateWordCommand`/`UpdateWordCommand`'a gönderilen tekrarlanan
      `categoryIds`, UNIQUE index ihlaliyle yakalanmayan bir 500'e yol açabiliyordu — `.Distinct()` ile
      düzeltildi. Ayrıca `WordConceptRepository.SoftDeleteWithWordsAsync`'e eksik olan `WordCategories`
      cascade soft-delete'i eklendi (silinen kelimenin kategori bağı yetim kalmasın diye). Toplam
      219/219 yeşil (193 + 26 yeni).
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`07_word-tarafi-retrofit.html` — retrofit + iki hata düzeltmesi
      kod-degisiklik slaytlarıyla işlendi, `08_ozet-sozluk.html` kapanış; kök `index.html`'e kart eklendi)

### A-07 — Admin API (Kullanıcı Yönetimi + İstatistik + Toplu Import + Log Görüntüleme) ✅
**Referans:** REFERENCE/API_ENDPOINTS.md §11
**Frontend karşılığı:** B-05 (Kullanıcı Yönetimi), B-06 (Paylaşım/İçerik Moderasyonu — bkz. not), B-07 (İstatistik Paneli), B-08 (Log Görüntüleme Paneli)
> **Not (A-06'dan sonra düzeltme, 2026-07-23):** Bu listenin ilk hâli "IAdminService/AdminService"
> deseninden bahsediyordu — CLAUDE.md §3'e göre bu desen artık TERK EDİLDİ (kanonik desen MediatR
> Command+Handler, A-05/A-06'da uygulandığı gibi). Aşağıdaki maddeler buna göre güncellendi.
> **Kapsam düzeltmesi:** "İçerik moderasyonu (kart liste + silme)" maddesi `UserCard` entity'sine
> bağımlı — o entity henüz kodda yok, ancak **C-02**'de (`TASK/C_kullanici_backend.md`) yazılacak.
> Roadmap sırası A→B→C olduğu için A-07 şu an bu entity'ye erişemez; bu madde A-07'nin kapsamından
> ÇIKARILDI, C-02 bitince küçük bir retrofit task'ı (**A-07.1**) olarak eklenecek — A-03.2/A-06'daki
> "sonradan borç kapatma" deseniyle aynı (bkz. `../wiki/Index.md` ilgili INGEST'ler).
- [x] **Entity/DTO yok** (kullanıcı yönetimi mevcut `User` entity'sini kullanır, `Role`/`IsActive`
      zaten `Auth.md`'de var) — `IUserRepository`'ye admin'e özel sorgular (liste/arama/sayfalama,
      istatistik) veya varsa mevcut Auth repository'sinin genişletilmesi
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`01_neden-entity-yok-repository.html`)
- [x] Kullanıcı: MediatR Command/Query (`Application/Features/Admin/`) — liste/arama/detay
      (`GetUsersQuery`/`GetUserByIdQuery`), rol değiştir (`UpdateUserRoleCommand`), hesap dondur/aktif
      (`UpdateUserStatusCommand`) — her işlem **`IActivityLogger`**'a (`UPDATE_USER_ROLE`/
      `UPDATE_USER_STATUS`) **ve** rol/durum değişimi admin'e özel hassas işlem olduğu için ayrıca
      **`ISecurityLogger`**'a (`LogEventType.AdminAction`) yazar (bkz. `CLAUDE.md` "İçerik değiştiren
      her CRUD..." kuralı). **Self-lockout koruması** (kod denetiminde bulunan açık soru, kullanıcı
      onayıyla eklendi): `Id==UserId` ise `SelfAdminActionNotAllowedException` (400,
      `CANNOT_MODIFY_OWN_ACCOUNT`) — bir admin kendi rolünü/durumunu DEĞİŞTİREMEZ.
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`02_command-query-handlerlari.html`)
- [x] Genel istatistik: `GetAdminStatisticsQuery` (`Application/Features/Admin/GetAdminStatisticsQuery.cs`)
      — `IUserRepository.GetStatisticsAsync` (toplam/aktif/dondurulmuş, bu dilimde daha önce
      tüketicisiz yazılıp kod denetiminde geri alınmıştı — CLAUDE.md §3 "spekülatif tip yazılmaz",
      bu Handler gerçek/ilk tüketicisi) + `IWordConceptRepository`/`ICategoryRepository`'ye
      `GetTotalCountAsync` (toplam kelime/kategori) + `IUserRepository.GetRegistrationDatesAsync`
      (son N günün kayıt grafiği, ham `CreatedAt` listesi — günlere gruplama Handler'da bellekte,
      sıfır-kayıtlı günler de doldurularak boşluksuz bir seri üretilir). **`LoginsByDay` BİLİNÇLİ
      OLARAK YAZILMADI:** `Users.LastLoginAt` yalnızca en son girişin üzerine yazıldığı TEK bir alan,
      bir login-event geçmişi tablosu YOK — "son N günün HER GÜNÜ kaç login oldu" sorusu mevcut
      şemayla cevaplanamaz; bunun için SecurityLog'a yeni bir `LogEventType` (ör. `LoginSucceeded`)
      eklenip her başarılı girişte loglanması gerekir — ayrı, kapsamı büyük bir task, burada
      SPEKÜLATİF olarak açılmadı (YAGNI). `AdminController`'a `GET /admin/statistics` eklendi.
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`04_istatistik.html`)
- [x] Toplu kelime import (`BulkImportWordsCommand`, `Application/Features/Admin/`) — Icerik.md
      "Eşleştirme" kararına göre HER SATIR bağımsız, TEK dilli bir `WordConcept` açar (A-05'in
      `translations[]`'ının AKSİNE, 2 dili tek satırda BİRLEŞTİRMEZ — eşleştirme A-05'in
      `GET /words/unmatched`/`POST /words/pair` akışına SONRADAN bırakılır). Satır bazında A-05
      `WordGrammarValidator` çağrılır (dil+tür'e göre koşullu kural, `GERMAN_LANGUAGE_FEATURES.md §10`)
      + A-05'in `WordTranslationInput`/`WordEntityBuilder`'ı YENİDEN KULLANILDI (yeni tip AÇILMADI).
      **Best-effort:** bir satır (duplikat/gramer hatası/dil-kategori bulunamadı) BAŞARISIZ olursa
      istek TÜMDEN reddedilmez — `BulkImportResultDto.Results[]` her satırı RowIndex+ErrorCode ile
      raporlar, admin yalnızca hatalı satırları düzeltir (795+ satırlık gerçek kullanım senaryosu).
      Import tek bir **`BULK_IMPORT_WORDS`** ActivityLog kaydı alır — satır sayısı `NewValue`'da
      (795 ayrı `CREATE_WORD` DEĞİL). `AdminController`'a `POST /admin/words/import` eklendi.
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`05_toplu-import.html`)
- [x] **Log görüntüleme:** `GET /admin/logs/activity`, `/admin/logs/application`, `/admin/logs/security`
      (filtre+sayfa) — 3 yeni Query (`GetActivityLogsQuery`/`GetApplicationLogsQuery`/
      `GetSecurityLogsQuery`, `Application/Features/Admin/`) + yeni sözlük `Common/Localization/
      LogMessages.cs` (`ErrorMessages`/`SuccessMessages`'ın PAYLAŞTIĞI `LocalizedMessageResolver`'ı
      yeniden kullanır). **Kapsam netleştirmesi (bu madde yazılırken bulundu):** yalnızca
      `SecurityLog.Detail` gerçekten sabit bir Code — `Accept-Language`'a göre admin OKURKEN çözülür
      (A-04'ten beri bekleyen borç kapandı). `ActivityLog.Action`/`OldValue`/`NewValue` BU SÖZLÜĞÜN
      KAPSAMI DIŞINDA bırakıldı: `Action` CLAUDE.md'nin AÇIKÇA belirttiği gibi zaten sabit/dilden
      bağımsız kalır (çevrilmez); `OldValue`/`NewValue` ise gerçek kullanımda (A-05/A-06/A-07'nin
      TÜM `IActivityLogger.LogAsync` çağrıları) sabit bir Code DEĞİL, alan adı+değer çiftlerinden
      oluşan YAPISAL JSON diff — bir Code sözlüğüne UYMUYOR, ham JSON olarak döner. CLAUDE.md
      §1'in "İkinci istisna" metni OldValue/NewValue'yü de anıyor ama gerçek kod bu genişlikte
      hiç uygulanmadı; bu, dokümanın lafzıyla pratik arasındaki bir hassasiyet farkı olarak burada
      not düşülüyor (CLAUDE.md metnine dokunulmadı, yalnızca gerçek davranış netleştirildi).
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`06_log-goruntuleme.html`)
- [x] `AdminController` (`[Authorize(Roles="Admin")]`) — 9 endpoint TAMAMLANDI: `GET /admin/users`,
      `GET /admin/users/{id}`, `PUT /admin/users/{id}/role`, `PUT /admin/users/{id}/status`,
      `GET /admin/statistics`, `POST /admin/words/import`, `GET /admin/logs/activity`,
      `GET /admin/logs/application`, `GET /admin/logs/security`
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`06_log-goruntuleme.html`, kapanış `07_ozet-sozluk.html`)
- [x] **Birim testleri:** her Command/Query Handler için ayrı test dosyası (`Tests/Features/Admin/`
      — rol değiştir, dondur/aktif, istatistik, import satır bazlı doğrulama, log filtreleme)
      TAMAMLANDI: 9 Handler test dosyası + `UserRepositoryTests`'e 4 yeni test, toplam **244/244
      yeşil** (219 A-06 sonu + 25 A-07) — 2 test self-lockout korumasını, 4 test
      `BulkImportWordsCommandHandler`'ın best-effort davranışını, 5 test log Query'lerinin filtre
      iletimi+Detail çözme davranışını (bilinen kod/bilinmeyen kod/null) doğrular.
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`06_log-goruntuleme.html`)

**A-07 TAMAMLANDI (2026-07-24).** Dört dilim (Kullanıcı Yönetimi, İstatistik, Toplu Kelime Import,
Log Görüntüleme), 9 endpoint, 244/244 birim testi yeşil, `BACKEND_AKADEMI/A-07_admin-api/` (7 bölüm)
işlendi, kök `BACKEND_AKADEMI/index.html`'e kart eklendi. Kod denetiminde bulunan 2 gerçek düzeltme:
tüketicisiz yazılan `AdminStatisticsDto`/`GetStatisticsAsync`'in geri alınıp gerçek tüketicisiyle
yeniden yazılması, ve self-lockout koruması (`SelfAdminActionNotAllowedException`). A-07.1
(UserCard Moderasyonu) hâlâ ERTELENMİŞ — C-02 bekliyor, Faz A'nın "bitti" sayılmasını ENGELLEMEZ.

### A-07.1 — UserCard Moderasyonu (ertelendi) ⬜
**Referans:** `TASK/C_kullanici_backend.md` C-02 (`UserCard` entity)
> **Neden ayrı task:** A-07'nin ilk kapsamındaydı, ama `UserCard` entity'si C-02'de yazılana kadar
> kodlanamaz (bkz. A-07'nin notu). C-02 tamamlanınca buraya dönülür — A-03.2/A-06'daki "sonradan borç
> kapatma" desenidir, Faz A'nın "bitti" sayılması bu task'ı beklemez (B/C fazlarına geçiş engellenmez).
- [ ] `GetUserCardsForModerationQuery` (admin — tüm kullanıcıların kartları, filtre+sayfa)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `DeleteUserCardAsAdminCommand` (`IActivityLogger` → **`DELETE_USER_CARD`**, `EntityType=UserCard`
      — `Loglama_Domain.md`'deki `Action` örneğiyle birebir)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `AdminController`'a `GET /admin/user-cards`, `DELETE /admin/user-cards/{id}` endpoint'leri
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** moderasyon liste filtresi, silme + audit log doğrulaması
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-08 — Medya / Dosya Yükleme API ✅
**Referans:** REFERENCE/ENV.md §7
**Frontend karşılığı:** B-03 (Admin — Kelime Yönetimi formundaki görsel yükleme)
> **Kapsam düzeltmesi (bu madde yazılırken bulundu):** `Word.ImageUrl` + migration zaten A-05'te
> yazılmış — `WordConcept.ImageUrl` (`nvarchar(500)`, `AddWordsSchema` migration'ında) dil bağımsız
> bir kavram özelliği olarak duruyor (bkz. `CreateWordCommand`/`UpdateWordCommand`). A-08 bu alana
> YENİ bir migration eklemez, yalnızca admin panelin bu alana yazılacak URL'i **üretmesini**
> sağlayan yükleme uç noktasını yazar.
- [x] `IFileStorageService` + `LocalFileStorageService` (uzantı: jpg/jpeg/png/webp, boyut: 5 MB
      üst sınır, `Guid` tabanlı benzersiz ad üretimi — orijinal dosya adı KORUNMAZ) +
      `UnsupportedFileTypeException`/`FileTooLargeException` (400) + `ErrorMessages.cs`'e 2 kod
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`BACKEND_AKADEMI/A-08_medya-api/01_dosya-depolama-servisi.html`)
- [x] `MediaController` (`POST /media/images/upload`, `[Authorize(Roles="Admin")]`, projedeki İLK
      `multipart/form-data`/`IFormFile` uç noktası — HealthController ile aynı desen, MediatR
      DIŞINDA), `UseStaticFiles` (`Program.cs`, auth'tan ÖNCE — `/uploads` herkese açık)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`BACKEND_AKADEMI/A-08_medya-api/02_media-controller-static-files.html`)
- [x] **`IActivityLogger` entegrasyonu** (A-04): `UPLOAD_MEDIA` (`EntityType=Word` — hangi kelimenin
      görseli olacaksa, henüz bağlanmadıysa `EntityId=NULL`)
- [x] ➜ **BACKEND_AKADEMI'ye işle** (aynı bölümde işlendi)
- [x] **Birim testleri:** `FileStorageServiceTests` (8 test — boyut/uzantı/İÇERİK doğrulama, diske
      gerçekten yazma, benzersiz ad üretimi, büyük/küçük harf duyarsız uzantı, sınır [boundary]
      testi, içerik-sahteciliği [spoofing] regresyon testi; Moq YERİNE gerçek geçici klasör
      kullanır çünkü servisin tek bağımlılığı `IConfiguration`). Toplam 252/252 yeşil (244 A-07
      sonu + 8 yeni).
- [x] ➜ **BACKEND_AKADEMI'ye işle** (`BACKEND_AKADEMI/A-08_medya-api/03_testler-ozet-sozluk.html`,
      kök `index.html`'e kart eklendi)

**Kod denetimi (2 bağımsız subagent — biri backend kodunu, biri Backend Akademi içeriğini
inceledi), 2 gerçek düzeltme:**
1. **Yalnızca uzantı kontrolü yeterli değildi (güvenlik):** `LocalFileStorageService` yalnızca dosya
   ADININ uzantısına bakıyordu, gerçek içeriğe (magic bytes) bakmıyordu — bir `.exe`, adı
   `foto.png` yapılarak yüklenip `/uploads` altında herkese açık servis edilebilirdi. Düzeltme:
   dosyanın ilk baytları (PNG/JPEG/WEBP imzaları) doğrulanır, uzantıyla eşleşmiyorsa
   `UnsupportedFileTypeException` (aynı kod, artık iki nedenden fırlıyor). Regresyon testi:
   `SaveImageAsync_ExtensionDoesNotMatchActualContent_ThrowsUnsupportedFileTypeException`.
2. **Eksik dosya yanlış hata şekli döndürüyordu (tutarlılık):** `IFormFile file` (nullable
   OLMAYAN) + `[ApiController]` kombinasyonu, `file` alanı hiç gönderilmediğinde ASP.NET Core'un
   kendi ham `ProblemDetails` JSON'ını dönüyordu — projenin her hata için kullandığı
   `ApiErrorResponse{code, message}` şeklinin DIŞINDA. Düzeltme: `IFormFile?` (nullable) + elle
   kontrol + yeni `FileRequiredException` (`FILE_REQUIRED`, 400). Ayrıca savunma-derinliği için
   `[RequestSizeLimit(5 MB)]` eklendi (Kestrel'in ~28.6 MB'lık varsayılanından çok daha erken
   reddeder).

Her iki düzeltme de `BACKEND_AKADEMI/A-08_medya-api/`'ye işlendi (bölüm 1: içerik doğrulaması,
bölüm 2: eksik dosya + RequestSizeLimit), test sayısı 250→252'ye çıktı.

**A-08 TAMAMLANDI (2026-07-24).** `IFileStorageService`/`LocalFileStorageService` + `MediaController`
(1 endpoint, projedeki ilk `multipart/form-data`) + `UseStaticFiles` + `IActivityLogger` (`UPLOAD_MEDIA`),
252/252 birim testi yeşil, `BACKEND_AKADEMI/A-08_medya-api/` (3 bölüm) işlendi. Kapsam düzeltmesi:
`Word.ImageUrl` için yeni migration gerekmedi (A-05'te zaten vardı).

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
- [ ] **Loglama entegrasyonu** (A-04): `PUT` **hem** `IActivityLogger` (`UPDATE_SMTP_SETTINGS`,
      `NewValue`'da şifre **asla** düz metin yazılmaz — `PasswordEncrypted` alanı JSON diff'ten
      hariç tutulur) **hem** `ISecurityLogger` (`LogEventType.AdminAction` — kimlik bilgisi
      değişikliği hassas, iki log'a da düşer) çağırır
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `AesEncryptionServiceTests` (encrypt/decrypt round-trip, 32 byte anahtar kontrolü)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**

### A-10 — E-posta Servisi + Hesap Temizleme Görevi ⬜
**Referans:** REFERENCE/SECURITY.md §7
- [ ] `SmtpEmailService` (MailKit; SMTP'yi repo'dan alır, `Decrypt` ile çözer) + DI (dev→Dev, prod→Smtp)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] E-posta şablonları (doğrulama, login OTP, şifre sıfırlama, hesap silme onayı, şifre değişti, hesap kurtarıldı)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] `AccountCleanupBackgroundService : IHostedService` (PII anonimleştirme, günde 1, 03:00 UTC —
      her anonimleştirilen hesap için `IActivityLogger`'a **`ANONYMIZE_ACCOUNT`** [`UserId` dolu,
      `ActorRole=NULL` çünkü sistem/background job yaptı, kişi değil] — A-04)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
- [ ] **Birim testleri:** `AccountCleanupServiceTests` (30 gün grace sonrası anonimleştirme, blok hash'i)
- [ ] ➜ **BACKEND_AKADEMI'ye işle**
