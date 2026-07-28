using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace WordLearner.Application.Validators.Words;

// WordDetail.GrammarData (JSON) alanını dile (de/tr) ve PartOfSpeech'e göre doğrular —
// GERMAN_LANGUAGE_FEATURES.md §10 / TURKISH_LANGUAGE_FEATURES.md §9'daki Zorunlu/Koşullu/Yasak
// matrislerinin kod karşılığı. Bağımsız `IValidator<WordGrammarInput>` olarak DI'a kayıtlı —
// Create/UpdateWordCommandValidator ve toplu import aynı kuralı buradan çağırır.
public record WordGrammarInput(string LanguageCode, string PartOfSpeech, string? GrammarDataJson);

public class WordGrammarValidator : AbstractValidator<WordGrammarInput>
{
    private static readonly string[] DeCaseFields = ["nominative", "accusative", "dative", "genitive"];
    private static readonly string[] TrCaseFields =
    [
        "nominative",
        "accusative",
        "dative",
        "locative",
        "ablative",
        "genitive",
    ];
    private static readonly string[] DeVerbTenses = ["present", "preterite", "perfect"];
    private static readonly string[] DePersons = ["ich", "du", "erSieEs", "wir", "ihr", "sie"];
    private static readonly string[] TrVerbTenses =
    [
        "presentContinuous",
        "aorist",
        "pastDefinite",
        "pastNarrative",
        "future",
    ];
    private static readonly string[] TrPersons = ["ben", "sen", "o", "biz", "siz", "onlar"];

    // Iyelik ekleri de conjugation gibi 6 kişilik ama tek "zaman" (tense kavramı yok) —
    // HasAllConjugationCells'in tense/person iki katmanına uymadığı için ayrı bir kontrol.
    private static readonly string[] TrPossessivePersons = ["ben", "sen", "o", "biz", "siz", "onlar"];

    private static readonly string[] VerbOnlyFields =
    [
        "isSeparableVerb",
        "separablePrefix",
        "auxiliaryVerb",
        "pastParticiple",
        "conjugation",
    ];
    // "vowelHarmony"/"possessive" tr'ye özgü (de'de kavram bile yok) ama VerbFieldsForbidden
    // kontrolü iki dilde de aynı diziyi paylaşıyor — Verb'de bulunmaları hep yasak olduğu için
    // ekstra bir dil-özel dizi açmaya gerek yok.
    private static readonly string[] NounOnlyFields = ["gender", "plural", "cases", "vowelHarmony", "possessive"];

    public WordGrammarValidator()
    {
        RuleFor(x => x)
            .Custom(
                (input, context) =>
                {
                    foreach (var failure in EnumerateFailures(input))
                        context.AddFailure(failure);
                }
            );
    }

    private static IEnumerable<ValidationFailure> EnumerateFailures(WordGrammarInput input)
    {
        JsonElement? root = null;
        if (!string.IsNullOrWhiteSpace(input.GrammarDataJson))
        {
            // catch bloğunun gövdesinde yield return kullanılamaz (CS1631) — parse denemesi bu yüzden ayrı bir yardımcıda.
            if (!TryParseJson(input.GrammarDataJson, out var parsed))
            {
                yield return Failure("GRAMMAR_DATA_INVALID_JSON");
                yield break;
            }

            root = parsed;
        }

        var isNounOrVerb = input.PartOfSpeech is "Noun" or "Verb";

        // Diğer türlerde (Adjective/Adverb/...) GrammarData tamamen NULL olmalı — iki dilde de ortak kural.
        if (!isNounOrVerb)
        {
            if (root is not null)
                yield return Failure("GRAMMAR_DATA_MUST_BE_NULL_FOR_OTHER");
            yield break;
        }

        if (root is null)
        {
            yield return Failure("GRAMMAR_DATA_REQUIRED");
            yield break;
        }

        var value = root.Value;

        var failures =
            input.LanguageCode == "de" ? ValidateGerman(value, input.PartOfSpeech)
            : input.LanguageCode == "tr" ? ValidateTurkish(value, input.PartOfSpeech)
            : [Failure("GRAMMAR_LANGUAGE_UNSUPPORTED")];

        foreach (var failure in failures)
            yield return failure;
    }

    private static IEnumerable<ValidationFailure> ValidateGerman(JsonElement data, string partOfSpeech)
    {
        if (partOfSpeech == "Noun")
        {
            if (!HasNonEmptyString(data, "gender"))
                yield return Failure("GRAMMAR_DE_NOUN_GENDER_REQUIRED");
            if (!HasNonEmptyString(data, "plural"))
                yield return Failure("GRAMMAR_DE_NOUN_PLURAL_REQUIRED");
            if (!HasAllCaseFields(data, DeCaseFields))
                yield return Failure("GRAMMAR_DE_NOUN_CASES_INCOMPLETE");
            if (HasAnyField(data, VerbOnlyFields))
                yield return Failure("GRAMMAR_DE_NOUN_VERB_FIELDS_FORBIDDEN");
            yield break;
        }

        // PartOfSpeech == "Verb"
        var isSeparable = TryGetBoolean(data, "isSeparableVerb", out var isSeparableValue);
        if (!isSeparable)
            yield return Failure("GRAMMAR_DE_VERB_ISSEPARABLE_REQUIRED");
        if (!HasNonEmptyString(data, "auxiliaryVerb"))
            yield return Failure("GRAMMAR_DE_VERB_AUXILIARY_REQUIRED");
        if (!HasNonEmptyString(data, "pastParticiple"))
            yield return Failure("GRAMMAR_DE_VERB_PASTPARTICIPLE_REQUIRED");
        if (!HasAllConjugationCells(data, DeVerbTenses, DePersons))
            yield return Failure("GRAMMAR_DE_VERB_CONJUGATION_INCOMPLETE");

        var hasPrefix = HasNonEmptyString(data, "separablePrefix");
        if (isSeparable && isSeparableValue && !hasPrefix)
            yield return Failure("GRAMMAR_DE_VERB_SEPARABLE_PREFIX_REQUIRED");
        if (isSeparable && !isSeparableValue && hasPrefix)
            yield return Failure("GRAMMAR_DE_VERB_SEPARABLE_PREFIX_FORBIDDEN");

        if (HasAnyField(data, NounOnlyFields))
            yield return Failure("GRAMMAR_DE_VERB_NOUN_FIELDS_FORBIDDEN");
    }

    private static IEnumerable<ValidationFailure> ValidateTurkish(JsonElement data, string partOfSpeech)
    {
        if (partOfSpeech == "Noun")
        {
            if (!HasNonEmptyString(data, "plural"))
                yield return Failure("GRAMMAR_TR_NOUN_PLURAL_REQUIRED");
            if (!HasAllCaseFields(data, TrCaseFields))
                yield return Failure("GRAMMAR_TR_NOUN_CASES_INCOMPLETE");
            // A-05.2 retrofit: kart tasarımı (TURKISH_LANGUAGE_FEATURES.md §7) ünlü uyumu grubunu
            // ve iyelik ekini isim kartının parçası sayıyor — A-05'te §9 matrisine hiç girmemişti.
            if (!HasNonEmptyString(data, "vowelHarmony"))
                yield return Failure("GRAMMAR_TR_NOUN_VOWELHARMONY_REQUIRED");
            if (!HasAllPossessiveFields(data))
                yield return Failure("GRAMMAR_TR_NOUN_POSSESSIVE_INCOMPLETE");
            if (HasAnyField(data, ["verbRoot", "negativeForm", "conjugation"]))
                yield return Failure("GRAMMAR_TR_NOUN_VERB_FIELDS_FORBIDDEN");
            yield break;
        }

        // PartOfSpeech == "Verb"
        if (!HasNonEmptyString(data, "verbRoot"))
            yield return Failure("GRAMMAR_TR_VERB_VERBROOT_REQUIRED");
        if (!HasNonEmptyString(data, "negativeForm"))
            yield return Failure("GRAMMAR_TR_VERB_NEGATIVEFORM_REQUIRED");
        if (!HasAllConjugationCells(data, TrVerbTenses, TrPersons))
            yield return Failure("GRAMMAR_TR_VERB_CONJUGATION_INCOMPLETE");
        if (HasAnyField(data, NounOnlyFields))
            yield return Failure("GRAMMAR_TR_VERB_NOUN_FIELDS_FORBIDDEN");
    }

    private static bool TryParseJson(string json, out JsonElement element)
    {
        try
        {
            element = JsonDocument.Parse(json).RootElement;
            return true;
        }
        catch (JsonException)
        {
            element = default;
            return false;
        }
    }

    private static bool HasNonEmptyString(JsonElement obj, string property) =>
        obj.ValueKind == JsonValueKind.Object
        && obj.TryGetProperty(property, out var value)
        && value.ValueKind == JsonValueKind.String
        && !string.IsNullOrWhiteSpace(value.GetString());

    private static bool HasAnyField(JsonElement obj, IReadOnlyList<string> properties) =>
        obj.ValueKind == JsonValueKind.Object && properties.Any(p => obj.TryGetProperty(p, out _));

    private static bool TryGetBoolean(JsonElement obj, string property, out bool value)
    {
        value = false;
        if (
            obj.ValueKind != JsonValueKind.Object
            || !obj.TryGetProperty(property, out var element)
            || (element.ValueKind != JsonValueKind.True && element.ValueKind != JsonValueKind.False)
        )
            return false;

        value = element.GetBoolean();
        return true;
    }

    private static bool HasAllCaseFields(JsonElement data, IReadOnlyList<string> caseFields)
    {
        if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty("cases", out var cases))
            return false;

        return caseFields.All(field => HasNonEmptyString(cases, field));
    }

    private static bool HasAllPossessiveFields(JsonElement data)
    {
        if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty("possessive", out var possessive))
            return false;

        return TrPossessivePersons.All(person => HasNonEmptyString(possessive, person));
    }

    private static bool HasAllConjugationCells(
        JsonElement data,
        IReadOnlyList<string> tenses,
        IReadOnlyList<string> persons
    )
    {
        if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty("conjugation", out var conjugation))
            return false;

        if (conjugation.ValueKind != JsonValueKind.Object)
            return false;

        return tenses.All(tense =>
            conjugation.TryGetProperty(tense, out var tenseValue)
            && persons.All(person => HasNonEmptyString(tenseValue, person))
        );
    }

    private static ValidationFailure Failure(string code) =>
        new(nameof(WordGrammarInput.GrammarDataJson), $"Grammar data validation failed: {code}.")
        {
            ErrorCode = code,
        };
}
