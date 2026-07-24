using MediatR;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Admin;

// Detail bir Code (CLAUDE.md "İkinci istisna") — admin panel de bir istemci, yazılırken
// değil admin OKURKEN kendi Accept-Language'ına göre çözülür.
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
