using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Interfaces.Services;

// Register/Login/ResetPassword/AccountDeletion'ın ortak OTP üretim/doğrulama mantığı — User'ın
// Pending* alanlarını mutasyona uğratır, DB'ye YAZMAZ (persist çağıran Handler'ın işi, ITokenService/
// IPasswordService ile aynı "servis saf mantık taşır" deseni).
public interface IOtpService
{
    string Generate(User user, OtpPurpose purpose);
    OtpVerificationResult Verify(User user, string code, OtpPurpose purpose);
}
