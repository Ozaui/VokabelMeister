/// <summary>
/// IClassWordRepository.cs
///
/// AMAÇ: Sınıfa özel kelime sorgularını tanımlar — listeleme, yetki kontrolü, CRUD.
/// NEDEN:
///   ClassWord, sistem Words tablosundan farklıdır; görünürlük sınıf üyeliğiyle sınırlıdır.
///   Tüm sorgularda ClassId + üyelik kontrolü zorunludur.
/// BAĞIMLILIKLAR: IRepository (generic), ClassWord entity, Class entity
/// </summary>
using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Sınıfa özel kelime repository arayüzü.
///
/// AMAÇ: Instructor'ın sınıfına özel kelimelerini yönetmek için sorgu sözleşmesini tanımlamak.
/// NEDEN: ClassWord görünürlüğü sınıf üyeliğiyle kısıtlıdır; her sorguda ClassId filtresi zorunludur.
/// </summary>
public interface IClassWordRepository : IRepository<ClassWord>
{
    /// <summary>
    /// AMAÇ: Belirtilen sınıfa ait tüm aktif kelimeleri sayfalı getirir.
    /// NEDEN: Sınıf detay sayfası ve öğrenme oturumu için kullanılır.
    /// NASIL: ClassId filtresi + IsActive=true + soft delete filtresi uygulanır.
    /// </summary>
    Task<IEnumerable<ClassWord>> GetByClassAsync(int classId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kullanıcının üye olduğu tüm sınıflara ait kelimeleri getirir.
    /// NEDEN: Öğrenme oturumunda "sınıf kelimeleri" kaynağı seçildiğinde kullanılır.
    /// NASIL: ClassMembership tablosundan kullanıcının sınıfları alınır; o sınıfların ClassWords'leri döner.
    /// </summary>
    Task<IEnumerable<ClassWord>> GetByMemberAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Belirli bir kullanıcının belirtilen sınıfın sahibi olup olmadığını doğrular.
    /// NEDEN: ClassWord ekleme/güncelleme/silme işlemlerinde yetki kontrolü için kullanılır.
    /// NASIL: Class.OwnerId == userId kontrolü yapılır.
    /// </summary>
    Task<bool> IsClassOwnerAsync(int classId, int userId, CancellationToken ct = default);
}
