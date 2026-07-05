// ─────────────────────────────────────────────────────────────────────────────
// EmailRuleExtensions.cs
//
// AMAÇ: E-posta doğrulama kuralını (boş olamaz + geçerli format) tek yerden
//       paylaşan FluentValidation extension metodu.
// NEDEN: 7 farklı Auth validator'ı (Register, VerifyEmail, ResendVerification,
//        Login, VerifyOtp, ForgotPassword, ResetPassword) AYNI e-posta kuralını
//        kullanıyor — kural tekrarını önlemek için ortak metoda alındı.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class EmailRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder
    ) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("E-posta boş olamaz.")
            .WithErrorCode("EMAIL_ZORUNLU")
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi girin.")
            .WithErrorCode("EMAIL_GECERSIZ");
}
