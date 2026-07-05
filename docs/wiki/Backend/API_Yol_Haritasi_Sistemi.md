# API Yol Haritası Sistemi (`docs/API_YOL_HARITASI/`)

**Özet:** Her API'ın adım adım nasıl yazıldığını gösteren, junior geliştirici eğitimi amaçlı interaktif HTML rehber sistemi — her API'ın kendi HTML sayfası vardır ve `window.API` JS objesine yazılan adımlar `render.js` tarafından görselleştirilir. [[Kodlama_Standartlari]]'nın "her kod parçası yazılır yazılmaz hemen işlenir" kuralı burada somutlaşır; bir API bu rehbere işlenmeden tamamlanmış sayılmaz.
**Kütüphaneler:** Saf HTML/CSS/JS (framework yok)
**Bağlantılar:** [[Gelistirme_Yol_Haritasi]] · [[Kodlama_Standartlari]] · [[Backend_Katmanli_Mimari]]

## Dosyalar
| Dosya | Amaç |
|-------|------|
| `docs/index.html` | **Hub** — kurallar + `LISTE` dizisindeki tüm API'ların kart listesi (klasörün bir üstünde durur, `API_YOL_HARITASI/style.css`'e görece referans verir) |
| `API_YOL_HARITASI/_TASLAK.html` | Yeni API şablonu — kopyalanıp doldurulur |
| `API_YOL_HARITASI/A-02_ortak-altyapi.html` | A-02 (Ortak Altyapı) rehberi — 11 adım, `durum: 'done'` |
| `API_YOL_HARITASI/A-03_auth-api.html` | A-03 (Auth API) rehberi — 36 adım, `durum: 'done'` |
| `API_YOL_HARITASI/render.js` | `window.API`/`const API` objesini okuyup `#content`'e basan paylaşımlı render motoru (kod bloklarını `esc()` ile XSS'e karşı güvenli işler) |
| `API_YOL_HARITASI/style.css` | Hem hub hem her API sayfasının ortak koyu-tema stili |

`API_YOL_HARITASI/index.html`'in (backend hub) `LISTE` dizisi şu an iki satır içeriyor:
```js
{ faz: 'A-02', metot: '-', yol: '-', baslik: 'Ortak Altyapı', durum: 'done', dosya: 'A-02_ortak-altyapi.html' },
{ faz: 'A-03', metot: '-', yol: '/auth/*', baslik: 'Auth API', durum: 'done', dosya: 'A-03_auth-api.html' },
```
Bu, [[Gelistirme_Yol_Haritasi]]'ndeki A-02/A-03 durumuyla birebir tutarlı — ikisi de tamamlandı.

## Yeni API Ekleme Kuralı (3 adım)
1. `_TASLAK.html`'i kopyala → `<faz>_<id>.html` adıyla kaydet (örn. `A-03_auth-login.html`)
2. `window.API` objesini doldur: `id`, `faz`, `metot`, `yol`, `auth`, `baslik`, `durum`
   (`wip`/`done`), `ozet`, `adimlar[]` (her adımda `num`, `tur`, `baslik`, `dosya`, `aciklama`, `kod`)
3. `index.html`'deki LİSTE dizisine bir satır ekle

`adim.tur` değerleri: `entity | config | enum | dto | validator | exception | repository | service
| controller | test`. **Kod alanları gerçek dosyanın birebir kopyasıdır** — kırpılmaz, `...` ile kapatılmaz.
(`test` türü ilk kez A-02'nin 7. adımıyla eklendi; henüz kendi UI sekmesi yok, `aciklama` +
`kod` alanları [[Kodlama_Standartlari]] §7.6'daki test dokümantasyonunu taşıyor.)

## A-02_ortak-altyapi.html — Mevcut İçerik (11 adım, `durum: 'done'`)

| # | Adım | Dosya | Wiki karşılığı |
|---|------|-------|-----------------|
| 1 | `entity` | `BaseEntity.cs` | [[BaseEntity]] |
| 2 | `config` | `WordLearnerDbContext.cs` | [[WordLearnerDbContext]] |
| 3 | `exception` | `EntityNotFoundException.cs` | [[EntityNotFoundException]] |
| 4 | `repository` | `IRepository.cs` | [[IRepository]] |
| 5 | `repository` | `Repository.cs` | [[Repository]] |
| 6 | `service` | `InfrastructureServiceExtensions.cs` | [[InfrastructureServiceExtensions]] |
| 7 | `test` | `RepositoryTests.cs` (10 test) | [[RepositoryTests]] |
| 8 | `test` | `EntityNotFoundExceptionTests.cs` (1 test) | — |
| 9 | `dto` | `ApiErrorResponse.cs` | [[ApiErrorResponse]] |
| 10 | `service` | Middleware (Exception/SecurityHeaders/RequestResponseLogging) | [[Middleware]] |
| 11 | `config` | `Program.cs` (JWT/CORS/Serilog/FluentValidation kaydı) | [[Program_cs]] |

## A-03_auth-api.html — Mevcut İçerik (36 adım, `durum: 'done'`)

A-02'den farklı olarak tek bir tabloya sığmayacak kadar büyük — 36 adım şu kategorilerde gruplanır:
entity/config/migration (1-4) → `IPasswordService`/`ITokenService` (5-8) → DTO'lar (9) →
`AppException`/`ErrorMessages` + 7 exception alt tipi (10-17, mimari karar: tr+de dile göre,
log/DB İngilizce) → repository'ler (18-20) → `IEmailService`/`DevEmailService`/Google-Apple
validator'ları (21-24) → `IAuthService`/`AuthService` (25-27) → FluentValidation validator'ları
(28-29) → `RequestLanguageResolver`/`ValidationFilter` (30-31) → `AuthController` (32) →
`Program.cs` rate limiting (33) → birim testler (34-36: `PasswordServiceTests`,
`JwtTokenServiceTests`, `AuthServiceTests`). Detay ve gerekçeler → sayfanın kendisi (her adım
`aciklama` alanında junior'a açıklanır) ve [[Auth_Domain]].

## Yeniden Kullanılan Kod Kuralı
`docs/index.html`'deki not: bir API daha önce yazılmış bir kodu/yardımcıyı (örn. [[Repository]],
`PasswordService.Hash`, ortak DTO) kullanıyorsa, o kodun **tam hâli** de o API'ın kendi sayfasına
ayrıca eklenir — her API sayfası tek başına baştan sona okunabilir olmalı (kod tekrarı bilinçli).

## Frontend Kardeşi (`docs/FRONTEND_YOL_HARITASI/`)
Web/Admin/Mobil feature'ları için aynı mantığın frontend'e uyarlanmış hâli — kendi hub'ı
(`FRONTEND_YOL_HARITASI/index.html`), kendi `_TASLAK.html`'i, kendi `render.js`'i (backend
motoruna dokunmadan bağımsız kopya) var; `style.css`'i bu klasörden (`API_YOL_HARITASI/style.css`)
paylaşır. `adim.tur` değerleri farklıdır: `tip | api | slice | hook | component | route | style | test`
(entity→controller zincirinin frontend karşılığı). Yöntem → `docs/TASK.md` **⭐ Frontend Çalışma
Yöntemi**; iki hub sayfası birbirine topbar'dan çapraz link verir. Henüz hiçbir feature sayfası
yazılmadı (`LISTE` boş) — Faz B/D/E başladıkça dolacak.

## Çapraz Link Kuralı (İki Yol Haritası Arasında Geçiş)
Bir API'yi bir frontend feature tüketiyorsa, iki sayfa **iki yönlü** birbirine bağlanır:
- Backend sayfası (`API` objesi): en alta `frontendRefs: [{ dosya, baslik }]` eklenir → sayfanın
  sonunda **"🧩 Buradan sonrası frontend tarafında"** bandı, ilgili `FRONTEND_YOL_HARITASI/*.html`
  sayfasına link verir (`render.js`'de `api.frontendRefs` render bloğu).
- Frontend sayfası (`FEATURE` objesi): `tur:'api'` adımına `backendRef: { dosya, baslik }` eklenir →
  o adımın hemen altında **"⚙️ Buradan sonrası backend tarafında"** bandı, ilgili
  `API_YOL_HARITASI/*.html` sayfasına link verir (`render.js`'de `a.backendRef` render bloğu).
- Stil: `.note.xref` sınıfı (`API_YOL_HARITASI/style.css`), her iki motor da paylaşır.
- **Kural gereği tek yönlü kalması yasak** — bir taraf yazılınca diğerine dönüp link eklenir
  (bkz. `docs/TASK.md` her iki ⭐ bölümündeki "🧩 Çapraz Link Kuralı"). `docs/TASK/A_admin_panel_backend.md`
  ve `C_kullanici_backend.md`'deki her API task'ının altına **"Frontend karşılığı:"** notu bu
  eşleştirmeyi önceden belgeler (örn. A-03 ↔ B-02/D-03/E-05).
