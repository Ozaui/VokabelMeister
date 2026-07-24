using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class RefreshTokenRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidRefreshToken<T>(
        this IRuleBuilder<T, string> ruleBuilder
    ) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Token must not be empty.")
            .WithErrorCode("TOKEN_REQUIRED");
}
