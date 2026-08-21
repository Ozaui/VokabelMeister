using MediatR;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Application.Features.Progress;

public record GetProgressSummaryQuery(int UserId) : IRequest<ProgressSummaryResponse>;

public class GetProgressSummaryQueryHandler : IRequestHandler<GetProgressSummaryQuery, ProgressSummaryResponse>
{
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IUserCardProgressRepository _userCardProgressRepository;

    public GetProgressSummaryQueryHandler(
        IUserProgressRepository userProgressRepository, IUserCardProgressRepository userCardProgressRepository)
    {
        _userProgressRepository = userProgressRepository;
        _userCardProgressRepository = userCardProgressRepository;
    }

    public async Task<ProgressSummaryResponse> Handle(GetProgressSummaryQuery request, CancellationToken cancellationToken)
    {
        var wordSnapshots = await _userProgressRepository.GetSnapshotsAsync(request.UserId, cancellationToken);
        var cardSnapshots = await _userCardProgressRepository.GetSnapshotsAsync(request.UserId, cancellationToken);

        int weak = 0, medium = 0, good = 0, dueNow = 0;
        var now = DateTime.UtcNow;

        foreach (var snapshot in wordSnapshots.Concat(cardSnapshots))
        {
            if (snapshot.Mastery < 40) weak++;
            else if (snapshot.Mastery < 70) medium++;
            else good++;

            // Askıya alınmış (leech) kayıtlar due sayımından hariç — UserProgressConfiguration'daki
            // "due sorgusundan hariç" kuralının özet sayımdaki karşılığı.
            if (!snapshot.IsSuspended && snapshot.NextReviewAt is { } nextReviewAt && nextReviewAt <= now)
                dueNow++;
        }

        return new ProgressSummaryResponse(weak, medium, good, dueNow);
    }
}
