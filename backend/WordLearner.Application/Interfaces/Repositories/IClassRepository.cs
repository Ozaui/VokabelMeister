/// <summary>
/// IClassRepository.cs
///
/// AMAÇ: Sanal sınıf sorgularını tanımlar — davet kodu ile katılım ve üye listeleme.
/// NEDEN: InviteCode araması ve üye yükleme generic CRUD'un ötesinde sorgular gerektirir.
/// BAĞIMLILIKLAR: IRepository (generic), Class entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Sanal sınıf repository arayüzü.
///
/// AMAÇ: Sınıf oluşturma, katılma ve yönetim sorgularını tanımlamak.
/// NEDEN: Davet kodu ile katılım ve üye istatistikleri özel sorgular gerektirir.
/// </summary>
public interface IClassRepository : IRepository<Class>
{
    /// <summary>
    /// AMAÇ: Davet koduyla sınıfı bulur — öğrenci sınıfa katılırken kullanılır.
    /// NEDEN: Katılım akışında kullanıcı kodu girer, sistem sınıfı bulur.
    /// NASIL: InviteCode UNIQUE kısıtlı, tek sonuç döner; IsActive=true filtresi uygulanır.
    /// </summary>
    Task<Class?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kullanıcının üye olduğu tüm sınıfları getirir.
    /// NEDEN: "Sınıflarım" ekranı hem üyelik hem sahiplik gösterir.
    /// NASIL: ClassMembership tablosunda UserId ile filtreleme yapılır.
    /// </summary>
    Task<IEnumerable<Class>> GetByMemberAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Sınıfı üyeleri ve kategorileriyle birlikte getirir.
    /// NEDEN: Sınıf detay ekranı tüm bilgileri tek sorguda gösterir.
    /// NASIL: ClassMemberships, ClassCategories ve ClassUserCategories eager load edilir.
    /// </summary>
    Task<Class?> GetWithMembersAsync(int classId, CancellationToken ct = default);
}
