## Teknoloji

| Katman | Stack |
|--------|-------|
| **Backend** | .NET 9 Web API + EF Core 9 + MS SQL Server |
| **Admin Panel** | React + Vite + TypeScript (`/admin`) — yalnızca Admin |
| **Web App** | React + Vite + TypeScript + React Router v6 (`/web`) |
| **Mobil** | React Native (Expo) + TypeScript (`/mobile`) |

**Kimlik doğrulama:** ASP.NET Identity KULLANILMIYOR. JWT + şifre hashleme manuel yazılır.

## Geliştirme Sırası (Faz)

```
A) Admin Panel Backend  →  B) Admin Panel  →  C) Kullanıcı Backend
   →  D) Web App  →  E) Mobil  →  F) Test & Yayın
```

**Neden bu sıra?** Admin backend + panel önce → gerçek kelime/kategori içeriği girilir →
kullanıcı backend bu içerikle test edilir → web (hızlı test döngüsü) → mobil (web referansı hazır).

## İki Temel Çalışma Kuralı

### 1. Bir API'ı Baştan Sona Bitir (Dikey Dilim)

Bir endpoint'i tüm katmanlarıyla **tek seferde** bitirip diğerine geç. Katman katman (önce tüm
entity'ler, sonra tüm DTO'lar) DEĞİL; API API ilerle:

```
Entity → EF Config → DTO → Validator → Repository → Servis → Controller → ✅ sonraki API
```

### 2. Yazarken API Yol Haritasını Doldur

Her kod parçasını yazar yazmaz **anında** `API_YOL_HARITASI/` rehberine işle (her API'ın kendi HTML
sayfası vardır; yeni API = `_TASLAK.html`'i kopyala + `index.html` LISTE'ye satır ekle). Entity yazdın → hemen
haritaya. DTO yazdın → hemen haritaya. Yol haritası, bir API'ın nasıl yazıldığını adım adım
gösteren ve junior geliştirici eğiten rehberdir.

## Dosyalar

| Dosya | İçerik |
|-------|--------|
| `TASK.md` | Yöntem/standart (⭐ Çalışma Yöntemi) + ilerleme tablosu — task listeleri `TASK/` klasöründe (faz başına 1 dosya) |
| `API_YOL_HARITASI/` | Junior eğitim rehberi (interaktif HTML) — her API'ın adım adım yazılışı. Hub sayfası `API_YOL_HARITASI/index.html`, `_TASLAK.html` = yeni API şablonu. `docs/index.html` — Backend/Frontend'e yönlendiren ana karşılama sayfası |
| `FRONTEND_YOL_HARITASI/` | API_YOL_HARITASI'nın frontend karşılığı — Web/Admin/Mobil feature'larının (tip→api→slice→hook→component→route→test) adım adım yazılışı. Kendi hub'ı (`index.html`), `_TASLAK.html` = yeni feature şablonu, stil `API_YOL_HARITASI/style.css`'i paylaşır |
| `REFERENCE/ARCHITECTURE.md` | Sistem mimarisi, akışlar, entity ilişkileri, roller |
| `DATABASE_SCHEMA.md` | ERD + seed data + genel kurallar — tam `CREATE TABLE` SQL'leri `DATABASE_SCHEMA/` klasöründe (domain başına 1 dosya) |
| `REFERENCE/API_ENDPOINTS.md` | Endpoint listesi, request/response örnekleri |
| `REFERENCE/TECHNICAL_SPECIFICATIONS.md` | NuGet, BaseEntity, JWT, Repository, SRS kod örnekleri |
| `REFERENCE/CODING_STANDARDS.md` | Türkçe yorum kuralları ve şablonlar |
| `REFERENCE/SECURITY.md` | Güvenlik kuralları (auth, şifreleme, loglama) |
| `REFERENCE/ENV.md` | Ortam değişkenleri |
| `REFERENCE/DEVELOPMENT_SETUP.md` | Kurulum, çalıştırma, yayınlama |
| `REFERENCE/GERMAN_LANGUAGE_FEATURES.md` | Almanca gramer referansı (kelime kartları için) — `WordDetail.GrammarData` şeması, dil=`de` |
| `REFERENCE/TURKISH_LANGUAGE_FEATURES.md` | Türkçe gramer referansı — `GrammarData` şeması, dil=`tr` (öncelikli, aktif içerik) |
| `REFERENCE/ENGLISH_LANGUAGE_FEATURES.md` | İngilizce gramer referansı — `GrammarData` şeması, dil=`en` (şema hazır, henüz kullanılmıyor) |
| `wiki/` | Obsidian bilgi grafiği (mimari hafıza) — kurallar: `../wiki_schema.md`. Ana harita: `wiki/Index.md` |

## Temel Kurallar

1. Her zaman **Türkçe** konuş.
2. Tüm yorum/log/exception mesajları **Türkçe**; method/class/property isimleri **İngilizce**.
3. Her dosya başında `AMAÇ / NEDEN / BAĞIMLILIKLAR`, her public metot üstünde `AMAÇ / NEDEN / NASIL` bloğu.
4. Sistem kelimesi/kategori şeması çoklu dile açık tasarlandı (`Languages`/`WordConcept` — bkz.
   `DATABASE_SCHEMA/Icerik.md`); yeni dil eklemek yeni migration gerektirmez. **Şu an yalnızca
   Almanca-Türkçe içerik yazılır** — entity/DTO'lara dile özel ad-hoc alan (`EnglishTranslation`,
   `NameEN` vb.) **eklenmez**, yeni dil her zaman `Languages` tablosuna satır + ilgili kavramlara
   `Words`/`CategoryTranslations` satırı olarak eklenir.
5. Sistemde **yalnızca iki rol**: `User` ve `Admin`. `Instructor`/`Teacher`/"öğretmen" kavramı **yoktur**.
6. Yeni servis eklenince `REFERENCE/ENV.md` güncellenir; hassas bilgiler asla kaynak koda/`appsettings.json`'a girmez.
7. **Obsidian Wiki (mimari hafıza):** Yeni bir mimari plan/özellik istenince önce kodu taramak yerine
   `wiki/Index.md`'den başla (**QUERY**). Projede/son değişikliklerde önemli bir şey değiştiyse ilgili
   wiki düğümünü güncelle ve `wiki/Index.md`'yi tazele (**INGEST**). Kurallar → `../wiki_schema.md`.
