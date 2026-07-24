// ─────────────────────────────────────────────────────────────────────────────
// SelfAdminActionNotAllowedException.cs
//
// AMAÇ: Bir admin kendi rolünü/hesap durumunu değiştirmeye çalıştığında fırlatılır.
// NEDEN: UpdateUserRoleCommand/UpdateUserStatusCommand hedef Id ile isteği yapan
//        adminin Id'si (UserId) AYNIYSA — kaza sonucu kendi rolünü User'a düşürmek
//        veya kendi hesabını dondurmak, TEK admin'li bir sistemde geri dönüşü olmayan
//        bir kilitlenmeye (hiçbir hesap admin işlemi yapamaz hale gelmesine) yol
//        açabilir. 400 döner (varsayılan AppException statüsü) — geçersiz bir istek,
//        çakışma değil.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class SelfAdminActionNotAllowedException : AppException
{
    public SelfAdminActionNotAllowedException()
        : base("CANNOT_MODIFY_OWN_ACCOUNT", "Admin update attempt: an admin cannot change their own role or status.")
    { }
}
