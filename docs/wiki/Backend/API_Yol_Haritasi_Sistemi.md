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
| `API_YOL_HARITASI/A-03_auth-api.html` | A-03 (Auth API) rehberi — 64 adım, 15 `grup`'a bölünmüş (MediatR CQRS refactor sonrası), `durum: 'done'` |
| `API_YOL_HARITASI/A-03.1_qr-login.html` | A-03.1 (QR Kod ile Giriş) rehberi — ayrı dosya (bkz. aşağıdaki "Adım Gruplama" kuralı, madde 2), `durum: 'done'`, `relatedRefs` ile A-03'e çift yönlü bağlı |
| `API_YOL_HARITASI/A-03.2_auth-success-message-localization.html` | A-03.2 (Auth Başarı Mesajlarının Lokalizasyonu) rehberi — `SuccessMessages` + 7 Handler güncellemesi, `durum: 'done'` |
| `API_YOL_HARITASI/render.js` | `window.API`/`const API` objesini okuyup `#content`'e basan paylaşımlı render motoru (kod bloklarını `esc()` ile XSS'e karşı güvenli işler) |
| `API_YOL_HARITASI/style.css` | Hem hub hem her API sayfasının ortak koyu-tema stili |

`API_YOL_HARITASI/index.html`'in (backend hub) `LISTE` dizisi şu an dört satır içeriyor:
```js
{ faz: 'A-02', metot: '-', yol: '-', baslik: 'Ortak Altyapı', durum: 'done', dosya: 'A-02_ortak-altyapi.html' },
{ faz: 'A-03', metot: '-', yol: '/auth/*', baslik: 'Auth API', durum: 'done', dosya: 'A-03_auth-api.html' },
{ faz: 'A-03.1', metot: '-', yol: '/auth/qr/*', baslik: 'QR Kod ile Giriş', durum: 'done', dosya: 'A-03.1_qr-login.html' },
{ faz: 'A-03.2', metot: 'POST', yol: '/api/v1/auth/*', baslik: 'Auth Başarı Mesajlarının Lokalizasyonu', durum: 'done', dosya: 'A-03.2_auth-success-message-localization.html' },
```
Bu, [[Gelistirme_Yol_Haritasi]]'ndeki A-02/A-03/A-03.1/A-03.2 durumuyla birebir tutarlı — dördü de tamamlandı.

## Yeni API Ekleme Kuralı (3 adım)
1. `_TASLAK.html`'i kopyala → `<faz>_<id>.html` adıyla kaydet (örn. `A-03_auth-login.html`)
2. `window.API` objesini doldur: `id`, `faz`, `metot`, `yol`, `auth`, `baslik`, `durum`
   (`wip`/`done`), `ozet`, `adimlar[]` (her adımda `num`, `tur`, `baslik`, `dosya`, `aciklama`, `kod`,
   opsiyonel `grup`), opsiyonel `relatedRefs[]`
3. `index.html`'deki LİSTE dizisine bir satır ekle

`adim.tur` değerleri: `entity | config | enum | dto | validator | exception | repository | service
| handler | controller | test`. **Kod alanları gerçek dosyanın birebir kopyasıdır** — kırpılmaz, `...` ile kapatılmaz.
(`test` türü ilk kez A-02'nin 7. adımıyla eklendi; henüz kendi UI sekmesi yok, `aciklama` +
`kod` alanları [[Kodlama_Standartlari]] §7.6'daki test dokümantasyonunu taşıyor. `handler` türü
A-03'ün MediatR CQRS refactor'ünde eklendi — bir Command record + Handler class'ı aynı dosyada
barındıran adımlar için, klasik `service` türünden ayrı bir kategori.)

## Adım Gruplama (`adim.grup`) — Büyük API'larda Okunabilirlik (2026-07-07'de eklendi)
**Sorun:** A-03 63→64 adıma çıkınca (MediatR CQRS refactor) tek düz akordiyon listesi okunamaz
hâle geldi; A-03.1 (QR Kod ile Giriş) aynı sayfaya eklenirse daha da büyüyecekti.
**Çözüm (iki parça, birbirinden bağımsız):**
1. **`adim.grup` (görsel bölümleme, tek dosya içinde):** Her adıma aynı metinli bir `grup: '...'`
   alanı eklenir (ör. `"13 Auth Command + Handler (MediatR CQRS)"`). `render.js` bunu görünce
   sayfanın üstüne tıklanabilir bir **İçindekiler** kutusu, adımların arasına da bölüm başlığı
   (`.step-group`, anchor'lı) basar. `kod` alanına DOKUNMAZ — hiçbir adım kırpılmaz/gizlenmez,
   yalnızca görsel bir ayraçtır. `grup` boşsa (ör. A-02'nin 11 adımı) render.js eskisiyle birebir
   aynı davranır — geriye dönük uyumlu.
2. **Ayrı dosya (dosyalar arası şişmeyi çözer, `grup` bunu ÇÖZMEZ):** Bir alt-task kendi
   entity/service/controller'ına sahipse (başlı başına bir "API"), `adimlar[]`'a eklenmek yerine
   kendi `<faz>.<alt>_<id>.html` dosyasını açar (örn. `A-03.1_qr-login.html`). İki sayfa
   `relatedRefs: [{dosya, baslik}]` ile **çift yönlü** birbirine bağlanır (aynı `frontendRefs`
   kuralı — tek yönlü kalması yasak) → sayfada **"🔗 İlgili API'lar"** bandı. `index.html`'in
   LİSTE'sine alt-task için ayrı bir satır eklenir (`faz: 'A-03.1'`).
**A-03_auth-api.html'in mevcut 15 grubu** (64 adım şu sırayla bölünür): Veri Modeli (1-7) →
Şifre Servisi (8-10) → Token Servisi (11-13) → DTO'lar (14) → Hata Yönetimi (15-17) →
Repository'ler (18-20) → E-posta & Sosyal Login Doğrulayıcılar (21-24) → Paylaşılan Servisler:
OTP/LoginCompletion/AutoMapper (25-27) → 13 Auth Command + Handler (28-40) → DI Kaydı (41) →
Validator'lar (42-43) → Dil Çözümleme & Doğrulama Filtresi (44-45) → Controller (46) →
Rate Limiting (47) → Birim Testler (48-64).
**A-03.1 ✅ tamamlandı** (`docs/TASK/A_admin_panel_backend.md`'de tüm alt-maddeleri ✅) — bu kurala
göre kendi dosyasını (`A-03.1_qr-login.html`) açtı, `relatedRefs` A-03↔A-03.1 arasında çift yönlü
eklendi. Aynı ayrı-dosya deseni A-03.2 için de tekrarlandı (`A-03.2_auth-success-message-localization.html`).

## İkinci Seviye: Katman Gruplama (`API.katmanlar`) — Aynı Gün Eklendi
15 grup bile TOC'ta uzun bir liste oluşturunca (A-03), bir üst seviye eklendi: `katmanlar:
[{ad, gruplar:[...]}]` — grup adlarını mimari katmana göre toplar. `adim`'a değil `API` objesine
yazılır çünkü bir katman birden çok grubu kapsar (adım başına yazmak 64 satırı tekrar değiştirmek
demekti); `render.js` bu eşlemeyi tek yerde okuyup hem İçindekiler'i (katman → grup → sayı,
3 seviyeli) hem adım listesindeki büyük `.step-katman` başlığını üretir. **A-03'ün 4 katmanı**
(15 grubu şöyle toplar): Domain & Altyapı (Veri Modeli) → Application — Servisler (Şifre/Token/
DTO/Hata/Repository/Email-Sosyal/Paylaşılan, 7 grup) → Application — CQRS (13 Command+Handler/
DI/Validator, 3 grup) → API & Test (Dil Çözümleme/Controller/Rate Limiting/Testler, 4 grup).
`katmanlar` boşsa (ör. A-02, veya `grup` kullanıp `katmanlar` kullanmayan bir sayfa) davranış
tek seviyeli `grup` sistemiyle birebir aynı kalır — tamamen opsiyonel, geriye dönük uyumlu.

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

## A-03_auth-api.html — Mevcut İçerik (64 adım, `durum: 'done'`, MediatR CQRS refactor sonrası)

A-02'den farklı olarak tek bir tabloya sığmayacak kadar büyük — bu yüzden A-03 artık kendi
`grup`/`katmanlar` alanlarıyla (bkz. aşağıdaki "Adım Gruplama" ve "İkinci Seviye: Katman Gruplama"
bölümleri) 4 katman → 15 grup → 64 adıma bölünmüş durumda; sayfanın kendisi bunu tıklanabilir bir
İçindekiler kutusu olarak gösteriyor. Detay ve gerekçeler → sayfanın kendisi (her adım `aciklama`
alanında junior'a açıklanır) ve [[Auth_Domain]]. Geçmiş adım-sayısı artışları (36→63→64) ve
nedenleri → wiki `Index.md` On yedinci/On sekizinci/On dokuzuncu INGEST kayıtları.

## Test Sonuçları (`adim.sonuclar`) — Gerçek Çalıştırma Kanıtı (2026-07-08'de eklendi)
**Sorun:** `tur: 'test'` adımları yalnızca `aciklama` (ne test edildiği) + `kod` (test dosyasının
kendisi) gösteriyordu — testin GERÇEKTEN çalışıp geçtiğine dair sayfada hiçbir kanıt yoktu.
**Çözüm:** Her `tur: 'test'` adımına `sonuclar: [{ test, durum, sure }]` dizisi eklenir —
`dotnet test --logger "trx;LogFileName=x.trx"` çalıştırılıp trx XML'indeki (`UnitTestResult`
testName/outcome/duration) gerçek verilerden doldurulur, **elle "Passed" yazmak yasak**. `render.js`
bunu kodun altına, varsayılan **KAPALI**, kendi başlığına (`▸ Sonuç · N/N başarılı`) tıklanınca açılan
ayrı bir dropdown olarak basar (`step-head`/`step-body` toggle'ının iç içe bir kopyası — 90+ test tek
sayfada varsayılan açık olsaydı sayfa kullanılamaz hâle gelirdi). Opsiyonel `hata` alanı yalnızca
`durum:'Failed'` olduğunda trx'teki hata mesajıyla doldurulur, o satırın altında kırmızı bir kutuda
gösterilir — testler geçerken hiç yazılmaz. `sonuclar` boş/yoksa render.js hiçbir şey basmaz (geriye
dönük uyumlu — eski sayfalar bu alanı eklemeden çalışmaya devam eder).
**Uygulandığı yer:** `A-02_ortak-altyapi.html` (2 adım/11 test), `A-03_auth-api.html` (17 adım/61
test → A-03.2'de 7 dil senaryosu eklendi, 68 test), `A-03.1_qr-login.html` (5 adım/18 test) —
toplam 97/97 yeşil, tek bir `dotnet test` koşusundan.

## Yeniden Kullanılan Kod Kuralı
`docs/index.html`'deki not: bir API daha önce yazılmış bir kodu/yardımcıyı (örn. [[Repository]],
`PasswordService.Hash`, ortak DTO) kullanıyorsa, o kodun **tam hâli** de o API'ın kendi sayfasına
ayrıca eklenir — her API sayfası tek başına baştan sona okunabilir olmalı (kod tekrarı bilinçli).

## Frontend Kardeşleri — Üç Ayrı Sistem (2026-07-07'de ayrıldı)
Admin (`docs/ADMIN_YOL_HARITASI/`, Faz B), Web (`docs/WEB_YOL_HARITASI/`, Faz D) ve Mobil
(`docs/MOBILE_YOL_HARITASI/`, Faz E) için aynı mantığın frontend'e uyarlanmış hâli — **üçü de
bağımsız sistem**, tek bir "Frontend Yol Haritası" değil. Sebep: admin panel, web app ve mobil
uygulama **ayrı projeler** olarak açılacak (`/admin`, `/web`, `/mobile` — kod paylaşımı yok, bkz.
[[Sistem_Mimarisi]]), bu yüzden roadmap'leri de ayrı. Eskiden tek bir `FRONTEND_YOL_HARITASI/`
(uygulama tag'iyle web/admin/mobile ayrımı yapan) sistemdi; hiç gerçek feature sayfası
yazılmamışken (yalnızca şablonlar vardı) üçe bölündü.

Her klasörün kendi hub'ı (`index.html`), kendi `_TASLAK.html`'i, kendi `render.js`'i (backend
motoruna dokunmadan bağımsız kopya) var; `style.css`'i hepsi `API_YOL_HARITASI/style.css`'ten
paylaşır. `adim.tur` değerleri üçünde de aynı: `tip | api | slice | hook | component | route |
style | test` (entity→controller zincirinin frontend karşılığı). Yöntem → `docs/TASK.md`
**⭐ Frontend Çalışma Yöntemi** (hangi fazın hangi klasöre yazacağı orada net). Dört hub sayfası
(`API_YOL_HARITASI` + üçü) birbirine topbar'dan çapraz link verir. Henüz hiçbir feature sayfası
yazılmadı (`LISTE`'ler boş) — Faz B/D/E başladıkça dolacak.

## Çapraz Link Kuralı (İki Yol Haritası Arasında Geçiş)
Bir API'yi bir frontend feature tüketiyorsa, iki sayfa **iki yönlü** birbirine bağlanır:
- Backend sayfası (`API` objesi): en alta `frontendRefs: [{ dosya, baslik }]` eklenir → sayfanın
  sonunda **"🧩 Buradan sonrası frontend tarafında"** bandı, ilgili `ADMIN_/WEB_/MOBILE_YOL_HARITASI/*.html`
  sayfasına link verir (`render.js`'de `api.frontendRefs` render bloğu) — bir API üç projeden birden
  fazlası tarafından tüketiliyorsa (örn. Auth) `frontendRefs` içine hepsi ayrı satır olarak eklenir.
- Frontend sayfası (`FEATURE` objesi, üç klasörden hangisiyse): `tur:'api'` adımına
  `backendRef: { dosya, baslik }` eklenir → o adımın hemen altında **"⚙️ Buradan sonrası backend
  tarafında"** bandı, ilgili `API_YOL_HARITASI/*.html` sayfasına link verir (`render.js`'de
  `a.backendRef` render bloğu).
- Stil: `.note.xref` sınıfı (`API_YOL_HARITASI/style.css`), her iki motor da paylaşır.
- **Kural gereği tek yönlü kalması yasak** — bir taraf yazılınca diğerine dönüp link eklenir
  (bkz. `docs/TASK.md` her iki ⭐ bölümündeki "🧩 Çapraz Link Kuralı"). `docs/TASK/A_admin_panel_backend.md`
  ve `C_kullanici_backend.md`'deki her API task'ının altına **"Frontend karşılığı:"** notu bu
  eşleştirmeyi önceden belgeler (örn. A-03 ↔ B-02/D-03/E-05).
