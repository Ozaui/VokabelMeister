// ─────────────────────────────────────────────────────────────────────────────
// WordGrammarValidatorTests.cs
//
// AMAÇ: WordGrammarValidator'ın de/tr × Noun/Verb/Diğer matrisini (GERMAN_LANGUAGE_
//       FEATURES.md §10 / TURKISH_LANGUAGE_FEATURES.md §9) doğru uyguladığını kanıtlar.
// NEDEN: Proje FluentValidation.TestHelper paketi kullanmıyor (yeni bir bağımlılık
//        eklemeden zaten yeterli) — WordGrammarValidator plain bir AbstractValidator
//        olduğu için doğrudan `Validate(...)` çağrılıp `ValidationResult.Errors`
//        üzerinden ErrorCode'lar kontrol edilir.
// BAĞIMLILIKLAR: WordGrammarValidator, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Application.Validators.Words;

namespace WordLearner.Tests.Validators.Words;

public class WordGrammarValidatorTests
{
    private readonly WordGrammarValidator _validator = new();

    private static string ErrorCodes(FluentValidation.Results.ValidationResult result) =>
        string.Join(",", result.Errors.Select(e => e.ErrorCode));

    [Fact]
    public void Validate_DeNoun_ValidData_ReturnsNoErrors()
    {
        var json = """
            {
              "gender": "Masculine",
              "plural": "Tische",
              "cases": { "nominative": "der Tisch", "accusative": "den Tisch", "dative": "dem Tisch", "genitive": "des Tisches" }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Noun", json));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeNoun_MissingGender_ReturnsGenderRequiredError()
    {
        var json = """
            {
              "plural": "Tische",
              "cases": { "nominative": "der Tisch", "accusative": "den Tisch", "dative": "dem Tisch", "genitive": "des Tisches" }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Noun", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_NOUN_GENDER_REQUIRED");
    }

    [Fact]
    public void Validate_DeNoun_IncompleteCases_ReturnsCasesIncompleteError()
    {
        var json = """
            {
              "gender": "Masculine",
              "plural": "Tische",
              "cases": { "nominative": "der Tisch", "accusative": "den Tisch" }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Noun", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_NOUN_CASES_INCOMPLETE");
    }

    [Fact]
    public void Validate_DeNoun_ContainsVerbFields_ReturnsVerbFieldsForbiddenError()
    {
        var json = """
            {
              "gender": "Masculine",
              "plural": "Tische",
              "cases": { "nominative": "der Tisch", "accusative": "den Tisch", "dative": "dem Tisch", "genitive": "des Tisches" },
              "auxiliaryVerb": "haben"
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Noun", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_NOUN_VERB_FIELDS_FORBIDDEN");
    }

    [Fact]
    public void Validate_DeVerb_ValidNonSeparableData_ReturnsNoErrors()
    {
        var json = """
            {
              "isSeparableVerb": false,
              "auxiliaryVerb": "sein",
              "pastParticiple": "gegangen",
              "conjugation": {
                "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
                "preterite":  { "ich":"ging", "du":"gingst", "erSieEs":"ging", "wir":"gingen", "ihr":"gingt", "sie":"gingen" },
                "perfect":    { "ich":"bin gegangen", "du":"bist gegangen", "erSieEs":"ist gegangen", "wir":"sind gegangen", "ihr":"seid gegangen", "sie":"sind gegangen" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Verb", json));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeVerb_MissingAuxiliary_ReturnsAuxiliaryRequiredError()
    {
        var json = """
            {
              "isSeparableVerb": false,
              "pastParticiple": "gegangen",
              "conjugation": {
                "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
                "preterite":  { "ich":"ging", "du":"gingst", "erSieEs":"ging", "wir":"gingen", "ihr":"gingt", "sie":"gingen" },
                "perfect":    { "ich":"bin gegangen", "du":"bist gegangen", "erSieEs":"ist gegangen", "wir":"sind gegangen", "ihr":"seid gegangen", "sie":"sind gegangen" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Verb", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_VERB_AUXILIARY_REQUIRED");
    }

    [Fact]
    public void Validate_DeVerb_IncompleteConjugation_ReturnsConjugationIncompleteError()
    {
        var json = """
            {
              "isSeparableVerb": false,
              "auxiliaryVerb": "sein",
              "pastParticiple": "gegangen",
              "conjugation": {
                "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Verb", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_VERB_CONJUGATION_INCOMPLETE");
    }

    [Fact]
    public void Validate_DeVerb_SeparableTrueWithoutPrefix_ReturnsPrefixRequiredError()
    {
        var json = """
            {
              "isSeparableVerb": true,
              "auxiliaryVerb": "haben",
              "pastParticiple": "angerufen",
              "conjugation": {
                "present":    { "ich":"rufe an", "du":"rufst an", "erSieEs":"ruft an", "wir":"rufen an", "ihr":"ruft an", "sie":"rufen an" },
                "preterite":  { "ich":"rief an", "du":"riefst an", "erSieEs":"rief an", "wir":"riefen an", "ihr":"rieft an", "sie":"riefen an" },
                "perfect":    { "ich":"habe angerufen", "du":"hast angerufen", "erSieEs":"hat angerufen", "wir":"haben angerufen", "ihr":"habt angerufen", "sie":"haben angerufen" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Verb", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_VERB_SEPARABLE_PREFIX_REQUIRED");
    }

    [Fact]
    public void Validate_DeVerb_SeparableFalseWithPrefix_ReturnsPrefixForbiddenError()
    {
        var json = """
            {
              "isSeparableVerb": false,
              "separablePrefix": "an",
              "auxiliaryVerb": "sein",
              "pastParticiple": "gegangen",
              "conjugation": {
                "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
                "preterite":  { "ich":"ging", "du":"gingst", "erSieEs":"ging", "wir":"gingen", "ihr":"gingt", "sie":"gingen" },
                "perfect":    { "ich":"bin gegangen", "du":"bist gegangen", "erSieEs":"ist gegangen", "wir":"sind gegangen", "ihr":"seid gegangen", "sie":"sind gegangen" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Verb", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_VERB_SEPARABLE_PREFIX_FORBIDDEN");
    }

    [Fact]
    public void Validate_DeVerb_ContainsNounFields_ReturnsNounFieldsForbiddenError()
    {
        var json = """
            {
              "isSeparableVerb": false,
              "auxiliaryVerb": "sein",
              "pastParticiple": "gegangen",
              "gender": "Masculine",
              "conjugation": {
                "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
                "preterite":  { "ich":"ging", "du":"gingst", "erSieEs":"ging", "wir":"gingen", "ihr":"gingt", "sie":"gingen" },
                "perfect":    { "ich":"bin gegangen", "du":"bist gegangen", "erSieEs":"ist gegangen", "wir":"sind gegangen", "ihr":"seid gegangen", "sie":"sind gegangen" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("de", "Verb", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_DE_VERB_NOUN_FIELDS_FORBIDDEN");
    }

    [Fact]
    public void Validate_TrNoun_ValidData_ReturnsNoErrors()
    {
        var json = """
            {
              "plural": "masalar",
              "cases": { "nominative": "masa", "accusative": "masayı", "dative": "masaya", "locative": "masada", "ablative": "masadan", "genitive": "masanın" }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("tr", "Noun", json));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TrNoun_MissingPlural_ReturnsPluralRequiredError()
    {
        var json = """
            {
              "cases": { "nominative": "masa", "accusative": "masayı", "dative": "masaya", "locative": "masada", "ablative": "masadan", "genitive": "masanın" }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("tr", "Noun", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_TR_NOUN_PLURAL_REQUIRED");
    }

    [Fact]
    public void Validate_TrNoun_IncompleteCases_ReturnsCasesIncompleteError()
    {
        var json = """
            {
              "plural": "masalar",
              "cases": { "nominative": "masa", "accusative": "masayı" }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("tr", "Noun", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_TR_NOUN_CASES_INCOMPLETE");
    }

    [Fact]
    public void Validate_TrVerb_ValidData_ReturnsNoErrors()
    {
        var json = """
            {
              "verbRoot": "gel",
              "negativeForm": "gelmemek",
              "conjugation": {
                "presentContinuous": { "ben":"geliyorum", "sen":"geliyorsun", "o":"geliyor", "biz":"geliyoruz", "siz":"geliyorsunuz", "onlar":"geliyorlar" },
                "aorist":            { "ben":"gelirim", "sen":"gelirsin", "o":"gelir", "biz":"geliriz", "siz":"gelirsiniz", "onlar":"gelirler" },
                "pastDefinite":      { "ben":"geldim", "sen":"geldin", "o":"geldi", "biz":"geldik", "siz":"geldiniz", "onlar":"geldiler" },
                "pastNarrative":     { "ben":"gelmişim", "sen":"gelmişsin", "o":"gelmiş", "biz":"gelmişiz", "siz":"gelmişsiniz", "onlar":"gelmişler" },
                "future":            { "ben":"geleceğim", "sen":"geleceksin", "o":"gelecek", "biz":"geleceğiz", "siz":"geleceksiniz", "onlar":"gelecekler" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("tr", "Verb", json));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TrVerb_MissingVerbRoot_ReturnsVerbRootRequiredError()
    {
        var json = """
            {
              "negativeForm": "gelmemek",
              "conjugation": {
                "presentContinuous": { "ben":"geliyorum", "sen":"geliyorsun", "o":"geliyor", "biz":"geliyoruz", "siz":"geliyorsunuz", "onlar":"geliyorlar" },
                "aorist":            { "ben":"gelirim", "sen":"gelirsin", "o":"gelir", "biz":"geliriz", "siz":"gelirsiniz", "onlar":"gelirler" },
                "pastDefinite":      { "ben":"geldim", "sen":"geldin", "o":"geldi", "biz":"geldik", "siz":"geldiniz", "onlar":"geldiler" },
                "pastNarrative":     { "ben":"gelmişim", "sen":"gelmişsin", "o":"gelmiş", "biz":"gelmişiz", "siz":"gelmişsiniz", "onlar":"gelmişler" },
                "future":            { "ben":"geleceğim", "sen":"geleceksin", "o":"gelecek", "biz":"geleceğiz", "siz":"geleceksiniz", "onlar":"gelecekler" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("tr", "Verb", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_TR_VERB_VERBROOT_REQUIRED");
    }

    [Fact]
    public void Validate_TrVerb_IncompleteConjugation_ReturnsConjugationIncompleteError()
    {
        var json = """
            {
              "verbRoot": "gel",
              "negativeForm": "gelmemek",
              "conjugation": {
                "presentContinuous": { "ben":"geliyorum", "sen":"geliyorsun", "o":"geliyor", "biz":"geliyoruz", "siz":"geliyorsunuz", "onlar":"geliyorlar" }
              }
            }
            """;

        var result = _validator.Validate(new WordGrammarInput("tr", "Verb", json));

        ErrorCodes(result).Should().Contain("GRAMMAR_TR_VERB_CONJUGATION_INCOMPLETE");
    }

    [Theory]
    [InlineData("de", "Adjective")]
    [InlineData("tr", "Adverb")]
    public void Validate_OtherPartOfSpeech_GrammarDataNull_ReturnsNoErrors(string languageCode, string partOfSpeech)
    {
        var result = _validator.Validate(new WordGrammarInput(languageCode, partOfSpeech, null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_OtherPartOfSpeech_GrammarDataProvided_ReturnsMustBeNullError()
    {
        var result = _validator.Validate(new WordGrammarInput("de", "Conjunction", """{ "gender": "Masculine" }"""));

        ErrorCodes(result).Should().Contain("GRAMMAR_DATA_MUST_BE_NULL_FOR_OTHER");
    }

    [Theory]
    [InlineData("de", "Noun")]
    [InlineData("tr", "Verb")]
    public void Validate_NounOrVerb_GrammarDataNull_ReturnsDataRequiredError(string languageCode, string partOfSpeech)
    {
        var result = _validator.Validate(new WordGrammarInput(languageCode, partOfSpeech, null));

        ErrorCodes(result).Should().Contain("GRAMMAR_DATA_REQUIRED");
    }

    [Fact]
    public void Validate_InvalidJson_ReturnsInvalidJsonError()
    {
        var result = _validator.Validate(new WordGrammarInput("de", "Noun", "{ not valid json"));

        ErrorCodes(result).Should().Contain("GRAMMAR_DATA_INVALID_JSON");
    }

    [Fact]
    public void Validate_UnsupportedLanguage_ReturnsLanguageUnsupportedError()
    {
        var result = _validator.Validate(new WordGrammarInput("en", "Noun", """{ "gender": "Masculine" }"""));

        ErrorCodes(result).Should().Contain("GRAMMAR_LANGUAGE_UNSUPPORTED");
    }
}
