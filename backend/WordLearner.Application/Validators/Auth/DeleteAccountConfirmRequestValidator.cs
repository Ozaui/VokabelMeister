// ─────────────────────────────────────────────────────────────────────────────
// DeleteAccountConfirmRequestValidator.cs
//
// AMAÇ: POST /auth/delete-account/confirm girdisinin doğrulama kuralları.
// NEDEN: Şifre burada GÜCÜ değil doluluğu kontrol edilir — bu mevcut hesabın
//        şifresi, yeni bir şifre değil (LoginRequestValidator ile aynı gerekçe).
// BAĞIMLILIKLAR: FluentValidation, OtpRuleExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Validators.Auth;

public class DeleteAccountConfirmRequestValidator : AbstractValidator<DeleteAccountConfirmRequest>
{
    public DeleteAccountConfirmRequestValidator()
    {
        RuleFor(x => x.OtpCode).ValidOtpCode();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty.").WithErrorCode("PASSWORD_REQUIRED");
    }
}
