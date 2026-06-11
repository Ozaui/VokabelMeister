/// <summary>
/// Friendship.cs
///
/// AMAÇ:
///   İki kullanıcı arasındaki arkadaşlık ilişkisini yönetir.
///   İstek → Onay → Kabul/Ret akışını destekler.
///
/// NEDEN:
///   Arkadaşlar birbirinin paylaşıma açık kategorilerini görebilir.
///   Sosyal motivasyon: Arkadaşların ilerlemesini takip etmek.
///
/// KISIT:
///   RequesterId != ReceiverId (kendine istek gönderilemez) — DB CHECK constraint ile korunur.
///   RequesterId + ReceiverId çifti UNIQUE'tir — aynı yönde çift istek gönderilemez.
///
/// BAĞIMLILIKLAR:
///   - User (N:1, Requester — isteği gönderen)
///   - User (N:1, Receiver — isteği alan)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Arkadaşlık ilişkisi entity'si.
///
/// AMAÇ: İki kullanıcı arasındaki arkadaşlık isteği ve durumunu saklamak.
/// NEDEN BaseEntity'den miras almaz: Soft delete yoktur; manuel silme veya Status değişikliği yeterlidir.
/// </summary>
public class Friendship
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Arkadaşlık isteğini gönderen kullanıcı ID'si (FK → Users)</summary>
    public int RequesterId { get; set; }

    /// <summary>Arkadaşlık isteğini alan kullanıcı ID'si (FK → Users)</summary>
    public int ReceiverId { get; set; }

    /// <summary>
    /// Arkadaşlık durumu: Pending | Accepted | Rejected | Blocked
    /// Pending  : İstek gönderildi, henüz yanıt bekleniyor
    /// Accepted : Karşılıklı arkadaş olundu
    /// Rejected : İstek reddedildi (tekrar gönderilebilir)
    /// Blocked  : Kullanıcı engellendi (bir daha istek gönderilemez)
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>İsteğin gönderildiği tarih (UTC)</summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Son durum değişikliği tarihi (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>İsteği gönderen kullanıcı (N:1)</summary>
    public User Requester { get; set; } = null!;

    /// <summary>İsteği alan kullanıcı (N:1)</summary>
    public User Receiver { get; set; } = null!;
}
