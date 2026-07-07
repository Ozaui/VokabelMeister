# ApplicationServiceExtensions

**Özet:** [[WordLearner_Application]] içindeki tek statik extension sınıfı — `AddApplicationServices(IServiceCollection)` metoduyla MediatR, AutoMapper ve FluentValidation'ı DI konteynerine kaydeder. [[InfrastructureServiceExtensions]]'ın Application katmanındaki karşılığıdır; [[Program_cs]] tek satırla çağırır. Reflection taraması kendi assembly'sinden (`typeof(ApplicationServiceExtensions).Assembly`) yapılır — A-03'teki 13 Auth Command+Handler'ı (`Application/Features/Auth/`) ve validator'ları bu sayede tek tek kayıt gerekmeden otomatik bulundu; bu, A-02'de öngörülen "ileride eklenecek her yeni sınıf otomatik bulunur" varsayımının ilk gerçek kanıtı oldu.
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

- **MediatR:** Controller'lar servisleri doğrudan çağırmak yerine `IMediator.Send(command)` ile
  çağırır; controller ince katman kalır — [[AuthController]] (A-03) ilk gerçek kullanıcısı,
  13 `IRequestHandler` (`Application/Features/Auth/`) burada otomatik bulunur.
- **AutoMapper:** Entity ↔ DTO dönüşümleri elle yazılmak yerine `Profile` sınıflarına devredilir —
  ilk (ve şu an tek) `Profile`, [[AuthProfile]] (`Application/Features/Auth/AuthProfile.cs`):
  `User → RegisterResponse` ve `User → AuthUserDto` eşlemeleri, alan adları entity ile birebir
  eşleştiği için ek `ForMember` konfigürasyonu gerektirmedi. `RegisterCommandHandler`,
  `RefreshCommandHandler` ve `LoginCompletionService` artık `IMapper`'ı enjekte edip
  `_mapper.Map<T>(user)` çağırıyor; A-02'de kurulup A-03'ün ilk halinde kullanılmadan
  kalan paket, bu retrofit ile fiilen devreye girdi (bkz. wiki Index.md INGEST notu).
- **FluentValidation:** Command doğrulama kuralları controller'a girmeden `ValidationFilter` (model
  binding sonrası, action çalışmadan önce) aşamasında çalışır — A-03'teki 12 `AbstractValidator<T>`
  (`Application/Validators/Auth/`) burada otomatik bulunur.

## Neden Assembly Marker Yerine Kendi Tipi?
Yaygın .NET pratiği bir "assembly marker" arayüzü/sınıfı tanımlamaktır (`IApplicationMarker` gibi),
ama bu projede `ApplicationServiceExtensions`'ın kendisi zaten Application katmanı assembly'sinde
yaşadığı için ekstra bir dosya açmadan `typeof(ApplicationServiceExtensions).Assembly` yeterli oldu —
gereksiz soyutlama eklenmedi (KISS).
