using FluentValidation;
using WordLearner.Application.Features.Admin;

namespace WordLearner.Application.Validators.Admin;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.Role)
            .Must(role => role is "User" or "Admin")
            .WithMessage("Role must be 'User' or 'Admin'.")
            .WithErrorCode("INVALID_USER_ROLE");
    }
}
