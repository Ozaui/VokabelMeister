using MediatR;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Admin;

public record GetActivityLogsQuery(
    int? UserId,
    string? Action,
    string? EntityType,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ActivityLogDto>>;

public class GetActivityLogsQueryHandler : IRequestHandler<GetActivityLogsQuery, PagedResult<ActivityLogDto>>
{
    private readonly IActivityLogRepository _activityLogRepository;

    public GetActivityLogsQueryHandler(IActivityLogRepository activityLogRepository) =>
        _activityLogRepository = activityLogRepository;

    public async Task<PagedResult<ActivityLogDto>> Handle(GetActivityLogsQuery request, CancellationToken ct)
    {
        var paged = await _activityLogRepository.GetPagedAsync(
            request.UserId,
            request.Action,
            request.EntityType,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            ct
        );

        return new PagedResult<ActivityLogDto>(
            paged.Items
                .Select(l => new ActivityLogDto(
                    l.Id,
                    l.UserId,
                    l.ActorRole,
                    l.Action,
                    l.EntityType,
                    l.EntityId,
                    l.OldValue,
                    l.NewValue,
                    l.IpAddress,
                    l.UserAgent,
                    l.CreatedAt
                ))
                .ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize
        );
    }
}
