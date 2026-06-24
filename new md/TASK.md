# YAPILACAKLAR LİSTESİ

> **Durum:** Proje sıfırdan başlatılıyor. Hiçbir task tamamlanmamış kabul edilir — tümü ⬜.

## Nasıl Kullanılır

- Task'lar **faz sırasıyla** yapılır. Bir faz bitmeden sonrakine geçilmez.
- Claude'a: **"X-YY task'ını yapalım."**
- Her kodda `CODING_STANDARDS.md` geçerlidir — tüm yorumlar Türkçe, AMAÇ/NEDEN/NASIL içerir.

## Geliştirme Sırası (Faz)

```
A) Admin Panel Backend  →  B) Admin Panel  →  C) Kullanıcı Backend
   →  D) Web App  →  E) Mobil  →  F) Test & Yayın
```

| Faz | Ne | Neden |
|-----|----|----|
| **A** | Admin panel backend (altyapı + auth + log + içerik + admin) | Admin panelin ihtiyacı olan tüm endpoint'ler önce |
| **B** | Admin panel frontend (`/admin`) | Kelime/kategori girilir, API gerçek veriyle test edilir |
| **C** | Kullanıcı backend (web+mobil ortak API) | Kart, SRS, öğrenme, sosyal özellikler |
| **D** | Web app (`/web`) | Tarayıcıda test hızlı; mobile referans olur |
| **E** | Mobil (`/mobile`) | API + içerik + web referansı hazır → hızlı geliştirme |
| **F** | Test & deployment | Son kontroller |

---

## ⭐ Çalışma Yöntemi — Bir API'ı Baştan Sona Yaz

Katman katman (önce tüm entity'ler, sonra tüm DTO'lar) **DEĞİL**. Bir API'ı tüm katmanlarıyla bitir,
sonra diğerine geç. Örnek: *Kullanıcı API'ı yazacaksak önce `User` entity'sini yazarız, sonra o API'ın
tamamını (DTO→…→Controller) bitiririz, API Yol Haritası'na işleriz, ancak ondan sonra diğer API'a geçeriz.*

### Bir API'ın Yazım Sırası (Gerçek Kod Akışı)

```
1.  Entity              → Domain/Entities/...        (API'ın çalışacağı veri modeli)
2.  EF Konfigürasyon    → Infrastructure/.../Configurations  (FK, index, check, soft delete)
3.  Migration           → dotnet ef migrations add ...        (DB'ye tabloyu ekle)
4.  Request DTO(lar)    → Application/DTOs/...        (endpoint'in aldığı girdi)
5.  Validator(lar)      → Application/Validators/...  (FluentValidation kuralları)
6.  Exception(lar)      → varsa (XxxNotFound, XxxDuplicate ...)
7.  Repository Arayüzü  → Application/Interfaces/Repositories/IXxxRepository
8.  Repository          → Infrastructure/Repositories/XxxRepository
9.  Response DTO        → Application/DTOs/... (servisin döndürdüğü çıktı)
10. Servis Arayüzü      → Application/Interfaces/Services/IXxxService
11. Servis              → Application/Services/... (tüm yardımcıları birleştiren iş mantığı)
12. Birim Test (Servis) → Tests/.../XxxServiceTests (repo/dış servis mock'lanır; servis bitince hemen)
13. Controller          → API/Controllers/XxxController (ince katman — her şeyi birleştirir)
14. DI kaydı            → Application/Infrastructure service extension'larına ekle
15. ➜ API YOL HARİTASI  → bu API için HTML sayfası oluştur (API_YOL_HARITASI/_TASLAK.html kopyala),
                          her parçayı yazar yazmaz objesine ekle, index.html LISTE'ye satır ekle
```

> **Test felsefesi:** Testler Faz F'ye bırakılmaz. Her API'nın **servis katmanı** bitince (adım 11),
> o servisin birim testi **aynı task içinde** (adım 12) yazılır — kod gibi sona bırakılması yasaktır.
> **Nasıl yazılır:** `CODING_STANDARDS.md §7` (AAA deseni, isimlendirme kalıbı, mock kuralları, minimum
> senaryo kapsamı). Test de kod gibi roadmap'e işlenir — API'ın HTML sayfasında kod adımlarının
> yanında **ayrı bir "Test" alanı** olur (§7.6); oraya test sınıfı birebir kopyalanır + her test
> metodunun altına 3 satırlık "Ne Test Edildi / Neden Önemli" açıklaması eklenir.
> Faz F (F-01/F-02), yeniden test yazma fazı değildir; var olan testleri topluca çalıştırıp eksik
> kalan **entegrasyon/regresyon** kapsamını tamamlama fazıdır.

### Her Parça İçin Döngü (Sona Bırakma!)

Yukarıdaki her adımı (entity, DTO, validator…) yazar yazmaz **hemen** şu 2 işi yap — API bitince
toplu yazmak **yasak**:

```
1. Kod parçasını yaz   (örn. entity)
2. ➜ TASK.md           → ilgili maddeyi [ ] → [x] işaretle
3. ➜ API Yol Haritası  → o parçayı (kod + AÇIKLAMA) API'ın HTML sayfasına ekle
   → sonraki parçaya geç (örn. DTO) ve aynı döngüyü tekrarla
```

**Kurallar:**
- **Açıklama zorunlu:** Her adımın `aciklama` alanı *neden* o parçanın yazıldığını anlatır (junior eğitimi).
- **Yeniden kullanılan kod tekrar yazılır:** Bu API daha önce yazılmış bir kod bloğunu/yardımcıyı
  kullanıyorsa (örn. `Repository<T>`, `PasswordService.Hash`, ortak bir DTO), o kodun **tam hâli**
  bu API'ın yol haritası sayfasına da eklenir — her API tek başına, baştan sona okunabilir olmalı.
- **Birebir kopya:** Kod blokları gerçek dosyanın aynısıdır; kırpılmaz, `...` ile kapatılmaz. Enum
  kullanılıyorsa ayrı bir adım (`tur: 'enum'`) olarak doc-comment'leriyle yazılır.
- **Test Alanı:** Her API'ın yol haritası sayfasında kod adımlarından ayrı, kendi **"Test"** bölümü
  olur. Test sınıfı da birebir kopyalanır + her test metoduna `CODING_STANDARDS.md §7.6`'daki
  3 satırlık (Test Adı / Ne Test Edildi / Neden Önemli) açıklama eklenir.
- Bir API, yol haritasına işlenmeden **tamamlandı sayılmaz.**

---

# FAZ A — Admin Panel Backend

### A-01 — Proje İskeleti ⬜
**Referans:** DEVELOPMENT_SETUP.md §3, ENV.md
- [ ] Solution + 4 proje (API, Application, Infrastructure, Domain) + referanslar (Domain ← Infra ← App ← API)
- [ ] NuGet paketleri (TECHNICAL_SPECIFICATIONS.md §1), `appsettings*.json`, `Program.cs` temel yapı

### A-02 — Ortak Altyapı ⬜
**Referans:** TECHNICAL_SPECIFICATIONS.md §5-8, §13
*(Feature entity'leri YOK — yalnızca her API'ın ihtiyaç duyduğu paylaşılan temel.)*
- [ ] `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `WordLearnerDbContext` (boş; `ApplyConfigurationsFromAssembly`, soft delete filter, `SaveChangesAsync` override)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IRepository<T>` + `Repository<T>` generic base + `AddInfrastructureServices()`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `RepositoryTests` (in-memory DB ile CRUD + soft delete filtresi — sonraki tüm API'lar bunu kullanır)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Ortak tipler: `ApiResponse<T>`, `ApiErrorResponse`, `PagedResult<T>`, `EntityNotFoundException`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Middleware: global exception, security headers, request/response log
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `Program.cs`: JWT auth, CORS, Serilog, FluentValidation, MediatR, AutoMapper kayıtları
- [ ] ➜ **API Yol Haritası'na işle**

### A-03 — Auth API (User) ⬜
**Referans:** API_ENDPOINTS.md §3, SECURITY.md §2, TECHNICAL_SPECIFICATIONS.md §6-7
*Dikey dilim: `User` + `RefreshToken` entity → servisler → controller → yol haritası.*
- [ ] **Entity:** `User`, `RefreshToken` + `OtpPurpose` enum + EF config + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IPasswordService` (BCrypt wf:12 + SHA-256 token hash)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ITokenService` (JWT access 15dk + refresh; algorithm-confusion önlemi)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IAuthService` akışları: register, verify-email, resend, login (2-adım OTP), login/verify-otp,
      google, apple, refresh, logout, forgot/reset-password, delete-account (request/confirm, 30 gün grace)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IEmailService` (sözleşme) + `DevEmailService`, `IAppleTokenValidator`, tüm DTO/exception
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `AuthController` (13 endpoint) + FluentValidation + rate limiting (login 5/15dk, OTP 3 yanlış)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `AuthServiceTests` (register, login 2-adım, OTP doğrula, refresh, forgot/reset,
      delete-account grace), `TokenServiceTests`, `PasswordServiceTests` (repo/email mock'lanır)
- [ ] ➜ **API Yol Haritası'na işle**
> **Not:** Bu API'daki `SecurityLog` (LoginFailed/OtpFailed/RateLimitHit) entegrasyonu A-04'te
> loglama altyapısı hazır olduktan **sonra** eklenir — A-03 bu adıma kadar log'suz tamamlanmış sayılır,
> A-04 bitince AuthService'e kısa bir entegrasyon dönüşü yapılır (tek istisna, kuralın bilinçli ihlali).

### A-04 — Loglama Sistemi (Audit + Application + Security → DB) ⬜
**Referans:** SECURITY.md §6, DATABASE_SCHEMA.md §3 (Log Tabloları)
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
**Referans:** API_ENDPOINTS.md §5
- [ ] **Entity:** `Word`, `WordDetail`, `WordExample` + EF config + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IWordService` + `WordService` (liste filtre+sayfa, detay, CRUD Admin, duplikat 409 + `?force=true`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `WordController` (`[Authorize]` liste/detay, `[Authorize(Roles="Admin")]` CRUD)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `WordServiceTests` (liste filtre, duplikat 409 + force, CRUD yetki)
- [ ] ➜ **API Yol Haritası'na işle**

### A-06 — Kategori API (Categories) ⬜
**Referans:** API_ENDPOINTS.md §6
- [ ] **Entity:** `Category` (self-ref hiyerarşi), `WordCategory` ara tablo + EF config + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ICategoryService` + `CategoryService` (hiyerarşik liste, kategoriye ait kelimeler, CRUD Admin)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Silme koruması (alt kategori/aktif kelime varsa 409), `CategoriesController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `CategoryServiceTests` (hiyerarşik liste, silme koruması 409)
- [ ] ➜ **API Yol Haritası'na işle**

### A-07 — Admin API (Kullanıcı Yönetimi + İstatistik + Log Görüntüleme) ⬜
**Referans:** API_ENDPOINTS.md §11
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
**Referans:** ENV.md §7
- [ ] `IFileStorageService` + `LocalFileStorageService`, `Word.ImageUrl` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `MediaController` (`POST /media/images/upload`), `UseStaticFiles`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `FileStorageServiceTests` (boyut/uzantı doğrulama, benzersiz ad üretimi)
- [ ] ➜ **API Yol Haritası'na işle**

### A-09 — SMTP Ayarları API ⬜
**Referans:** SECURITY.md §3.4, ENV.md §5
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
**Referans:** SECURITY.md §7
- [ ] `SmtpEmailService` (MailKit; SMTP'yi repo'dan alır, `Decrypt` ile çözer) + DI (dev→Dev, prod→Smtp)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] E-posta şablonları (doğrulama, login OTP, şifre sıfırlama, hesap silme onayı, şifre değişti, hesap kurtarıldı)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `AccountCleanupBackgroundService : IHostedService` (PII anonimleştirme, günde 1, 03:00 UTC)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `AccountCleanupServiceTests` (30 gün grace sonrası anonimleştirme, blok hash'i)
- [ ] ➜ **API Yol Haritası'na işle**

---

# FAZ B — Admin Panel (`/admin`)

### B-01 — Kurulum ⬜
**Referans:** TECHNICAL_SPECIFICATIONS.md §3, DEVELOPMENT_SETUP.md §4
- [ ] React + Vite + TS, TailwindCSS, RTK Query, React Hook Form, JWT auth (Admin), korumalı route

### B-02 — Auth Sayfaları ⬜
- [ ] Giriş (yalnızca e-posta + şifre, Google/Apple yok), OTP doğrulama (2FA)

### B-03 — Kelime Yönetimi ⬜
- [ ] Liste (filtre/sayfa/arama), ekleme formu (WordDetail + örnek cümle + kategori), düzenleme, silme

### B-04 — Kategori Yönetimi ⬜
- [ ] Hiyerarşik liste, ekleme/düzenleme (üst kategori, ikon, renk, seviye), silme

### B-05 — Kullanıcı Yönetimi ⬜
- [ ] Liste (arama, rol filtresi), rol değiştir, hesap aktif/pasif, kullanıcı detayı + istatistik

### B-06 — Paylaşım/İçerik Moderasyonu ⬜
> **Not:** "Herkese açık + admin onayı" modeli kaldırıldı — DATABASE_SCHEMA.md'de `IsPublic`/`IsApproved`
> alanı yok, gerçek mekanizma `SharedContents` (link tabanlı, admin onayı gerektirmez). Bu sayfa onun
> yerine **şikayet edilen** kişisel kartları listeler/siler (`GET/DELETE /admin/user-cards`).
- [ ] Şikayet/raporlanan kullanıcı kartları listesi, inceleme, silme

### B-07 — İstatistik Paneli ⬜
- [ ] Toplam/aktif kullanıcı, en çok öğrenilen/sorunlu kelimeler, günlük/haftalık grafik

### B-08 — Log Görüntüleme Paneli ⬜
**Referans:** A-04, A-07
- [ ] Activity / Application / Security log tabloları (filtre + sayfalama + tarih aralığı), CSV dışa aktarma (ops.)

### B-09 — SMTP Ayarları Sayfası ⬜
**Referans:** A-09
- [ ] Form (Host, Port, SSL, Kullanıcı, Şifre `***`, From), kaydet, "Test e-postası gönder"

---

# FAZ C — Kullanıcı Backend (Web + Mobil Ortak API)

> Her task = bir API'ı dikey dilim olarak bitir + `API_YOL_HARITASI/` rehberine işle.

### C-01 — User Profil API (`/users/me`) ⬜
**Referans:** API_ENDPOINTS.md §4
- [ ] `UserController`: `GET /users/me`, `PUT /users/me` (CurrentLevel dahil), `GET /users/me/statistics`, `DELETE /users/me`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserServiceTests` (profil güncelleme, istatistik hesaplama)
- [ ] ➜ **API Yol Haritası'na işle**

### C-02 — Kişisel Kategori API ⬜
> **Not:** Sıra değişti (eski C-03). C-04'ün ihtiyaç duyduğu `UserCategory` entity'si önce hazır
> olmalı (`UserCardUserCategories` ara tablosu buna FK verir) → dikey dilim bütünlüğü için öne çekildi.
- [ ] **Entity:** `UserCategory` + migration, `IUserCategoryService` + `UserCategoryController` (yalnızca sahibi)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserCategoryServiceTests` (sahiplik filtresi, CRUD)
- [ ] ➜ **API Yol Haritası'na işle**

### C-03 — SRS / İlerleme API (UserProgress) ⬜
**Referans:** TECHNICAL_SPECIFICATIONS.md §11
> **Not:** Sıra değişti (eski C-04). `POST /user-cards/learn-system-word` (C-04'te yazılacak) bu
> entity'yi (`UserProgress`) kullanır; o yüzden Kişisel Kart API'sından **önce** bitirilmesi gerekir.
- [ ] **Entity:** `UserProgress`, `UserCardProgress`, `LearningHistory` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `SrsCalculator` (SM-2: interval, easiness factor, mastery 0-5)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `SrsCalculatorTests` (quality<3 sıfırlama, EF alt sınır 1.3, interval hesapları)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IProgressService` + `ProgressService` (XP, streak), `ProgressController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `ProgressServiceTests` (XP/streak güncelleme, NextReviewAt hesaplama)
- [ ] ➜ **API Yol Haritası'na işle**

### C-04 — Kişisel Kart API ⬜
**Referans:** API_ENDPOINTS.md §7
> **Not:** Sıra değişti (eski C-02). `UserCategory` (C-02) ve `UserProgress` (C-03) artık hazır;
> bu sayede aşağıdaki entity/endpoint'ler **eksiksiz** tek seferde yazılabilir (dikey dilim bozulmaz).
- [ ] **Entity:** `UserCard`, `UserCardExample` + ara tablolar (`UserCardCategory`, `UserCardUserCategory`) + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IUserCardService` + `UserCardService` (liste/detay/CRUD — yalnızca sahibi, UserId filtresi zorunlu)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Duplikat uyarısı (409 + `?force=true`), sistem kelimesi eşleşme uyarısı (`suggestedSystemWordId`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `POST /user-cards/learn-system-word` → UserCard değil **UserProgress** açar, `UserCardController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserCardServiceTests` (sahiplik filtresi, duplikat 409, learn-system-word akışı)
- [ ] ➜ **API Yol Haritası'na işle**

### C-05 — Öğrenme / Sınav API ⬜
**Referans:** API_ENDPOINTS.md §9
- [ ] **Entity:** `LearningSession` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ILearningSessionService` + `LearningSessionService` (başlat, kelime seçim önceliği, Mixed dedup, cevap, tamamla, bırak)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `LearningSessionController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `LearningSessionServiceTests` (Mixed dedup, SRS önceliği, tamamla/bırak)
- [ ] ➜ **API Yol Haritası'na işle**

### C-06 — Paylaşım API ⬜
**Referans:** API_ENDPOINTS.md §14
- [ ] **Entity:** `SharedContent`, `SharedContentImport` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IShareService` + `ShareService` (UUID link, anonim önizleme, listene kopyala, sil), `SharedContentController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `ShareServiceTests` (link üretimi, expiresAt kontrolü, anonim önizleme)
- [ ] ➜ **API Yol Haritası'na işle**

### C-07 — Sınıf API ⬜
**Referans:** API_ENDPOINTS.md §12
- [ ] **Entity:** `Class`, `ClassMembership`, `ClassWord`, `ClassCategory`, `ClassUserCategory` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IClassService` + `ClassService` (oluştur+davet kodu, katıl, kategori ekle, istatistik, ayrıl/sil)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IClassWordService` + `ClassWordService` (yalnızca sahibi ekler/düzenler/siler, üyeler görür; duplikat + sistem uyarısı)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ClassController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `ClassServiceTests` (davet kodu, katılım, sahiplik), `ClassWordServiceTests` (üye görünürlüğü)
- [ ] ➜ **API Yol Haritası'na işle**

### C-08 — Arkadaş API ⬜
**Referans:** API_ENDPOINTS.md §13
- [ ] **Entity:** `Friendship` + migration, `IFriendshipService` + `FriendshipService`, `FriendshipController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `FriendshipServiceTests` (istek/kabul/reddet, self-friendship engeli)
- [ ] ➜ **API Yol Haritası'na işle**

### C-09 — Avatar Yükleme API ⬜
- [ ] `POST /users/me/avatar` (multipart, max 5MB, jpg/png/webp, benzersiz ad, eski avatar silinir)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** boyut/uzantı reddi, eski dosyanın silindiğinin doğrulanması
- [ ] ➜ **API Yol Haritası'na işle**

### C-10 — Push Notification (OneSignal) ⬜
**Referans:** ENV.md §6
- [ ] `INotificationService` + `OneSignalNotificationService`, `User.OneSignalPlayerId` + migration, `PUT /users/me/device-token`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `OneSignalNotificationServiceTests` (HTTP client mock'lanır, hata yönetimi)
- [ ] ➜ **API Yol Haritası'na işle**

---

# FAZ D — Web Kullanıcı Uygulaması (`/web`)

### D-01 — Kurulum ⬜
- [ ] React + Vite + TS, Tailwind, Redux Toolkit + RTK Query, React Router v6, RHF, Axios
- [ ] `.env*` (VITE_API_URL, VITE_GOOGLE_CLIENT_ID), `GoogleOAuthProvider`, ProtectedRoute, temel layout

### D-02 — Redux Store + Auth Service ⬜
- [ ] `store.ts`, `authSlice`, `uiSlice`, RTK Query slice'ları, TS arayüzleri, Axios interceptor (401 refresh, `localStorage`)

### D-03 — Auth Sayfaları ⬜
- [ ] Kayıt, e-posta doğrulama (OTP), giriş, OTP (2FA), Google, şifre sıfırlama, seviye seçim

### D-04 — Kelime Kartı Komponenti ⬜
**Referans:** GERMAN_LANGUAGE_FEATURES.md §7
- [ ] `SystemWordCard` (artikel + cinsiyet rengi + 4 hâl + çoğul; fiil çekim; ayrılabilir), `PersonalCard` (flip)

### D-05 — Öğrenme / Sınav Sayfası ⬜
- [ ] Başlatma, Flashcard (4'lü öz değerlendirme), çoktan seçmeli, artikel, çoğul, geri bildirim, özet + XP

### D-06 — Kategoriler Sayfası ⬜
### D-07 — Kişisel Kartlar Sayfası ⬜
### D-08 — Sınıf Sayfası ⬜
### D-09 — Arkadaş Sayfası ⬜
### D-10 — Paylaşım Linki Sayfası ⬜ *(anonim `/share/{token}`)*
### D-11 — İlerleme Sayfası ⬜
### D-12 — Profil Sayfası ⬜ *(avatar, şifre değiştir, hesap sil OTP)*

---

# FAZ E — Mobil Uygulama (`/mobile`)

### E-01 — Proje Kurulumu ⬜ *(Expo TS, paketler, klasör yapısı, `.env*`)*
### E-02 — Redux Store ⬜
### E-03 — Axios + Auth Service ⬜ *(Expo Secure Store)*
### E-04 — Navigasyon ⬜ *(Auth Stack + Tab + splash)*
### E-05 — Kimlik Doğrulama + Seviye Seçim ⬜ *(Google + Apple iOS)*
### E-06 — Kelime Kartı Komponenti ⬜ *(+ ses/görsel/IPA)*
### E-07 — Öğrenme / Sınav Ekranı ⬜
### E-08 — Kategoriler Ekranı ⬜
### E-09 — Kişisel Kartlar Ekranı ⬜
### E-10 — Sınıf Ekranı ⬜
### E-11 — Arkadaş Ekranı ⬜
### E-12 — Paylaşım Linki Ekranı ⬜
### E-13 — İlerleme Ekranı ⬜
### E-14 — Profil Ekranı ⬜

---

# FAZ F — Test ve Yayın

### F-01 — Backend Test Konsolidasyonu (Regresyon) ⬜
> **Not:** Bu fazda testler **sıfırdan yazılmaz** — her API kendi task'ında (A-xx/C-xx) zaten birim
> testiyle bitirilmiş olmalı. Burada yapılan: (1) tüm test projesini topluca çalıştır, (2) coverage
> raporuna bak, (3) sadece **eksik kalan** servis/yardımcı sınıflar için test tamamla.
- [ ] Tüm birim testlerini topluca çalıştır (CI script), kırmızı/eksik olanları düzelt
- [ ] Coverage raporu çıkar, kritik servislerde (Auth, SRS, UserCard) eksik dal/senaryo varsa tamamla

### F-02 — Backend Integration Testler ⬜
- [ ] Auth (gerçek/test DB ile uçtan uca login akışı), kelime (rol yetkisi), UserCard (sahiplik),
      paylaşım akışı (anonim önizleme), sınıf görünürlük (üye/sahip)

### F-03 — Frontend Testler ⬜
- [ ] SystemWordCard, PersonalCard, authSlice, Axios interceptor

### F-04 — Deployment ⬜
**Referans:** SECURITY.md §10
- [ ] IIS publish, production secrets (ENV.md), güvenlik checklist, DB backup
- [ ] GDPR/KVKK: hesap silme anonimleştirme, `OriginalEmailHash` blok testi, log saklama politikası

---

## İlerleme Durumu

| Faz | Task Aralığı | Başlık | Durum |
|-----|--------------|--------|-------|
| A | A-01…A-10 | Admin Panel Backend | ⬜ |
| B | B-01…B-09 | Admin Panel | ⬜ |
| C | C-01…C-10 | Kullanıcı Backend | ⬜ |
| D | D-01…D-12 | Web App | ⬜ |
| E | E-01…E-14 | Mobil | ⬜ |
| F | F-01…F-04 | Test & Yayın | ⬜ |

**Sıradaki task:** `A-01 — Proje İskeleti`

## Durum Göstergesi
⬜ Başlanmadı · 🔄 Devam ediyor · ✅ Tamamlandı · ⛔ Engellendi
