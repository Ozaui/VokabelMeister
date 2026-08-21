namespace Zausel.Application.Interfaces.Repositories.Srs;

// UserProgressRepository'nin bant/askıya-alınmış listeleme metotlarının döndürdüğü şekil —
// UserProgress tek başına anlamsız (kelimenin metnini taşımaz), Word'le CategoryAggregate'in
// Translations deseniyle AYNI şekilde (navigation property YOK) elle birleştirilir.
public record WordProgressItem(
    int WordId, string Text, string? Definition, int CurrentLevel, decimal Mastery,
    DateTime? NextReviewAt, bool IsSuspended, int ConsecutiveIncorrect);
