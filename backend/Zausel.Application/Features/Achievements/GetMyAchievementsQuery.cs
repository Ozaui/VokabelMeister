using MediatR;
using Zausel.Application.Common;
using Zausel.Application.DTOs.Achievements;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Application.Features.Achievements;

public record GetMyAchievementsQuery(int UserId, string? AcceptLanguage) : IRequest<List<AchievementResponse>>;

public class GetMyAchievementsQueryHandler : IRequestHandler<GetMyAchievementsQuery, List<AchievementResponse>>
{
    private readonly IUserAchievementRepository _userAchievementRepository;

    public GetMyAchievementsQueryHandler(IUserAchievementRepository userAchievementRepository) =>
        _userAchievementRepository = userAchievementRepository;

    public async Task<List<AchievementResponse>> Handle(GetMyAchievementsQuery request, CancellationToken cancellationToken)
    {
        var items = await _userAchievementRepository.GetUnlockedForUserAsync(request.UserId, cancellationToken);

        return items.Select(item =>
        {
            var (name, description) = AchievementMessages.Resolve(item.AchievementId, request.AcceptLanguage);
            return new AchievementResponse(item.AchievementId, name, description, item.Icon, item.RewardXP, item.Rarity, item.UnlockedAt);
        }).ToList();
    }
}
