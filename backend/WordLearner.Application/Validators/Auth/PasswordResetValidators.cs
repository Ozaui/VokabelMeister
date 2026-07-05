// ─────────────────────────────────────────────────────────────────────────────
// PasswordResetValidators.cs
//
// AMAÇ: POST /auth/forgot-password ve POST /auth/reset-password girdilerinin
//       doğrulama kuralları.
// NEDEN: 2 adımlı OTP tabanlı şifre sıfırlama akışının iki adımı, tek dosyada
//        toplandı (DTOs/Auth/PasswordResetDtos.cs ile aynı gruplama).
// BAĞIMLILIKLAR: FluentValidation, EmailRuleExtensions, OtpRuleExtensions, PasswordRuleExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Validators.Auth;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.OtpCode).ValidOtpCode();
        RuleFor(x => x.NewPassword).ValidPassword();
    }
}
