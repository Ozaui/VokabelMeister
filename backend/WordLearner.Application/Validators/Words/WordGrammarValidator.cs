// ─────────────────────────────────────────────────────────────────────────────
// WordGrammarValidator.cs
//
// AMAÇ: Bir Word'ün WordDetail.GrammarData (JSON) alanını, dile (`de`/`tr`) ve
//       WordConcept.PartOfSpeech'e göre doğrular — GERMAN_LANGUAGE_FEATURES.md §10
//       / TURKISH_LANGUAGE_FEATURES.md §9'daki Zorunlu/Koşullu/Yasak matrislerinin
//       kod karşılığı.
// NEDEN: Hem tekil `CreateWordCommand`/`UpdateWordCommand` (ileriki bölüm) hem A-07
//        toplu import AYNI kuralları uygulamalı — bu yüzden FluentValidation
//        `AbstractValidator<WordGrammarInput>` olarak, kendi başına bir tipe
//        (`WordGrammarInput`) bağlı, DI'a otomatik kayıtlı (AddValidatorsFromAssembly)
//        bağımsız bir validator yazıldı. Gelecekteki `CreateWordCommandValidator`
//        bunu `IValidator<WordGrammarInput>` olarak enjekte edip her translation için
//        çağıracak — kural burada TEK yerde tanımlanır.
//        Yalnızca dokümanların "Zorunlu"/"Koşullu"/"Yasak" listelerindeki alanlar
//        kontrol edilir — `TURKISH_LANGUAGE_FEATURES.md §9`'da tanımlı ama §9'un
//        matrisine dahil edilmeyen alanlar (possessive, vowelHarmony, pluralForm,
//        consonantMutation) zorunlu TUTULMAZ (dokümanın kendi sınırı).
// BAĞIMLILIKLAR: FluentValidation, System.Text.Json.
// ─────────────────────────────────────────────────────────────────────────────

using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace WordLearner.Application.Validators.Words;

// AMAÇ: WordGrammarValidator'ın doğrulayacağı ham girdi — bir Command/DTO değil,
//       yalnızca bu validator'a özel, tekrar kullanılabilir bir değer nesnesi.
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

    // AMAÇ: Noun/Verb'de GrammarData'da HİÇ bulunmaması gereken üst düzey alanlar.
    private static readonly string[] VerbOnlyFields =
    [
        "isSeparableVerb",
        "separablePrefix",
        "auxiliaryVerb",
        "pastParticiple",
        "conjugation",
    ];
    private static readonly string[] NounOnlyFields = ["gender", "plural", "cases"];

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

    // AMAÇ: Girdiyi dile ve türe göre dağıtıp ilgili kural setini uygular.
    // NASIL: 1) LanguageCode'a göre de/tr dalına gir  2) PartOfSpeech Noun/Verb/Diğer'e
    //        göre o dilin matrisini uygula  3) Her ihlal için ayrı bir ValidationFailure üret.
    private static IEnumerable<ValidationFailure> EnumerateFailures(WordGrammarInput input)
    {
        JsonElement? root = null;
        if (!string.IsNullOrWhiteSpace(input.GrammarDataJson))
        {
            // NEDEN ayrı TryParseJson metodu: bir catch bloğunun gövdesinde yield return
            // KULLANILAMAZ (CS1631) — parse denemesi bu yüzden bool dönen bir yardımcıya taşındı.
            if (!TryParseJson(input.GrammarDataJson, out var parsed))
            {
                yield return Failure("GRAMMAR_DATA_INVALID_JSON");
                yield break;
            }

            root = parsed;
        }

        var isNounOrVerb = input.PartOfSpeech is "Noun" or "Verb";

        // NEDEN: Diğer türlerde (Adjective/Adverb/Conjunction/Preposition/Pronoun/Other)
        //        GrammarData tamamen NULL olmalı — iki dilde de ortak kural.
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
