using FluentValidation;
using Zausel.Application.Features.Categories;

namespace Zausel.Application.Validators.Categories;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0).WithErrorCode("CATEGORY_ID_INVALID");
    }
}
