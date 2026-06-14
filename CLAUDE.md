
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
7. **Uygulama yalnızca Türkçe-Almanca destekler.** İngilizce çeviri/isim alanları (`EnglishTranslation`, `SentenceEN`, `NameEN` vb.) entity'lere ve DTO'lara eklenmez. İleride başka dil eklenmesi gerekirse bu kural TASK listesine eklenerek ayrıca ele alınır.

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

## Junior Rehberi Güncelleme Kuralı

Her task tamamlandıktan sonra `docs/junior-guide.html` dosyasındaki `TASKS` dizisine yeni bir obje eklenmeli.

**Kural:** Task statusu `'done'` olan bir task tamamlandığında, `TASKS` dizisinde o task'ı `status: 'next'` iken `status: 'done'` olarak güncelle. Sıradaki task'ı `status: 'next'` olarak ekle.

**Ekleme formatı:**
```js
{
  id: 'task-XXX',
  num: 'TASK-XXX',
  title: 'Task Başlığı',
  faz: 1,           // 1 | 2 | 3 | 4
  status: 'done',   // 'done' | 'next' | 'todo'
  summary: 'Ne yapıldığını 1-2 cümleyle anlat.',
  checklist: [
    { done: true, text: 'Yapılan adım 1' },
    { done: true, text: 'Yapılan adım 2' },
  ],
  why: 'Bu task neden bu sırada yapıldı? Ne için gerekli?',
  keyPoints: [
    { icon: '💡', title: 'Kavram Adı', desc: 'Açıklama' },
  ],
  code: [
    { title: 'Dosya.cs — Açıklama', lang: 'csharp', body: `// kod buraya` },
  ],
  unlocks: [
    'TASK-XXX: Bu task olmadan yapılamayan şey',
  ]
}
```

**Dosya konumu:** `docs/junior-guide.html` — `TASKS` dizisinin içine, son `done` task'ın hemen arkasına ekle.

**ÖNEMLİ:** Bu kuralı takip etmeden task tamamlandı demek yasak. Her task sonunda HTML güncellenir.

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
