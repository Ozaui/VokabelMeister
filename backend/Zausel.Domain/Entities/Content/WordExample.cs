using Zausel.Domain.Entities;
using Zausel.Domain.Enums.Content;

namespace Zausel.Domain.Entities.Content;

// Seviyeli örnek cümle — 1:N Word. PairedExampleId YALNIZCA gerçekten birlikte girildiyse veya admin
// eşleştirme sırasında elle bağladıysa dolar (bkz. Icerik.md) — NULL ise bağımsız bir cümledir.
public class WordExample : BaseEntity
{
    public int WordId { get; set; }
    public string SentenceText { get; set; } = string.Empty;
    public string Level { get; set; } = "A1";
    public ExampleType ExampleType { get; set; } = ExampleType.Normal;
    public int? PairedExampleId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
