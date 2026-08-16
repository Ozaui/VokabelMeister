using WordLearner.Domain.Entities.Content;

namespace WordLearner.Application.Interfaces.Repositories.Content;

// IWordConceptRepository'nin okuma metotlarının döndürdüğü şekil — WordConcept tek başına
// anlamsız (bir kavramın karşılığı olmadan gösterilecek bir şeyi yok), bu yüzden repository
// Concept + o kavramın TÜM çevirilerini TEK bir çağrıda birlikte döndürüyor.
public record WordConceptAggregate(WordConcept Concept, List<WordTranslationAggregate> Translations);

// Her çeviri kendi Word'ü + hangi dile ait olduğu (Language) + varsa WordDetail + örnek listesi.
public record WordTranslationAggregate(Word Word, Language Language, WordDetail? Detail, List<WordExample> Examples);
