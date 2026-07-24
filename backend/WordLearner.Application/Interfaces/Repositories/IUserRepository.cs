// ─────────────────────────────────────────────────────────────────────────────
// IUserRepository.cs
//
// AMAÇ: User'a özel sorguları (e-posta/GoogleId/AppleId ile arama, e-posta
//       benzersizlik kontrolü, admin liste/istatistik) IRepository<T>'nin genel CRUD'una ekler.
// NEDEN: AuthService, User entity'sini yalnızca Id ile değil e-posta/sosyal kimlikle
//        de bulmak zorunda — bu metotlar IRepository<T>'de olmadığı için genişletilir.
//        GetPagedAsync A-07'de (Admin API, Kullanıcı Yönetimi dilimi) eklendi.
//        GetStatisticsAsync/GetRegistrationDatesAsync A-07'nin istatistik diliminde
//        eklendi — ikisi de GetAdminStatisticsQuery'nin gerçek tüketicisi (bu ikisi
//        DAHA ÖNCE tüketicisiz yazılıp kod denetiminde geri alınmıştı, bkz. TASK notu).
// BAĞIMLILIKLAR: IRepository<T>, WordLearner.Domain.Entities.User, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    // AMAÇ: `GET /admin/users` — arama (Email/FirstName/LastName) + role filtresiyle
    //       sayfalı kullanıcı listesi. Soft-delete filtresi NORMAL uygulanır
    //       (GetByEmailAsync'in aksine) — admin panel silinmiş/anonimleştirilmiş
    //       hesapları bu genel listede GÖRMEZ, yalnızca aktif hesap tabanını yönetir.
    Task<PagedResult<User>> GetPagedAsync(
        string? search,
        string? role,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    // AMAÇ: `GET /admin/statistics` — genel sayaçlar (toplam/aktif/dondurulmuş kullanıcı).
    // NEDEN ayrı bir metot (GetPagedAsync'in TotalCount'undan türetilmez): "dondurulmuş"
    //       sayısı ayrı bir COUNT sorgusu gerektirir, tek bir GetPagedAsync çağrısıyla
    //       elde edilemez.
    Task<(int TotalUsers, int ActiveUsers, int FrozenUsers)> GetStatisticsAsync(CancellationToken ct = default);

    // AMAÇ: `GET /admin/statistics`'in kayıt grafiği — `fromUtc` (dahil) ve sonrasındaki
    //       her User.CreatedAt değerini HAM olarak döner; günlere GRUPLAMA Handler'da
    //       (bellekte) yapılır.
    // NEDEN Repository'de gruplanmıyor: EF Core'un SQL Server sağlayıcısı `DateTime.Date`
    //       gibi bir gruplama ifadesini HER ZAMAN güvenilir şekilde SQL'e çeviremiyor
    //       (CategoryRepository'nin "bellekte filtre" kararıyla AYNI gerekçe, bkz.
    //       ICategoryRepository.cs); bu sorgunun döndürdüğü satır sayısı (son 30 günün
    //       kayıtları) admin panel için KÜÇÜK bir veri, bellekte gruplamak güvenli.
    Task<IReadOnlyList<DateTime>> GetRegistrationDatesAsync(DateTime fromUtc, CancellationToken ct = default);

    // AMAÇ: E-postaya göre kullanıcı bulur — soft delete filtresini YOK SAYAR.
    // NEDEN: Grace period (30 gün) içindeki soft-delete'li bir hesap login/register
    //        akışlarında hâlâ görülebilmeli (hesap kurtarma, tekrar kayıt engeli için) —
    //        global soft delete filtresi bu senaryoda bilerek bypass edilir; IsDeleted/
    //        IsAnonymized kontrolü çağıran (AuthService) tarafında yapılır.
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    // AMAÇ: Id'ye göre kullanıcı bulur — soft delete filtresini YOK SAYAR.
    // NEDEN: GetByIdAsync (Repository<T>'den miras) soft-delete filtresi uygular;
    //        bu, grace-period içindeki (soft-delete'li) bir kullanıcının Id'siyle
    //        bulunması gereken akışlarda (ör. QR ile giriş tamamlanırken
    //        ILoginCompletionService.CompleteLoginAsync'in IsDeleted kurtarma
    //        mantığına ulaşabilmesi için) GetByEmailAsync ile aynı gerekçeyle var.
    Task<User?> GetByIdIncludingDeletedAsync(int id, CancellationToken ct = default);

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
