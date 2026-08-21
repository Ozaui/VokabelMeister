using MediatR;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Application.Features.Progress;

public record GetProgressWordsQuery(int UserId, string Band) : IRequest<List<ProgressWordResponse>>;

public class GetProgressWordsQueryHandler : IRequestHandler<GetProgressWordsQuery, List<ProgressWordResponse>>
{
    private readonly IUserProgressRepository _userProgressRepository;

    public GetProgressWordsQueryHandler(IUserProgressRepository userProgressRepository) =>
        _userProgressRepository = userProgressRepository;

    public async Task<List<ProgressWordResponse>> Handle(GetProgressWordsQuery request, CancellationToken cancellationToken)
    {
        // Zayıf/Orta/İyi 0-40/40-70/70-100 — üst sınır "İyi" için yok, Mastery hiçbir zaman 100'ü aşmaz.
        var (min, max) = request.Band switch
        {
            "Weak" => (0m, (decimal?)40m),
            "Medium" => (40m, (decimal?)70m),
            "Good" => (70m, (decimal?)null),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Band), request.Band, "Beklenmeyen bant değeri — validator bunu ENGELLEMİŞ olmalıydı.")
        };

        var items = await _userProgressRepository.GetByMasteryRangeAsync(request.UserId, min, max, cancellationToken);
        return items
            .Select(i => new ProgressWordResponse(i.WordId, i.Text, i.Definition, i.CurrentLevel, i.Mastery, i.NextReviewAt, i.IsSuspended))
            .ToList();
    }
}
