# InfrastructureServiceExtensions

**Özet:** [[WordLearner_Infrastructure]] içindeki tek statik extension sınıfı — `AddInfrastructureServices(IServiceCollection, IConfiguration)` metoduyla infrastructure servislerini DI konteynerine kaydeder, böylece [[Program_cs]] tek satırla altyapıyı hazırlayabilir (henüz o satır Program.cs'e eklenmedi). Şu an yalnızca [[WordLearnerDbContext]]'i `Scoped` olarak kaydediyor.
**Kütüphaneler:** Microsoft.EntityFrameworkCore, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Configuration
**Bağlantılar:** [[WordLearnerDbContext]] · [[Program_cs]] · [[WordLearner_Infrastructure]] · [[Ortam_Degiskenleri]]

## Konum
`backend/WordLearner.Infrastructure/Extensions/InfrastructureServiceExtensions.cs`

## Mevcut Davranış
```csharp
services.AddDbContext<WordLearnerDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```
`Scoped` seçildi çünkü DbContext request başına bir instance olmalı — `Singleton` olsaydı eş zamanlı
istekler aynı context'i paylaşır, thread-safety sorunu çıkardı.

## Henüz Eksik
- [[Program_cs]] içinde bu metodun çağrılması (`builder.Services.AddInfrastructureServices(builder.Configuration)`)
- Feature repository kayıtları (`IUserRepository` → `UserRepository` vb., A-03+'ta eklenecek)
- Generic `IRepository<>` → `Repository<>` DI kaydı
