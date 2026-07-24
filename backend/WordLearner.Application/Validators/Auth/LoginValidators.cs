using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();

        // Şifre GÜCÜ değil yalnızca doluluğu kontrol edilir — kural sonradan sıkılaşmış olabilir.
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty.").WithErrorCode("PASSWORD_REQUIRED");
    }
}

public class VerifyLoginOtpCommandValidator : AbstractValidator<VerifyLoginOtpCommand>
{
    public VerifyLoginOtpCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.OtpCode).ValidOtpCode();
    }
}
