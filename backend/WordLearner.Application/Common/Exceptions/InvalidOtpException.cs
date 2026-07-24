namespace WordLearner.Application.Common.Exceptions;

// Yanlış/süresi dolmuş kod aynı mesajı döner (InvalidCredentialsException'daki gerekçeyle aynı).
public class InvalidOtpException : AppException
{
    public InvalidOtpException()
        : base("INVALID_OTP", "OTP verification: code invalid or expired.") { }
}
