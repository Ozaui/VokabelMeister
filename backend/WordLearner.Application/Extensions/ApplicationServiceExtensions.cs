// ─────────────────────────────────────────────────────────────────────────────
// ApplicationServiceExtensions.cs
//
// AMAÇ: Application katmanına ait tüm servisleri (MediatR, AutoMapper, FluentValidation)
//       DI konteynerine kaydetmek için Program.cs'den çağrılan tek extension metot.
// NEDEN: Program.cs'i temiz tutar; InfrastructureServiceExtensions'ın Application
//        katmanındaki karşılığıdır — yeni bir validator/handler/mapping profili
//        eklenince yalnızca assembly taraması sayesinde otomatik kaydolur, Program.cs'e dokunulmaz.
// BAĞIMLILIKLAR: MediatR, AutoMapper, FluentValidation.DependencyInjectionExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace WordLearner.Application.Extensions;

public static class ApplicationServiceExtensions
{
    // AMAÇ: MediatR handler'larını, AutoMapper profillerini ve FluentValidation
    //       validator'larını Application katmanı assembly'sinden tarayıp kaydeder.
    // NEDEN: Henüz (A-02) hiçbir feature command/handler/validator yazılmadığı için
    //        somut bir tip yerine bu extension'ın bulunduğu assembly referans alınır;
    //        A-03'ten itibaren eklenecek her CreateWordCommand/CreateWordCommandValidator
    //        vb. otomatik olarak bulunur, tek tek kayıt gerekmez.
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationServiceExtensions).Assembly;

        // NEDEN MediatR: Controller'lar servisleri doğrudan çağırmak yerine
        //       IMediator.Send(command/query) ile çağırır; controller ince katman kalır.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        // NEDEN AutoMapper: Entity ↔ DTO dönüşümlerini elle yazmak yerine profil
        //       sınıflarına (Profile) devreder; her feature kendi profilini ekler.
        services.AddAutoMapper(applicationAssembly);

        // NEDEN FluentValidation: Request DTO'larının doğrulama kuralları (RuleFor)
        //       controller'a girmeden, ASP.NET Core'un model binding aşamasında çalışır.
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
