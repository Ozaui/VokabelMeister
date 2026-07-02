# WordLearnerDbContext

**Özet:** [[WordLearner_Infrastructure]] içindeki merkezi EF Core `DbContext` sınıfı — henüz hiçbir `DbSet<T>` içermiyor (feature entity'ler A-03'ten itibaren eklenecek), ama iki önemli global davranışı zaten hazır: tüm [[BaseEntity]] türevlerine soft delete query filter'ı ve `SaveChangesAsync` override'ıyla otomatik `UpdatedAt`.
**Kütüphaneler:** Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.SqlServer
**Bağlantılar:** [[BaseEntity]] · [[Repository]] · [[InfrastructureServiceExtensions]] · [[Veritabani_Semasi]] · [[WordLearner_Infrastructure]]

## Konum
`backend/WordLearner.Infrastructure/Data/WordLearnerDbContext.cs`

## Davranışlar

### 1. `OnModelCreating`
- `modelBuilder.ApplyConfigurationsFromAssembly(...)` — Infrastructure assembly'sindeki tüm
  `IEntityTypeConfiguration<T>` sınıflarını otomatik tarar (henüz hiç `Configurations/` dosyası yok).
- **Global soft delete filtresi:** Reflection ile her `BaseEntity` türevine `HasQueryFilter(e => !e.IsDeleted)`
  uygular — hiçbir sorguda elle `.Where(e => !e.IsDeleted)` yazmaya gerek kalmaz.
  Admin'in silinmiş kayıtları görmesi gerektiğinde `IgnoreQueryFilters()` ile bypass edilir.

### 2. `SaveChangesAsync` override
`ChangeTracker.Entries<BaseEntity>()` üzerinde `EntityState.Modified` olanların `UpdatedAt`'ini
otomatik `DateTime.UtcNow` yapar — servis/repository katmanında elle set etmeye gerek kalmaz.

## Bağlantı Dizesi
`AddDbContext` çağrısı ve connection string okuma → [[InfrastructureServiceExtensions]] içinde
(`ConnectionStrings:DefaultConnection`, gerçek değer [[Ortam_Degiskenleri]]'nden).

## Henüz Yok
`DbSet<T>` property'leri (`Users`, `Words`, ...) — her feature entity'siyle birlikte eklenecek,
bkz. [[Veritabani_Semasi]] ve [[Gelistirme_Yol_Haritasi]].
