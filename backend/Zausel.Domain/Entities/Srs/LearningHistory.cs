namespace Zausel.Domain.Entities.Srs;

// BaseEntity'den BİLİNÇLİ olarak türemez — her cevap denemesinin insert-only kaydı, soft
// delete/UpdatedAt anlamsız (bir geçmiş satırı ne silinir ne güncellenir); DATABASE_SCHEMA/SRS.md'deki
// LearningHistory tablosuyla birebir eşleşir.
public class LearningHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // İkisi birden NULL olamaz (uygulama kontrolü) — hangi kaynaktan sorulduğunu ayırır.
    public int? WordId { get; set; }
    public int? UserCardId { get; set; }

    // LearningSession entity'si henüz yok (A-11'de gelir) — FK constraint o zaman eklenir, şimdilik düz sütun.
    public int? LearningSessionId { get; set; }
    public string? SessionType { get; set; }

    public bool IsCorrect { get; set; }
    public int? ResponseTime { get; set; }
    public int? TimeSpentSeconds { get; set; }
    public string? UserResponse { get; set; }
    public string? CorrectResponse { get; set; }

    // İpucu istendi mi — quality tavanını düşürür.
    public bool HintUsed { get; set; }
    // "Aynı Kelimelerle Tekrar Et" — SM-2 güncellemez, yalnızca istatistik.
    public bool IsExtraPractice { get; set; }

    // Yalnızca IsExtraPractice=false iken dolar.
    public decimal? MasteryBefore { get; set; }
    public decimal? MasteryAfter { get; set; }

    public DateTime CreatedAt { get; set; }
}
