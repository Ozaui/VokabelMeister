using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

// CreateWordCommandHandler ve UpdateWordCommandHandler'ın paylaştığı entity kurma mantığı.
public static class WordEntityBuilder
{
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
