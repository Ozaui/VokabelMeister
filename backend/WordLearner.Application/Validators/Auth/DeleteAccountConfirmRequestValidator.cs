// ─────────────────────────────────────────────────────────────────────────────
// DeleteAccountConfirmRequestValidator.cs
//
// AMAÇ: POST /auth/delete-account/confirm girdisinin doğrulama kuralları.
// NEDEN: Şifre burada GÜCÜ değil doluluğu kontrol edilir — bu mevcut hesabın
//        şifresi, yeni bir şifre değil (LoginRequestValidator ile aynı gerekçe).
// BAĞIMLILIKLAR: FluentValidation, OtpRuleExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class ConfirmAccountDeletionCommandValidator : AbstractValidator<ConfirmAccountDeletionCommand>
{
    public ConfirmAccountDeletionCommandValidator()
    {
        RuleFor(x => x.OtpCode).ValidOtpCode();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty.").WithErrorCode("PASSWORD_REQUIRED");
    }
}
