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
- [[API_Yol_Haritasi_Sistemi]] — `docs/API_YOL_HARITASI/` HTML rehber sistemi (junior eğitimi)

## 3. Veritabanı (planlanan şema — `DATABASE_SCHEMA.md`, henüz migration yok)

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
Bu wiki, `docs/` altındaki **tüm** insan-yazımı dokümanların (00_INDEX, ARCHITECTURE, DATABASE_SCHEMA,
TASK, SECURITY, CODING_STANDARDS, ENV, GERMAN_LANGUAGE_FEATURES, API_ENDPOINTS,
TECHNICAL_SPECIFICATIONS, DEVELOPMENT_SETUP, `index.html` + `API_YOL_HARITASI/*`) ve gerçek kaynak
kodun (`backend/`, `.csproj`'lar, `.sln`, `.gitignore`, `launchSettings.json`) taranmasıyla üretildi.
Çelişki durumunda `docs/` klasörü otorite kaynağıdır; bu wiki onun bağlantılı bir haritasıdır.

**Bilinçli olarak wiki içeriğine taşınmadı (okundu ama hassas/düşük değerli):**
- `docs/CONNECTION_STRING.txt` — gerçek DB şifresi içeriyor, **asla** wiki'ye kopyalanmaz
- `backend/WordLearner.API/Properties/launchSettings.json` — dev portları (5001/7001) zaten
  [[WordLearner_API]]'de var; içindeki `AES_ENCRYPTION_KEY` örnek değeri hassas olduğu için atlandı
- `.claude/settings.local.json` — Claude Code araç izinleri, proje mimarisiyle ilgisiz
- `docs/API_YOL_HARITASI/render.js`/`style.css` — sunum kodu, davranışı [[API_Yol_Haritasi_Sistemi]]'nde özetlendi, kod tekrarı gerekmiyor

**Proje dizini şu an %100 taranmış durumda** — okunmamış dosya yok.

*Son INGEST: 2026-07-02 — kapsam: tüm proje (ilk tarama + tamamlama turu: TECHNICAL_SPECIFICATIONS, DEVELOPMENT_SETUP, API_YOL_HARITASI, .sln/.gitignore/launchSettings/CONNECTION_STRING dahil son dosyalar).*
