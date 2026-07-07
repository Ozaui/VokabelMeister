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

### Bir API'ın Yazım Sırası (Gerçek Kod Akışı — MediatR CQRS)

> **2026-07-07 itibarıyla:** A-03 (Auth API), A-02'de kurulan ama kullanılmayan MediatR'ı gerçekten
> devreye alacak şekilde Command+Handler desenine refactor edildi (bkz. `wiki/Index.md` On yedinci
> INGEST). **Kesim noktası A-05 değil — A-03'ün bu retrofit'inden sonraki HER task (A-03.1 dahil)
> en baştan bu desenle yazılır** — aşağıdaki sıra artık kanonik yöntemdir, "Servis Arayüzü/Servis"
> deseni terk edildi. (Not: A-03.1'in checklist metni bu karardan önce yazıldığı için hâlâ eski
> "IQrLoginService/QrLoginService" ifadesini taşıyor — geçersiz, bkz. `TASK/A_admin_panel_backend.md`.)
>
> **Aynı gün, aynı desen — AutoMapper:** A-02'de kurulan ama A-03'ün ilk hâlinde (MediatR retrofit'i
> sonrasında bile) hiç kullanılmayan AutoMapper de aynı şekilde retrofit edildi (bkz. `wiki/Index.md`
> On sekizinci INGEST). Farkı: MediatR'ı kullanmamak mimari kayıptı, AutoMapper'ı kullanmamak
> YAGNI'ye daha uygundu (çoğu Command sabit `MessageResponse` döner, gerçek Entity→DTO dönüşümü
> nadir) — bu yüzden aşağıdaki adım **koşullu**: yalnızca Handler gerçekten bir Entity'yi DTO'ya
> çeviriyorsa yazılır, her API'da otomatik olarak yazılmaz.

```
1.  Entity              → Domain/Entities/<Domain>/...  (API'ın çalışacağı veri modeli; enum varsa
                          Domain/Enums/<Domain>/ — bkz. "Klasör Organizasyonu",
                          `wiki/Standartlar/Kodlama_Standartlari.md`, BaseEntity hariç her şey domain
                          alt klasöründe, namespace klasörle eşleşir: `...Entities.<Domain>`)
2.  EF Konfigürasyon    → Infrastructure/Data/Configurations/<Domain>/  (FK, index, check, soft delete)
3.  Migration           → dotnet ef migrations add ...        (DB'ye tabloyu ekle)
4.  Command/Query       → Application/Features/Xxx/XxxCommand.cs — yalnızca
                          `public record XxxCommand(...) : IRequest<XxxResponse>;` satırı yazılır
                          (endpoint'in aldığı girdi, eski "Request DTO"nun yerini alır)
5.  Validator(lar)      → Application/Validators/...  (FluentValidation kuralları, Command'ı doğrular)
6.  Exception(lar)      → varsa (XxxNotFound, XxxDuplicate ...)
7.  Repository Arayüzü  → Application/Interfaces/Repositories/IXxxRepository
8.  Repository          → Infrastructure/Repositories/XxxRepository
9.  Response DTO        → Application/DTOs/... (Handler'ın döndürdüğü çıktı)
10. AutoMapper Profile  → KOŞULLU — yalnızca Handler gerçekten `new XxxResponse(entity.Id, ...)`
                          gibi bir Entity→DTO dönüşümü yapıyorsa yazılır (sabit mesaj/`MessageResponse`
                          dönen Handler'lar bunu atlar, bkz. aşağıdaki "AutoMapper kuralı"). Varsa:
                          Application/Features/Xxx/XxxProfile.cs — `CreateMap<Entity, XxxResponse>()`
                          (alan adları uyumluysa ek `ForMember` gerekmez, bkz. [[AuthProfile]])
11. Handler             → AYNI dosyaya (Application/Features/Xxx/XxxCommand.cs) eklenir —
                          `XxxCommandHandler : IRequestHandler<XxxCommand, XxxResponse>`,
                          tüm yardımcıları birleştiren iş mantığı (dikey dilim: Command+Handler
                          hep aynı dosyada, ayrı Commands/Handlers klasörlerine bölünmez);
                          AutoMapper Profile yazıldıysa dönüş satırı `_mapper.Map<XxxResponse>(entity)`
12. Birim Test (Handler) → Tests/Features/Xxx/XxxCommandHandlerTests (repo/dış servis mock'lanır; Handler bitince hemen).
                          IMapper mock'lanmaz — gerçek Profile'dan kurulmuş bir IMapper kullanılır
                          (`new MapperConfiguration(cfg => cfg.AddProfile<XxxProfile>()).CreateMapper()`),
                          bu hem Handler'ı besler hem Profile konfigürasyonunun geçerliliğini doğrular
13. Controller          → API/Controllers/XxxController (ince katman — yalnızca
                          `await _mediator.Send(command, ct)` çağırır, iş mantığı içermez)
14. DI kaydı            → GENELLİKLE gerekmez — MediatR/AutoMapper/FluentValidation assembly-scan ile
                          Command/Handler/Profile/Validator'ı otomatik bulur (bkz. [[ApplicationServiceExtensions]]).
                          İstisna: birden fazla Handler'ın paylaşacağı yeni bir yardımcı servis
                          varsa (aşağıdaki "Paylaşılan Servis" notuna bkz.), o servis DI'a eklenir.
15. ➜ API YOL HARİTASI  → bu API için HTML sayfası oluştur (API_YOL_HARITASI/_TASLAK.html kopyala),
                          her Command+Handler çiftini TEK bir adım (`tur: 'handler'`) olarak
                          objesine ekle, AutoMapper Profile yazıldıysa onu da ayrı bir adım
                          (`tur: 'service'`) olarak ekle, index.html LISTE'ye satır ekle
```

> **AutoMapper kuralı:** Her Response DTO için otomatik olarak bir Profile yazılmaz — bu YAGNI'ye
> aykırıdır (bkz. A-03'ün ilk hâlindeki hata: kurulan paket hiç kullanılmadan kaldı). Yalnızca
> Handler'ın döndürdüğü DTO gerçekten bir Entity'nin alanlarından üretiliyorsa (`new XxxResponse(entity.Id,
> entity.Alan, ...)` deseni) Profile yazılır; DTO sabit bir string (`MessageResponse("...")`) veya
> birden fazla bağımsız kaynaktan (token, config, hesaplanmış bool gibi) inşa ediliyorsa AutoMapper'a
> konu olmaz, elle inşa edilmeye devam eder (bkz. A-03'teki `AuthTokenResponse` örneği — yalnızca
> içindeki `AuthUserDto` alt-nesnesi mapping'e taşındı, geri kalanı elle kaldı).

> **Paylaşılan Servis kuralı:** Birden fazla Handler'ın (aynı API içinde) AYNI mantığı (ör. OTP
> üretimi, ortak bir "akışı tamamlama" adımı) kullanması gerekiyorsa, bu mantık handler'lar arası
> kod tekrarını önlemek için küçük bir arayüz+implementasyon çiftine çıkarılır (`Application/Services/`,
> `Application/Interfaces/Services/` — flat, feature'a özel alt klasör açılmaz, bkz. A-03'teki
> `IOtpService`/`ILoginCompletionService` örneği). **Handler'lar birbirini `_mediator.Send()` ile
> ASLA çağırmaz** (anti-pattern, döngüsel bağımlılık ve gizli sıralama riski yaratır) — paylaşılan
> mantık her zaman bu tarz bağımsız bir servise çıkarılır, başka bir Handler'a değil.

> **Test felsefesi:** Testler Faz F'ye bırakılmaz. Her API'nın **Handler'ı** bitince (adım 10),
> o Handler'ın birim testi **aynı task içinde** (adım 11) yazılır — kod gibi sona bırakılması yasaktır.
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
  bir Handler) yazılırken, o parçanın gerçek ihtiyacına göre yazılır — **bu ilk tüketici commit'inin
  parçası olarak**, ayrı bir "ortak altyapı" adımı olarak değil.
  **İstisna:** Bir tipin o an yazılmakta olan başka bir koddan (aynı task içinde) gerçek, somut bir
  tüketicisi varsa (örn. `ExceptionHandlingMiddleware`'in `ApiErrorResponse`'u kullanması) erken
  yazılabilir — çünkü bu artık spekülatif değil, kanıtlanmış bir ihtiyaçtır.
  **Örnek (A-02 düzeltmesi):** `ApiErrorResponse` A-02'de kaldı çünkü `ExceptionHandlingMiddleware`
  onu gerçekten kullanıyordu; `ApiResponse<T>` ve `PagedResult<T>` hiçbir controller yokken
  yazılmıştı (spekülatifti) — bu yüzden A-02'den çıkarıldı, ilk gerçek controller'ın (muhtemelen
  A-03 veya A-05) o parçayı gerçekten yazdığı anda, o anki gerçek ihtiyaca göre yazılacak.
- **API tamamlandığında** git commit/push + durum güncelleme adımları için → aşağıdaki
  **⭐ Bir API/Feature Tamamlandığında** bölümü.

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
8. ➜ FRONTEND YOL HARİTASI → bu feature için HTML sayfası oluştur, ilgili projenin roadmap
   klasöründe (`_TASLAK.html` kopyala), her parçayı yazar yazmaz objesine ekle, o klasörün
   `index.html` LISTE'sine satır ekle
```

> **Hangi roadmap klasörü?** Admin (Faz B) → `docs/ADMIN_YOL_HARITASI/`, Web (Faz D) →
> `docs/WEB_YOL_HARITASI/`, Mobil (Faz E) → `docs/MOBILE_YOL_HARITASI/`. Üçü ayrı projeler
> olduğu için (kod paylaşımı yok, bkz. [[Sistem_Mimarisi]]) eskiden tek olan
> `FRONTEND_YOL_HARITASI/` üçe ayrıldı (2026-07-07) — her birinin kendi hub'ı, `_TASLAK.html`'i,
> `render.js` kopyası var.

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
  (Bu kural üç roadmap klasörünün — `ADMIN_YOL_HARITASI`/`WEB_YOL_HARITASI`/`MOBILE_YOL_HARITASI` —
  hepsinde birebir aynı şekilde işler.)
- Bir feature, yol haritasına işlenmeden **tamamlandı sayılmaz.**
- **Feature tamamlandığında** git commit/push + durum güncelleme adımları için → aşağıdaki
  **⭐ Bir API/Feature Tamamlandığında** bölümü (backend ile birebir aynı).

Detay ve `adım.tur` değerleri (`tip`/`api`/`slice`/`hook`/`component`/`route`/`test`) →
`FRONTEND_YOL_HARITASI/_TASLAK.html` içindeki yorum bloğu.

---

## ⭐ Bir API/Feature Tamamlandığında — Git + Durum Güncelleme

Yukarıdaki "Her Parça İçin Döngü" (backend ve frontend, ikisi de) yalnızca kod↔roadmap↔TASK
işaretleme döngüsünü kapsar — alt-adım başına tekrarlanır. Bir API/feature'ın **tamamı** bitince
(tüm alt-adımlar `[x]`, roadmap sayfası eksiksiz, testler yeşil) ek olarak bu 4 adım işletilir
(2026-07-07 itibariyle eklendi — daha önce hiçbir yerde yazılı değildi):

1. **Git commit** — anlamlı, Türkçe, task numarasıyla başlayan mesaj (mevcut commit geçmişiyle aynı
   format, örn. `"A-03: AuthController (13 endpoint) + FluentValidation + rate limiting"`). Alt-adım
   başına değil, **API/feature başına** (veya mantıklı alt-gruplar hâlinde) commit atılır — her
   DTO/entity/component için ayrı commit açılmaz.
2. **Git push (GitHub'a gönder)** — bu adım her seferinde kullanıcı onayıyla yapılır (git safety
   protokolü gereği otomatik/sorulmadan push yapılmaz); ama task tamamlanınca push'un **sorulması**
   standart akışın bir parçasıdır, unutulmamalıdır.
3. **`docs/TASK.md` güncelle** — aşağıdaki İlerleme Durumu tablosundaki faz durumunu (⬜→🔄→✅) ve
   "Sıradaki task" satırını güncelle.
4. **(Varsa) `docs/wiki/Index.md`'ye yeni INGEST kaydı** — `wiki_schema.md` kuralı gereği.
5. **Kurulu-ama-kullanılmayan paket taraması (2026-07-07'den itibaren zorunlu):** MediatR ve
   AutoMapper'ın A-02'de kurulup A-03 boyunca (birden fazla task) fark edilmeden kullanılmadan
   kalması bu kontrolün eksikliğinden oldu — `git log` yerine `grep` ile fiilen doğrulanmalı,
   "muhtemelen kullanılıyordur" varsayılmamalı. Bu API/task'ta (veya önceki fazlardan kalan) her
   `.csproj`'a eklenmiş paket için gerçek bir çağrı noktası olduğu tek satırlık bir `grep` ile
   kontrol edilir (bkz. bu session'daki tam paket taraması — [[Teknik_Ozellikler]]). Sıfır kullanım
   çıkarsa iki seçenek vardır: (a) paket kaldırılır (YAGNI), (b) paket **bilinçli olarak** sonraki
   somut bir task'a hazırlık içindir (ör. `Serilog.Sinks.MSSqlServer` → A-04) — bu durumda
   `Teknik_Ozellikler.md`'deki paket tablosuna "⚠️ kurulu, henüz bağlı değil — hedef: A-0X" notu
   düşülür ve o task'a başlanırken bu not kontrol edilip paket fiilen bağlanır. **Asla** sessizce
   "ileride kullanılır" diye bırakılıp bir dahaki denetime kadar unutulmaz.

> Not: "Route/Import" (backend'de Controller kaydı, frontend'de `App.tsx`/Navigator'a ekleme) adımı
> zaten tüm alt-parçaları (tip/api/slice/hook/component) **tek bir çalışan bütüne birleştiren**
> adımdır — ayrı bir "birleştirme" adımına gerek yok, mevcut sıradaki son adım budur.

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
