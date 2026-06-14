/// <summary>
/// AuditLog.cs
///
/// AMAÇ:
///   Sistemde gerçekleşen önemli olayları (giriş, kayıt, içerik değişikliği, silme vb.)
///   kalıcı olarak kaydeden denetim izi tablosu.
///
/// NEDEN:
///   Güvenlik analizi: Başarısız giriş denemeleri, şüpheli aktivite tespiti.
///   Compliance: Kim ne zaman ne yaptı? sorusunun cevabı her zaman mevcut olmalı.
///   Hata ayıklama: Production'da bir sorun çıktığında geçmişe bakılabilir.
///
/// NOT:
///   Id tipi BIGINT (long) — yüksek hacimli kayıt için int yetmez.
///   Bu tablo ASLA güncellenmez ve silinmez (append-only).
///
/// BAĞIMLILIKLAR:
///   - User (N:1, NULL olabilir — sistem olayları veya silinmiş kullanıcılar için)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sistem denetim izi (audit log) entity'si.
///
/// AMAÇ: Tüm kritik sistem olaylarını değiştirilemez biçimde kayıt altına almak.
/// NEDEN BaseEntity'den miras almaz: Id tipi long'dur (BIGINT); soft delete ve UpdatedAt yoktur.
/// </summary>
public class AuditLog
{
    /// <summary>Birincil anahtar (BIGINT — yüksek hacim için)</summary>
    public long Id { get; set; }

    /// <summary>
    /// Olayı gerçekleştiren kullanıcı ID'si (NULL olabilir).
    /// NEDEN NULL: Kayıt olmayan sistem olayları veya kullanıcı silinmiş olabilir.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Olay türü (max 100 karakter).
    /// ÖRNEKLER: LOGIN, REGISTER, CREATE_WORD, DELETE_USER, PASSWORD_CHANGE, ROLE_CHANGE
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Etkilenen tablo adı (max 50 karakter).
    /// ÖRNEK: "Users", "Words", "UserCards"
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>Etkilenen kaydın ID'si</summary>
    public int? RecordId { get; set; }

    /// <summary>Değişiklik öncesi değer (JSON formatında, max metin)</summary>
    public string? OldValue { get; set; }

    /// <summary>Değişiklik sonrası değer (JSON formatında, max metin)</summary>
    public string? NewValue { get; set; }

    /// <summary>Olayın gerçekleştiği IP adresi (max 45 karakter)</summary>
    public string? IpAddress { get; set; }

    /// <summary>HTTP User-Agent bilgisi (tarayıcı/uygulama)</summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Olay sonucu: Success | Failure
    /// NEDEN: Başarısız girişimler güvenlik analizinde önemlidir.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>Olayın gerçekleştiği tarih (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Olayı gerçekleştiren kullanıcı (N:1, opsiyonel)</summary>
    public User? User { get; set; }
}
