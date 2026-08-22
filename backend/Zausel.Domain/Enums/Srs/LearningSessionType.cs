namespace Zausel.Domain.Enums.Srs;

// Yeni kelime oturumu = Flashcard sabit; review oturumunun sorusu-başına gerçek formatı
// backend'de bu 6 değer arasından rastgele atanır (bkz. LearningHistory.SessionType).
public enum LearningSessionType
{
    Flashcard,
    MultipleChoice,
    ArticleQuiz,
    PluralQuiz,
    TranslationQuiz,
    TrueFalse
}
