using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WordLearner.API.Common;

namespace WordLearner.API.Controllers;

// AuthController/QrLoginController'ın PAYLAŞTIĞI dört küçük okuma — [Authorize] bir istekte JWT'den
// kullanıcı kimliği, ve HER isteğin taşıdığı Accept-Language/IP/User-Agent. Handler'lara Command
// inşa ederken geçirilir; Controller'ın kendisi bunların DIŞINDA hiçbir iş mantığı taşımaz.
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected string? AcceptLanguage => HttpContext.GetLanguage();

    protected string? ClientIpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

    protected string? DeviceInfo => Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
}
