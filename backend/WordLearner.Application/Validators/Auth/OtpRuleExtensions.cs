// ─────────────────────────────────────────────────────────────────────────────
// OtpRuleExtensions.cs
//
// AMAÇ: 6 haneli OTP kodu doğrulama kuralını tek yerden paylaşan FluentValidation
//       extension metodu.
// NEDEN: VerifyEmail/VerifyOtp/ResetPassword/DeleteAccountConfirm validator'ları
//        AYNI OTP format kuralını kullanıyor.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class OtpRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidOtpCode<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Doğrulama kodu boş olamaz.")
            .WithErrorCode("OTP_ZORUNLU")
            .Matches(@"^\d{6}$")
            .WithMessage("Doğrulama kodu 6 haneli olmalı.")
            .WithErrorCode("OTP_FORMAT_GECERSIZ");
}
