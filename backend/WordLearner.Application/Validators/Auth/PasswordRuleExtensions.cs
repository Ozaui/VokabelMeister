using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class PasswordRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder
    ) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Password must not be empty.")
            .WithErrorCode("PASSWORD_REQUIRED")
            .MinimumLength(12) // brute-force direnci
            .WithMessage("Password must be at least 12 characters long.")
            .WithErrorCode("PASSWORD_TOO_SHORT")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least 1 uppercase letter.")
            .WithErrorCode("PASSWORD_MISSING_UPPERCASE")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least 1 lowercase letter.")
            .WithErrorCode("PASSWORD_MISSING_LOWERCASE")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least 1 digit.")
            .WithErrorCode("PASSWORD_MISSING_DIGIT")
            .Matches(@"[!@#$%^&*]")
            .WithMessage("Password must contain at least 1 special character (!@#$%^&*).")
            .WithErrorCode("PASSWORD_MISSING_SPECIAL_CHAR");
}
