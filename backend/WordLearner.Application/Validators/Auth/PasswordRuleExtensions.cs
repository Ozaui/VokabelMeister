// ─────────────────────────────────────────────────────────────────────────────
// PasswordRuleExtensions.cs
//
// AMAÇ: Şifre gücü kurallarını (REFERENCE/SECURITY.md §1) tek bir yerden
//       paylaşan FluentValidation extension metodu.
// NEDEN: RegisterRequestValidator ve ResetPasswordRequestValidator AYNI şifre
//        kurallarını uyguluyor — kural tekrarını önlemek için ortak metoda alındı.
//        Her kural hem WithMessage (sabit Türkçe, yalnızca log/geliştirici içindir)
//        hem WithErrorCode (ValidationFilter'ın ErrorMessages'ten dile göre
//        çözeceği kod) taşır — AppException ile birebir aynı ayrım.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class PasswordRuleExtensions
{
    // AMAÇ: Min 12 karakter + büyük/küçük harf + rakam + özel karakter kuralını uygular.
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder
    ) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Şifre boş olamaz.")
            .WithErrorCode("SIFRE_ZORUNLU")
            .MinimumLength(12) // NEDEN: brute-force direnci
            .WithMessage("Şifre en az 12 karakter olmalı.")
            .WithErrorCode("SIFRE_KISA")
            .Matches(@"[A-Z]")
            .WithMessage("Şifre en az 1 büyük harf içermeli.")
            .WithErrorCode("SIFRE_BUYUK_HARF_EKSIK")
            .Matches(@"[a-z]")
            .WithMessage("Şifre en az 1 küçük harf içermeli.")
            .WithErrorCode("SIFRE_KUCUK_HARF_EKSIK")
            .Matches(@"[0-9]")
            .WithMessage("Şifre en az 1 rakam içermeli.")
            .WithErrorCode("SIFRE_RAKAM_EKSIK")
            .Matches(@"[!@#$%^&*]")
            .WithMessage("Şifre en az 1 özel karakter içermeli (!@#$%^&*).")
            .WithErrorCode("SIFRE_OZEL_KARAKTER_EKSIK");
}
