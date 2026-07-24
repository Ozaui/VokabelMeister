using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Interfaces.Services;

public interface IOtpService
{
    const int OtpExpiryMinutes = 5;

    // Hesap silme geri alınamaz bir işlem olduğu için pencere daraltılır.
    const int DeleteAccountOtpExpiryMinutes = 15;

    (string Code, string Hash) Generate();
    void Validate(User? user, string otpCode, OtpPurpose expectedPurpose);
    void Clear(User user);
}
