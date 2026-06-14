/// <summary>
/// ClassRepository.cs
///
/// AMAÇ: Sanal sınıf sorgularının implementasyonu — davet kodu ve üye listeleme.
/// NEDEN: InviteCode araması ve üye yükleme generic CRUD'a ek operasyonlar.
/// BAĞIMLILIKLAR: Repository&lt;T&gt; (base), IClassRepository (Application), WordLearnerDbContext
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Social;

/// <summary>
/// Sanal sınıf repository implementasyonu.
///
/// AMAÇ: IClassRepository sözleşmesini karşılamak.
/// NEDEN: Davet kodu ile katılım ve üye istatistikleri özel sorgular gerektirir.
/// </summary>
public class ClassRepository : Repository<Class>, IClassRepository
{
    public ClassRepository(WordLearnerDbContext db) : base(db) { }

    /// <summary>
    /// AMAÇ: Davet koduyla aktif sınıfı bulur — öğrenci katılım akışı.
    /// NEDEN: Kullanıcı kodu girer; sistem sınıfı bulur ve üye ekler.
    /// NASIL: InviteCode UNIQUE kısıt — tek sonuç beklenir; IsActive=true filtresi.
    /// </summary>
    public Task<Class?> GetByInviteCodeAsync(string inviteCode, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(c => c.InviteCode == inviteCode && c.IsActive, ct);

    /// <summary>
    /// AMAÇ: Kullanıcının üye olduğu tüm sınıfları getirir.
    /// NEDEN: "Sınıflarım" ekranı — hem üye hem sahip olduğu sınıflar listelenir.
    /// NASIL: ClassMembership tablosunda UserId filtresi; sınıf bilgisi Include ile yüklenir.
    /// </summary>
    public async Task<IEnumerable<Class>> GetByMemberAsync(int userId, CancellationToken ct = default)
        => await _db.ClassMemberships
            .Where(m => m.UserId == userId)
            .Include(m => m.Class)
            .Select(m => m.Class)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Sınıfı üyeleri ve kategorileriyle getirir — sınıf detay ekranı için.
    /// NEDEN: Tek sorguda sınıf bilgisi + üyeler + atanan kategoriler gerekli.
    /// NASIL: Include zinciri ile eager loading; ClassMemberships.User dahil edilir.
    /// </summary>
    public Task<Class?> GetWithMembersAsync(int classId, CancellationToken ct = default)
        => _set
            .Include(c => c.ClassMemberships)
                .ThenInclude(m => m.User)
            .Include(c => c.ClassCategories)
                .ThenInclude(cc => cc.Category)
            .Include(c => c.ClassUserCategories)
                .ThenInclude(cuc => cuc.UserCategory)
            .FirstOrDefaultAsync(c => c.Id == classId, ct);
}
