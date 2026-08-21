using Zausel.Domain.Entities;

namespace Zausel.Domain.Entities.PersonalContent;

// Kullanıcının kendi kartlarını gruplamak için kişisel kategori — sistem Category'sinden (Content
// domain'i) bağımsız, çeviri taşımaz (kullanıcı tek dilde kendi ismini yazar).
public class UserCategory : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}
