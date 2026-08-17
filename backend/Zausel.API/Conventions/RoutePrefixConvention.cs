using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Zausel.API.Conventions;

// Tüm controller'lara "api/v1" önekini TEK yerden ekler (AddControllers'a kayıt anında) —
// TASK/A_backend.md A-01 versiyonlama kararı: v2 ileride açılabilsin diye URL prefix'i baştan
// konur, Asp.Versioning gibi bir kütüphane henüz eklenmez (tek versiyon var, YAGNI).
public class RoutePrefixConvention : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefix;

    public RoutePrefixConvention(string prefix)
    {
        _prefix = new AttributeRouteModel(new RouteAttribute(prefix));
    }

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        foreach (var selector in controller.Selectors)
        {
            selector.AttributeRouteModel = selector.AttributeRouteModel is null
                ? _prefix
                : AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
        }
    }
}
