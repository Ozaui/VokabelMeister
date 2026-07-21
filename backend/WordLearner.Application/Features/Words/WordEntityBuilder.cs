// ─────────────────────────────────────────────────────────────────────────────
// WordEntityBuilder.cs
//
// AMAÇ: WordTranslationInput'tan (API girdisi) yeni bir Word (+WordDetail+
//       WordExample) entity ağacı kurar.
// NEDEN: CreateWordCommandHandler (yeni WordConcept) ve UpdateWordCommandHandler
//        (mevcut kavrama yeni bir dil eklerken) AYNI kurma mantığını paylaşıyor —
//        Handler'lar birbirini `_mediator.Send()` ile çağıramadığı için (CLAUDE.md
//        §3) bu, IOtpService örneğindeki gibi küçük, paylaşılan bir yardımcıya çıkarıldı.
// BAĞIMLILIKLAR: Word/WordDetail/WordExample entity'leri, CreateWordCommand.cs'teki input tipleri.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public static class WordEntityBuilder
{
    // AMAÇ: Bir dil girdisinden tam bir Word entity ağacı (WordDetail+WordExample dahil) kurar.
    public static Word Build(WordTranslationInput translation, Language language, int? userId)
    {
        var word = new Word
        {
            Language = language,
            Text = translation.Text,
            Definition = translation.Definition,
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
        };

        if (translation.WordDetail is not null)
            word.WordDetail = BuildWordDetail(translation.WordDetail, userId);

        if (translation.Examples is not null)
        {
            var displayOrder = 0;
            foreach (var example in translation.Examples)
            {
                word.WordExamples.Add(
                    new WordExample
                    {
                        SentenceText = example.SentenceText,
                        Level = example.Level,
                        ExampleType = example.ExampleType,
                        DisplayOrder = displayOrder++,
                        CreatedByUserId = userId,
                        UpdatedByUserId = userId,
                    }
                );
            }
        }

        return word;
    }

    // AMAÇ: WordDetailInput'tan bir WordDetail entity'si kurar — GrammarData ham
    //       JSON metnine (`GetRawText()`) çevrilip öyle saklanır (DB kolonu string).
    public static WordDetail BuildWordDetail(WordDetailInput input, int? userId) =>
        new()
        {
            Pronunciation = input.Pronunciation,
            AudioUrl = input.AudioUrl,
            Notes = input.Notes,
            CommonMistakes = input.CommonMistakes,
            GrammarData = input.GrammarData?.GetRawText(),
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
        };
}
