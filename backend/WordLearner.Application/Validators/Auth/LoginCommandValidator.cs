using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required").WithErrorCode("EMAIL_REQUIRED")
            .EmailAddress().WithMessage("Email is not a valid address").WithErrorCode("EMAIL_INVALID");

        // Şifre GÜCÜ burada kontrol edilmez (RegisterCommandValidator'daki 5 kural) — bu adım var
        // olan bir şifreyi DOĞRULUYOR, YENİ bir şifre BELİRLEMİYOR; boş olmaması yeterli.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required").WithErrorCode("PASSWORD_REQUIRED");
    }
}
