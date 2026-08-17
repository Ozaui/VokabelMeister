namespace Zausel.Application.DTOs;

// Unit döndüren Command'ların (Login adım 1, ResendVerification, VerifyEmail, Logout,
// ForgotPassword, ResetPassword, RequestAccountDeletion, ConfirmAccountDeletion, QR Confirm/Deny)
// istemciye giden ortak yanıt şekli — ApiErrorResponse'un başarı karşılığı.
public record MessageResponse(string Message);
