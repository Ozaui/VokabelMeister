# MİMARİ TASARIM

> Roller, yöntem (dikey dilim), yazım sırası → `CLAUDE.md`. Bu dosya: sistem yapısı, istemci farkları,
> görünürlük matrisi, akışlar.

## 1. Sistem Mimarisi

```
[React Native /mobile] [React Web /web] [React Admin /admin]
            └──────────────┬──────────────┘
                    HTTPS / TLS 1.3
              ┌────────────▼────────────┐
              │  .NET 9 Web API         │  Controllers → (MediatR) Handlers → Repositories
              └────────────┬────────────┘
              ┌────────────▼────────────┐
              │  MS SQL Server          │  (içerik + sosyal + log tabloları)
              └─────────────────────────┘
```

**Üç istemci, tek API:**
- **Mobil:** Google + Apple + e-posta + **QR tarayıcı** (web oturumu onaylar). Token → Expo Secure Store.
- **Web:** Google + e-posta + **QR ile giriş** (Apple ileriye bırakıldı — bkz. `SECURITY.md §1.2`). Token → localStorage.
- **Admin:** E-posta+şifre + OTP (2FA) + **QR ile giriş** (Google/Apple yok, admin panelde asla). Yalnızca `Admin` rolü.

## 2. Backend Katmanlı Mimari

```
WordLearner.API            → HTTP (Controllers, Middleware, Program.cs)
WordLearner.Application    → İş mantığı (Features/Handlers, DTOs, Validators, Interfaces, Services)
WordLearner.Infrastructure → Veri erişimi (DbContext, Repositories, Configurations, Serilog sink)
WordLearner.Domain         → Entities, Enums
```
Bağımlılık: `Domain ← Application ← Infrastructure ← API`.

## 3. İçerik Sahipliği ve Görünürlük

| İçerik | Entity | Oluşturan | Gören |
|--------|--------|-----------|-------|
| Sistem içeriği | `Word`, `Category` | Admin | Tüm giriş yapmışlar (okuma) |
| Kişisel içerik | `UserCard`, `UserCategory` | Sahibi (UserId) | Sahibi (+ sınıfa atanırsa üyeler) |
| Sınıf kelimesi | `ClassWord` | Sınıf sahibi | Yalnızca sınıf üyeleri |

**Görünürlük:** Sistem kelimeleri → herkese açık. Sınıf kelimeleri → yalnızca üyeler (public/link yok).
Kullanıcı kartı/kategorisi → varsayılan sahibi; paylaşım linki olan görebilir; sınıfa atanırsa üyeler.
**Admin onayı/global görünürlük YOK** (şema ve API'de karşılığı yok).

**Sınıflar:** Herhangi bir `User` sınıf oluşturabilir (sahip = `OwnerId`). Sahip: ad güncelle, kelime/kategori ata, sil. Davet koduyla katılım; sınıf içi rol yalnızca Owner/Member.

## 4. Loglama Mimarisi

| Tablo | Kim yazar | İçerik |
|-------|-----------|--------|
| `ApplicationLog` | Serilog (`_logger`) + MSSqlServer sink | Teknik log — konsol+dosya+DB |
| `ActivityLog` | `IActivityLogger` | Audit: kim ne yaptı (old/new JSON) |
| `SecurityLog` | `ISecurityLogger` | Güvenlik: başarısız giriş, rate-limit, yetkisiz erişim |

Hepsi `GET /admin/logs/*` (B-08). Detay → `DATABASE_SCHEMA/Loglama.md`, `SECURITY.md §6`.

## 5. Kullanım Akışları

- **İlk kayıt:** Kayıt (e-posta+şifre) → e-posta doğrulama OTP → seviye seçimi → ana ekran.
- **QR ile giriş:** Web/Admin `/auth/qr/generate` → QR gösterir → mobilde giriş yapmış kullanıcı okutur (`.../scan`) → cihaz bilgisi + eşleşme kodunu görüp onaylar (`.../confirm`) → web/admin polling ile aynı `ITokenService` token'ını alır (normal login ile birebir). Endpoint istemciye göre dallanmaz — admin panelde de aynı akış (bkz. `TASK/TASK_B_admin_panel.md` B-02.1). Detay → `SECURITY.md §1.3`.
- **Öğrenme:** "Öğren" → filtre (seviye/kategori/tür) → SRS sıralaması → kart → cevap → XP → sonraki review.
- **Kişisel kart:** FrontText sistem `Words.Text` ile (aynı dilde) eşleşirse → "sisteme ekleyelim mi?" → Evet: `learn-system-word` (UserProgress, UserCard yok) / Hayır: UserCard.
- **Paylaşım:** "Paylaş" → UUID link → arkadaş anonim önizler → giriş yapıp "listeme ekle".
- **Sınıf:** Oluştur (davet kodu) → içerik ata → öğrenci katılır → sahip istatistik görür.

## 6. Teknolojiler

**Backend:** .NET 9, EF Core 9, FluentValidation 11, BCrypt.Net-Next 4, Serilog 3 (+MSSqlServer sink), MediatR 12, AutoMapper 13.
**Admin/Web/Mobil ortak kütüphaneler → `CLAUDE.md` §4.1** (Redux Toolkit yalnızca local/auth/theme/
language/filter UI state — RTK Query/React Query YOK, sunucu verisi axios + `useApiQuery`/
`useApiMutation` ile çekilir; Formik + Yup formlarda; `react-i18next` arayüz dilinde).
**Admin:** React+Vite, TS, TailwindCSS, React Router DOM, ⚠️ ikon kütüphanesi seçilecek (`lucide-react` YASAK, bkz. `CLAUDE.md` §1), `react-toastify` (bildirim), `qrcode.react` (QR ile giriş), `date-fns` (log tarihleri) — henüz başlamadı (2026-08-16 baştan yazım).
**Web:** React+Vite, TS, TailwindCSS, React Router v6, ⚠️ ikon kütüphanesi seçilecek (`lucide-react` YASAK, bkz. `CLAUDE.md` §1), `react-toastify` (bildirim), `@react-oauth/google`, `qrcode.react` (QR ile giriş) (Faz C — henüz başlamadı).
**Mobil:** React Native + Expo, React Navigation (tek routing istisnası — `react-router-dom` değil), `expo-camera` (QR tarayıcı), Expo Secure Store (Faz D — henüz başlamadı).
