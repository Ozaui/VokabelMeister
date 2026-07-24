using System.Security.Cryptography;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Services;

public class OtpService : IOtpService
{
    private readonly IPasswordService _passwordService;

    public OtpService(IPasswordService passwordService) => _passwordService = passwordService;

    // RandomNumberGenerator kriptografik olarak güvenli rastgelelik sağlar — Random sınıfı
    // tahmin edilebilir olduğu için OTP üretiminde kullanılmaz.
    public (string Code, string Hash) Generate()
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        return (code, _passwordService.HashToken(code));
    }

    public void Validate(User? user, string otpCode, OtpPurpose expectedPurpose)
    {
        var isValid =
            user is not null
            && user.PendingOtpCodePurpose == expectedPurpose
            && user.PendingOtpCodeExpiresAt is not null
            && user.PendingOtpCodeExpiresAt >= DateTime.UtcNow
            && user.PendingOtpCodeHash == _passwordService.HashToken(otpCode);

        if (!isValid)
            throw new InvalidOtpException();
    }

    public void Clear(User user)
    {
        user.PendingOtpCodeHash = null;
        user.PendingOtpCodeExpiresAt = null;
        user.PendingOtpCodePurpose = null;
    }
}
