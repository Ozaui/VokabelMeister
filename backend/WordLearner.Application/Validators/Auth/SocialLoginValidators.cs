// ─────────────────────────────────────────────────────────────────────────────
// SocialLoginValidators.cs
//
// AMAÇ: POST /auth/google ve POST /auth/apple girdilerinin doğrulama kuralları.
// NEDEN: İkisi de yalnızca bir token'ın doluluğunu kontrol eder — asıl doğrulama
//        (imza/audience/süre) IGoogleTokenValidator/IAppleTokenValidator'da yapılır,
//        burada yalnızca boş gönderilmediği garanti edilir.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Validators.Auth;

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().WithMessage("Token must not be empty.").WithErrorCode("TOKEN_REQUIRED");
    }
}

public class AppleLoginRequestValidator : AbstractValidator<AppleLoginRequest>
{
    public AppleLoginRequestValidator()
    {
        RuleFor(x => x.IdentityToken)
            .NotEmpty()
            .WithMessage("Token must not be empty.")
            .WithErrorCode("TOKEN_REQUIRED");
    }
}
