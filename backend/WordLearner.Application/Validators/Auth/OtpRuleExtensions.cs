using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class OtpRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidOtpCode<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Verification code must not be empty.")
            .WithErrorCode("OTP_REQUIRED")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must be 6 digits.")
            .WithErrorCode("OTP_INVALID_FORMAT");
}
