# CLAUDE.md — Agent Anayasası

> **Bu dosyayı her oturumun başında oku.** Tüm değişmez kurallar burada, tek yerde. Diğer
> dosyaları yalnızca o task gerektirince aç (bkz. §2 Yönlendirme). Bir kuralı öğrenmek için
> birden çok dosya okuman gerekmiyor — hepsi burada.
>
> **Proje:** WordLearner — Almanca↔Türkçe kelime öğrenme (SRS). .NET 9 Web API + React (admin/web) +
> React Native (mobil). Junior eğitimi amaçlı yazılır: kod kendini anlatır, Türkçe yorumlar *neden*i.

---

## 1. Değişmez Kurallar (her kodda geçerli)

**Dil**
- Kullanıcıyla **Türkçe** konuş.
- **Türkçe:** kod yorumları (kısa, NEDEN odaklı — bkz. "Yorum satırları"), MD dosyaları, roadmap.
- **İngilizce:** method/class/property/DB kolon/JS değişken adları, test metodu adları, `_logger.Log*` mesajları, exception `.Message`, hata `Code` sabitleri (ör. `INVALID_CREDENTIALS`).
- **İstisna — istemciye giden mesaj:** `AppException.Code`/FluentValidation `ErrorCode`, isteğin `Accept-Language`'ına göre `ErrorMessages` sözlüğünden çözülür. Şu an yalnızca **tr/de** dolu (hedef kitle DE↔TR); sözlük dile göre anahtarlandığı için yeni bir dil (ör. `en`) yalnızca `ErrorMessages`/`SuccessMessages` sözlüklerine bir sütun eklemekle desteklenir, başka hiçbir koda dokunulmaz. Kullanıcı seçtiği dili görür (desteklenmiyorsa tr'ye düşer); DB/log/geliştirici İngilizce görür. Ayrı iki kanal.
- **İkinci istisna — `ActivityLog`/`SecurityLog`'un admin'e görünen alanları:** `Action`/`EventType` sabit/dilden bağımsız kod kalır (`_logger.Log*` ile aynı kural) ama `Detail`/`OldValue`/`NewValue` **admin panelin kendisi bir istemci olduğu için** (admin de dil tercihine sahip) serbest metin yerine bir **Code** olarak yazılır — log satırı yazılırken (ör. anonim bir isteğin `Accept-Language`'ıyla) hangi admin'in ne zaman hangi dille okuyacağı bilinmediğinden, tr/de çözümü ancak admin `GET /admin/logs/*` (A-07) ile okurken KENDİ `Accept-Language`'ıyla yapılabilir — `ErrorMessages` ile birebir aynı Code-sonra-çöz deseni, yalnızca çözme anı farklı (istek anı değil, okuma anı).
- **Admin panelin kendi dil tercihi (B-01'den itibaren):** `admin/src/store/slices/languageSlice.ts` (tr/de, `localStorage` persist, varsayılan **tr** — `ErrorMessages` ile aynı "desteklenmiyorsa tr'ye düş" kuralı), `admin/src/store/api.ts` her istekte bu tercihi `Accept-Language` header'ı olarak backend'e yollar (yukarıdaki iki istisnanın frontend tarafı budur — backend mesajları VE log okuma bu sayede admin'in seçtiği dilde döner). Admin panelin KENDİ statik arayüz metinleri (buton/etiket, backend'den gelmeyen) `react-i18next` ile ayrıca tr/de çevrilir — bu backend `ErrorMessages`'tan bağımsız, saf frontend kopyası (`admin/src/i18n/locales/{tr,de}.json`). Web/Mobil (Faz D/E) aynı deseni kendi fazları başladığında kurar.

**Yorum satırları**
- Kod kendini anlatır (iyi isimlendirme). Yorum yalnızca kodun anlatamadığını açıklar: gizli bir kısıt, non-obvious bir NEDEN, bilerek alınmış bir karar. NE yaptığını değil NEDEN öyle yaptığını anlat.
- Zorunlu dosya-başı/method-başı AMAÇ/NEDEN/NASIL bloğu **YOK** — bu blok kaldırıldı (eski kod tabanında vardı, kaldırılıyor). Dosya/sınıf/metot adı zaten ne yaptığını söylüyor.
- Yorum kısa ve Türkçe — genelde tek satır, gerekirse iki. Paragraf hâlinde uzun blok yorum **yasak**; anlatılacak şey birkaç satıra sığmıyorsa muhtemelen bir yardımcı metot/isimlendirme sorunu var, yorum onu telafi etmez.
- Bu kural yalnızca **kaynak kod**(`.cs`/`.ts`/`.tsx`) yorumları için geçerli. Akademi klasörlerinin (§6 — kök `AKADEMI/` altında `backend/`, `admin/`, gelecekte `web/`/`mobile/`) içindeki `aciklama`/`neden`/`olmasaydi` alanları öğretim materyali — bu kuralın dışında, ayrıntılı kalmaya devam eder.

**Roller ve sahiplik**
- **Yalnızca iki rol:** `User` (herkes kayıt olur) ve `Admin` (elle atanır). `Instructor`/`Teacher`/"öğretmen" **YOK**. Hiçbir public endpoint rol yükseltemez.
- Sistem içeriği (`Word`, `Category`) CRUD → `[Authorize(Roles="Admin")]`; okuma → `[Authorize]`.
- Kişisel içerik (`UserCard`, `UserCategory`) → yalnızca sahibi; her sorguda `UserId` filtresi zorunlu, başkasının kaydı 404/403.

**Çoklu dil**
- Kelime = dilden bağımsız `WordConcept` + her dile bir `Words` satırı (`Languages`'e bağlı). Kategori adı = `Categories` + `CategoryTranslations`.
- Dile özel ad-hoc alan (`NameEN`, `EnglishTranslation`, `GermanWord` vb.) entity/DTO'ya **EKLENMEZ**.
- Şu an yalnızca `de`+`tr` içerik. Yeni dil = `Languages`'e 1 satır + kavramlara `Words` satırı → **migration gerekmez**.

**Veri katmanı**
- Her tablo `BaseEntity` taşır (log tabloları hariç): `Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt, CreatedByUserId, UpdatedByUserId, DeletedByUserId`.
- Repository sorgularında **soft delete filtresi** + kişisel içerikte **UserId filtresi** zorunlu.
- Parametreli sorgu / EF Core LINQ. String birleştirmeyle SQL **yasak**.
- `async/await` + `CancellationToken` her I/O metodunda.
- Log tabloları değişmez (insert-only): soft delete yok, güncellenmez.
- **İçerik değiştiren her CRUD** (`Word`/`Category`/`UserCard`/`Class`/`SharedContent` vb. create/
  update/delete, admin toplu import, medya yükleme, hesap anonimleştirme) A-04'te yazılan
  `IActivityLogger`'a yazar (`Action=CREATE_X`/`UPDATE_X`/`DELETE_X`, `EntityType`+`EntityId`,
  `OldValue`/`NewValue` JSON diff — şifre/hash gibi hassas alanlar diff'ten hariç tutulur). Admin'e
  özel hassas işlemler (rol/hesap durumu değişimi, SMTP ayarları) **ayrıca** `ISecurityLogger`'a
  (`LogEventType.AdminAction`) da yazar. Yeni bir task'a başlarken bu kural unutulursa `TASK/
  A_admin_panel_backend.md` A-04 sonrası eklenen per-task notlarına bakılır.

**Kimlik & güvenlik**
- ASP.NET Identity **KULLANILMAZ** — JWT + şifre hashleme manuel.
- Access 15dk, Refresh 7gün (her refresh'te rotation, Token Family Pattern). Şifre: bcrypt wf=12.
- Hassas değer (`SecretKey`, bağlantı dizesi, AES anahtarı) **asla** `appsettings.json`/kaynak koda girmez → ENV (`REFERENCE/ENV.md`). Yeni servis eklenince ENV.md güncelle.
- Loglarda ham e-posta yok → `SHA-256(email)`. Şifre/token asla loglanmaz.

**Test**
- Her public servis/Handler metodunun birim testi **aynı task içinde** yazılır (Faz F'ye bırakma). Standart → `REFERENCE/CODING_STANDARDS.md §7`.

---

## 2. Yönlendirme — Hangi Task İçin Hangi Dosya

Bu dosyayı okuduktan sonra, task'a göre **yalnızca** ilgili dosyayı aç:

| Ne yapıyorsun | Oku |
|---------------|-----|
| Task'a başlıyorum, sıradaki ne? | `TASK.md` → ilgili `TASK/<faz>.md` |
| Bir tabloyu/entity'yi yazacağım | `DATABASE_SCHEMA.md` → ilgili `DATABASE_SCHEMA/<domain>.md` (yalnızca o domain) |
| Endpoint imzası/istek-yanıt şekli | `REFERENCE/API_ENDPOINTS.md` (ilgili bölüm) |
| Sistem mimarisi/akış/rol matrisi | `REFERENCE/ARCHITECTURE.md` |
| Kod yorum/isim/test standardı | `REFERENCE/CODING_STANDARDS.md` |
| Auth/JWT/OTP/QR/şifreleme detayı | `REFERENCE/SECURITY.md` |
| NuGet/npm, JWT/SM-2/Repository kod örneği | `REFERENCE/TECHNICAL_SPECIFICATIONS.md` |
| Ortam değişkeni ekleyeceğim | `REFERENCE/ENV.md` |
| Kurulum/çalıştırma/yayınlama | `REFERENCE/DEVELOPMENT_SETUP.md` |
| Kelime kartı gramer JSON'u (Almanca) | `REFERENCE/GERMAN_LANGUAGE_FEATURES.md` |
| Kelime kartı gramer JSON'u (Türkçe) | `REFERENCE/TURKISH_LANGUAGE_FEATURES.md` |
| Admin panel görsel tasarımı (renk/tipografi/stil) | `REFERENCE/DESIGN_SYSTEM.md` |

---

## 3. Backend API Yazım Sırası (dikey dilim + MediatR CQRS)

Bir API'ı **tüm katmanlarıyla bitir, sonra diğerine geç.** Katman katman (önce tüm entity'ler) DEĞİL.
Kanonik desen MediatR Command+Handler; "Servis Arayüzü/Servis" deseni **terk edildi**.

```
1.  Entity            → Domain/Entities/<Domain>/     (enum varsa Domain/Enums/<Domain>/)
2.  EF Konfigürasyon  → Infrastructure/Data/Configurations/<Domain>/  (FK, index, check, soft delete)
3.  Migration         → dotnet ef migrations add AddXxx
4.  Command/Query     → Application/Features/Xxx/XxxCommand.cs
                        (public record XxxCommand(...) : IRequest<XxxResponse>;)
5.  Validator(lar)    → Application/Validators/         (FluentValidation)
6.  Exception(lar)    → varsa (XxxNotFound, XxxDuplicate…)
7.  Repository arayüz → Application/Interfaces/Repositories/IXxxRepository
8.  Repository        → Infrastructure/Repositories/XxxRepository
9.  Response DTO      → Application/DTOs/
10. AutoMapper Profile→ KOŞULLU (aşağıya bkz.) → Application/Features/Xxx/XxxProfile.cs
11. Handler           → AYNI dosyaya (XxxCommand.cs): XxxCommandHandler : IRequestHandler<...>
12. Birim Test        → Tests/Features/Xxx/XxxCommandHandlerTests (repo/dış servis mock; Handler bitince hemen)
13. Controller        → API/Controllers/XxxController (ince: yalnızca _mediator.Send(command, ct))
14. DI kaydı          → GENELLİKLE gerekmez (assembly-scan). İstisna: paylaşılan yardımcı servis.
15. Backend Akademi   → AKADEMI/backend/<faz>/ HTML bölümü, controller `kod` slaytının HEMEN
                        ARDINDAN o endpoint'in `postman` slaytı dahil (bkz. §6)
```

**Koşullu kurallar (YAGNI):**
- **AutoMapper Profile yalnızca** Handler gerçekten `new XxxResponse(entity.Id, entity.Alan…)` gibi bir Entity→DTO dönüşümü yapıyorsa yazılır. DTO sabit mesaj (`MessageResponse("…")`) veya token/config/hesaplanmış değerlerden inşa ediliyorsa elle inşa edilir. Test: `IMapper` mock'lanmaz, gerçek Profile'dan kurulur (`new MapperConfiguration(cfg => cfg.AddProfile<XxxProfile>()).CreateMapper()`).
- **Paylaşılan mantık** (OTP üretimi vb.) birden çok Handler'da gerekiyorsa küçük bir arayüz+impl'e çıkarılır (`Application/Services/` + `Application/Interfaces/Services/`, flat). **Handler'lar birbirini `_mediator.Send()` ile ASLA çağırmaz** (döngüsel bağımlılık).
- **Spekülatif ortak tip yazılmaz.** Bir DTO/response zarfı/yardımcı, onu **fiilen kullanan ilk somut kod** yazılırken, o kodun parçası olarak yazılır — "ileride lazım olur" diye önceden değil.

---

## 4. Frontend Feature Yazım Sırası (Faz B/D/E)

Backend'le aynı disiplin: bir feature'ı tüm katmanlarıyla bitir, sonra diğerine geç.

```
1. TS Tipi/Arayüz     → types/
2. API Fonksiyonu     → store/api/xxxApi.ts     (admin: axios `apiClient` + ince `useApiQuery`/
                        `useApiMutation` hook'u, `hooks/useApiQuery.ts`/`useApiMutation.ts`;
                        backend'e istek — cross-link noktası)
3. Redux Slice        → store/slices/xxxSlice.ts (yalnızca ek local/UI state gerekirse — admin'de
                        RTK Query YOK, Redux Toolkit yalnızca auth/theme/language/filter gibi
                        local state için)
4. Custom Hook        → hooks/useXxx.ts          (varsa)
5. Component          → components/Xxx.tsx
6. Route/Import       → App.tsx / (mobil) navigation/*Navigator.tsx
7. Birim Test         → Xxx.test.tsx             (RTL; hemen)
8. İşleme              → B/D/E: AKADEMI/<katman>/<faz>/ HTML bölümü (§6 — component `kod`
                        slaytından hemen sonra `onizleme` slaytı dahil)
```

Admin farkı: Google/Apple yok, endpoint'ler `/admin/*`. Mobil farkı: adım 6 React Navigation; state katmanı web'le paylaşılabilir.

---

## 5. Klasör / Namespace

- `Domain ← Application ← Infrastructure ← API` (bağımlılık yönü).
- `BaseEntity` hariç her entity/enum bir **domain alt klasöründe**; namespace klasörle eşleşir (`...Entities.<Domain>`).
- Command+Handler (+Profile) **aynı dosyada**; ayrı Commands/Handlers klasörüne bölünmez.
- `Application/Services` ve `Application/Interfaces/Services` **flat** (feature alt klasörü açma).

---

## 6. Kod Akademisi Kuralı — her parça yazılınca HEMEN

Her katman (backend, admin, gelecekte web/mobil) kendi **akademi klasörüne** ve kendi
`STANDART.md`'sine sahiptir — aynı slayt motoru (`engine/`), aynı disiplin, tek somut fark
"bunu gerçekte nasıl denerim" slaytının türü (backend'de `postman`, frontend'de `onizleme`):

| Faz | Akademi klasörü | Task kodu | "Nasıl denerim" slaytı |
|---|---|---|---|
| A (backend) | `AKADEMI/backend/` | `A-0X` | `postman` |
| B (admin) | `AKADEMI/admin/` | `B-0X` | `onizleme` |
| D (web) | `AKADEMI/web/` (D fazı başlayınca açılır) | `D-0X` | `onizleme` |
| E (mobil) | `AKADEMI/mobile/` (E fazı başlayınca açılır) | `E-0X` | `onizleme` |

Tüm akademiler kök `AKADEMI/` klasörü altında yaşar (kök dizin kalabalıklaşmasın diye), `AKADEMI/index.html` hepsine tek giriş noktasıdır. Toplu yazma **yasak**. Her kod parçasını yazar yazmaz: (1) ilgili `TASK/` maddesini `[ ]→[x]`, (2) parçayı ilgili `AKADEMI/<katman>/<faz>_.../` klasöründeki ilgili bölüme işle. Şema/kurallar tek doğruluk kaynağı — backend için `AKADEMI/backend/STANDART.md`, admin için `AKADEMI/admin/STANDART.md` (ikincisi ortak kurallarda birinciye referans verir, tekrar etmez) — burada tekrar edilmez.

- **Slayt tabanlı, tek görev = tek klasör:** Yeni bir görev (`A-0X`/`B-0X`/...) ilgili akademinin `_TASLAK/` klasöründen kopyalanır; her bölüm dosyası `01_...html`, `02_...html`… numaralanır ve `window.MODULE` objesiyle çalışır (`slaytlar[]` türleri: `kapak/kavram/kod/karsilastirma/sozluk/ozet/kod-degisiklik` + katmana özel "nasıl denerim" türü).
- **Birebir kopya:** `kod` slaytları gerçek dosyanın aynısı, kırpılmaz, uydurulmaz.
- **Zorunlu üçlü:** her `kod`/`kavram` slaytında ne (`aciklama`) → neden (`neden`) → böyle olmasaydı ne olurdu (`olmasaydi`) — "kural böyle" yetersiz, somut mühendislik gerekçesi şart.
- **"Nasıl denerim" slaytı zorunlu:** Backend'de bir endpoint controller'a bağlandığında (§3 adım 13), o endpoint'i akademiye işlerken (§3 adım 15) controller'ın `kod` slaytından HEMEN SONRA bir `postman` slaytı eklenir. Admin'de (ve gelecekte web/mobilde) bir component bir route'a bağlandığında (§4 adım 6), o component'i akademiye işlerken component'in `kod` slaytından HEMEN SONRA bir `onizleme` slaytı eklenir — gerçek route, kullanıcı akışı (`akis[]`), varsa gerçek backend endpoint çağrısı (`apiCagrisi`), durumlar. Alan şeması ve örnek: ilgili akademinin `STANDART.md` §3. İstisna yok.
- **Temsili öğretim (YAGNI):** Tekrarlayan kod aileleri (ör. 13 handler testinden yalnızca biri, veya birbirine çok benzeyen birden fazla form component'i) TEK bir temsili `kod` slaytıyla öğretilir + `sozluk` slaytında geri kalanlar "aynı pattern'i izler" notuyla listelenir. Her tekil dosya için ayrı slayt açılmaz.
- **Zincir bütünlüğü:** Yeni bölüm eklenince `oncekiBolum`/`sonrakiBolum` hem kendi klasöründe hem (varsa) komşu görevin ilk/son dosyasında güncellenir — akademi baştan sona kesintisiz gezilebilir kalmalı. Faz geçişinde (ör. Faz A'nın son bölümünden Faz B'nin ilk bölümüne) `oncekiBolum`/`sonrakiBolum` KLASÖRLER ARASI göreli yolla da bağlanır (ör. `../../admin/B-01_.../01_....html` — `AKADEMI/` altında kardeş klasöre) — iki ayrı akademi klasörü olması, okuyucunun tek bir doğrusal akışta gezinmesini engellemez. Kapanış (`ozet-sozluk`) her zaman klasörün SON numarası olmalı; araya yeni bölüm girince kapanış bir üst numaraya taşınır.
- Klasörün `index.html`'ine yeni bölüm için bir liste satırı eklenir; ilgili akademinin kök `index.html`'ine yeni GÖREV tamamlanınca bir kart eklenir. Mevcut kartlara/satırlara dokunulmaz.
- **Motor değişikliği:** `engine/` klasörleri akademiler arası PAYLAŞILMAZ (her akademi kendi kopyasını taşır — ayrı hızda değişebilirler). Genel bir motor iyileştirmesi (yeni slayt türü DEĞİL, ör. bir render bug fix) yapılırsa, ilgili TÜM akademilerin `engine/` kopyasına uygulanır — tek bir akademide sessizce farklı davranış bırakılmaz.

---

## 7. Bir API/Feature Tamamlandığında

Tüm alt-adımlar `[x]`, ilgili akademiye (§6) işlendi, testler yeşilse:

1. **Git commit** — Türkçe, task no ile başlar (ör. `A-03: AuthController (13 endpoint) + rate limiting`, `B-02: Auth Sayfaları`). API/feature başına, alt-parça başına değil. **Commit mesajına asla `Co-Authored-By: Claude` (veya başka bir AI/asistan) satırı eklenmez** — yazarlık tek kullanıcıya (`Ozaui`) aittir, GitHub'da ek bir "contributor" görünmemeli.
2. **Git push** — her zaman kullanıcı onayıyla (otomatik push yok). Onayı **sormak** akışın parçası.
3. **`TASK.md` güncelle** — faz durumu (⬜→🔄→✅) + "Sıradaki task".
4. **Kullanılmayan paket taraması** — backend'de her `.csproj` paketi, frontend'de her `package.json` bağımlılığı için `grep` ile gerçek çağrı noktası doğrula ("muhtemelen kullanılıyordur" varsayma). Sıfır kullanım → (a) kaldır (YAGNI) veya (b) bilinçli hazırlıksa `Teknik_Ozellikler.md`'ye "⚠️ kurulu, henüz bağlı — hedef: A-0X/B-0X" notu.
