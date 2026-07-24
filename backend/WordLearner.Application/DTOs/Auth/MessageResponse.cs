namespace WordLearner.Application.DTOs.Auth;

// Code, ApiErrorDetail(Code, Message) ile simetriktir — Handler Code'u sabit üretir
// (ör. "OTP_SENT"), Message isteğin diline göre SuccessMessages.Resolve ile çözülür.
public record MessageResponse(string Code, string Message);
