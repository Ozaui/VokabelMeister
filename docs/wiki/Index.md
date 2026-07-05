# VokabelMeister — Wiki İndeksi (Ana Harita)

**Özet:** VokabelMeister, Almanca-Türkçe kelime öğrenme uygulamasının backend'i (.NET 9) ve planlanan üç istemcisini (Web/Mobil/Admin) haritalayan Obsidian bilgi grafiğinin giriş noktasıdır. Proje şu an **Faz A (Admin Panel Backend)**'in erken adımlarında (A-01 ✅, A-02 ✅ tamamlandı; A-03 Auth API 🔄 devam ediyor — `User`/`RefreshToken` entity yazıldı, sırada `IPasswordService`). A-03'ün ardından **A-03.1 (QR Kod ile Giriş)** planlandı, henüz kod yok. Her INGEST sonrası bu dosya güncel tutulur (kural kaynağı: `/wiki_schema.md`).

**Kütüphaneler:** —
**Bağlantılar:** [[Sistem_Mimarisi]] · [[Backend_Katmanli_Mimari]] · [[Gelistirme_Yol_Haritasi]] · [[Veritabani_Semasi]]

---

## 1. Mimari

- [[Sistem_Mimarisi]] — 3 istemci + tek API + MSSQL, genel akış
- [[Backend_Katmanli_Mimari]] — Domain ← Application ← Infrastructure ← API bağımlılık zinciri
- [[Roller_ve_Erisim]] — User/Admin rolleri, içerik sahipliği, görünürlük matrisi
- [[Gelistirme_Yol_Haritasi]] — Faz A→F sırası ve güncel ilerleme durumu

## 2. Backend Projeleri (`backend/`)

- [[WordLearner_API]] — HTTP giriş noktası (Controllers, Program.cs, Swagger)
- [[WordLearner_Application]] — İş mantığı katmanı sözleşmeleri (Interfaces, Exceptions)
- [[WordLearner_Infrastructure]] — Veri erişimi (DbContext, Repository, DI extension)
- [[WordLearner_Domain]] — Entity'ler (şu an yalnızca `BaseEntity`)
- [[WordLearner_Tests]] — xUnit test projesi (henüz test yok)

### Yazılmış Kod Düğümleri (A-01/A-02 — A-02 tamamlandı)
- [[Program_cs]] — composition root (**tam yapılandırma:** Serilog, JWT, CORS, MediatR/AutoMapper/FluentValidation, middleware pipeline)
- [[BaseEntity]] — tüm entity'lerin taban sınıfı
- [[IRepository]] / [[Repository]] — generic CRUD sözleşmesi + EF Core implementasyonu
- [[WordLearnerDbContext]] — merkezi DbContext (soft delete filtresi, `UpdatedAt` otomasyonu)
- [[InfrastructureServiceExtensions]] — DI kayıt extension'ı (Infrastructure)
- [[ApplicationServiceExtensions]] — DI kayıt extension'ı (Application — MediatR/AutoMapper/FluentValidation)
- [[EntityNotFoundException]] — özel exception tipi
- [[ApiErrorResponse]] — hata yanıtı zarfı (`ApiResponse<T>`/`PagedResult<T>` YAGNI kuralıyla ertelendi)
- [[Middleware]] — `ExceptionHandlingMiddleware` / `SecurityHeadersMiddleware` / `RequestResponseLoggingMiddleware`
- [[RepositoryTests]] — `Repository<T>` + soft delete filtresi + `userId` audit alanları için 10 birim test (hepsi yeşil, İngilizce isimlendirme)
- [[API_Yol_Haritasi_Sistemi]] — `docs/API_YOL_HARITASI/` HTML rehber sistemi (junior eğitimi)

### Yazılmış Kod Düğümleri (A-03 — devam ediyor)
- `User`/`RefreshToken` entity + `OtpPurpose` enum, `UserConfiguration`/`RefreshTokenConfiguration`
  (Fluent API), `AddUserAndRefreshToken` migration (VokabelMeisterDB'ye uygulandı) — detay bir kod
  sayfası yerine [[Auth_Domain]]'de (şema açıklaması zaten birebir bu koda karşılık geliyor) ve
  `API_YOL_HARITASI/A-03_auth-api.html`'de (birebir kod + junior açıklaması).
- `IPasswordService`/`PasswordService` ve `ITokenService`/`JwtTokenService` yazıldı — ayrı wiki
  sayfası yok (kod zaten [[Auth_Domain]]'in "Referans Kod" bölümünde özetlenmişti), tam hâli
  `API_YOL_HARITASI/A-03_auth-api.html`'de.
- [[AppException]] + [[ErrorMessages]] — Auth exception'ları (`DuplicateEmailException` vb.) için
  yeni taban sınıf + dil sözlüğü; [[EntityNotFoundException]] ve [[ApiErrorResponse]] güncellendi,
  [[Middleware]] (`ExceptionHandlingMiddleware`) `Accept-Language`'a göre mesaj çözecek şekilde
  değiştirildi. Bu A-03'e özel değil, mimari bir karar — tüm gelecekteki iş kuralı exception'ları
  bundan türeyecek. Sırada `IAuthService`/`AuthService`.

## 3. Veritabanı (planlanan şema — `DATABASE_SCHEMA.md` index + `DATABASE_SCHEMA/` domain dosyaları, henüz migration yok)

- [[Veritabani_Semasi]] — ERD özeti, genel kurallar
- [[Auth_Domain]] — `Users`, `RefreshTokens`
- [[Icerik_Domain]] — `Languages`, `WordConcepts`, `Words`, `WordDetails`, `WordExamples`, `Categories`, `CategoryTranslations`, `WordCategories`
- [[Kisisel_Icerik_Domain]] — `UserCards`, `UserCategories` ve ara tablolar
- [[SRS_Domain]] — `UserProgress`, `UserCardProgress`, `LearningSessions`, `LearningHistory`, `Achievements`
- [[Sosyal_Domain]] — `Classes`, `ClassWords`, `Friendships`, `SharedContents`
- [[Loglama_Domain]] — `ActivityLog`, `ApplicationLog`, `SecurityLog`

## 4. Standartlar ve Kurallar

- [[Kodlama_Standartlari]] — Türkçe yorum kuralı, AMAÇ/NEDEN/NASIL blokları, test standardı
- [[Guvenlik_Politikalari]] — JWT, RBAC, şifreleme, rate limiting
- [[Alman_Dili_Ozellikleri]] — cinsiyet/artikel/hâl/çoğul referansı (`GrammarData` şeması, dil=`de`)
- [[Turkce_Dili_Ozellikleri]] — ünlü uyumu/hâl eki/iyelik referansı (`GrammarData` şeması, dil=`tr`, öncelikli)
- [[Ingilizce_Dili_Ozellikleri]] — çoğul/fiil/karşılaştırma referansı (`GrammarData` şeması, dil=`en`, henüz kullanılmıyor)
- [[Ortam_Degiskenleri]] — ENV değişkenleri listesi
- [[API_Sozlesmesi]] — standart response formatı, endpoint listesi özeti
- [[Teknik_Ozellikler]] — NuGet/npm paket listeleri + JWT/Şifre/SRS/Serilog referans kod örnekleri
- [[Gelistirme_Kurulumu]] — araç kurulumu, `dotnet`/migration komutları, IIS yayınlama

---

## Proje Durumu Özeti

| Faz | Aralık | Başlık | Durum |
|-----|--------|--------|-------|
| A | A-01…A-10 | Admin Panel Backend | 🔄 (A-01 ✅, A-02 ✅, sıradaki A-03) |
| B | B-01…B-09 | Admin Panel (frontend) | ⬜ |
| C | C-01…C-10 | Kullanıcı Backend | ⬜ |
| D | D-01…D-12 | Web App | ⬜ |
| E | E-01…E-14 | Mobil | ⬜ |
| F | F-01…F-04 | Test & Yayın | ⬜ |

Detay → [[Gelistirme_Yol_Haritasi]].

## Kaynak Dokümanlar (`/docs`)
Bu wiki, `docs/` altındaki **tüm** insan-yazımı dokümanların taranmasıyla üretildi. `docs/` artık
şu klasör yapısındadır (token tasarrufu için bölündü, içerik kaybı yok):
- `docs/00_INDEX.md`, `docs/index.html`, `docs/CONNECTION_STRING.txt` — giriş noktaları (kökte)
- `docs/REFERENCE/` — ARCHITECTURE, API_ENDPOINTS, CODING_STANDARDS, DEVELOPMENT_SETUP, ENV,
  GERMAN_LANGUAGE_FEATURES, TURKISH_LANGUAGE_FEATURES, ENGLISH_LANGUAGE_FEATURES (henüz kullanılmıyor),
  SECURITY, TECHNICAL_SPECIFICATIONS
- `docs/TASK.md` (yöntem/standart + ilerleme) + `docs/TASK/` (faz başına 1 dosya: A_admin_panel_backend,
  B_admin_panel, C_kullanici_backend, D_web_app, E_mobil, F_test_yayin)
- `docs/DATABASE_SCHEMA.md` (index: ERD/seed/genel kurallar) + `docs/DATABASE_SCHEMA/` (domain başına
  1 dosya: Auth, Icerik, Kisisel_Icerik, SRS, Sosyal, Loglama, Sistem)
- `docs/API_YOL_HARITASI/*` (backend) + `docs/FRONTEND_YOL_HARITASI/*` (Web/Admin/Mobil — aynı
  sistemin frontend kardeşi, henüz hiçbir feature sayfası yazılmadı)

Ayrıca gerçek kaynak kodun (`backend/`, `.csproj`'lar, `.sln`, `.gitignore`, `launchSettings.json`)
taranmasıyla üretildi. Çelişki durumunda `docs/` klasörü otorite kaynağıdır; bu wiki onun bağlantılı
bir haritasıdır.

**Bilinçli olarak wiki içeriğine taşınmadı (okundu ama hassas/düşük değerli):**
- `docs/CONNECTION_STRING.txt` — gerçek DB şifresi içeriyor, **asla** wiki'ye kopyalanmaz
- `backend/WordLearner.API/Properties/launchSettings.json` — dev portları (5001/7001) zaten
  [[WordLearner_API]]'de var; içindeki `AES_ENCRYPTION_KEY` örnek değeri hassas olduğu için atlandı
- `.claude/settings.local.json` — Claude Code araç izinleri, proje mimarisiyle ilgisiz
- `docs/API_YOL_HARITASI/render.js`/`style.css` — sunum kodu, davranışı [[API_Yol_Haritasi_Sistemi]]'nde özetlendi, kod tekrarı gerekmiyor

**Proje dizini şu an %100 taranmış durumda** — okunmamış dosya yok.

*Son INGEST: 2026-07-02 — kapsam: [[BaseEntity]]'ye "kim yaptı" audit alanları eklendi
(`CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId`, `int?`), [[IRepository]]/[[Repository]]
`AddAsync`/`UpdateAsync`/`SoftDeleteAsync`'e opsiyonel `userId` parametresi eklendi,
[[RepositoryTests]] 7→9 teste çıktı; ayrıca [[Kodlama_Standartlari]] §7.2 test metodu adlandırma
kuralı Türkçeden İngilizceye çevrildi (yapı `{Metot}_{Senaryo}_{BeklenenSonuç}` sabit kaldı) ve
mevcut 9 test bu kurala göre yeniden adlandırıldı. `API_YOL_HARITASI/A-02_ortak-altyapi.html`
1/4/5/7. adımlar buna göre güncellendi.*

*İkinci INGEST (aynı gün): `docs/` token tüketimini azaltmak için yeniden klasörlendi — hiçbir
içerik silinmedi, sadece dosyalar/bölümler taşındı. `docs/TASK.md` artık yalnızca yöntem/standart +
ilerleme tablosu; task checklist'leri `docs/TASK/` altında faz başına 1 dosyaya bölündü.
`docs/DATABASE_SCHEMA.md` artık yalnızca ERD/seed/genel kurallar; tam `CREATE TABLE` SQL'leri
`docs/DATABASE_SCHEMA/` altında domain başına 1 dosyaya bölündü. Kökte duran 8 dosya
(ARCHITECTURE, API_ENDPOINTS, CODING_STANDARDS, DEVELOPMENT_SETUP, ENV, GERMAN_LANGUAGE_FEATURES,
SECURITY, TECHNICAL_SPECIFICATIONS) `docs/REFERENCE/` klasörüne taşındı; `00_INDEX.md`/`index.html`/
`CONNECTION_STRING.txt` giriş noktası oldukları için kökte kaldı. Bu wiki'deki tüm `docs/X.md` yol
referansları yeni konumlara göre güncellendi (bu düğüm dahil).*

*Üçüncü INGEST (aynı gün): Frontend için `docs/API_YOL_HARITASI/` ile birebir aynı mantıkta yeni bir
sistem eklendi — `docs/FRONTEND_YOL_HARITASI/` (kendi hub'ı, `_TASLAK.html`'i, bağımsız `render.js`
kopyası; stil `API_YOL_HARITASI/style.css`'i paylaşır, yeni `t-tip/t-api/t-slice/t-hook/t-component/
t-route/t-style` ve `u-web/u-admin/u-mobile` CSS sınıfları o dosyaya eklendi). `docs/TASK.md`'ye
backend'deki **⭐ Çalışma Yöntemi**'nin frontend karşılığı olan **⭐ Frontend Çalışma Yöntemi**
eklendi (adım sırası: tip→api→slice→hook→component→route→test). `docs/TASK/B_admin_panel.md`,
`D_web_app.md`, `E_mobil.md` — önceden tek satırlık başlıklardan ibaretti; artık backend'deki
A/C fazlarıyla aynı granülaritede, her feature için alt-adım checklist'i + roadmap işaretçileriyle
detaylandırıldı. [[API_Yol_Haritasi_Sistemi]] bu yeni sistemi "Frontend Kardeşi" bölümünde anlatıyor.*

*Dördüncü INGEST (aynı gün): İki yol haritası arasında **iki yönlü çapraz link kuralı** eklendi —
bir API'yi bir frontend feature tüketiyorsa, backend sayfası (`frontendRefs`) "buradan sonrası
frontend tarafında", frontend sayfasının `api` adımı (`backendRef`) "buradan sonrası backend
tarafında" bandı gösterip birbirine link verir (`render.js` iki motorda da güncellendi, `.note.xref`
stili `API_YOL_HARITASI/style.css`'e eklendi, her iki `_TASLAK.html` örnek alanla güncellendi).
`docs/TASK.md`'nin her iki ⭐ bölümüne kural yazıldı; `docs/TASK/A_admin_panel_backend.md` ve
`C_kullanici_backend.md`'deki her API task'ının altına **"Frontend karşılığı:"** notu eklenerek
gelecekteki eşleştirmeler (örn. A-03 ↔ B-02/D-03/E-05, C-04 ↔ D-07/E-09) önceden belgelendi.
[[API_Yol_Haritasi_Sistemi]]'ne "Çapraz Link Kuralı" bölümü eklendi.*

*Beşinci INGEST (2026-07-02): [[BaseEntity]].`UpdatedAt` non-nullable `DateTime` (varsayılan
`DateTime.UtcNow`) iken insert anında `CreatedAt`'e eşit bir değer alıyordu — "hiç güncellenmedi"
durumu ayırt edilemiyordu. `UpdatedAt` → `DateTime?` (varsayılansız) yapıldı;
[[WordLearnerDbContext]].`SaveChangesAsync` zaten yalnızca `EntityState.Modified`'ta set ettiği için
davranış değişmedi, sadece insert sonrası artık `null` kalıyor. [[RepositoryTests]]'e
`AddAsync_ValidEntity_LeavesUpdatedAtNull` testi eklendi (9→10 test). `docs/API_YOL_HARITASI/
A-02_ortak-altyapi.html`'deki BaseEntity/WordLearnerDbContext/RepositoryTests adımları (kod +
açıklama) buna göre güncellendi.*

*Altıncı INGEST (2026-07-03): [[EntityNotFoundException]]'a `Type`+key alan bir overload eklendi
(`"{Entity} bulunamadı: Id={key}"` formatını otomatik üretir) — [[Repository]].`SoftDeleteAsync`
artık elle string interpolasyonu yerine bu overload'ı çağırıyor, format tek yerden yönetiliyor.
Yeni `EntityNotFoundExceptionTests` sınıfı (`WordLearner.Tests/Common/Exceptions/`) bu formatı
doğruluyor (10→11 test, bkz. [[WordLearner_Tests]]). `docs/API_YOL_HARITASI/A-02_ortak-altyapi.html`'e
8. adım (`tur:'test'`) olarak işlendi; ayrıca [[WordLearner_Tests]] ve [[Repository]] wiki
sayfalarındaki test sayıları (7/9 gibi eski değerler) güncel duruma (11/10) çekildi.*

*Yedinci INGEST (2026-07-03): **A-02 (Ortak Altyapı) tamamlandı.** Kalan 3 checklist maddesi
yazıldı: (1) `ApiResponse<T>`/`ApiErrorResponse`/`PagedResult<T>` (`Application/Common/Models/`),
REFERENCE/API_ENDPOINTS.md §1'deki "Standart Yanıt" sözleşmesinin kod karşılığı — **NOT: `ApiResponse<T>`
ve `PagedResult<T>` sonradan (Sekizinci INGEST'te) YAGNI kuralıyla geri alındı, yalnızca
[[ApiErrorResponse]] kaldı.** (2) [[Middleware]] — `ExceptionHandlingMiddleware` (exception → `ApiErrorResponse`
JSON, `EntityNotFoundException`→404 diğerleri→500), `SecurityHeadersMiddleware` (5 güvenlik başlığı,
REFERENCE/SECURITY.md §5), `RequestResponseLoggingMiddleware` (Stopwatch + try/finally ile istek/yanıt
logu) — `API/Middleware/` altında. (3) [[Program_cs]] tam yapılandırıldı: Serilog (konsol+dosya;
DB/`ApplicationLog` sink'i A-04'e bırakıldı çünkü tablo migration'ı henüz yok), yeni
[[ApplicationServiceExtensions]] (`AddApplicationServices()` — MediatR/AutoMapper/FluentValidation,
kendi assembly'sinden reflection taraması), JWT Bearer authentication, CORS, ve middleware pipeline
sıralaması (loglama en dışta → güvenlik başlıkları → exception handling en içte). Bu iş için
`WordLearner.Application.csproj`'a 4 NuGet paketi eklendi: `MediatR` 12.1.1, `AutoMapper` 13.0.1,
`FluentValidation`+`FluentValidation.DependencyInjectionExtensions` 11.9.2 (AutoMapper 13.0.1'in
NU1903 güvenlik uyarısı var — REFERENCE/TECHNICAL_SPECIFICATIONS.md §1'de pinlenen sürüm olduğu için
bilinçli olarak korundu). Gerçek bir çalıştırmayla doğrulandı: uygulama ayağa kalkıyor, güvenlik
başlıkları her yanıtta mevcut, Türkçe istek/yanıt logları konsola düşüyor, mevcut 11 test hâlâ yeşil.
`docs/API_YOL_HARITASI/A-02_ortak-altyapi.html`'e 9/10/11. adımlar eklendi (A-02 artık 11 adımlı,
sayfa `durum: 'done'`). `docs/TASK/A_admin_panel_backend.md`'de A-02 ✅ işaretlendi,
`docs/TASK.md`'de sıradaki task A-03 olarak güncellendi. [[WordLearner_API]] ve
[[WordLearner_Application]] sayfaları yeni dosya/klasörleri yansıtacak şekilde güncellendi.*

*Sekizinci INGEST (2026-07-03) — YAGNI düzeltmesi: Kullanıcı, A-02'de `ApiResponse<T>` ve
`PagedResult<T>`'ın **hiçbir controller/endpoint yokken spekülatif olarak** yazıldığını fark etti
("bir yazılımcı AI olmadan bu sıraya göre mi yazardı?" sorusu) — REFERENCE/API_ENDPOINTS.md'deki
somut örnekler (`GET /words`, `POST /auth/register`) zaten bu zarfı kullanmıyor, yani tahmin edilen
şekil dokümanla bile örtüşmüyordu. Karar: `docs/TASK.md`'ye **"Spekülatif ortak tip yazılmaz (YAGNI)"**
kuralı eklendi — bir ortak tip yalnızca gerçek bir tüketicisi (o an yazılmakta olan başka bir kod
parçası) varken yazılır; istisna, o tipin zaten kanıtlanmış bir tüketicisi varsa (`ApiErrorResponse`'un
`ExceptionHandlingMiddleware` tarafından kullanılması gibi). Uygulama: `ApiResponse.cs` ve
`PagedResult.cs` silindi (`ApiErrorResponse.cs` kaldı); `docs/API_YOL_HARITASI/A-02_ortak-altyapi.html`
adım 9 yalnızca `ApiErrorResponse`'u içerecek şekilde daraltıldı (11 adım sayısı değişmedi, içerik
daraldı); `docs/TASK/A_admin_panel_backend.md`'deki A-02 "Ortak tipler" maddesi bu kararı açıklayan
bir nota dönüştürüldü. Wiki: [[ApiResponseModels]] silindi, yerine yalnızca [[ApiErrorResponse]]
düğümü kondu (içinde "YAGNI Düzeltmesi" başlıklı bir bölümle *neden* `ApiResponse<T>`/`PagedResult<T>`
burada olmadığı açıklanıyor — gelecekte biri bunları "zaten yazılmıştı" sanıp yanlışlıkla tekrar
eklemesin diye). [[Middleware]], [[Program_cs]], [[WordLearner_Application]] sayfalarındaki
`[[ApiResponseModels]]` linkleri `[[ApiErrorResponse]]`'a güncellendi. Build + 11 test tekrar
doğrulandı, hiçbir regresyon yok (bu iki tipin hiçbir gerçek tüketicisi olmadığı için silinmeleri
derlemeyi bozmadı — spekülatif olduklarının kanıtı).*

*Dokuzuncu INGEST (2026-07-05) — Çoklu dil altyapısı (WordConcept redesign): Kullanıcı ileride
İngilizce eklemek istediğini belirtti (DE-EN/EN-DE/TR-EN/EN-TR — yön fark etmeksizin); henüz A-05/A-06
kodlanmadığı için `docs/DATABASE_SCHEMA/Icerik.md` şeması koda geçmeden yeniden tasarlandı. **Seçilen
model: WordConcept** — dilden bağımsız bir kavram (`WordConcepts`: `PartOfSpeech`/`DifficultyLevel`/
`ImageUrl`) + her dildeki karşılığı için ayrı bir `Words` satırı (`WordConceptId`+`LanguageId`+`Text`),
yeni `Languages` tablosu (seed: yalnızca `de`+`tr`). Bir kelime tüm dilleriyle (şu an DE+TR) **aynı
işlemde** girilir/düzenlenir, sıralı değil. `WordDetails`'teki Almanca'ya özgü gramer alanları
(`Gender`, artikeller, `ConjugationData` vb.) tek bir `GrammarData` JSON kolonuna taşındı (şekli
`LanguageId`'ye göre değişir — trade-off: `Gender` üzerindeki DB-level `CHECK`/`INDEX` kayboldu).
`WordExamples` basitleştirildi (`SentenceDE`+`SentenceTR` yerine tek `SentenceText`). `Categories` aynı
mantıkla `Categories` (çekirdek) + yeni `CategoryTranslations` (dil başına ad) olarak ayrıldı;
`WordCategories` artık `WordId` yerine `WordConceptId`↔`CategoryId` (kategori kavram üzerinden bir kez
etiketlenir, tüm diller otomatik kapsanır). Bilinçli olarak kapsam dışı bırakıldı: `UserCards`
(kişisel kartlar) ve `ClassWords` (sınıf ad-hoc kelimeleri) — bunlar sistem sözlüğü değil, serbest
metin olarak kalıyor. Etkilenen dosyalar: `docs/DATABASE_SCHEMA.md` (domain haritası/ERD/seed/Kural 4),
`docs/REFERENCE/API_ENDPOINTS.md` §5-6 (`translations[]` şekli), `docs/REFERENCE/
GERMAN_LANGUAGE_FEATURES.md` (artık yalnızca Almanca'nın `GrammarData` şemasını tanımladığı netleşti),
`docs/REFERENCE/ARCHITECTURE.md` (`GermanWord`→`Words.Text`), `docs/00_INDEX.md` §4 ("yalnızca
Almanca-Türkçe" kuralı "şema çoklu dile açık, şu an DE-TR içerik" olarak güncellendi),
`docs/TASK/A_admin_panel_backend.md` (A-05/A-06 entity adımları `Language`/`WordConcept`/
`CategoryTranslation` içerecek şekilde güncellendi). Kontrol edildi: mevcut backend kodunda
(`backend/`) `Word`/`Category` ile ilgili hiçbir entity/alan yoktu, yani bu redesign hiçbir gerçek kodu
etkilemedi — yalnızca A-05/A-06 başlamadan önceki tasarım dokümanı düzeltildi. [[Icerik_Domain]] ve
[[Alman_Dili_Ozellikleri]] bu yeni modele göre güncellendi.*

*Onuncu INGEST (2026-07-05) — Türkçe ve İngilizce dil özellikleri referansları eklendi: Kullanıcı
`GrammarData`'nın yalnızca Almanca için tanımlı olmasının, "bir Almanın Türkçe öğrenmesi" senaryosunda
eksik kalacağını fark etti (önceki tasarımda Türkçe `GrammarData=NULL` varsayılmıştı — bu, Türkçe'nin
her zaman "bilinen dil/çeviri" olacağı, hiç "öğrenilecek hedef" olmayacağı varsayımına dayanıyordu, ki
yön-bağımsız eşleştirme hedefiyle çelişiyordu). Yeni dosyalar: `docs/REFERENCE/
TURKISH_LANGUAGE_FEATURES.md` (öncelikli, aktif içerik — ünlü uyumu, 6 hâl eki, çoğul, iyelik ekleri,
ünsüz yumuşaması, fiil çekimi) ve `docs/REFERENCE/ENGLISH_LANGUAGE_FEATURES.md` (şema hazır, henüz
kullanılmıyor — `Languages`'de `en` satırı yok). `docs/DATABASE_SCHEMA/Icerik.md`'deki `GrammarData`
notu güncellendi (artık üç dilin de kaynağına işaret ediyor). Wiki: yeni düğümler
[[Turkce_Dili_Ozellikleri]] ve [[Ingilizce_Dili_Ozellikleri]] eklendi, [[Icerik_Domain]] ve
[[Alman_Dili_Ozellikleri]]'ndeki çapraz linkler güncellendi. Mimari değişiklik yok — `GrammarData`
zaten `Words.LanguageId`'ye göre şekil değiştiren bir JSON alanıydı (dokuzuncu INGEST'te kuruldu), bu
yalnızca eksik olan dil içeriğini (Türkçe/İngilizce şeması) tamamladı.*

*On birinci INGEST (2026-07-05) — A-03 başladı (`User`/`RefreshToken` entity + migration) + iki yeni
tasarım kararı: **(1) Apple platformlar arası tutarlılık:** Kullanıcı "mobilde Apple ile kayıt olan
biri web'de nasıl giriş yapar?" sorusunu sordu — Apple'ın kullanıcı kimliğini (`sub`) client bazında
(Bundle ID/Services ID) ürettiği, gruplanmazsa aynı kişi için iki hesap açılacağı ortaya çıktı.
Çözüm bir Apple Developer Console ayarı (web Services ID'yi mobil Bundle ID'ye Primary App ID olarak
gruplama) — kod tarafında hiçbir karşılığı yok, `REFERENCE/ENV.md §4`'e ileriye dönük not eklendi.
Bugünkü pratik cevap: (a) `forgot-password` akışı `PasswordHash`'in var olup olmadığına bakmaz, sosyal
girişli hesaplar da şifre belirleyip web'de girebilir (kasıtlı davranış, bozulmamalı) — (b) aşağıdaki
QR girişi. **(2) QR Kod ile Giriş (Steam benzeri) eklendi — yeni task A-03.1:** Ayrı bir kimlik
doğrulama sistemi değil, A-03'teki `ITokenService`'e bağlanan yeni bir "kimliği kanıtlama yöntemi".
Yeni tablo `QrLoginSessions` (`QrTokenHash` SHA-256 + `PairingCode` 4 haneli — ikinci bir savunma
katmanı, DB sızıntısından bağımsız relay/phishing önlemi; `Status`: Pending→Scanned→Confirmed→Consumed
veya Denied/Expired) `DATABASE_SCHEMA/Auth.md`'ye eklendi. 5 yeni endpoint (`REFERENCE/API_ENDPOINTS.md
§3.1`), güvenlik akışı (`REFERENCE/SECURITY.md §1.2-1.3`), `SecurityLog` yeni event tipleri
(`QrLoginConfirmed`/`QrLoginDenied`, `DATABASE_SCHEMA/Loglama.md`), npm paketleri (`qrcode.react` web,
`expo-camera` mobil, `REFERENCE/TECHNICAL_SPECIFICATIONS.md`), `docs/TASK/A_admin_panel_backend.md`
(yeni A-03.1 bölümü), `docs/TASK/D_web_app.md` (D-03.1), `docs/TASK/E_mobil.md` (E-05.1). Henüz hiç
kod yazılmadı (yalnızca tasarım/dokümantasyon) — mevcut `User`/`RefreshToken`/migration kodunda hiçbir
değişiklik gerekmedi, ikisi de tamamen ek/yeni. [[Auth_Domain]], [[Guvenlik_Politikalari]],
[[Sistem_Mimarisi]] güncellendi.*
