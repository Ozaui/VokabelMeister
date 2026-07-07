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
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.OtpCode).ValidOtpCode();
    }
}

public class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}
