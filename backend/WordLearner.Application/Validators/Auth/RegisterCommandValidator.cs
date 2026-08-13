using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required").WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithMessage("Email is not a valid address").WithErrorCode("EMAIL_INVALID");

        RuleFor(x => x.Password)
            .MinimumLength(12).WithMessage("Password must be at least 12 characters").WithErrorCode("PASSWORD_TOO_SHORT")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least 1 uppercase letter").WithErrorCode("PASSWORD_MISSING_UPPERCASE")
            .Matches(@"[a-z]").WithMessage("Password must contain at least 1 lowercase letter").WithErrorCode("PASSWORD_MISSING_LOWERCASE")
            .Matches(@"[0-9]").WithMessage("Password must contain at least 1 digit").WithErrorCode("PASSWORD_MISSING_DIGIT")
            .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least 1 special character").WithErrorCode("PASSWORD_MISSING_SPECIAL_CHAR");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required").WithErrorCode("FIRST_NAME_REQUIRED")
            .MaximumLength(50).WithMessage("First name must be at most 50 characters").WithErrorCode("FIRST_NAME_TOO_LONG");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required").WithErrorCode("LAST_NAME_REQUIRED")
            .MaximumLength(50).WithMessage("Last name must be at most 50 characters").WithErrorCode("LAST_NAME_TOO_LONG");
    }
}
