// ─────────────────────────────────────────────────────────────────────────────
// GetApplicationLogsQuery.cs
//
// AMAÇ: GET /admin/logs/application — level/tarih aralığı/serbest metin arama
//       filtreli sayfalı teknik log listesi (Serilog'un yazdığı satırlar).
// NEDEN Language YOK: ApplicationLog Serilog'un ürettiği, geliştirici odaklı teknik
//        loglar — CLAUDE.md "DB/log/geliştirici İngilizce görür" kuralı, çevrilecek
//        bir şey YOK (Message zaten İngilizce sabit şablonlarla üretiliyor).
// BAĞIMLILIKLAR: IApplicationLogRepository, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Admin;

public record GetApplicationLogsQuery(
    string? Level,
    DateTime? From,
    DateTime? To,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ApplicationLogDto>>;

public class GetApplicationLogsQueryHandler : IRequestHandler<GetApplicationLogsQuery, PagedResult<ApplicationLogDto>>
{
    private readonly IApplicationLogRepository _applicationLogRepository;

    public GetApplicationLogsQueryHandler(IApplicationLogRepository applicationLogRepository) =>
        _applicationLogRepository = applicationLogRepository;

    public async Task<PagedResult<ApplicationLogDto>> Handle(GetApplicationLogsQuery request, CancellationToken ct)
    {
        var paged = await _applicationLogRepository.GetPagedAsync(
            request.Level,
            request.From,
            request.To,
            request.Search,
            request.Page,
            request.PageSize,
            ct
        );

        return new PagedResult<ApplicationLogDto>(
            paged.Items
                .Select(l => new ApplicationLogDto(
                    l.Id,
                    l.Level,
                    l.Message,
                    l.Exception,
                    l.SourceContext,
                    l.RequestPath,
                    l.UserId,
                    l.TimeStamp
                ))
                .ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize
        );
    }
}
