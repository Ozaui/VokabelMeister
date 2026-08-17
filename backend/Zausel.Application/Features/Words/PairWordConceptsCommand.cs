using MediatR;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Words;

public record PairWordConceptsCommand(int PrimaryId, int OtherConceptId, int? UserId, string? ActorRole) : IRequest<WordResponse>;

public class PairWordConceptsCommandHandler : IRequestHandler<PairWordConceptsCommand, WordResponse>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly IActivityLogger _activityLogger;

    public PairWordConceptsCommandHandler(IWordConceptRepository wordConceptRepository, IActivityLogger activityLogger)
    {
        _wordConceptRepository = wordConceptRepository;
        _activityLogger = activityLogger;
    }

    public async Task<WordResponse> Handle(PairWordConceptsCommand request, CancellationToken cancellationToken)
    {
        // `primaryId`nin var OLDUĞUNU burada doğruluyoruz — kendi alanları (PartOfSpeech/Category/
        // DifficultyLevel) hiç OKUNMUYOR, çünkü aşağıda yalnızca onun WordConcept satırı hayatta
        // kalıyor ve çakışma kontrolü BİLEREK yapılmıyor (dilden dile gramer türü kayması bir veri
        // hatası değil, primaryId'ninki zaten dokunulmadan kalıyor). Yine de aggregate'in TAMAMI
        // ActivityLog'un OldValue'suna (aşağıda) giriyor — "eşleşmeden ÖNCE primaryId neydi" sorusu
        // audit için değerli, PartOfSpeech'in OKUNMAMASI ile OldValue'YA hiç GİRMEMESİ FARKLI şeyler.
        var primaryBefore = await _wordConceptRepository.GetAggregateAsync(request.PrimaryId, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found: Id={request.PrimaryId}");
        var other = await _wordConceptRepository.GetAggregateAsync(request.OtherConceptId, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found: Id={request.OtherConceptId}");
        var otherBeforeResponse = WordMapping.ToResponse(other);

        foreach (var translation in other.Translations)
            await _wordConceptRepository.MoveWordToConceptAsync(translation.Word.Id, request.PrimaryId, request.UserId, cancellationToken);

        await _wordConceptRepository.SoftDeleteConceptOnlyAsync(request.OtherConceptId, request.UserId, cancellationToken);
        await _wordConceptRepository.SaveChangesAsync(cancellationToken);

        var merged = await _wordConceptRepository.GetAggregateAsync(request.PrimaryId, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found after pairing: Id={request.PrimaryId}");
        var response = WordMapping.ToResponse(merged);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "PAIR_WORD_CONCEPTS", entityType: "Word", entityId: request.PrimaryId,
            oldValue: new { Primary = WordMapping.ToResponse(primaryBefore), Other = otherBeforeResponse },
            newValue: response, cancellationToken: cancellationToken);

        return response;
    }
}
