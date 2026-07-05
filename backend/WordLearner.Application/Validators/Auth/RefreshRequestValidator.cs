// ─────────────────────────────────────────────────────────────────────────────
// RefreshRequestValidator.cs
//
// AMAÇ: POST /auth/refresh ve POST /auth/logout girdisinin doğrulama kuralı.
// NEDEN: İkisi de aynı DTO'yu (RefreshRequest) paylaşır — tek validator yeterli.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Validators.Auth;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Token boş olamaz.")
            .WithErrorCode("TOKEN_ZORUNLU");
    }
}
