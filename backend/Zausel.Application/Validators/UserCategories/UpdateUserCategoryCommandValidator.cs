using FluentValidation;
using Zausel.Application.Features.UserCategories;

namespace Zausel.Application.Validators.UserCategories;

public class UpdateUserCategoryCommandValidator : AbstractValidator<UpdateUserCategoryCommand>
{
    public UpdateUserCategoryCommandValidator()
    {
        RuleFor(x => x.UserCategoryId).GreaterThan(0).WithErrorCode("USER_CATEGORY_ID_INVALID");
        RuleFor(x => x.Name).NotEmpty().WithErrorCode("USER_CATEGORY_NAME_REQUIRED");
        RuleFor(x => x.Name).MaximumLength(100).WithErrorCode("USER_CATEGORY_NAME_TOO_LONG");
        RuleFor(x => x.Description).MaximumLength(500).WithErrorCode("USER_CATEGORY_DESCRIPTION_TOO_LONG");
        RuleFor(x => x.Color).MaximumLength(10).WithErrorCode("USER_CATEGORY_COLOR_TOO_LONG");
        RuleFor(x => x.Icon).MaximumLength(100).WithErrorCode("USER_CATEGORY_ICON_TOO_LONG");
    }
}
