# YAPILACAKLAR LİSTESİ

> **Durum:** Proje sıfırdan başlatılıyor. Hiçbir task tamamlanmamış kabul edilir — tümü ⬜.
>
> **Bu dosya artık bir INDEX'tir.** Faz task listeleri `TASK/` klasöründeki ayrı dosyalarda
> (aşağıdaki tablo). Bu sayfada yalnızca **değişmeyen yöntem/standart** (⭐ Çalışma Yöntemi,
> Her Parça İçin Döngü) ve genel ilerleme durumu bulunur — bir task'a başlarken önce bu sayfayı,
> sonra ilgili faz dosyasını oku.

## Nasıl Kullanılır

- Task'lar **faz sırasıyla** yapılır. Bir faz bitmeden sonrakine geçilmez.
- Claude'a: **"X-YY task'ını yapalım."** (task'ın hangi faz dosyasında olduğunu aşağıdaki tablodan bul)
- Her kodda `REFERENCE/CODING_STANDARDS.md` geçerlidir — tüm yorumlar Türkçe, AMAÇ/NEDEN/NASIL içerir.

## Geliştirme Sırası (Faz)

```
A) Admin Panel Backend  →  B) Admin Panel  →  C) Kullanıcı Backend
   →  D) Web App  →  E) Mobil  →  F) Test & Yayın
```

| Faz | Ne | Neden | Dosya |
|-----|----|----|-------|
| **A** | Admin panel backend (altyapı + auth + log + içerik + admin) | Admin panelin ihtiyacı olan tüm endpoint'ler önce | `TASK/A_admin_panel_backend.md` |
| **B** | Admin panel frontend (`/admin`) | Kelime/kategori girilir, API gerçek veriyle test edilir | `TASK/B_admin_panel.md` |
| **C** | Kullanıcı backend (web+mobil ortak API) | Kart, SRS, öğrenme, sosyal özellikler | `TASK/C_kullanici_backend.md` |
| **D** | Web app (`/web`) | Tarayıcıda test hızlı; mobile referans olur | `TASK/D_web_app.md` |
| **E** | Mobil (`/mobile`) | API + içerik + web referansı hazır → hızlı geliştirme | `TASK/E_mobil.md` |
| **F** | Test & deployment | Son kontroller | `TASK/F_test_yayin.md` |

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
> **Nasıl yazılır:** `REFERENCE/CODING_STANDARDS.md §7` (AAA deseni, isimlendirme kalıbı, mock kuralları, minimum
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
2. ➜ İlgili TASK/ dosyası → o maddeyi [ ] → [x] işaretle
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
- **🟩🟥 Yeni/eski satır vurgusu:** Yukarıdaki "yeniden kullanılan kod" kuralı gereği tam dosya tekrar
  gösterilirken, bu API için gerçekten **değişen** satırlar ham `kod` string'inde (escape'ten önce)
  `##NEW##` (eklendi → yeşil) veya `##OLD##` (kaldırıldı/değişti → kırmızı, üstü çizili) marker'ıyla
  işaretlenir — render.js bunu git diff gibi gösterir. Reused kısım işaretsiz kalır; tamamı yeni bir
  dosyaysa hiç marker kullanılmaz (detay → `API_YOL_HARITASI/_TASLAK.html`).
- **📦 Paket Kuralı:** Bu API için `dotnet add` ile eklenen **her yeni** NuGet paketi, sayfanın
  `paketler: [{ paket, versiyon, proje, neden }]` dizisine yazılır (adımlardan önce görünür şekilde
  gösterilir). Paket daha önceki bir API'de zaten eklenmişse burada **tekrar yazılmaz** — yalnızca bu
  API'nin ilk kez eklettiği paketler listelenir. NEDEN: Her API tek başına baştan sona kurulabilir
  olmalı; okuyucu kodu çalıştırmadan önce hangi paketi eklemesi gerektiğini görmeli.
- **Test Alanı:** Her API'ın yol haritası sayfasında kod adımlarından ayrı, kendi **"Test"** bölümü
  olur. Test sınıfı da birebir kopyalanır + her test metoduna `REFERENCE/CODING_STANDARDS.md §7.6`'daki
  3 satırlık (Test Adı / Ne Test Edildi / Neden Önemli) açıklama eklenir.
- **🧩 Çapraz Link Kuralı (Backend ↔ Frontend):** Bu API'yi tüketen bir frontend feature (Web/Admin/
  Mobil) zaten yazılmışsa veya bu task'la aynı anda planlanıyorsa, API'nın HTML sayfasına
  `frontendRefs: [{ dosya, baslik }]` eklenir — sayfanın en altında **"Buradan sonrası frontend
  tarafında"** bandı gösterir ve doğrudan ilgili `FRONTEND_YOL_HARITASI/*.html` sayfasına link verir.
  Karşılığı zorunludur: o feature sayfasındaki `tur:'api'` adımına da `backendRef` eklenir (bkz.
  aşağıdaki ⭐ Frontend Çalışma Yöntemi). Feature henüz yazılmadıysa `frontendRefs` boş bırakılır,
  feature yazılınca **geri dönüp** hem oraya hem buraya link eklenir (tek yönlü kalması yasak).
- **Spekülatif ortak tip yazılmaz (YAGNI):** "Ortak Altyapı" (A-02) dâhil hiçbir task'ta, bir
  ortak/paylaşılan tip (DTO, response zarfı, yardımcı sınıf vb.) **gerçek bir tüketicisi olmadan**
  önceden yazılmaz. "İleride lazım olur" varsayımıyla yazılan kod, gerçek ihtiyaç doğduğunda
  yanlış şekilde tasarlanmış çıkabilir (örn. bir controller yazılmadan tahmin edilen response şekli).
  Bunun yerine: bir tip, onu **fiilen kullanan** ilk somut kod parçası (bir middleware, bir controller,
  bir servis) yazılırken, o parçanın gerçek ihtiyacına göre yazılır — **bu ilk tüketici commit'inin
  parçası olarak**, ayrı bir "ortak altyapı" adımı olarak değil.
  **İstisna:** Bir tipin o an yazılmakta olan başka bir koddan (aynı task içinde) gerçek, somut bir
  tüketicisi varsa (örn. `ExceptionHandlingMiddleware`'in `ApiErrorResponse`'u kullanması) erken
  yazılabilir — çünkü bu artık spekülatif değil, kanıtlanmış bir ihtiyaçtır.
  **Örnek (A-02 düzeltmesi):** `ApiErrorResponse` A-02'de kaldı çünkü `ExceptionHandlingMiddleware`
  onu gerçekten kullanıyordu; `ApiResponse<T>` ve `PagedResult<T>` hiçbir controller yokken
  yazılmıştı (spekülatifti) — bu yüzden A-02'den çıkarıldı, ilk gerçek controller'ın (muhtemelen
  A-03 veya A-05) o parçayı gerçekten yazdığı anda, o anki gerçek ihtiyaca göre yazılacak.

---

## ⭐ Frontend Çalışma Yöntemi — Bir Feature'ı Baştan Sona Yaz

Backend'deki **⭐ Çalışma Yöntemi** ile birebir aynı disiplin, frontend'e uyarlanmış: bir feature'ı
(component/sayfa) tüm katmanlarıyla bitir, sonra diğerine geç. Katman katman (önce tüm tipler,
sonra tüm component'ler) **DEĞİL**. Bu kural Faz B (Admin Panel), D (Web App), E (Mobil) için geçerlidir.

### Bir Feature'ın Yazım Sırası (Gerçek Kod Akışı)

```
1. TS Tipi/Arayüz     → types/...              (API'dan gelen/giden verinin şekli)
2. RTK Query Endpoint → store/api/xxxApi.ts    ("API'a istek atıldı" — query/mutation)
3. Redux Slice        → store/slices/xxxSlice.ts (yalnızca ek local/UI state gerekiyorsa)
4. Custom Hook        → hooks/useXxx.ts        (varsa — component'ten iş mantığını ayırır)
5. Component          → components/Xxx.tsx     (JSX + mantık; component'in kendisi)
6. Route/Import       → App.tsx veya XxxPage.tsx (component nereye/nasıl import edildi)
7. Birim Test         → Xxx.test.tsx           (RTL; component/hook/slice testi — hemen)
8. ➜ FRONTEND YOL HARİTASI → bu feature için HTML sayfası oluştur
   (FRONTEND_YOL_HARITASI/_TASLAK.html kopyala), her parçayı yazar yazmaz objesine ekle,
   FRONTEND_YOL_HARITASI/index.html LISTE'ye satır ekle
```

> **Mobil (Faz E) farkı:** Adım 6 `React Navigation` route tanımıdır (`App.tsx` yerine
> `navigation/*Navigator.tsx`), state/veri katmanı (1-4) aynıdır — web ve mobil aynı backend API'sini
> ve mümkünse aynı RTK Query/tip tanımlarını paylaşabilir.
> **Admin (Faz B) farkı:** Google/Apple auth yok; RTK Query endpoint'leri Admin-only endpoint'lere
> (`/admin/*`) bağlanır.

### Her Parça İçin Döngü (Sona Bırakma! — Backend'deki kuralla aynı)

```
1. Kod parçasını yaz   (örn. RTK Query endpoint)
2. ➜ İlgili TASK/ dosyası → o maddeyi [ ] → [x] işaretle
3. ➜ Frontend Yol Haritası → o parçayı (kod + AÇIKLAMA) feature'ın HTML sayfasına ekle
   → sonraki parçaya geç (örn. component) ve aynı döngüyü tekrarla
```

**Kurallar (backend ile aynı):**
- **Açıklama zorunlu:** Her adımın `aciklama` alanı *neden* o parçanın yazıldığını anlatır.
- **Yeniden kullanılan kod tekrar yazılır:** Bu feature daha önce yazılmış bir component/hook/slice
  kullanıyorsa (örn. ortak `Button`, `useAuth`), o kodun **tam hâli** bu feature'ın sayfasına da eklenir.
- **Birebir kopya:** Kod blokları gerçek dosyanın aynısıdır; kırpılmaz, `...` ile kapatılmaz.
- **🟩🟥 Yeni/eski satır vurgusu:** Reused kodda bu feature için gerçekten değişen satırlar
  `##NEW##`/`##OLD##` marker'ıyla işaretlenir (bkz. yukarıdaki ⭐ Çalışma Yöntemi — birebir aynı kural).
- **📦 Paket Kuralı:** Bu feature için `npm i` ile eklenen **her yeni** paket, sayfanın
  `paketler: [{ paket, versiyon, neden }]` dizisine yazılır (paket daha önce başka bir feature'da
  eklenmişse tekrar yazılmaz — bkz. yukarıdaki ⭐ Çalışma Yöntemi).
- **🧩 Çapraz Link Kuralı (Frontend ↔ Backend):** `tur:'api'` adımı (RTK Query — backend'e istek
  atılan tam nokta) bir `backendRef: { dosya, baslik }` alanı alır — o adımın hemen altında
  **"Buradan sonrası backend tarafında"** bandı gösterilir ve ilgili `API_YOL_HARITASI/*.html`
  sayfasına link verir. Karşılığı zorunludur: o API'nın backend sayfasındaki `frontendRefs`
  dizisine de bu feature eklenir (iki yönlü link — bkz. yukarıdaki ⭐ Çalışma Yöntemi). Backend API'ı
  henüz yazılmadıysa `backendRef` boş bırakılır, API yazılınca **geri dönüp** her iki tarafa da eklenir.
- Bir feature, yol haritasına işlenmeden **tamamlandı sayılmaz.**

Detay ve `adım.tur` değerleri (`tip`/`api`/`slice`/`hook`/`component`/`route`/`test`) →
`FRONTEND_YOL_HARITASI/_TASLAK.html` içindeki yorum bloğu.

---

## İlerleme Durumu

| Faz | Task Aralığı | Başlık | Durum | Dosya |
|-----|--------------|--------|-------|-------|
| A | A-01…A-10 | Admin Panel Backend | 🔄 | `TASK/A_admin_panel_backend.md` |
| B | B-01…B-09 | Admin Panel | ⬜ | `TASK/B_admin_panel.md` |
| C | C-01…C-10 | Kullanıcı Backend | ⬜ | `TASK/C_kullanici_backend.md` |
| D | D-01…D-12 | Web App | ⬜ | `TASK/D_web_app.md` |
| E | E-01…E-14 | Mobil | ⬜ | `TASK/E_mobil.md` |
| F | F-01…F-04 | Test & Yayın | ⬜ | `TASK/F_test_yayin.md` |

**Sıradaki task:** `A-03.1 — QR Kod ile Giriş` (A-03 ✅ tamamlandı) → `TASK/A_admin_panel_backend.md`

## Durum Göstergesi
⬜ Başlanmadı · 🔄 Devam ediyor · ✅ Tamamlandı · ⛔ Engellendi
