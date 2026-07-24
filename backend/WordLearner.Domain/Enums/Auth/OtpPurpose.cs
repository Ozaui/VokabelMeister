namespace WordLearner.Domain.Enums.Auth;

// User tablosundaki tek OTP alan seti (PendingOtpCodeHash/ExpiresAt) birden fazla akışta
// paylaşılır; bu enum hangi kodun hangi işlem için üretildiğini ayırt eder.
public enum OtpPurpose
{
    EmailVerification,
    LoginOtp,
    PasswordReset,
    AccountDeletion,
}
