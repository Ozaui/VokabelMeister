using FluentValidation;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Validators.Words;

public class PairWordConceptsCommandValidator : AbstractValidator<PairWordConceptsCommand>
{
    public PairWordConceptsCommandValidator()
    {
        // Aynı kavram kendisiyle eşleştirilirse PairAsync aynı EF Core tracked instance'ı iki kez
        // yükleyip Words koleksiyonunu bozar (identity map çakışması).
        RuleFor(x => x.OtherConceptId)
            .NotEqual(x => x.PrimaryId)
            .WithMessage("otherConceptId must differ from primaryId.")
            .WithErrorCode("SAME_CONCEPT_PAIR_NOT_ALLOWED");
    }
}
