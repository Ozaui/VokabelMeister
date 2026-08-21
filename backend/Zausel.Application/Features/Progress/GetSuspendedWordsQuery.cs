using MediatR;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Application.Features.Progress;

public record GetSuspendedWordsQuery(int UserId) : IRequest<List<SuspendedWordResponse>>;

public class GetSuspendedWordsQueryHandler : IRequestHandler<GetSuspendedWordsQuery, List<SuspendedWordResponse>>
{
    private readonly IUserProgressRepository _userProgressRepository;

    public GetSuspendedWordsQueryHandler(IUserProgressRepository userProgressRepository) =>
        _userProgressRepository = userProgressRepository;

    public async Task<List<SuspendedWordResponse>> Handle(GetSuspendedWordsQuery request, CancellationToken cancellationToken)
    {
        var items = await _userProgressRepository.GetSuspendedAsync(request.UserId, cancellationToken);
        return items
            .Select(i => new SuspendedWordResponse(i.WordId, i.Text, i.Definition, i.CurrentLevel, i.Mastery, i.ConsecutiveIncorrect))
            .ToList();
    }
}
