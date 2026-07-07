// ─────────────────────────────────────────────────────────────────────────────
// RefreshTokenRuleExtensions.cs
//
// AMAÇ: Refresh token doluluk kuralını tek yerden paylaşan FluentValidation
//       extension metodu.
// NEDEN: RefreshCommand ve LogoutCommand eskiden aynı RefreshRequest DTO'sunu
//        paylaşıyordu (tek validator yeterliydi); MediatR'a geçişte ayrı
//        Command tiplerine bölündüler (bkz. RefreshCommand.cs/LogoutCommand.cs'teki
//        not) — kural tekrarını önlemek için EmailRuleExtensions/OtpRuleExtensions
//        ile aynı desende ortak metoda alındı.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;

namespace WordLearner.Application.Validators.Auth;

public static class RefreshTokenRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidRefreshToken<T>(
        this IRuleBuilder<T, string> ruleBuilder
    ) =>
        ruleBuilder
            .NotEmpty()
            .WithMessage("Token must not be empty.")
            .WithErrorCode("TOKEN_REQUIRED");
}
