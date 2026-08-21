using FluentValidation;
using Zausel.Application.Features.UserCategories;

namespace Zausel.Application.Validators.UserCategories;

public class DeleteUserCategoryCommandValidator : AbstractValidator<DeleteUserCategoryCommand>
{
    public DeleteUserCategoryCommandValidator()
    {
        RuleFor(x => x.UserCategoryId).GreaterThan(0).WithErrorCode("USER_CATEGORY_ID_INVALID");
    }
}
