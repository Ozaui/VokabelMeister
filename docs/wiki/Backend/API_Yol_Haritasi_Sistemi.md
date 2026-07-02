# API Yol Haritası Sistemi (`docs/API_YOL_HARITASI/`)

**Özet:** Her API'ın adım adım nasıl yazıldığını gösteren, junior geliştirici eğitimi amaçlı interaktif HTML rehber sistemi — her API'ın kendi HTML sayfası vardır ve `window.API` JS objesine yazılan adımlar `render.js` tarafından görselleştirilir. [[Kodlama_Standartlari]]'nın "her kod parçası yazılır yazılmaz hemen işlenir" kuralı burada somutlaşır; bir API bu rehbere işlenmeden tamamlanmış sayılmaz.
**Kütüphaneler:** Saf HTML/CSS/JS (framework yok)
**Bağlantılar:** [[Gelistirme_Yol_Haritasi]] · [[Kodlama_Standartlari]] · [[Backend_Katmanli_Mimari]]

## Dosyalar
| Dosya | Amaç |
|-------|------|
| `docs/index.html` | **Hub** — kurallar + `LISTE` dizisindeki tüm API'ların kart listesi (klasörün bir üstünde durur, `API_YOL_HARITASI/style.css`'e görece referans verir) |
| `API_YOL_HARITASI/_TASLAK.html` | Yeni API şablonu — kopyalanıp doldurulur |
| `API_YOL_HARITASI/A-02_ortak-altyapi.html` | A-02 (Ortak Altyapı) rehberi — **tek gerçek API sayfası, şu an `durum: 'wip'`** |
| `API_YOL_HARITASI/render.js` | `window.API`/`const API` objesini okuyup `#content`'e basan paylaşımlı render motoru (kod bloklarını `esc()` ile XSS'e karşı güvenli işler) |
| `API_YOL_HARITASI/style.css` | Hem hub hem her API sayfasının ortak koyu-tema stili |

`docs/index.html`'in `LISTE` dizisi şu an tek satır içeriyor:
```js
{ faz: 'A-02', metot: '-', yol: '-', baslik: 'Ortak Altyapı', durum: 'wip', dosya: 'API_YOL_HARITASI/A-02_ortak-altyapi.html' }
```
Bu, [[Gelistirme_Yol_Haritasi]]'ndeki A-02 durumuyla birebir tutarlı — henüz hiçbir API `done` işaretli değil.

## Yeni API Ekleme Kuralı (3 adım)
1. `_TASLAK.html`'i kopyala → `<faz>_<id>.html` adıyla kaydet (örn. `A-03_auth-login.html`)
2. `window.API` objesini doldur: `id`, `faz`, `metot`, `yol`, `auth`, `baslik`, `durum`
   (`wip`/`done`), `ozet`, `adimlar[]` (her adımda `num`, `tur`, `baslik`, `dosya`, `aciklama`, `kod`)
3. `index.html`'deki LİSTE dizisine bir satır ekle

`adim.tur` değerleri: `entity | config | enum | dto | validator | exception | repository | service
| controller | test`. **Kod alanları gerçek dosyanın birebir kopyasıdır** — kırpılmaz, `...` ile kapatılmaz.
(`test` türü ilk kez A-02'nin 7. adımıyla eklendi; henüz kendi UI sekmesi yok, `aciklama` +
`kod` alanları [[Kodlama_Standartlari]] §7.6'daki test dokümantasyonunu taşıyor.)

## A-02_ortak-altyapi.html — Mevcut İçerik (7 adım, `durum: 'wip'`)

| # | Adım | Dosya | Wiki karşılığı |
|---|------|-------|-----------------|
| 1 | `entity` | `BaseEntity.cs` | [[BaseEntity]] |
| 2 | `config` | `WordLearnerDbContext.cs` | [[WordLearnerDbContext]] |
| 3 | `exception` | `EntityNotFoundException.cs` | [[EntityNotFoundException]] |
| 4 | `repository` | `IRepository.cs` | [[IRepository]] |
| 5 | `repository` | `Repository.cs` | [[Repository]] |
| 6 | `service` | `InfrastructureServiceExtensions.cs` | [[InfrastructureServiceExtensions]] |
| 7 | `test` | `RepositoryTests.cs` | [[RepositoryTests]] |

Bu 7 adım, `TASK.md`'deki A-02'nin ilk 4 alt-maddesiyle (BaseEntity, DbContext, IRepository/
Repository+extension, RepositoryTests) birebir örtüşüyor. **Henüz roadmap'e işlenmemiş** kalan
A-02 adımları: ortak tipler (`ApiResponse<T>`, `PagedResult<T>`), middleware'ler, Program.cs
genişletme — bu adımlar yazıldıkça buraya (yeni `adimlar` girdisi olarak) ve
[[Gelistirme_Yol_Haritasi]]'ne işlenmeli.

## Yeniden Kullanılan Kod Kuralı
`docs/index.html`'deki not: bir API daha önce yazılmış bir kodu/yardımcıyı (örn. [[Repository]],
`PasswordService.Hash`, ortak DTO) kullanıyorsa, o kodun **tam hâli** de o API'ın kendi sayfasına
ayrıca eklenir — her API sayfası tek başına baştan sona okunabilir olmalı (kod tekrarı bilinçli).
