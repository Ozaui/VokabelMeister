using Zausel.Domain.Entities;

namespace Zausel.Domain.Entities.PersonalContent;

// Kullanıcının kendi eklediği kelime kartı — sistem Word'ünden bağımsız, tek kullanıcıya ait.
// Ses: ayrı bir AudioUrl sütunu yok, istemci TTS'i FrontText/BackText üzerinden doğrudan çalışır.
public class UserCard : BaseEntity
{
    public int UserId { get; set; }
    public string FrontText { get; set; } = string.Empty;
    public string BackText { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
