namespace WordLearner.Application.Common.Exceptions;

// Hedef Id == isteği yapan adminin Id'si — tek admin'li bir sistemde kaza sonucu kendi
// rolünü/durumunu değiştirmek geri dönüşü olmayan bir kilitlenmeye yol açabilir.
public class SelfAdminActionNotAllowedException : AppException
{
    public SelfAdminActionNotAllowedException()
        : base("CANNOT_MODIFY_OWN_ACCOUNT", "Admin update attempt: an admin cannot change their own role or status.")
    { }
}
