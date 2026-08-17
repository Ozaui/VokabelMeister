using Zausel.Application.Interfaces.Repositories.Logging;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Logging;
using Zausel.Domain.Enums.Logging;

namespace Zausel.Application.Services;

public class SecurityLogger : ISecurityLogger
{
    private readonly ISecurityLogRepository _securityLogRepository;
    private readonly IPasswordService _passwordService;

    public SecurityLogger(ISecurityLogRepository securityLogRepository, IPasswordService passwordService)
    {
        _securityLogRepository = securityLogRepository;
        _passwordService = passwordService;
    }

    public async Task LogAsync(
        LogEventType eventType,
        int? userId = null,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? detail = null,
        CancellationToken cancellationToken = default)
    {
        var log = new SecurityLog
        {
            EventType = eventType,
            UserId = userId,
            EmailHash = string.IsNullOrWhiteSpace(email) ? null : _passwordService.HashToken(email),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Detail = detail
        };

        await _securityLogRepository.AddAsync(log, cancellationToken);
        await _securityLogRepository.SaveChangesAsync(cancellationToken);
    }
}
