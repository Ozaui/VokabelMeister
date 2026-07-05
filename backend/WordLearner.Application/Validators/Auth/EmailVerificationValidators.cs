// ─────────────────────────────────────────────────────────────────────────────
// EmailVerificationValidators.cs
//
// AMAÇ: POST /auth/verify-email ve POST /auth/resend-verification girdilerinin
//       doğrulama kuralları.
// NEDEN: Her ikisi de e-posta doğrulama akışının parçası, tek dosyada toplandı
//        (DTOs/Auth/EmailVerificationDtos.cs ile aynı gruplama).
// BAĞIMLILIKLAR: FluentValidation, EmailRuleExtensions, OtpRuleExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Validators.Auth;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.OtpCode).ValidOtpCode();
    }
}

public class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}
