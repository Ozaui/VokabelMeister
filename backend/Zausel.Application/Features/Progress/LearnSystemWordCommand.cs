using MediatR;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Srs;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Progress;

public record LearnSystemWordCommand(int WordId, int UserId, string? ActorRole) : IRequest<LearnSystemWordResponse>;

public class LearnSystemWordCommandHandler : IRequestHandler<LearnSystemWordCommand, LearnSystemWordResponse>
{
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;
    private readonly IAchievementService _achievementService;

    public LearnSystemWordCommandHandler(
        IUserProgressRepository userProgressRepository, IWordConceptRepository wordConceptRepository,
        ILanguageRepository languageRepository, IActivityLogger activityLogger, IAchievementService achievementService)
    {
        _userProgressRepository = userProgressRepository;
        _wordConceptRepository = wordConceptRepository;
        _languageRepository = languageRepository;
        _activityLogger = activityLogger;
        _achievementService = achievementService;
    }

    public async Task<LearnSystemWordResponse> Handle(LearnSystemWordCommand request, CancellationToken cancellationToken)
    {
        var word = await _wordConceptRepository.GetWordByIdAsync(request.WordId, cancellationToken)
            ?? throw new EntityNotFoundException($"Word not found: Id={request.WordId}");

        var germanLanguage = await _languageRepository.GetByCodeAsync("de", cancellationToken)
            ?? throw new EntityNotFoundException("Language not found: Code=de");
        var germanWord = await _wordConceptRepository.FindWordAsync(word.WordConceptId, germanLanguage.Id, cancellationToken);
        // Kavram henüz Almanca'yla eşleşmemişse (tek dilli) kendi metnine düşülür — bu ekranın hiçbir
        // zaman boş bir başlıkla gösterilmemesi için.
        var germanText = germanWord?.Text ?? word.Text;

        var existing = await _userProgressRepository.GetByUserAndWordAsync(request.UserId, request.WordId, cancellationToken);
        if (existing is not null)
            return new LearnSystemWordResponse(existing.Id, request.WordId, germanText, AlreadyExists: true);

        var userProgress = new UserProgress { UserId = request.UserId, WordId = request.WordId };
        await _userProgressRepository.AddAsync(userProgress, request.UserId, cancellationToken);
        await _userProgressRepository.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "LEARN_SYSTEM_WORD", entityType: "UserProgress", entityId: userProgress.Id,
            oldValue: null, newValue: new { userProgress.WordId }, cancellationToken: cancellationToken);
        await _achievementService.EvaluateAndUnlockAsync(request.UserId, cancellationToken: cancellationToken);

        return new LearnSystemWordResponse(userProgress.Id, request.WordId, germanText, AlreadyExists: false);
    }
}
