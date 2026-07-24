using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class EmailRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder
    ) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Email must not be empty.")
            .WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress()
            .WithMessage("Enter a valid email address.")
            .WithErrorCode("EMAIL_INVALID");
}
