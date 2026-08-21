using FluentValidation;
using Zausel.Application.Features.Progress;

namespace Zausel.Application.Validators.Progress;

public class GetProgressWordsQueryValidator : AbstractValidator<GetProgressWordsQuery>
{
    public GetProgressWordsQueryValidator()
    {
        RuleFor(x => x.Band).Must(b => b is "Weak" or "Medium" or "Good").WithErrorCode("PROGRESS_BAND_INVALID");
    }
}
