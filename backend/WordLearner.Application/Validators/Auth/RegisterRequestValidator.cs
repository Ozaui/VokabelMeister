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
            .WithMessage("First name must not be empty.")
            .WithErrorCode("FIRST_NAME_REQUIRED")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name must not be empty.")
            .WithErrorCode("LAST_NAME_REQUIRED")
            .MaximumLength(50);
    }
}
