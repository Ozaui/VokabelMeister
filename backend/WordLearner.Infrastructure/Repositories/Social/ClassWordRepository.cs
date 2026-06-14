/// <summary>
/// ClassWordRepository.cs
///
/// AMAÇ: Sınıfa özel kelime sorgularının implementasyonu.
/// NEDEN:
///   ClassWord yalnızca sınıf üyelerine görünür; her sorguda ClassId filtresi zorunludur.
///   IsClassOwnerAsync ile ekleme/güncelleme/silme öncesi yetki doğrulanır.
/// BAĞIMLILIKLAR: Repository&lt;T&gt; (base), IClassWordRepository (Application), WordLearnerDbContext
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Social;

/// <summary>
/// Sınıfa özel kelime repository implementasyonu.
///
/// AMAÇ: IClassWordRepository sözleşmesini karşılamak.
/// NEDEN: Görünürlük sınıf üyeliğiyle kısıtlı olduğundan özel sorgular gerekir.
/// </summary>
public class ClassWordRepository : Repository<ClassWord>, IClassWordRepository
{
    public ClassWordRepository(WordLearnerDbContext db) : base(db) { }

    /// <summary>
    /// AMAÇ: Belirtilen sınıfa ait tüm aktif kelimeleri getirir.
    /// NEDEN: Sınıf detay ekranı ve öğrenme oturumu bu listeyi kullanır.
    /// NASIL: ClassId filtresi + IsActive=true; soft delete filtresi global query filter ile otomatik uygulanır.
    /// </summary>
    public async Task<IEnumerable<ClassWord>> GetByClassAsync(int classId, CancellationToken ct = default)
        => await _set
            .Where(cw => cw.ClassId == classId && cw.IsActive)
            .OrderBy(cw => cw.GermanWord)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Kullanıcının üye olduğu tüm sınıfların kelimelerini getirir.
    /// NEDEN: "Sınıf kelimeleriyle öğren" seçeneği tüm üyelik sınıflarının kelimelerini getirir.
    /// NASIL: ClassMembership üzerinden kullanıcının sınıfları bulunur; Join ile ClassWords alınır.
    /// </summary>
    public async Task<IEnumerable<ClassWord>> GetByMemberAsync(int userId, CancellationToken ct = default)
        => await _db.ClassMemberships
            .Where(m => m.UserId == userId && m.IsActive)
            .Join(
                _set.Where(cw => cw.IsActive),
                m => m.ClassId,
                cw => cw.ClassId,
                (m, cw) => cw
            )
            .OrderBy(cw => cw.GermanWord)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Kullanıcının belirtilen sınıfın sahibi olup olmadığını kontrol eder.
    /// NEDEN: ClassWord CRUD işlemlerinde yetki doğrulaması için servis katmanı tarafından çağrılır.
    /// NASIL: Classes tablosunda OwnerId = userId ve Id = classId kontrolü yapılır.
    /// </summary>
    public Task<bool> IsClassOwnerAsync(int classId, int userId, CancellationToken ct = default)
        => _db.Classes.AnyAsync(c => c.Id == classId && c.OwnerId == userId, ct);
}
