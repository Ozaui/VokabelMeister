using FluentValidation;
using Zausel.Application.Features.Categories;

namespace Zausel.Application.Validators.Categories;

public class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
{
    private static readonly string[] ValidLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];

    public GetCategoriesQueryValidator()
    {
        RuleFor(x => x.Level).Must(v => v is null || ValidLevels.Contains(v)).WithErrorCode("DIFFICULTY_LEVEL_INVALID");
    }
}
