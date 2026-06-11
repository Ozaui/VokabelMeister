# CLAUDE.md

## Proje

Türkçe-Almanca kelime öğrenme uygulaması.

**Backend:** .NET 9 Web API + EF Core 9 + MS SQL Server  
**Mobile:** React Native (Expo) + TypeScript + Redux Toolkit + Axios  
**Admin Panel:** React + Vite + TypeScript (aynı API, Admin/Instructor rolü)  

**Kimlik doğrulama:** ASP.NET Identity KULLANILMIYOR. JWT ve şifre hashleme tamamen manuel yazılacak.

---

## Geliştirme Sırası

**FAZ 1 (Backend) → FAZ 3 (Admin Panel) → FAZ 1+ (Backend Eksikleri) → FAZ 2 (Mobile) → FAZ 4 (Test)**

Admin paneli kullanırken backend'de eksik veya hatalı şeyler görülür, mobile başlamadan önce düzeltilir.

---

## Temel Kurallar

1. Her zaman **Türkçe** konuş
2. Tüm kod yorumları, log ve exception mesajları **Türkçe**
3. Method/class/property isimleri **İngilizce** (C# ve JS convention)
4. Her dosyanın başında `AMAÇ / NEDEN / BAĞIMLILIKLAR` bloğu zorunlu
5. Her public metodun üstünde `AMAÇ / NEDEN / NASIL` bloğu zorunlu
6. Karmaşık kod bloklarında adım adım Türkçe yorum zorunlu

Yorum şablonu için → `docs/CODING_STANDARDS.md`

---

## Referans Dosyalar

| Dosya | Ne Zaman Bakılır |
|-------|-----------------|
| `docs/TASK.md` | Her task başında — ne yapılacağını ve sırayı anlamak için |
| `docs/ARCHITECTURE.md` | Genel yapı, kullanım akışları, entity ilişkileri |
| `docs/DATABASE_SCHEMA.md` | Tablo yapıları (sistem tabloları §2, sosyal tablolar §5) |
| `docs/API_ENDPOINTS.md` | Endpoint detayları, request/response örnekleri |
| `docs/TECHNICAL_SPECIFICATIONS.md` | NuGet paketleri, BaseEntity, JWT, Repository, Redux, SRS kod örnekleri |
| `docs/CODING_STANDARDS.md` | Türkçe yorum kuralları ve şablonlar |
| `docs/SECURITY.md` | Güvenlik kuralları — auth yazarken |
| `docs/GERMAN_LANGUAGE_FEATURES.md` | Almanca gramer — WordDetail ve kelime kartı yazarken |
| `docs/DEVELOPMENT_SETUP.md` | Proje ilk kurulumda |

---

## Her Yeni Oturumda

1. `docs/TASK.md` dosyasını oku — ilerleme tablosunu gör
2. Hangi task'ta kaldığımızı söyle
3. Onay bekle, başla

---

## Klasör Yapısı

```
WordLearner/
├── CLAUDE.md
├── docs/
├── backend/
│   ├── WordLearner.API/
│   ├── WordLearner.Application/
│   ├── WordLearner.Infrastructure/
│   └── WordLearner.Domain/
├── mobile/
├── admin/
└── WordLearner.sln
```
