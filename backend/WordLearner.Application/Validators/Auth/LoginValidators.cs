// ─────────────────────────────────────────────────────────────────────────────
// LoginValidators.cs
//
// AMAÇ: POST /auth/login ve POST /auth/login/verify-otp girdilerinin doğrulama
//       kuralları.
// NEDEN: 2 adımlı OTP login akışının iki adımı, tek dosyada toplandı
//        (DTOs/Auth/LoginDtos.cs ile aynı gruplama).
// BAĞIMLILIKLAR: FluentValidation, EmailRuleExtensions, OtpRuleExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();

        // NEDEN: Login'de şifre GÜCÜ değil yalnızca doluluğu kontrol edilir —
        //        zaten kayıtlı bir şifrenin bugünkü kurallara uyup uymadığını
        //        burada yeniden zorlamak yanlış (kural sonradan sıkılaşmış olabilir).
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password must not be empty.").WithErrorCode("PASSWORD_REQUIRED");
    }
}

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.OtpCode).ValidOtpCode();
    }
}
