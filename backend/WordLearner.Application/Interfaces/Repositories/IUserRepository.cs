// ─────────────────────────────────────────────────────────────────────────────
// IUserRepository.cs
//
// AMAÇ: User'a özel sorguları (e-posta/GoogleId/AppleId ile arama, e-posta
//       benzersizlik kontrolü) IRepository<T>'nin genel CRUD'una ekler.
// NEDEN: AuthService, User entity'sini yalnızca Id ile değil e-posta/sosyal kimlikle
//        de bulmak zorunda — bu metotlar IRepository<T>'de olmadığı için genişletilir.
// BAĞIMLILIKLAR: IRepository<T>, WordLearner.Domain.Entities.User.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    // AMAÇ: E-postaya göre kullanıcı bulur — soft delete filtresini YOK SAYAR.
    // NEDEN: Grace period (30 gün) içindeki soft-delete'li bir hesap login/register
    //        akışlarında hâlâ görülebilmeli (hesap kurtarma, tekrar kayıt engeli için) —
    //        global soft delete filtresi bu senaryoda bilerek bypass edilir; IsDeleted/
    //        IsAnonymized kontrolü çağıran (AuthService) tarafında yapılır.
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    // AMAÇ: Google Sign-In sub (GoogleId) değerine göre kullanıcı bulur.
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);

    // AMAÇ: Apple Sign-In sub (AppleId) değerine göre kullanıcı bulur.
    Task<User?> GetByAppleIdAsync(string appleId, CancellationToken ct = default);

    // AMAÇ: Verilen SHA-256 e-posta hash'i, daha önce anonimleştirilmiş bir hesabın
    //       OriginalEmailHash'iyle eşleşiyor mu kontrol eder.
    // NEDEN: Silinip anonimleştirilmiş bir hesabın e-postasıyla tekrar kayıt açılmasını
    //        engellemek için (REFERENCE/SECURITY.md §9).
    Task<bool> OriginalEmailHashExistsAsync(string emailHash, CancellationToken ct = default);
}
