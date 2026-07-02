# Backend Katmanlı Mimari

**Özet:** Backend, klasik katmanlı (layered) mimariyle dört .NET projesine ayrılır ve bağımlılık yönü tek yönlüdür: `Domain ← Infrastructure ← Application ← API`. Her katman yalnızca kendi altındaki katmana referans verir; Domain hiçbir şeye bağımlı değildir. Çalışma yöntemi "dikey dilim"dir — bir API tüm katmanlarıyla bitirilip sonra diğerine geçilir.
**Kütüphaneler:** .NET 9, EF Core 9, ASP.NET Core
**Bağlantılar:** [[WordLearner_API]] · [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[WordLearner_Domain]] · [[WordLearner_Tests]] · [[Gelistirme_Yol_Haritasi]] · [[Sistem_Mimarisi]]

## Katmanlar

```
WordLearner.API            → HTTP (Controllers, Middleware, Program.cs)
WordLearner.Application     → İş mantığı (Services, DTOs, Validators, Interfaces)
WordLearner.Infrastructure  → Veri erişimi (DbContext, Repositories, Configurations, Logging sink)
WordLearner.Domain          → Entities, Enums
```

Proje referansları (`.csproj`):
- [[WordLearner_API]] → [[WordLearner_Application]], [[WordLearner_Infrastructure]]
- [[WordLearner_Infrastructure]] → [[WordLearner_Application]], [[WordLearner_Domain]]
- [[WordLearner_Application]] → [[WordLearner_Domain]]
- [[WordLearner_Domain]] → (bağımsız)
- [[WordLearner_Tests]] → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## Dikey Dilim Çalışma Yöntemi

Bir API'ın tüm parçaları (Entity → EF Config → Migration → DTO → Validator → Repository →
Servis → Birim Test → Controller → DI kaydı) **tek task içinde** tamamlanır, ardından
`API_YOL_HARITASI/` rehberine işlenir. Katman katman ilerleme (önce tüm entity'ler, sonra tüm
DTO'lar) yasaktır. Detay ve gerekçe → [[Gelistirme_Yol_Haritasi]].

## Şu Ana Kadar Yazılan Kod

Yalnızca A-01/A-02 kapsamındaki paylaşılan altyapı yazıldı — feature entity/servis/controller
henüz **yok**:
- [[Program_cs]] (API katmanı, iskelet hâlde)
- [[BaseEntity]] (Domain)
- [[IRepository]] (Application)
- [[Repository]], [[WordLearnerDbContext]], [[InfrastructureServiceExtensions]] (Infrastructure)
- [[EntityNotFoundException]] (Application)
