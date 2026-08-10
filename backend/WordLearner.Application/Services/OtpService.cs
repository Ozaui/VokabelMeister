using System.Security.Cryptography;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Services;

public class OtpService : IOtpService
{
    private const int CodeLength = 6;
    private const int ExpirationMinutes = 5;
    private const int MaxAttempts = 3;

    private readonly IPasswordService _passwordService;

    public OtpService(IPasswordService passwordService) => _passwordService = passwordService;

    public string Generate(User user, OtpPurpose purpose)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString($"D{CodeLength}");

        user.PendingOtpCodeHash = _passwordService.HashToken(code);
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        user.PendingOtpCodePurpose = purpose;
        user.PendingOtpCodeAttempts = 0;

        return code;
    }

    public OtpVerificationResult Verify(User user, string code, OtpPurpose purpose)
    {
        if (user.PendingOtpCodeHash is null || user.PendingOtpCodePurpose != purpose)
            return OtpVerificationResult.InvalidCode;

        if (user.PendingOtpCodeExpiresAt is null || user.PendingOtpCodeExpiresAt < DateTime.UtcNow)
        {
            ClearPendingOtp(user);
            return OtpVerificationResult.Expired;
        }

        if (user.PendingOtpCodeHash != _passwordService.HashToken(code))
        {
            user.PendingOtpCodeAttempts++;
            // 3. yanlış denemede kod tamamen temizlenir — kullanıcı doğru kodu bilse bile artık geçersiz.
            if (user.PendingOtpCodeAttempts >= MaxAttempts)
                ClearPendingOtp(user);

            return OtpVerificationResult.InvalidCode;
        }

        ClearPendingOtp(user);
        return OtpVerificationResult.Success;
    }

    private static void ClearPendingOtp(User user)
    {
        user.PendingOtpCodeHash = null;
        user.PendingOtpCodeExpiresAt = null;
        user.PendingOtpCodePurpose = null;
        user.PendingOtpCodeAttempts = 0;
    }
}
