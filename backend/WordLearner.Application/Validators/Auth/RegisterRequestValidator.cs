// ─────────────────────────────────────────────────────────────────────────────
// RegisterRequestValidator.cs
//
// AMAÇ: POST /auth/register girdisinin doğrulama kuralları.
// NEDEN: Sunucu tarafı doğrulama her zaman gerekir (CODING_STANDARDS.md §6);
//        client doğrulamasına güvenilmez.
// BAĞIMLILIKLAR: FluentValidation, EmailRuleExtensions, PasswordRuleExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Ad boş olamaz.")
            .WithErrorCode("AD_ZORUNLU")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Soyad boş olamaz.")
            .WithErrorCode("SOYAD_ZORUNLU")
            .MaximumLength(50);
    }
}
