using WordLearner.Application.DTOs.Auth;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Services;

// OTP/Google/Apple girişlerinin ortak son adımı — MediatR handler'ları birbirini
// çağıramadığı için üçünün paylaştığı mantık buraya çıkarıldı.
public interface ILoginCompletionService
{
    // language yalnızca "hesabınız geri alındı" bilgilendirme e-postasının dili için gerekir.
    Task<AuthTokenResponse> CompleteLoginAsync(
        User user,
        string? ipAddress,
        string? language,
        CancellationToken ct = default
    );

    // RefreshCommandHandler de aynı hesaplamaya (CompleteLoginAsync çağırmadan) ihtiyaç duyar.
    int ExpiresInSeconds();
}
