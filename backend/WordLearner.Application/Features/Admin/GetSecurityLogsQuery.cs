// ─────────────────────────────────────────────────────────────────────────────
// GetSecurityLogsQuery.cs
//
// AMAÇ: GET /admin/logs/security — eventType/ip/tarih aralığı filtreli sayfalı
//       güvenlik olayı listesi. `Detail` alanı ÇÖZÜLEREK (LogMessages.Resolve)
//       döner — bu, A-07'nin 3 log Query'si arasında Language TAŞIYAN TEK olanı.
// NEDEN: SecurityLog.Detail bir Code (CLAUDE.md "İkinci istisna") — admin panel
//        de bir istemci, log satırı YAZILIRKEN değil admin OKURKEN kendi
//        Accept-Language'ına göre çözülür (ErrorMessages/SuccessMessages ile
//        AYNI Code-sonra-çöz deseni, RequestLanguageResolver → Controller →
//        `with { Language = ... }` — AuthController'ın MessageResponse
//        akışlarındaki AYNI zincir).
// BAĞIMLILIKLAR: ISecurityLogRepository, LogMessages, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Admin;

public record GetSecurityLogsQuery(
    LogEventType? EventType,
    string? IpAddress,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SecurityLogDto>>
{
    public string? Language { get; init; }
}

public class GetSecurityLogsQueryHandler : IRequestHandler<GetSecurityLogsQuery, PagedResult<SecurityLogDto>>
{
    private readonly ISecurityLogRepository _securityLogRepository;

    public GetSecurityLogsQueryHandler(ISecurityLogRepository securityLogRepository) =>
        _securityLogRepository = securityLogRepository;

    public async Task<PagedResult<SecurityLogDto>> Handle(GetSecurityLogsQuery request, CancellationToken ct)
    {
        var paged = await _securityLogRepository.GetPagedAsync(
            request.EventType,
            request.IpAddress,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            ct
        );

        return new PagedResult<SecurityLogDto>(
            paged.Items
                .Select(l => new SecurityLogDto(
                    l.Id,
                    l.EventType.ToString(),
                    l.UserId,
                    l.IpAddress,
                    l.UserAgent,
                    LogMessages.Resolve(l.Detail, request.Language),
                    l.CreatedAt
                ))
                .ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize
        );
    }
}
