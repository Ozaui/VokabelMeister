// ─────────────────────────────────────────────────────────────────────────────
// SecurityLogger.cs
//
// AMAÇ: ISecurityLogger'ın implementasyonu.
// NEDEN: E-posta hash'lemesini (IPasswordService.HashToken — RefreshTokens.TokenHash/
//        Users.OriginalEmailHash ile AYNI SHA-256→Base64 fonksiyonu, PII kuralı gereği
//        ham e-posta hiçbir log satırına yazılmaz) ve SecurityLog entity'sinin
//        kurulmasını tek yerde toplar.
// BAĞIMLILIKLAR: ISecurityLogRepository, IPasswordService.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Services;

public class SecurityLogger : ISecurityLogger
{
    private readonly ISecurityLogRepository _repository;
    private readonly IPasswordService _passwordService;

    public SecurityLogger(ISecurityLogRepository repository, IPasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public Task LogAsync(
        LogEventType eventType,
        int? userId = null,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? detail = null,
        CancellationToken ct = default
    )
    {
        var log = new SecurityLog
        {
            EventType = eventType,
            UserId = userId,
            EmailHash = email is null ? null : _passwordService.HashToken(email),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Detail = detail,
        };

        return _repository.AddAsync(log, ct);
    }
}
