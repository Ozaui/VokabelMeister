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
- **Türkçe:** kod yorumları (AMAÇ/NEDEN/NASIL), XML doc, MD dosyaları, roadmap.
- **İngilizce:** method/class/property/DB kolon/JS değişken adları, test metodu adları, `_logger.Log*` mesajları, exception `.Message`, hata `Code` sabitleri (ör. `INVALID_CREDENTIALS`).
- **İstisna — istemciye giden mesaj:** `AppException.Code`/FluentValidation `ErrorCode`, isteğin `Accept-Language`'ına göre `ErrorMessages` sözlüğünden (tr/en) çözülür. Kullanıcı seçtiği dili görür; DB/log/geliştirici İngilizce görür. Ayrı iki kanal.

**Yorum blokları (zorunlu)**
- Her dosya başı: `AMAÇ / NEDEN / BAĞIMLILIKLAR`.
- Her public metot: `AMAÇ / NEDEN / NASIL` (+ param/return).
- Karmaşık bloklar: `// ADIM N:` + `// NEDEN:`.

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

**Kimlik & güvenlik**
- ASP.NET Identity **KULLANILMAZ** — JWT + şifre hashleme manuel.
- Access 15dk, Refresh 7gün (her refresh'te rotation, Token Family Pattern). Şifre: bcrypt wf=12.
- Hassas değer (`SecretKey`, bağlantı dizesi, AES anahtarı) **asla** `appsettings.json`/kaynak koda girmez → ENV (`REFERENCE/ENV.md`). Yeni servis eklenince ENV.md güncelle.
- Loglarda ham e-posta yok → `SHA-256(email)`. Şifre/token asla loglanmaz.

**Test**
- Her public servis/Handler metodunun birim testi **aynı task içinde** yazılır (Faz F'ye bırakma). Standart → `REFERENCE/CODING_STANDARDS.md §7`.

**Wiki (mimari hafıza)**
- Yeni plan/özellik istenince kodu taramadan önce `wiki/Index.md`'den başla (**QUERY**). Önemli bir şey değiştiyse ilgili wiki düğümünü + `wiki/Index.md`'yi güncelle (**INGEST**). Kurallar → `wiki_schema.md`.

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
15. Yol Haritası       → API_YOL_HARITASI/ HTML sayfası (bkz. §6)
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
2. RTK Query Endpoint → store/api/xxxApi.ts     (backend'e istek — cross-link noktası)
3. Redux Slice        → store/slices/xxxSlice.ts (yalnızca ek local/UI state gerekirse)
4. Custom Hook        → hooks/useXxx.ts          (varsa)
5. Component          → components/Xxx.tsx
6. Route/Import       → App.tsx / (mobil) navigation/*Navigator.tsx
7. Birim Test         → Xxx.test.tsx             (RTL; hemen)
8. Yol Haritası        → ilgili roadmap: B→ADMIN_YOL_HARITASI/, D→WEB_YOL_HARITASI/, E→MOBILE_YOL_HARITASI/
```

Admin farkı: Google/Apple yok, endpoint'ler `/admin/*`. Mobil farkı: adım 6 React Navigation; state katmanı web'le paylaşılabilir.

---

## 5. Klasör / Namespace

- `Domain ← Application ← Infrastructure ← API` (bağımlılık yönü).
- `BaseEntity` hariç her entity/enum bir **domain alt klasöründe**; namespace klasörle eşleşir (`...Entities.<Domain>`).
- Command+Handler (+Profile) **aynı dosyada**; ayrı Commands/Handlers klasörüne bölünmez.
- `Application/Services` ve `Application/Interfaces/Services` **flat** (feature alt klasörü açma).

---

## 6. Yol Haritası (roadmap) Kuralı — her parça yazılınca HEMEN

Toplu yazma **yasak**. Her kod parçasını yazar yazmaz: (1) ilgili `TASK/` maddesini `[ ]→[x]`, (2) parçayı API/feature'ın HTML sayfasına ekle.

- **Her adımda `aciklama`** — o parçanın *neden* yazıldığını junior'a anlatır.
- **Birebir kopya:** kod blokları gerçek dosyanın aynısı, kırpılmaz (`...` yok). Enum ayrı adım.
- **Yeniden kullanılan kod tam hâliyle tekrar yazılır** — her sayfa baştan sona okunabilir olmalı. Gerçekten değişen satırlar `##NEW##`/`##OLD##` marker'ıyla işaretlenir (git diff gibi render edilir).
- **Paketler:** bu API'nin ilk kez eklettiği her NuGet/npm paketi `paketler:[]` dizisine (daha önce eklenen tekrar yazılmaz).
- **Test alanı:** ayrı bölüm; test sınıfı birebir kopya + her metoda 3 satır (Test Adı / Ne Test Edildi / Neden Önemli) — `CODING_STANDARDS.md §7.6`.
- **Çapraz link (iki yönlü):** `tur:'api'` adımı `backendRef`, API sayfası `frontendRefs` alır. Karşı taraf yazılınca geri dönüp iki tarafa da eklenir.
- Detaylı marker/JSON şeması → ilgili `_TASLAK.html` yorum bloğu.

---

## 7. Bir API/Feature Tamamlandığında

Tüm alt-adımlar `[x]`, roadmap eksiksiz, testler yeşilse:

1. **Git commit** — Türkçe, task no ile başlar (ör. `A-03: AuthController (13 endpoint) + rate limiting`). API/feature başına, alt-parça başına değil.
2. **Git push** — her zaman kullanıcı onayıyla (otomatik push yok). Onayı **sormak** akışın parçası.
3. **`TASK.md` güncelle** — faz durumu (⬜→🔄→✅) + "Sıradaki task".
4. **`wiki/Index.md`'ye INGEST** (varsa).
5. **Kullanılmayan paket taraması** — her `.csproj` paketi için `grep` ile gerçek çağrı noktası doğrula ("muhtemelen kullanılıyordur" varsayma). Sıfır kullanım → (a) kaldır (YAGNI) veya (b) bilinçli hazırlıksa `Teknik_Ozellikler.md`'ye "⚠️ kurulu, henüz bağlı — hedef: A-0X" notu.
