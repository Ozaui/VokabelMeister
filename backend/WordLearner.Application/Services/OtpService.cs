// ─────────────────────────────────────────────────────────────────────────────
// OtpService.cs
//
// AMAÇ: IOtpService'in implementasyonu — AuthService'in eski private
//       GenerateOtp/ValidateOtp/ClearOtp metotlarının birebir taşınmış hâli.
// NEDEN: Register/ResendVerification/Login/VerifyLoginOtp/ForgotPassword/
//        ResetPassword/RequestAccountDeletion/ConfirmAccountDeletion Command
//        Handler'larının HEPSİ aynı OTP mantığını paylaşır — handler'lar
//        birbirini MediatR üzerinden çağırmadığı için bu mantık paylaşılan
//        bir servise çıkarıldı (bkz. wiki INGEST notu).
// BAĞIMLILIKLAR: IPasswordService (HashToken).
// ─────────────────────────────────────────────────────────────────────────────

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

    // AMAÇ: 6 haneli rastgele OTP kodu + DB'ye yazılacak hash'ini üretir.
    // NEDEN: RandomNumberGenerator kriptografik olarak güvenli rastgelelik sağlar
    //        (Random sınıfı tahmin edilebilir olduğu için OTP üretiminde kullanılmaz).
    public (string Code, string Hash) Generate()
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        return (code, _passwordService.HashToken(code));
    }

    // AMAÇ: Bir kullanıcının bekleyen OTP'sini amaç/hash/süre bakımından doğrular.
    // NEDEN: EmailVerification/LoginOtp/PasswordReset/AccountDeletion akışlarının HEPSİ
    //        aynı doğrulama mantığını paylaşır — kod tekrarını önler.
    //        NOT: "3 yanlış deneme → kod geçersiz" sayacı SecurityLog'a bağımlı olduğu
    //        için A-04 (loglama) tamamlandıktan sonra buraya entegre edilecek — bkz.
    //        TASK/A_admin_panel_backend.md A-03'ün sonundaki not.
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

    // AMAÇ: Kullanılan/süresi dolan bir OTP'nin alanlarını temizler.
    public void Clear(User user)
    {
        user.PendingOtpCodeHash = null;
        user.PendingOtpCodeExpiresAt = null;
        user.PendingOtpCodePurpose = null;
    }
}
