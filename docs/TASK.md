# YAPILACAKLAR LİSTESİ

## Nasıl Kullanılır

Her task sırayla yapılır. Bir task tamamlanmadan bir sonrakine geçilmez.

Claude Code'a şunu söyle:
> "TASK-XXX'i yapalım."

Her kodda CODING_STANDARDS.md kuralları geçerlidir — tüm yorumlar Türkçe, WHY + HOW içerir.

## Geliştirme Sırası

```
FAZ 1 → FAZ 3 → FAZ 1+ → FAZ 2 → FAZ 4
```

| Adım | Ne yapılır | Neden |
|------|-----------|-------|
| FAZ 1 | Backend tamamlanır | API sözleşmesi ve veritabanı hazır olur |
| FAZ 3 | Admin panel yapılır | Kelime ve kategori girilir, API test edilir |
| FAZ 1+ | Backend'e eksik endpoint'ler eklenir | Admin paneli kullanırken eksik şeyler görülür, düzeltilir |
| FAZ 2 | Mobile yazılır | Hem API hem içerik hazır — gerçekçi geliştirme |
| FAZ 4 | Test ve deployment | Her şey çalışıyor, son kontroller |

---

## FAZ 1 — Backend

### TASK-001 — Proje İskeleti
**Referans:** DEVELOPMENT_SETUP.md, ARCHITECTURE.md §9

- [x] Solution ve 4 proje oluştur: API, Application, Infrastructure, Domain
- [x] Projeler arası referanslar (Domain ← Infrastructure ← Application ← API)
- [x] NuGet paketlerini yükle (TECHNICAL_SPECIFICATIONS.md §1.1)
- [x] `appsettings.json` ve `appsettings.Development.json` oluştur
- [x] `Program.cs` temel yapılandırması
- [x] `appsettings.Development.json` bağlantı dizesini ayarla (local SQL Server)
- [x] `docs/` klasörüne MD dosyalarını kopyala

---

### TASK-002 — Domain Entities
**Referans:** TECHNICAL_SPECIFICATIONS.md §2, DATABASE_SCHEMA.md §2–§5, ARCHITECTURE.md §7

**Temel:**
- [x] `BaseEntity` abstract class (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
- [x] `User` entity
- [x] `RefreshToken` entity

**Sistem Kelimeleri:**
- [x] `Word` entity
- [x] `WordDetail` entity (Almanca gramer — cinsiyet, artikeller, çekimler JSON)
- [x] `WordExample` entity (seviyeli + türlü örnek cümleler)

**Kategoriler:**
- [x] `Category` entity (self-referencing hiyerarşi)
- [x] `WordCategory` ara tablo (Word ↔ Category M:N)

**Kişisel Kartlar:**
- [x] `UserCard` entity
- [x] `UserCardExample` entity
- [x] `UserCategory` entity (kişisel kategoriler)
- [x] `UserCardCategory` ara tablo (UserCard ↔ Category M:N)
- [x] `UserCardUserCategory` ara tablo (UserCard ↔ UserCategory M:N)

**Öğrenme:**
- [x] `UserProgress` entity (sistem kelimesi SRS)
- [x] `UserCardProgress` entity (kişisel kart SRS)
- [x] `LearningHistory` entity
- [x] `LearningSession` entity

**Gamification:**
- [x] `Achievement` ve `UserAchievement` entity

**Sosyal:**
- [x] `Class` entity (sınıf)
- [x] `ClassMembership` entity
- [x] `ClassWord` entity (Instructor'ın sınıfına özel kelimeleri — yalnızca sınıf üyeleri görür)
- [x] `ClassCategory` ara tablo (Class ↔ Category M:N)
- [x] `ClassUserCategory` ara tablo (Class ↔ UserCategory M:N)
- [x] `Friendship` entity
- [x] `SharedContent` entity (paylaşım linkleri)
- [x] `SharedContentImport` entity

- [x] Tüm entity'lere Türkçe XML dokümantasyon ekle

---

### TASK-003 — DbContext ve Konfigürasyonlar
**Referans:** DATABASE_SCHEMA.md §2–§6, TECHNICAL_SPECIFICATIONS.md §5

- [x] `WordLearnerDbContext` oluştur
- [x] Her entity için ayrı `IEntityTypeConfiguration<T>` sınıfı yaz
- [x] İlişkileri Fluent API ile tanımla (FK, Cascade, Unique, Check Constraints)
- [x] Soft delete global query filter (`HasQueryFilter(e => !e.IsDeleted)`)
- [x] `SaveChangesAsync` override → `UpdatedAt` otomatik güncelleme
- [x] İlk migration: `dotnet ef migrations add InitialCreate`
- [x] Seed data (başlangıç kategorileri — DATABASE_SCHEMA.md §3)

---

### TASK-004 — Repository Pattern
**Referans:** TECHNICAL_SPECIFICATIONS.md §5

- [x] `IRepository<T>` generic interface + `Repository<T>` implementasyon
- [x] `IUserRepository` + `UserRepository`
- [x] `IWordRepository` + `WordRepository`
- [x] `IUserCardRepository` + `UserCardRepository` (UserId filtresi zorunlu)
- [x] `ICategoryRepository` + `CategoryRepository`
- [x] `IUserCategoryRepository` + `UserCategoryRepository`
- [x] `IUserProgressRepository` + `UserProgressRepository` (SRS sorguları)
- [x] `IUserCardProgressRepository` + `UserCardProgressRepository`
- [x] `IRefreshTokenRepository` + `RefreshTokenRepository`
- [x] `IClassRepository` + `ClassRepository`
- [x] `IClassWordRepository` + `ClassWordRepository`
- [x] `IFriendshipRepository` + `FriendshipRepository`
- [x] `ISharedContentRepository` + `SharedContentRepository`
- [x] DI extension metodu: `AddInfrastructureServices()`

---

### TASK-005 — Kimlik Doğrulama Servisleri
**Referans:** TECHNICAL_SPECIFICATIONS.md §4, SECURITY.md §2, DEVELOPMENT_SETUP.md §6

- [ ] `IPasswordService` + `PasswordService` (bcrypt, work factor 12)
- [ ] `ITokenService` + `JwtTokenService` (manuel JWT)
- [ ] Token Family Pattern (refresh token yeniden kullanım tespiti)
- [ ] `IAuthService` + `AuthService`
  - [ ] Register (e-posta + şifre)
  - [ ] Login (e-posta + şifre)
  - [ ] Google ile giriş (`GoogleLoginAsync` — Google token doğrula, kendi JWT üret)
  - [ ] Apple ile giriş (`AppleLoginAsync` — Apple identity token doğrula, kendi JWT üret)
  - [ ] Refresh (token family kontrolü)
  - [ ] Logout
  - [ ] Şifre sıfırlama isteği (15 dk token)
  - [ ] Şifre sıfırlama onayı
- [ ] `Google.Apis.Auth` NuGet paketi ekle (Google token doğrulama)
- [ ] Users tablosuna `GoogleId`, `AppleId`, `AuthProvider` alanları ekle

---

### TASK-006 — Auth Controller ve Middleware
**Referans:** API_ENDPOINTS.md §3, SECURITY.md §2, §5

- [ ] `AuthController` (tüm auth endpoint'leri)
- [ ] FluentValidation kuralları (tüm request DTO'ları)
- [ ] Global exception handling middleware
- [ ] Security headers middleware
- [ ] Request/Response loglama middleware

---

### TASK-007 — Sistem Kelimesi Servisi ve Controller
**Referans:** API_ENDPOINTS.md §5

- [ ] `IWordService` + `WordService`
  - [ ] Listeleme (seviye, kategori, kelime türü, arama + sayfalama)
  - [ ] Detay (WordDetail + WordExamples kullanıcı seviyesine göre)
  - [ ] Oluşturma (yalnızca Admin)
    - [ ] **Duplikat uyarısı:** Oluşturma isteğinde `GermanWord` zaten Words tablosunda varsa
          `HTTP 409` dön, yanıta mevcut kelimenin `id` ve `germanWord` bilgisini ekle.
          Admin `?force=true` query parametresiyle uyarıyı geçip yine de ekleyebilir.
  - [ ] Güncelleme (yalnızca Admin)
  - [ ] Silme soft delete (yalnızca Admin)
- [ ] `WordController`
- [ ] `PagedResult<T>` yardımcı sınıfı

---

### TASK-008 — Kategori Servisi ve Controller
**Referans:** API_ENDPOINTS.md §6

- [ ] `ICategoryService` + `CategoryService`
  - [ ] Hiyerarşik listeleme (seviye filtresi, kelime sayısı)
  - [ ] Kategoriye ait kelimeler (sayfalı)
  - [ ] CRUD (Admin)
- [ ] `CategoryController`

---

### TASK-009 — Kişisel Kart Servisi ve Controller
**Referans:** API_ENDPOINTS.md §7, DATABASE_SCHEMA.md §2.8–§2.9

- [ ] `IUserCardService` + `UserCardService`
  - [ ] Listeleme (kategori / kişisel kategori filtresi) — sadece kart sahibi
  - [ ] Detay — sadece kart sahibi
  - [ ] Oluşturma (sistem + kişisel kategoriye bağlama)
    - [ ] **Duplikat uyarısı — kendi kartları:** `FrontText` kullanıcının mevcut UserCard'larında
          zaten varsa `HTTP 409` dön; yanıta çakışan kartın `id` ve `frontText`'ini ekle.
          Kullanıcı `?force=true` ile yine de ekleyebilir (farklı çeviri veya not için).
    - [ ] **Sistem kelimesi eşleşme uyarısı:** `FrontText` sistem Words tablosundaki bir
          `GermanWord` ile birebir eşleşiyorsa yanıta `suggestedSystemWordId` ekle ve
          "Bu kelime sistemde zaten var, öğrenme listene eklemek ister misin?" mesajı döndür.
          Bu uyarı **engelleyici değildir** — kullanıcı kartını yine de oluşturabilir.
  - [ ] Güncelleme — sadece kart sahibi
  - [ ] Silme soft delete — sadece kart sahibi
  - [ ] **Sistem kelimesini öğrenme listesine ekle:** `POST /user-cards/learn-system-word`
        Body: `{ "wordId": 5 }` → Yeni UserCard oluşturmak yerine UserProgress kaydı açar
        (eğer yoksa) ve kelimeyi öğrenme kuyruğuna alır. Yanıt: `{ "userProgressId": ... }`
- [ ] `UserCardController`
- [ ] Yetki kontrolü: kullanıcı başkasının kartına erişememeli

---

### TASK-010 — Kişisel Kategori Servisi ve Controller
**Referans:** API_ENDPOINTS.md §8

- [ ] `IUserCategoryService` + `UserCategoryService`
- [ ] `UserCategoryController`
- [ ] Yetki kontrolü: sadece sahibi erişebilir

---

### TASK-011 — SRS ve İlerleme Servisi
**Referans:** API_ENDPOINTS.md §10, GERMAN_LANGUAGE_FEATURES.md §2.5.1

- [ ] SM-2 algoritması
  - [ ] Interval hesaplama (1, 3, 7, 14, 30, 60 gün)
  - [ ] Easiness Factor
  - [ ] Mastery level (0–5)
- [ ] `IProgressService` + `ProgressService`
  - [ ] Sistem kelimesi ilerlemesi (UserProgress)
  - [ ] Kişisel kart ilerlemesi (UserCardProgress)
  - [ ] XP hesaplama ve güncelleme
  - [ ] Streak güncelleme
  - [ ] **Sistem kelimesini öğrenme listesine al:** TASK-009'daki
        `POST /user-cards/learn-system-word` isteğini karşılar — mevcut UserProgress
        kaydı varsa döndür, yoksa yeni kayıt aç ve SRS kuyruğuna ekle.
- [ ] `ProgressController`
- [ ] SM-2 unit testleri

---

### TASK-012 — Öğrenme/Sınav Servisi ve Controller
**Referans:** API_ENDPOINTS.md §9

- [ ] `ILearningSessionService` + `LearningSessionService`
  - [ ] Oturum başlat (kaynak + seviye + çoklu kategori + sınıf içeriği)
  - [ ] Cevap işle (sistem kelimesi veya kişisel kart)
  - [ ] Oturumu tamamla (XP, rozet kontrolü)
  - [ ] Oturumu bırak
- [ ] `LearningSessionController`

---

### TASK-013 — Paylaşım Servisi ve Controller
**Referans:** API_ENDPOINTS.md §14, DATABASE_SCHEMA.md §5.6–§5.7

- [ ] `IShareService` + `ShareService`
  - [ ] Paylaşım linki oluştur (UUID token)
  - [ ] Token ile içerik önizle (giriş gerekmez)
  - [ ] İçeriği kendi listene kopyala
  - [ ] Link sil / pasif yap
- [ ] `SharedContentController`

---

### TASK-014 — Sınıf Servisi ve Controller
**Referans:** API_ENDPOINTS.md §12, DATABASE_SCHEMA.md §5.1–§5.6

- [ ] `IClassService` + `ClassService`
  - [ ] Sınıf oluştur (davet kodu üret)
  - [ ] Davet koduyla katıl
  - [ ] Sınıfa kategori / kişisel kategori ekle
  - [ ] Sınıf istatistikleri (üye başarı oranları)
  - [ ] Sınıftan ayrıl / sınıfı sil
- [ ] `IClassWordService` + `ClassWordService`
  - [ ] Sınıfa özel kelime ekle (yalnızca sınıf sahibi Instructor/Admin)
    - [ ] **Duplikat uyarısı — aynı sınıf içi:** `GermanWord` aynı sınıfın ClassWords tablosunda
          zaten varsa `HTTP 409` dön; yanıta çakışan kelimenin `id` ve `germanWord`'ünü ekle.
          Instructor `?force=true` ile yine de ekleyebilir.
    - [ ] **Sistem kelimesi eşleşme uyarısı:** `GermanWord` sistem Words tablosunda varsa
          engelleyici olmayan bir uyarı döndür: `"warningSystemWordExists": true, "systemWordId": 12`
          Instructor sistem kelimesini bilmesine rağmen sınıfına özel bir versiyon eklemek
          isteyebilir (farklı not veya açıklama); bu nedenle engellemez, yalnızca uyarır.
  - [ ] Sınıfın kelimelerini listele (yalnızca sınıf üyeleri)
  - [ ] Sınıf kelimesini güncelle (yalnızca sınıf sahibi)
  - [ ] Sınıf kelimesini sil soft delete (yalnızca sınıf sahibi)
  - [ ] Yetki kontrolü: Kullanıcı o sınıfın sahibi değilse 403 döner
- [ ] `ClassController` (sınıf endpoint'leri + ClassWord endpoint'leri)

---

### TASK-015 — Arkadaş Servisi ve Controller
**Referans:** API_ENDPOINTS.md §13, DATABASE_SCHEMA.md §5.5

- [ ] `IFriendshipService` + `FriendshipService`
  - [ ] Arkadaşlık isteği gönder
  - [ ] İsteği kabul / reddet
  - [ ] Arkadaşları listele
  - [ ] Arkadaşı kaldır
- [ ] `FriendshipController`

---

### TASK-016 — User Profil Controller
**Referans:** API_ENDPOINTS.md §4

- [ ] `UserController`
  - [ ] `GET /users/me`
  - [ ] `PUT /users/me`
  - [ ] `GET /users/me/statistics`
  - [ ] `DELETE /users/me`

---

### TASK-017 — Admin Endpoints
**Referans:** API_ENDPOINTS.md §11

- [ ] `AdminController`
  - [ ] Kullanıcı listeleme, arama
  - [ ] Rol değiştirme
  - [ ] Hesap dondurma/aktif etme
  - [ ] İçerik moderasyonu (kullanıcı kartları)
  - [ ] Genel istatistikler

---

### TASK-018 — Loglama ve İzleme
**Referans:** SECURITY.md §6

- [ ] Serilog yapılandır (konsol + dosya)
- [ ] `SecurityLogger` servisi
- [ ] Health check endpoint (`GET /health`)

---

## FAZ 3 — Admin Panel (React Web)

*Not: Faz 2 (Mobil) başlamadan önce yapılır — mobil test için içerik lazım.*

### TASK-019 — Admin Panel Kurulumu
**Referans:** ARCHITECTURE.md §9

- [ ] React + Vite + TypeScript projesi
- [ ] TailwindCSS
- [ ] RTK Query
- [ ] React Hook Form
- [ ] JWT auth (Admin/Instructor rolü kontrolü)
- [ ] Korumalı route wrapper

---

### TASK-020 — Admin Kelime Yönetimi
**Referans:** API_ENDPOINTS.md §5

- [ ] Kelime listesi (filtreli, sayfalı, arama)
- [ ] Kelime ekleme formu
  - [ ] Temel alanlar (kelime, çeviri, seviye, tür)
  - [ ] WordDetail formu (cinsiyet, artikeller, çekimler, çoğullar)
  - [ ] Örnek cümle ekleme (seviye + tür seçimi)
  - [ ] Kategori seçimi (birden fazla)
- [ ] Kelime düzenleme
- [ ] Kelime silme (onay modalı)

---

### TASK-021 — Admin Kategori Yönetimi
**Referans:** API_ENDPOINTS.md §6

- [ ] Kategori listesi (hiyerarşik görünüm)
- [ ] Kategori ekleme / düzenleme (üst kategori, ikon, renk, seviye aralığı)
- [ ] Kategori silme

---

### TASK-022 — Admin Kullanıcı Yönetimi
**Referans:** API_ENDPOINTS.md §11

- [ ] Kullanıcı listesi (arama, rol filtresi)
- [ ] Rol değiştirme
- [ ] Hesap aktif / pasif
- [ ] Kullanıcı detayı ve istatistikleri

---

### TASK-023 — Admin İçerik Moderasyonu

- [ ] "Herkese Açık" olarak işaretlenen kullanıcı kartları listesi
- [ ] Onayla / Reddet işlemi
- [ ] Uygunsuz içerik bildirimleri

---

### TASK-024 — Admin İstatistik Paneli
**Referans:** API_ENDPOINTS.md §11

- [ ] Toplam kullanıcı, aktif kullanıcı
- [ ] En çok öğrenilen kelimeler
- [ ] En çok sorun çıkan kelimeler
- [ ] Günlük/haftalık grafik

---

## FAZ 2 — Mobil Uygulama (React Native)

### TASK-025 — Mobil Proje Kurulumu
**Referans:** DEVELOPMENT_SETUP.md §4, TECHNICAL_SPECIFICATIONS.md §1.2

- [ ] Expo projesi (TypeScript template)
- [ ] Tüm paketler
- [ ] Klasör yapısı (screens, components, store, services, hooks, types)
- [ ] `.env.development` + `.env.production`
- [ ] ESLint + Prettier

---

### TASK-026 — Redux Toolkit Store
**Referans:** TECHNICAL_SPECIFICATIONS.md §7

- [ ] `store.ts`
- [ ] `authSlice.ts`
- [ ] `uiSlice.ts`
- [ ] RTK Query API slice (words, categories, userCards, userCategories, classes, friends)
- [ ] TypeScript arayüzleri (tüm entity'ler için)

---

### TASK-027 — Axios + Auth Service
**Referans:** TECHNICAL_SPECIFICATIONS.md §7.2

- [ ] Axios instance
- [ ] Request interceptor (Authorization header)
- [ ] Response interceptor (401 → token yenile)
- [ ] `authService.ts`
- [ ] Expo Secure Store token yönetimi

---

### TASK-028 — Navigasyon
**Referans:** ARCHITECTURE.md §9

- [ ] Auth Stack (Giriş, Kayıt, Şifre Sıfırlama)
- [ ] Ana Tab Navigasyon (Öğren, Kategoriler, Kartlarım, Sınıfım, Profil)
- [ ] Korumalı route wrapper
- [ ] Splash screen (token kontrolü)

---

### TASK-029 — Kimlik Doğrulama + Seviye Seçimi Ekranları
**Referans:** API_ENDPOINTS.md §3, ARCHITECTURE.md §3.1

- [ ] Giriş ekranı
- [ ] Kayıt ekranı
- [ ] Seviye seçim ekranı (ilk girişte)
- [ ] Şifre sıfırlama ekranı

---

### TASK-030 — Kelime Kartı Komponenti
**Referans:** GERMAN_LANGUAGE_FEATURES.md §7

- [ ] `SystemWordCard` (sistem kelimesi kartı)
  - [ ] İsimler: artikel + cinsiyet rengi + 4 hâl + çoğul
  - [ ] Fiiller: çekim tablosu (present, preterite, perfect)
  - [ ] Ayrılabilir fiil göstergesi
  - [ ] Kullanıcı seviyesine göre örnek cümle
  - [ ] Ses, görsel, IPA
- [ ] `PersonalCard` (kişisel kart)
  - [ ] Ön yüz / arka yüz
  - [ ] Kart çevirme animasyonu
- [ ] Cinsiyet renk sistemi (Maskulin: mavi, Feminin: kırmızı, Neuter: yeşil)

---

### TASK-031 — Öğrenme / Sınav Ekranı
**Referans:** API_ENDPOINTS.md §9, ARCHITECTURE.md §3.2

- [ ] Sınav başlatma ekranı
  - [ ] Kaynak seçimi (sistem / kişisel / karma / sınıf)
  - [ ] Seviye filtresi
  - [ ] Kategori seçimi (birden fazla)
  - [ ] Sınav türü seçimi
- [ ] Flashcard modu
- [ ] Çoktan seçmeli quiz
- [ ] Artikel quiz (der/die/das)
- [ ] Çoğul quiz
- [ ] Cevap geri bildirimi + açıklama
- [ ] Oturum özeti ve XP animasyonu

---

### TASK-032 — Kategoriler Ekranı
**Referans:** API_ENDPOINTS.md §6

- [ ] Kategori grid (ikon, isim, ilerleme yüzdesi)
- [ ] Seviye filtresi
- [ ] Kategori detay (kelime listesi)
- [ ] Kategoriye göre sınav başlatma

---

### TASK-033 — Kişisel Kartlar Ekranı
**Referans:** API_ENDPOINTS.md §7, §8, ARCHITECTURE.md §3.3

- [ ] Kartlar listesi (kategori / kişisel kategori filtreli)
- [ ] Kart oluşturma formu (ön yüz, arka yüz, notlar, örnek cümle)
- [ ] Kategori seçimi (sistem + kişisel, birden fazla)
- [ ] Kart düzenleme / silme
- [ ] Kişisel kategori yönetimi (oluştur, düzenle, sil)
- [ ] Karta "Paylaş" butonu → Paylaşım linki oluştur

---

### TASK-034 — Sınıf Ekranı
**Referans:** API_ENDPOINTS.md §12, ARCHITECTURE.md §3.5

- [ ] Sınıflarım listesi (üye olunan + sahibi olunan)
- [ ] Sınıf oluşturma formu (isim, açıklama)
- [ ] Davet koduyla katılma
- [ ] Sınıf detayı (kategoriler, üyeler)
- [ ] Sınıf içeriğiyle sınav başlatma
- [ ] Öğretmen görünümü: üye istatistikleri

---

### TASK-035 — Arkadaş Ekranı
**Referans:** API_ENDPOINTS.md §13, ARCHITECTURE.md §3.6

- [ ] Arkadaş listesi (seviye, streak bilgisi)
- [ ] Arkadaşlık isteği gönder (e-posta / kullanıcı adı)
- [ ] Gelen istekler (kabul / reddet)
- [ ] Arkadaşın paylaşıma açık kategorilerini görüntüle / ekle

---

### TASK-036 — Paylaşım Linki Ekranı
**Referans:** API_ENDPOINTS.md §14, ARCHITECTURE.md §3.4

- [ ] Paylaşım önizleme ekranı (giriş yapmadan erişilebilir)
- [ ] "Kendi listeme ekle" butonu
- [ ] Giriş yoksa → Kayıt/Giriş ekranına yönlendir, sonra import et

---

### TASK-037 — İlerleme Ekranı
**Referans:** API_ENDPOINTS.md §4

- [ ] Genel istatistikler
- [ ] Seviye ilerleme çubuğu
- [ ] Streak göstergesi
- [ ] Zorluk çekilen kelimeler
- [ ] Haftalık/aylık grafik

---

### TASK-038 — Profil Ekranı
**Referans:** API_ENDPOINTS.md §4

- [ ] Profil görüntüleme ve düzenleme
- [ ] Şifre değiştirme
- [ ] Dil tercihi
- [ ] Çıkış yap / Hesabı sil

---

## FAZ 4 — Test ve Yayın

### TASK-039 — Backend Unit Testler
**Referans:** DEVELOPMENT_SETUP.md §7

- [ ] SM-2 SRS algoritması
- [ ] PasswordService
- [ ] JwtTokenService
- [ ] AuthService
- [ ] WordService
- [ ] UserCardService (yetki kontrolleri)
- [ ] ShareService (link üretimi, import)
- [ ] ClassService
- [ ] FriendshipService

---

### TASK-040 — Backend Integration Testler

- [ ] Auth endpoint testleri
- [ ] Kelime endpoint testleri (rol bazlı yetki)
- [ ] UserCard endpoint testleri (sahiplik kontrolü)
- [ ] Paylaşım linki akışı
- [ ] Sınıf katılma / içerik görünürlük testleri

---

### TASK-041 — Frontend Testler

- [ ] SystemWordCard komponent
- [ ] PersonalCard komponent
- [ ] Auth slice
- [ ] Axios interceptor

---

### TASK-042 — Deployment Hazırlığı
**Referans:** SECURITY.md §10

- [ ] IIS publish ayarlarını kontrol et (web.config)
- [ ] Production secrets yönetimi
- [ ] IIS publish ayarlarını kontrol et (web.config)
- [ ] Güvenlik checklist (SECURITY.md §10.3)
- [ ] Database backup stratejisi

---

## İlerleme Durumu

| Faz | Task | Başlık | Durum |
|-----|------|--------|-------|
| 1 | TASK-001 | Proje İskeleti | ✅ |
| 1 | TASK-002 | Domain Entities | ✅ |
| 1 | TASK-003 | DbContext ve Konfigürasyonlar | ✅ |
| 1 | TASK-004 | Repository Pattern | ✅ |
| 1 | TASK-005 | Kimlik Doğrulama Servisleri | ⬜ |
| 1 | TASK-006 | Auth Controller ve Middleware | ⬜ |
| 1 | TASK-007 | Sistem Kelimesi Servisi ve Controller | ⬜ |
| 1 | TASK-008 | Kategori Servisi ve Controller | ⬜ |
| 1 | TASK-009 | Kişisel Kart Servisi ve Controller | ⬜ |
| 1 | TASK-010 | Kişisel Kategori Servisi ve Controller | ⬜ |
| 1 | TASK-011 | SRS ve İlerleme Servisi | ⬜ |
| 1 | TASK-012 | Öğrenme/Sınav Servisi | ⬜ |
| 1 | TASK-013 | Paylaşım Servisi | ⬜ |
| 1 | TASK-014 | Sınıf Servisi | ⬜ |
| 1 | TASK-015 | Arkadaş Servisi | ⬜ |
| 1 | TASK-016 | User Profil Controller | ⬜ |
| 1 | TASK-017 | Admin Endpoints | ⬜ |
| 1 | TASK-018 | Loglama ve İzleme | ⬜ |
| 3 | TASK-019 | Admin Panel Kurulumu | ⬜ |
| 3 | TASK-020 | Admin Kelime Yönetimi | ⬜ |
| 3 | TASK-021 | Admin Kategori Yönetimi | ⬜ |
| 3 | TASK-022 | Admin Kullanıcı Yönetimi | ⬜ |
| 3 | TASK-023 | Admin İçerik Moderasyonu | ⬜ |
| 3 | TASK-024 | Admin İstatistik Paneli | ⬜ |
| 2 | TASK-025 | Mobil Proje Kurulumu | ⬜ |
| 2 | TASK-026 | Redux Toolkit Store | ⬜ |
| 2 | TASK-027 | Axios + Auth Service | ⬜ |
| 2 | TASK-028 | Navigasyon | ⬜ |
| 2 | TASK-029 | Kimlik Doğrulama Ekranları | ⬜ |
| 2 | TASK-030 | Kelime Kartı Komponenti | ⬜ |
| 2 | TASK-031 | Öğrenme / Sınav Ekranı | ⬜ |
| 2 | TASK-032 | Kategoriler Ekranı | ⬜ |
| 2 | TASK-033 | Kişisel Kartlar Ekranı | ⬜ |
| 2 | TASK-034 | Sınıf Ekranı | ⬜ |
| 2 | TASK-035 | Arkadaş Ekranı | ⬜ |
| 2 | TASK-036 | Paylaşım Linki Ekranı | ⬜ |
| 2 | TASK-037 | İlerleme Ekranı | ⬜ |
| 2 | TASK-038 | Profil Ekranı | ⬜ |
| 4 | TASK-039 | Backend Unit Testler | ⬜ |
| 4 | TASK-040 | Backend Integration Testler | ⬜ |
| 4 | TASK-041 | Frontend Testler | ⬜ |
| 4 | TASK-042 | Deployment Hazırlığı | ⬜ |

---

## Durum Göstergesi

| Simge | Anlam |
|-------|-------|
| ⬜ | Başlanmadı |
| 🔄 | Devam ediyor |
| ✅ | Tamamlandı |
| ⛔ | Engellendi |
