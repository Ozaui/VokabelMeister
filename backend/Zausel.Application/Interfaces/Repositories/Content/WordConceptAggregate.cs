using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;

namespace Zausel.Application.Interfaces.Repositories.Content;

// IWordConceptRepository'nin okuma metotlarının döndürdüğü şekil — WordConcept tek başına
// anlamsız (bir kavramın karşılığı olmadan gösterilecek bir şeyi yok), bu yüzden repository
// Concept + o kavramın TÜM çevirilerini TEK bir çağrıda birlikte döndürüyor.
public record WordConceptAggregate(WordConcept Concept, List<WordTranslationAggregate> Translations);

// Her çeviri kendi Word'ü + hangi dile ait olduğu (Language) + varsa WordDetail + örnek listesi.
public record WordTranslationAggregate(Word Word, Language Language, WordDetail? Detail, List<WordExample> Examples);

// Eşleşmemiş (tek dilli) bir WordConcept'in GetUnmatchedAsync/GetUnmatchedPoolAsync'in döndürdüğü
// HAFİF izdüşümü — WordConceptAggregate'in AKSİNE WordDetail/WordExample TAŞIMAZ, öneri hesaplaması
// yalnızca Text+Definition'a bakar, geri kalanı hiç SORGULANMAZ.
public record UnmatchedWordAggregate(int WordConceptId, string Text, string? Definition, PartOfSpeech PartOfSpeech, string DifficultyLevel);
