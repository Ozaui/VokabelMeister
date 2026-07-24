namespace WordLearner.Domain.Enums.Auth;

public enum QrLoginStatus
{
    Pending,
    Scanned,
    Confirmed,

    // Web tarafı token'ları bir kez okudu; oturum artık tekrar kullanılamaz.
    Consumed,

    Denied,
    Expired,
}
