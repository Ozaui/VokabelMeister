// ─────────────────────────────────────────────────────────────────────────────
// IOtpService.cs
//
// AMAÇ: OTP üretimi/doğrulanması/temizlenmesi için EmailVerification/LoginOtp/
//       PasswordReset/AccountDeletion akışlarının paylaştığı ortak sözleşme.
// NEDEN: Bu mantık eskiden AuthService'in private metotlarıydı; MediatR CQRS'e
//        geçişte (Auth API 13 ayrı Command+Handler'a bölündüğünde) her handler
//        birbirini çağıramayacağı için paylaşılan bir servise çıkarıldı.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.User, WordLearner.Domain.Enums.OtpPurpose.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Interfaces.Services;

public interface IOtpService
{
    // NEDEN 5dk: REFERENCE/SECURITY.md §1'de pinlenen OTP geçerlilik süresi (login/kayıt/şifre sıfırlama).
    const int OtpExpiryMinutes = 5;

    // NEDEN 15dk: Hesap silme OTP'si diğerlerinden daha kısa — geri alınamaz bir işlem
    //        olduğu için pencere daraltılır (REFERENCE/API_ENDPOINTS.md §3).
    const int DeleteAccountOtpExpiryMinutes = 15;

    // AMAÇ: 6 haneli rastgele OTP kodu + DB'ye yazılacak hash'ini üretir.
    (string Code, string Hash) Generate();

    // AMAÇ: Bir kullanıcının bekleyen OTP'sini amaç/hash/süre bakımından doğrular;
    //       geçersizse InvalidOtpException fırlatır.
    void Validate(User? user, string otpCode, OtpPurpose expectedPurpose);

    // AMAÇ: Kullanılan/süresi dolan bir OTP'nin alanlarını temizler.
    void Clear(User user);
}
