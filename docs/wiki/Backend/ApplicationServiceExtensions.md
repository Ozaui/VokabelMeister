# ApplicationServiceExtensions

**Özet:** [[WordLearner_Application]] içindeki tek statik extension sınıfı — `AddApplicationServices(IServiceCollection)` metoduyla MediatR, AutoMapper ve FluentValidation'ı DI konteynerine kaydeder. [[InfrastructureServiceExtensions]]'ın Application katmanındaki karşılığıdır; [[Program_cs]] tek satırla çağırır. Somut bir feature command/validator/profile henüz yok (A-02), bu yüzden reflection taraması kendi assembly'sinden (`typeof(ApplicationServiceExtensions).Assembly`) yapılır — A-03'ten itibaren eklenecek her yeni sınıf otomatik bulunur, bu dosyaya dokunmaya gerek kalmaz.
**Kütüphaneler:** MediatR 12.1.1, AutoMapper 13.0.1, FluentValidation 11.9.2 + FluentValidation.DependencyInjectionExtensions 11.9.2
**Bağlantılar:** [[Program_cs]] · [[InfrastructureServiceExtensions]] · [[WordLearner_Application]] · [[Teknik_Ozellikler]]

## Konum
`backend/WordLearner.Application/Extensions/ApplicationServiceExtensions.cs`

## Mevcut Davranış
```csharp
var applicationAssembly = typeof(ApplicationServiceExtensions).Assembly;

services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
services.AddAutoMapper(applicationAssembly);
services.AddValidatorsFromAssembly(applicationAssembly);
```

- **MediatR:** Controller'lar servisleri doğrudan çağırmak yerine `IMediator.Send(command/query)` ile
  çağıracak; controller ince katman kalır (henüz hiçbir `IRequestHandler` yok).
- **AutoMapper:** Entity ↔ DTO dönüşümleri elle yazılmak yerine `Profile` sınıflarına devredilecek
  (henüz hiçbir `Profile` yok).
- **FluentValidation:** Request DTO doğrulama kuralları controller'a girmeden model binding
  aşamasında çalışacak (henüz hiçbir `AbstractValidator<T>` yok).

## Neden Assembly Marker Yerine Kendi Tipi?
Yaygın .NET pratiği bir "assembly marker" arayüzü/sınıfı tanımlamaktır (`IApplicationMarker` gibi),
ama bu projede `ApplicationServiceExtensions`'ın kendisi zaten Application katmanı assembly'sinde
yaşadığı için ekstra bir dosya açmadan `typeof(ApplicationServiceExtensions).Assembly` yeterli oldu —
gereksiz soyutlama eklenmedi (KISS).

## Henüz Eksik
- Somut `IRequestHandler`, `Profile`, `AbstractValidator<T>` sınıfları — A-03'ten itibaren eklenecek,
  bu extension'a dokunulmadan otomatik bulunacaklar.
