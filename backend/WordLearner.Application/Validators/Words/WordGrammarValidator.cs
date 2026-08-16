using System.Text.Json;
using FluentValidation;
using WordLearner.Application.DTOs.Words;
using WordLearner.Domain.Enums.Content;

namespace WordLearner.Application.Validators.Words;

// languageCode+partOfSpeech İNŞA anında sabitlenir — bu yüzden dispatch RuleFor içinde .When()
// DEĞİL, constructor'da hangi kural grubunun ekleneceğine karar veren bir if/else zinciridir.
public class WordGrammarValidator : AbstractValidator<WordGrammarInput>
{
    private static readonly string[] DeNounCases = ["nominative", "accusative", "dative", "genitive"];
    private static readonly string[] TrNounCases = ["nominative", "accusative", "dative", "locative", "ablative", "genitive"];
    private static readonly string[] DeVerbTenses = ["present", "preterite", "perfect"];
    private static readonly string[] TrVerbTenses = ["presentContinuous", "aorist", "pastDefinite", "pastNarrative", "future"];
    private static readonly string[] DePersons = ["ich", "du", "erSieEs", "wir", "ihr", "sie"];
    private static readonly string[] TrPersons = ["ben", "sen", "o", "biz", "siz", "onlar"];

    public WordGrammarValidator(string languageCode, PartOfSpeech partOfSpeech)
    {
        RuleFor(x => x.Text).NotEmpty().WithErrorCode("WORD_TEXT_REQUIRED");
        RuleFor(x => x.Definition).NotEmpty().WithErrorCode("WORD_DEFINITION_REQUIRED");

        if (languageCode == "de" && partOfSpeech == PartOfSpeech.Noun)
            AddDeNounRules();
        else if (languageCode == "de" && partOfSpeech == PartOfSpeech.Verb)
            AddDeVerbRules();
        else if (languageCode == "tr" && partOfSpeech == PartOfSpeech.Noun)
            AddTrNounRules();
        else if (languageCode == "tr" && partOfSpeech == PartOfSpeech.Verb)
            AddTrVerbRules();
        else
            RuleFor(x => x.GrammarData).Must(g => g is null).WithErrorCode("GRAMMAR_DATA_MUST_BE_NULL");
    }

    private void AddDeNounRules()
    {
        RuleFor(x => x.GrammarData).Must(g => HasNonEmptyString(g, "gender")).WithErrorCode("DE_NOUN_GENDER_REQUIRED");
        RuleFor(x => x.GrammarData).Must(g => HasNonEmptyString(g, "plural")).WithErrorCode("DE_NOUN_PLURAL_REQUIRED");
        RuleFor(x => x.GrammarData).Must(g => HasCompleteObject(g, "cases", DeNounCases)).WithErrorCode("DE_NOUN_CASES_INCOMPLETE");
        RuleFor(x => x.GrammarData)
            .Must(g => !HasAnyKey(g, "isSeparableVerb", "separablePrefix", "auxiliaryVerb", "pastParticiple", "conjugation"))
            .WithErrorCode("DE_NOUN_VERB_FIELDS_FORBIDDEN");
    }

    private void AddDeVerbRules()
    {
        RuleFor(x => x.GrammarData).Must(g => TryGetBool(g, "isSeparableVerb", out _)).WithErrorCode("DE_VERB_IS_SEPARABLE_REQUIRED");
        RuleFor(x => x.GrammarData).Must(g => HasNonEmptyString(g, "auxiliaryVerb")).WithErrorCode("DE_VERB_AUXILIARY_REQUIRED");
        RuleFor(x => x.GrammarData).Must(g => HasNonEmptyString(g, "pastParticiple")).WithErrorCode("DE_VERB_PAST_PARTICIPLE_REQUIRED");
        RuleFor(x => x.GrammarData).Must(SeparablePrefixMatchesFlag).WithErrorCode("DE_VERB_SEPARABLE_PREFIX_MISMATCH");
        RuleFor(x => x.GrammarData).Must(g => HasCompleteConjugation(g, DeVerbTenses, DePersons)).WithErrorCode("DE_VERB_CONJUGATION_INCOMPLETE");
        RuleFor(x => x.GrammarData).Must(g => !HasAnyKey(g, "gender", "plural", "cases")).WithErrorCode("DE_VERB_NOUN_FIELDS_FORBIDDEN");
    }

    private void AddTrNounRules()
    {
        RuleFor(x => x.GrammarData).Must(g => HasNonEmptyString(g, "plural")).WithErrorCode("TR_NOUN_PLURAL_REQUIRED");
        RuleFor(x => x.GrammarData).Must(g => HasCompleteObject(g, "cases", TrNounCases)).WithErrorCode("TR_NOUN_CASES_INCOMPLETE");
        RuleFor(x => x.GrammarData)
            .Must(g => TryGetString(g, "vowelHarmony", out var v) && (v == "kalın" || v == "ince"))
            .WithErrorCode("TR_NOUN_VOWEL_HARMONY_INVALID");
        RuleFor(x => x.GrammarData).Must(g => HasCompleteObject(g, "possessive", TrPersons)).WithErrorCode("TR_NOUN_POSSESSIVE_INCOMPLETE");
        RuleFor(x => x.GrammarData)
            .Must(g => !HasAnyKey(g, "verbRoot", "negativeForm", "conjugation"))
            .WithErrorCode("TR_NOUN_VERB_FIELDS_FORBIDDEN");
    }

    private void AddTrVerbRules()
    {
        RuleFor(x => x.GrammarData).Must(g => HasNonEmptyString(g, "verbRoot")).WithErrorCode("TR_VERB_ROOT_REQUIRED");
        RuleFor(x => x.GrammarData).Must(g => HasNonEmptyString(g, "negativeForm")).WithErrorCode("TR_VERB_NEGATIVE_FORM_REQUIRED");
        RuleFor(x => x.GrammarData).Must(g => HasCompleteConjugation(g, TrVerbTenses, TrPersons)).WithErrorCode("TR_VERB_CONJUGATION_INCOMPLETE");
        RuleFor(x => x.GrammarData)
            .Must(g => !HasAnyKey(g, "plural", "cases", "vowelHarmony", "possessive"))
            .WithErrorCode("TR_VERB_NOUN_FIELDS_FORBIDDEN");
    }

    private static bool SeparablePrefixMatchesFlag(JsonElement? grammarData)
    {
        if (!TryGetBool(grammarData, "isSeparableVerb", out var isSeparable)) return false;
        var hasPrefix = HasNonEmptyString(grammarData, "separablePrefix");
        return isSeparable == hasPrefix;
    }

    private static bool HasCompleteConjugation(JsonElement? grammarData, string[] tenses, string[] persons)
    {
        if (!TryGetObject(grammarData, "conjugation", out var conjugation)) return false;
        return tenses.All(tense => HasCompleteObject(conjugation, tense, persons));
    }

    private static bool HasCompleteObject(JsonElement? parent, string propertyName, string[] requiredKeys)
    {
        if (!TryGetObject(parent, propertyName, out var obj)) return false;
        return requiredKeys.All(key => HasNonEmptyString(obj, key));
    }

    private static bool HasNonEmptyString(JsonElement? obj, string propertyName) =>
        TryGetString(obj, propertyName, out var value) && !string.IsNullOrWhiteSpace(value);

    private static bool TryGetString(JsonElement? obj, string propertyName, out string? value)
    {
        value = null;
        if (obj is not { ValueKind: JsonValueKind.Object } data) return false;
        if (!data.TryGetProperty(propertyName, out var prop) || prop.ValueKind != JsonValueKind.String) return false;
        value = prop.GetString();
        return true;
    }

    private static bool TryGetObject(JsonElement? obj, string propertyName, out JsonElement value)
    {
        value = default;
        if (obj is not { ValueKind: JsonValueKind.Object } data) return false;
        if (!data.TryGetProperty(propertyName, out var prop) || prop.ValueKind != JsonValueKind.Object) return false;
        value = prop;
        return true;
    }

    private static bool TryGetBool(JsonElement? obj, string propertyName, out bool value)
    {
        value = false;
        if (obj is not { ValueKind: JsonValueKind.Object } data) return false;
        if (!data.TryGetProperty(propertyName, out var prop)) return false;
        if (prop.ValueKind != JsonValueKind.True && prop.ValueKind != JsonValueKind.False) return false;
        value = prop.GetBoolean();
        return true;
    }

    private static bool HasAnyKey(JsonElement? obj, params string[] keys) =>
        obj is { ValueKind: JsonValueKind.Object } data && keys.Any(k => data.TryGetProperty(k, out _));
}
