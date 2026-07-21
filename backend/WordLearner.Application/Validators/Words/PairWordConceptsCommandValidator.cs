// ─────────────────────────────────────────────────────────────────────────────
// PairWordConceptsCommandValidator.cs
//
// AMAÇ: PairWordConceptsCommand'ın tek gerçek sınır kontrolü — `otherConceptId`
//       `primaryId` ile aynı olamaz.
// NEDEN: Aynı kavram kendisiyle eşleştirilirse WordConceptRepository.PairAsync
//        aynı EF Core tracked instance'ı iki kez yükleyip Words koleksiyonunu
//        bozardı (identity map) — bu, iş kuralı hatasından önce engellenmesi
//        gereken bir girdi hatası, admin panel bunu göndermemeli ama API sınırında
//        yine de doğrulanır.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Validators.Words;

public class PairWordConceptsCommandValidator : AbstractValidator<PairWordConceptsCommand>
{
    public PairWordConceptsCommandValidator()
    {
        RuleFor(x => x.OtherConceptId)
            .NotEqual(x => x.PrimaryId)
            .WithMessage("otherConceptId must differ from primaryId.")
            .WithErrorCode("SAME_CONCEPT_PAIR_NOT_ALLOWED");
    }
}
