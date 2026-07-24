namespace WordLearner.Application.Common.Exceptions;

// 410 Gone — istemci yeni bir QR oturumu başlatmalı.
public class QrSessionGoneException : AppException
{
    public QrSessionGoneException()
        : base("QR_SESSION_GONE", "QR login session expired, consumed, or not in expected state.") { }
}
