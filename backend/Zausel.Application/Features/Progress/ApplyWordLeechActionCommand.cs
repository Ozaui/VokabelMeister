using MediatR;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Application.Interfaces.Services;
using Zausel.Application.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Progress;

public record ApplyWordLeechActionCommand(int WordId, string Action, int UserId, string? ActorRole) : IRequest<LeechActionResponse>;

public class ApplyWordLeechActionCommandHandler : IRequestHandler<ApplyWordLeechActionCommand, LeechActionResponse>
{
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IActivityLogger _activityLogger;

    public ApplyWordLeechActionCommandHandler(IUserProgressRepository userProgressRepository, IActivityLogger activityLogger)
    {
        _userProgressRepository = userProgressRepository;
        _activityLogger = activityLogger;
    }

    public async Task<LeechActionResponse> Handle(ApplyWordLeechActionCommand request, CancellationToken cancellationToken)
    {
        var userProgress = await _userProgressRepository.GetByUserAndWordAsync(request.UserId, request.WordId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserProgress not found: WordId={request.WordId}");

        // Continue hiçbir alanı DEĞİŞTİRMEZ — kullanıcı "biliyorum, YİNE DE bu şekilde devam et" der,
        // veritabanına yazılacak bir şey yok, yalnızca istemciye bir onay dönülür.
        if (request.Action == "Continue")
            return new LeechActionResponse(IsSuspended: null, CurrentLevel: null, NextReviewAt: null, Acknowledged: true);

        var oldValue = new { userProgress.IsSuspended, userProgress.CurrentLevel, userProgress.NextReviewAt };

        LeechActionResponse response;
        if (request.Action == "Suspend")
        {
            userProgress.IsSuspended = true;
            response = new LeechActionResponse(IsSuspended: true, CurrentLevel: null, NextReviewAt: null, Acknowledged: null);
        }
        else
        {
            // Reset — SM-2 durumu SIFIRLANIR (yeni bir kelimeymiş gibi baştan başlar), TimesCorrect/
            // TimesIncorrect/SuccessRate gibi geçmiş istatistikler KORUNUR (yalnızca ileri gidiş sıfırlanır).
            userProgress.CurrentLevel = 0;
            userProgress.Mastery = SrsCalculator.CalculateMastery(0, userProgress.SuccessRate);
            userProgress.EasinessFactor = 2.5m;
            userProgress.IntervalDays = 1;
            userProgress.RepetitionNumber = 0;
            userProgress.ConsecutiveIncorrect = 0;
            userProgress.IsSuspended = false;
            userProgress.NextReviewAt = null;
            response = new LeechActionResponse(IsSuspended: null, CurrentLevel: 0, NextReviewAt: null, Acknowledged: null);
        }

        await _userProgressRepository.UpdateAsync(userProgress, request.UserId, cancellationToken);
        await _userProgressRepository.SaveChangesAsync(cancellationToken);

        var newValue = new { userProgress.IsSuspended, userProgress.CurrentLevel, userProgress.NextReviewAt };
        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "APPLY_LEECH_ACTION", entityType: "UserProgress", entityId: userProgress.Id,
            oldValue: oldValue, newValue: newValue, cancellationToken: cancellationToken);

        return response;
    }
}
