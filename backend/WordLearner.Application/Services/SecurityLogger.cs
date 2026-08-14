using WordLearner.Application.Interfaces.Repositories.Logging;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Services;

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
