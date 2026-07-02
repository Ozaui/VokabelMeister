# VokabelMeister — Wiki İndeksi (Ana Harita)

**Özet:** VokabelMeister, Almanca-Türkçe kelime öğrenme uygulamasının backend'i (.NET 9) ve planlanan üç istemcisini (Web/Mobil/Admin) haritalayan Obsidian bilgi grafiğinin giriş noktasıdır. Proje şu an **Faz A (Admin Panel Backend)**'in erken adımlarında (A-01 tamamlandı, A-02 devam ediyor); henüz hiçbir Controller veya feature entity yazılmadı. Her INGEST sonrası bu dosya güncel tutulur (kural kaynağı: `/wiki_schema.md`).

**Kütüphaneler:** —
**Bağlantılar:** [[Sistem_Mimarisi]] · [[Backend_Katmanli_Mimari]] · [[Gelistirme_Yol_Haritasi]] · [[Veritabani_Semasi]]

---

## 1. Mimari

- [[Sistem_Mimarisi]] — 3 istemci + tek API + MSSQL, genel akış
- [[Backend_Katmanli_Mimari]] — Domain ← Infrastructure ← Application ← API bağımlılık zinciri
- [[Roller_ve_Erisim]] — User/Admin rolleri, içerik sahipliği, görünürlük matrisi
- [[Gelistirme_Yol_Haritasi]] — Faz A→F sırası ve güncel ilerleme durumu

## 2. Backend Projeleri (`backend/`)

- [[WordLearner_API]] — HTTP giriş noktası (Controllers, Program.cs, Swagger)
- [[WordLearner_Application]] — İş mantığı katmanı sözleşmeleri (Interfaces, Exceptions)
- [[WordLearner_Infrastructure]] — Veri erişimi (DbContext, Repository, DI extension)
- [[WordLearner_Domain]] — Entity'ler (şu an yalnızca `BaseEntity`)
- [[WordLearner_Tests]] — xUnit test projesi (henüz test yok)

### Yazılmış Kod Düğümleri (A-01/A-02)
- [[Program_cs]] — composition root (mevcut iskelet + hedef tam yapılandırma)
- [[BaseEntity]] — tüm entity'lerin taban sınıfı
- [[IRepository]] / [[Repository]] — generic CRUD sözleşmesi + EF Core implementasyonu
- [[WordLearnerDbContext]] — merkezi DbContext (soft delete filtresi, `UpdatedAt` otomasyonu)
- [[InfrastructureServiceExtensions]] — DI kayıt extension'ı
- [[EntityNotFoundException]] — özel exception tipi
- [[RepositoryTests]] — `Repository<T>` + soft delete filtresi + `userId` audit alanları için 9 birim test (hepsi yeşil, İngilizce isimlendirme)
- [[API_Yol_Haritasi_Sistemi]] — `docs/API_YOL_HARITASI/` HTML rehber sistemi (junior eğitimi)

## 3. Veritabanı (planlanan şema — `DATABASE_SCHEMA.md` index + `DATABASE_SCHEMA/` domain dosyaları, henüz migration yok)

- [[Veritabani_Semasi]] — ERD özeti, genel kurallar
- [[Auth_Domain]] — `Users`, `RefreshTokens`
- [[Icerik_Domain]] — `Words`, `WordDetails`, `WordExamples`, `Categories`, `WordCategories`
- [[Kisisel_Icerik_Domain]] — `UserCards`, `UserCategories` ve ara tablolar
- [[SRS_Domain]] — `UserProgress`, `UserCardProgress`, `LearningSessions`, `LearningHistory`, `Achievements`
- [[Sosyal_Domain]] — `Classes`, `ClassWords`, `Friendships`, `SharedContents`
- [[Loglama_Domain]] — `ActivityLog`, `ApplicationLog`, `SecurityLog`

## 4. Standartlar ve Kurallar

- [[Kodlama_Standartlari]] — Türkçe yorum kuralı, AMAÇ/NEDEN/NASIL blokları, test standardı
- [[Guvenlik_Politikalari]] — JWT, RBAC, şifreleme, rate limiting
- [[Alman_Dili_Ozellikleri]] — cinsiyet/artikel/hâl/çoğul referansı (`WordDetail` alanlarının kaynağı)
- [[Ortam_Degiskenleri]] — ENV değişkenleri listesi
- [[API_Sozlesmesi]] — standart response formatı, endpoint listesi özeti
- [[Teknik_Ozellikler]] — NuGet/npm paket listeleri + JWT/Şifre/SRS/Serilog referans kod örnekleri
- [[Gelistirme_Kurulumu]] — araç kurulumu, `dotnet`/migration komutları, IIS yayınlama

---

## Proje Durumu Özeti

| Faz | Aralık | Başlık | Durum |
|-----|--------|--------|-------|
| A | A-01…A-10 | Admin Panel Backend | 🔄 (A-01 ✅, A-02 🔄) |
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
  GERMAN_LANGUAGE_FEATURES, SECURITY, TECHNICAL_SPECIFICATIONS
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
