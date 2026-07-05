# MİMARİ TASARIM

## 1. Sistem Mimarisi

```
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│ React Native   │  │ React Web App  │  │ React Admin    │
│ /mobile        │  │ /web           │  │ /admin         │
└───────┬────────┘  └───────┬────────┘  └───────┬────────┘
        └──────────────────┬┴───────────────────┘
                           │ HTTPS / TLS 1.3
                           ▼
              ┌────────────────────────┐
              │   .NET 9 Web API       │  Controllers → Services → Repositories
              └───────────┬────────────┘
                          ▼
              ┌────────────────────────┐
              │   MS SQL Server        │  (içerik + sosyal + log tabloları)
              └────────────────────────┘
```

**Üç istemci, tek API:**
- **Mobil (`/mobile`):** Google + Apple + e-posta. Token → Expo Secure Store.
- **Web (`/web`):** Google + e-posta (Apple ileriye bırakıldı). Token → localStorage.
- **Admin (`/admin`):** Yalnızca e-posta + şifre (Google/Apple yok). Yalnızca `Admin` rolü.

## 2. Geliştirme Sırası

`A) Admin BE → B) Admin Panel → C) Kullanıcı BE → D) Web → E) Mobil → F) Test`

Admin önce: içerik girilmeden kullanıcı tarafı test edilemez. Web mobilden önce: tarayıcıda test
hızlı, mobile referans olur. Detay → `TASK.md`.

## 3. Backend Katmanlı Mimari

```
WordLearner.API            → HTTP (Controllers, Middleware, Program.cs)
WordLearner.Application     → İş mantığı (Services, DTOs, Validators, Interfaces)
WordLearner.Infrastructure  → Veri erişimi (DbContext, Repositories, Configurations, Logging sink)
WordLearner.Domain          → Entities, Enums
```
Bağımlılık yönü: `Domain ← Application ← Infrastructure ← API`.

**Çalışma yöntemi (dikey dilim):** Bir API'ı tüm katmanlarıyla (Entity→…→Controller) bitir, sonra
diğerine geç. Her parça yazıldıkça `API_YOL_HARITASI/` rehberine işlenir. Detay → `TASK.md`.

## 4. Roller ve İçerik Sahipliği

Sistemde **yalnızca iki rol:** `User` ve `Admin`. (`Instructor`/`Teacher`/"öğretmen" kavramı YOK.)

| İçerik | Entity | Oluşturan/düzenleyen | Gören |
|--------|--------|----------------------|-------|
| Sistem içeriği | `Word`, `Category` | Yalnızca **Admin** | Tüm giriş yapmışlar (okuma) |
| Kişisel içerik | `UserCard`, `UserCategory` | Yalnızca **sahibi** (UserId) | Yalnızca sahibi (+ sınıfa atanırsa üyeler) |
| Sınıf kelimesi | `ClassWord` | Yalnızca **sınıf sahibi** | Yalnızca sınıf üyeleri |

- **Herkes `User` kayıt olur.** `Admin` yalnızca elle atanır; hiçbir public endpoint rol yükseltemez.
- **Sahiplik kuralı:** Kişisel kayda yalnızca yazan kullanıcı erişir; sorgularda `UserId` filtresi
  zorunlu, başkasının kaydı 404/403.
- Sistem içeriği CRUD → `[Authorize(Roles="Admin")]`; okuma → `[Authorize]`.

### Sınıflar
Herhangi bir `User` sınıf (`Class`) oluşturabilir; sahip `Class.OwnerId`. Sahip: ad/bilgi güncelle,
kelime/kategori ata, sınıfı sil. Kullanıcılar davet koduyla katılır, ayrılabilir. Sınıf içi rol
yalnızca `Owner`/`Member` (eski Teacher/Student yok).

## 5. İçerik Görünürlük Matrisi

| İçerik | Kim ekler | Varsayılan | Herkese açık | Paylaşım linki | Sınıf |
|--------|-----------|-----------|--------------|----------------|-------|
| Sistem kelimeleri | Admin | ✅ herkes | — | — | — |
| Sınıf kelimeleri | Sınıf sahibi | 🔒 üyeler | ❌ | ❌ | — |
| Kullanıcı kartı/kategorisi | Her user | 🔒 sahibi | ❌ (admin onayı/global görünürlük YOK — şema ve API'de karşılığı yok) | ✅ linki olan | ✅ üyeler |

## 6. Loglama Mimarisi

Üç hedef, tek amaç değişmez kayıt + admin görünürlüğü:

| Tablo | Kim yazar | İçerik |
|-------|-----------|--------|
| `ApplicationLog` | **Serilog** (`_logger`) + MSSqlServer sink | Teknik log (hata/uyarı/info) — konsol + dosya + DB |
| `ActivityLog` | `IActivityLogger` servisi | Audit: kim ne yaptı (rol değişti, kelime silindi; old/new JSON) |
| `SecurityLog` | `ISecurityLogger` servisi | Güvenlik: başarısız giriş, rate-limit, yetkisiz erişim |

Hepsi `GET /admin/logs/*` ile filtreli + sayfalı; admin panel B-08'de görüntülenir. Detay →
`DATABASE_SCHEMA/Loglama.md`, `REFERENCE/SECURITY.md §6`.

## 7. Entity İlişkileri (özet)

```
User ─┬ RefreshToken · UserProgress·UserCard·UserCategory·UserCardProgress
      ├ LearningSession·LearningHistory·UserAchievement
      └ Class(owner)·ClassMembership·Friendship·SharedContent
Word ── WordDetail(1:1)·WordExample(1:N)·Category(M:N)·UserProgress
UserCard ── UserCardExample·Category(M:N)·UserCategory(M:N)·UserCardProgress
Category ── self-ref(ParentCategoryId)·Word(M:N)·UserCard(M:N)
Class ── ClassMembership·ClassWord·Category(M:N)·UserCategory(M:N)
SharedContent ── ContentType: UserCard|UserCategory|Class + ContentId
```
Tam tablo listesi → `DATABASE_SCHEMA.md`.

## 8. Kullanım Akışları (özet)

**İlk kayıt:** Kayıt (e-posta+şifre) → e-posta doğrulama OTP → seviye seçimi → ana ekran.

**Öğrenme:** "Öğren" → filtre (seviye/kategori/tür) → SRS sıralaması → kart → cevap → XP → sonraki
review hesaplanır.

**Kişisel kart:** FrontText sistem `Words.Text` ile (aynı dilde) eşleşirse → "Bu kelime sistemde var,
öğrenme listene ekleyelim mi?" → Evet ise `learn-system-word` (UserProgress, UserCard YOK) / Hayır ise UserCard.

**Paylaşım:** "Paylaş" → UUID link → arkadaş açar (anonim önizleme) → giriş yapıp "listeme ekle".

**Sınıf:** Oluştur (davet kodu) → içerik (kategori/kişisel kategori/sınıf kelimesi) → öğrenci katılır →
içerik öğrencinin ekranında → sahip istatistik görür.

## 9. Klasör Yapısı

```
WordLearner/
├── CLAUDE.md · docs/ · "new md"/
├── backend/{WordLearner.API, .Application, .Infrastructure, .Domain}
├── admin/   ← React + Vite (Admin paneli)
├── web/     ← React + Vite (Kullanıcı web)
├── mobile/  ← React Native Expo
└── WordLearner.sln
```

## 10. Teknolojiler

**Backend:** .NET 9, EF Core 9, FluentValidation 11, BCrypt.Net-Next 4, Serilog 3
(+ `Serilog.Sinks.MSSqlServer`), MediatR 12, AutoMapper 13.
**Web/Admin:** React + Vite, TypeScript, TailwindCSS, Redux Toolkit + RTK Query, React Hook Form,
Axios; Web ayrıca React Router v6 + `@react-oauth/google`.
**Mobil:** React Native + Expo, Redux Toolkit + RTK Query, React Navigation, Axios, i18next, Expo Secure Store.
