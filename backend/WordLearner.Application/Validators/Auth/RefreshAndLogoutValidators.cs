// ─────────────────────────────────────────────────────────────────────────────
// RefreshAndLogoutValidators.cs
//
// AMAÇ: POST /auth/refresh (RefreshCommand) ve POST /auth/logout (LogoutCommand)
//       girdilerinin doğrulama kuralları.
// NEDEN: İkisi de yalnızca ham refresh token'ın doluluğunu kontrol eder. Eskiden
//        aynı RefreshRequest DTO'sunu paylaştıkları için tek validator yeterliydi;
//        MediatR CQRS'e geçişte ayrı Command tiplerine bölündükleri için (bkz.
//        RefreshCommand.cs/LogoutCommand.cs'teki not) artık iki ayrı sınıf, ama
//        aynı kural (RefreshTokenRuleExtensions.ValidRefreshToken) paylaşılıyor.
// BAĞIMLILIKLAR: FluentValidation, RefreshTokenRuleExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.Features.Auth;

namespace WordLearner.Application.Validators.Auth;

public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken();
    }
}

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).ValidRefreshToken();
    }
}
