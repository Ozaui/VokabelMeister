// ─────────────────────────────────────────────────────────────────────────────
// ILoginCompletionService.cs
//
// AMAÇ: OTP doğrulama/Google/Apple girişlerinin ORTAK son adımını (grace period
//       kurtarma, anonimleştirme kontrolü, giriş istatistikleri, token üretimi)
//       tek bir yerden sağlayan sözleşme.
// NEDEN: VerifyLoginOtpCommandHandler/LoginWithGoogleCommandHandler/
//        LoginWithAppleCommandHandler'ın üçü de aynı mantığı kullanır; MediatR
//        handler'ları birbirini çağırmadığı için bu ortak servise çıkarıldı
//        (bkz. wiki INGEST notu).
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.User, DTOs.Auth.AuthTokenResponse.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.DTOs.Auth;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Services;

public interface ILoginCompletionService
{
    // AMAÇ: Grace period kurtarma + giriş istatistikleri + access/refresh token üretimi.
    Task<AuthTokenResponse> CompleteLoginAsync(User user, string? ipAddress, CancellationToken ct = default);

    // AMAÇ: appsettings.json'daki Jwt:ExpirationMinutes'i saniyeye çevirir.
    // NEDEN: RefreshCommandHandler de aynı hesaplamaya ihtiyaç duyar (CompleteLoginAsync
    //        çağırmadan, kendi token rotation akışında) — tekrar yazılmasın diye burada
    //        public olarak açığa çıkarıldı.
    int ExpiresInSeconds();
}
