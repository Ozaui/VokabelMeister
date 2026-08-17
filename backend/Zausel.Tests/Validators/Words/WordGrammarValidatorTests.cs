using System.Text.Json;
using FluentAssertions;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Validators.Words;
using Zausel.Domain.Enums.Content;

namespace Zausel.Tests.Validators.Words;

public class WordGrammarValidatorTests
{
    private static readonly string[] DeVerbTenses = ["present", "preterite", "perfect"];
    private static readonly string[] DePersons = ["ich", "du", "erSieEs", "wir", "ihr", "sie"];
    private static readonly string[] TrVerbTenses = ["presentContinuous", "aorist", "pastDefinite", "pastNarrative", "future"];
    private static readonly string[] TrPersons = ["ben", "sen", "o", "biz", "siz", "onlar"];

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    // Her tense'e her kişi için AYNI placeholder değeri ("x") yazan bir conjugation JSON'u üretir —
    // testin ilgilendiği şey DOLU/BOŞ ayrımı, gerçek çekim metinlerinin İÇERİĞİ değil.
    private static string Conjugation(string[] tenses, string[] persons)
    {
        var personEntries = string.Join(",", persons.Select(p => $"\"{p}\":\"x\""));
        var tenseEntries = string.Join(",", tenses.Select(t => $"\"{t}\":{{{personEntries}}}"));
        return "{" + tenseEntries + "}";
    }

    private const string ValidDeNounJson =
        """{"gender":"Masculine","plural":"Männer","cases":{"nominative":"der Mann","accusative":"den Mann","dative":"dem Mann","genitive":"des Mannes"}}""";

    private static string ValidDeVerbJson(bool separable) =>
        $$"""{"isSeparableVerb":{{(separable ? "true" : "false")}},{{(separable ? "\"separablePrefix\":\"an\"," : "")}}"auxiliaryVerb":"haben","pastParticiple":"gemacht","conjugation":{{Conjugation(DeVerbTenses, DePersons)}}}""";

    private const string ValidTrNounJson =
        """{"plural":"kediler","vowelHarmony":"ince","cases":{"nominative":"kedi","accusative":"kediyi","dative":"kediye","locative":"kedide","ablative":"kediden","genitive":"kedinin"},"possessive":{"ben":"kedim","sen":"kedin","o":"kedisi","biz":"kedimiz","siz":"kediniz","onlar":"kedileri"}}""";

    private static string ValidTrVerbJson() =>
        $$"""{"verbRoot":"gel","negativeForm":"gelmemek","conjugation":{{Conjugation(TrVerbTenses, TrPersons)}}}""";

    // --- Ortak kurallar (dil/tür fark etmeksizin) ---

    [Fact]
    public void Validate_TextEmpty_ReturnsWordTextRequired()
    {
        // ARRANGE — "Diğer" tür seçildi ki gramer kuralları devreye girmesin, yalnızca Text/Definition test edilsin
        var validator = new WordGrammarValidator("de", PartOfSpeech.Other);

        // ACT
        var result = validator.Validate(new WordGrammarInput("", "anlam notu", null));

        // ASSERT
        result.Errors.Should().Contain(e => e.ErrorCode == "WORD_TEXT_REQUIRED");
    }

    [Fact]
    public void Validate_DefinitionEmpty_ReturnsWordDefinitionRequired()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Other);

        var result = validator.Validate(new WordGrammarInput("Wort", "", null));

        result.Errors.Should().Contain(e => e.ErrorCode == "WORD_DEFINITION_REQUIRED");
    }

    // --- de × Diğer (Noun/Verb dışı) ---

    [Fact]
    public void Validate_DeOtherWithNullGrammarData_IsValid()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Adjective);

        var result = validator.Validate(new WordGrammarInput("schnell", "hızlı", null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeOtherWithGrammarData_ReturnsGrammarDataMustBeNull()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Adjective);

        var result = validator.Validate(new WordGrammarInput("schnell", "hızlı", Parse("""{"gender":"Masculine"}""")));

        result.Errors.Should().Contain(e => e.ErrorCode == "GRAMMAR_DATA_MUST_BE_NULL");
    }

    // --- de × Noun ---

    [Fact]
    public void Validate_DeNounValidData_IsValid()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Noun);

        var result = validator.Validate(new WordGrammarInput("Mann", "erkek, adam", Parse(ValidDeNounJson)));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeNounMissingGender_ReturnsDeNounGenderRequired()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Noun);
        var json = """{"plural":"Männer","cases":{"nominative":"der Mann","accusative":"den Mann","dative":"dem Mann","genitive":"des Mannes"}}""";

        var result = validator.Validate(new WordGrammarInput("Mann", "erkek", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_NOUN_GENDER_REQUIRED");
    }

    [Fact]
    public void Validate_DeNounMissingPlural_ReturnsDeNounPluralRequired()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Noun);
        var json = """{"gender":"Masculine","cases":{"nominative":"der Mann","accusative":"den Mann","dative":"dem Mann","genitive":"des Mannes"}}""";

        var result = validator.Validate(new WordGrammarInput("Mann", "erkek", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_NOUN_PLURAL_REQUIRED");
    }

    [Fact]
    public void Validate_DeNounIncompleteCases_ReturnsDeNounCasesIncomplete()
    {
        // ARRANGE — dört hâlden yalnızca ikisi dolu (genitive/dative eksik)
        var validator = new WordGrammarValidator("de", PartOfSpeech.Noun);
        var json = """{"gender":"Masculine","plural":"Männer","cases":{"nominative":"der Mann","accusative":"den Mann"}}""";

        var result = validator.Validate(new WordGrammarInput("Mann", "erkek", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_NOUN_CASES_INCOMPLETE");
    }

    [Fact]
    public void Validate_DeNounWithVerbField_ReturnsDeNounVerbFieldsForbidden()
    {
        // ARRANGE — geçerli bir isim, ama YANLIŞLIKLA fiil alanı (auxiliaryVerb) da eklenmiş
        var validator = new WordGrammarValidator("de", PartOfSpeech.Noun);
        var json = """{"gender":"Masculine","plural":"Männer","cases":{"nominative":"der Mann","accusative":"den Mann","dative":"dem Mann","genitive":"des Mannes"},"auxiliaryVerb":"haben"}""";

        var result = validator.Validate(new WordGrammarInput("Mann", "erkek", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_NOUN_VERB_FIELDS_FORBIDDEN");
    }

    // --- de × Verb ---

    [Fact]
    public void Validate_DeVerbValidData_IsValid()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);

        var result = validator.Validate(new WordGrammarInput("machen", "yapmak", Parse(ValidDeVerbJson(separable: false))));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeVerbSeparableWithMatchingPrefix_IsValid()
    {
        // ARRANGE — ayrılabilir fiil (isSeparableVerb=true) + eşlik eden separablePrefix — SeparablePrefixMatchesFlag'in "eşleşen" dalı
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);

        var result = validator.Validate(new WordGrammarInput("anfangen", "başlamak", Parse(ValidDeVerbJson(separable: true))));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeVerbMissingIsSeparable_ReturnsDeVerbIsSeparableRequired()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);
        var json = $$"""{"auxiliaryVerb":"haben","pastParticiple":"gemacht","conjugation":{{Conjugation(DeVerbTenses, DePersons)}}}""";

        var result = validator.Validate(new WordGrammarInput("machen", "yapmak", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_VERB_IS_SEPARABLE_REQUIRED");
    }

    [Fact]
    public void Validate_DeVerbMissingAuxiliary_ReturnsDeVerbAuxiliaryRequired()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);
        var json = $$"""{"isSeparableVerb":false,"pastParticiple":"gemacht","conjugation":{{Conjugation(DeVerbTenses, DePersons)}}}""";

        var result = validator.Validate(new WordGrammarInput("machen", "yapmak", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_VERB_AUXILIARY_REQUIRED");
    }

    [Fact]
    public void Validate_DeVerbMissingPastParticiple_ReturnsDeVerbPastParticipleRequired()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);
        var json = $$"""{"isSeparableVerb":false,"auxiliaryVerb":"haben","conjugation":{{Conjugation(DeVerbTenses, DePersons)}}}""";

        var result = validator.Validate(new WordGrammarInput("machen", "yapmak", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_VERB_PAST_PARTICIPLE_REQUIRED");
    }

    [Fact]
    public void Validate_DeVerbSeparableFlagWithoutPrefix_ReturnsDeVerbSeparablePrefixMismatch()
    {
        // ARRANGE — isSeparableVerb=true ama separablePrefix HİÇ YOK — SeparablePrefixMatchesFlag'in "uyuşmuyor" dalı
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);
        var json = $$"""{"isSeparableVerb":true,"auxiliaryVerb":"haben","pastParticiple":"angefangen","conjugation":{{Conjugation(DeVerbTenses, DePersons)}}}""";

        var result = validator.Validate(new WordGrammarInput("anfangen", "başlamak", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_VERB_SEPARABLE_PREFIX_MISMATCH");
    }

    [Fact]
    public void Validate_DeVerbIncompleteConjugation_ReturnsDeVerbConjugationIncomplete()
    {
        // ARRANGE — yalnızca "present" zamanı dolu, preterite/perfect eksik
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);
        var json = """{"isSeparableVerb":false,"auxiliaryVerb":"haben","pastParticiple":"gemacht","conjugation":{"present":{"ich":"mache","du":"machst","erSieEs":"macht","wir":"machen","ihr":"macht","sie":"machen"}}}""";

        var result = validator.Validate(new WordGrammarInput("machen", "yapmak", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_VERB_CONJUGATION_INCOMPLETE");
    }

    [Fact]
    public void Validate_DeVerbWithNounField_ReturnsDeVerbNounFieldsForbidden()
    {
        var validator = new WordGrammarValidator("de", PartOfSpeech.Verb);
        var json = $$"""{"isSeparableVerb":false,"auxiliaryVerb":"haben","pastParticiple":"gemacht","conjugation":{{Conjugation(DeVerbTenses, DePersons)}},"gender":"Masculine"}""";

        var result = validator.Validate(new WordGrammarInput("machen", "yapmak", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "DE_VERB_NOUN_FIELDS_FORBIDDEN");
    }

    // --- tr × Noun ---

    [Fact]
    public void Validate_TrNounValidData_IsValid()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Noun);

        var result = validator.Validate(new WordGrammarInput("kedi", "Katze", Parse(ValidTrNounJson)));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TrNounMissingPlural_ReturnsTrNounPluralRequired()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Noun);
        var json = """{"vowelHarmony":"ince","cases":{"nominative":"kedi","accusative":"kediyi","dative":"kediye","locative":"kedide","ablative":"kediden","genitive":"kedinin"},"possessive":{"ben":"kedim","sen":"kedin","o":"kedisi","biz":"kedimiz","siz":"kediniz","onlar":"kedileri"}}""";

        var result = validator.Validate(new WordGrammarInput("kedi", "Katze", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_NOUN_PLURAL_REQUIRED");
    }

    [Fact]
    public void Validate_TrNounIncompleteCases_ReturnsTrNounCasesIncomplete()
    {
        // ARRANGE — altı hâlden yalnızca üçü dolu (locative/ablative/genitive eksik)
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Noun);
        var json = """{"plural":"kediler","vowelHarmony":"ince","cases":{"nominative":"kedi","accusative":"kediyi","dative":"kediye"},"possessive":{"ben":"kedim","sen":"kedin","o":"kedisi","biz":"kedimiz","siz":"kediniz","onlar":"kedileri"}}""";

        var result = validator.Validate(new WordGrammarInput("kedi", "Katze", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_NOUN_CASES_INCOMPLETE");
    }

    [Fact]
    public void Validate_TrNounInvalidVowelHarmony_ReturnsTrNounVowelHarmonyInvalid()
    {
        // ARRANGE — "kalın"/"ince" DIŞINDA bir değer
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Noun);
        var json = """{"plural":"kediler","vowelHarmony":"orta","cases":{"nominative":"kedi","accusative":"kediyi","dative":"kediye","locative":"kedide","ablative":"kediden","genitive":"kedinin"},"possessive":{"ben":"kedim","sen":"kedin","o":"kedisi","biz":"kedimiz","siz":"kediniz","onlar":"kedileri"}}""";

        var result = validator.Validate(new WordGrammarInput("kedi", "Katze", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_NOUN_VOWEL_HARMONY_INVALID");
    }

    [Fact]
    public void Validate_TrNounIncompletePossessive_ReturnsTrNounPossessiveIncomplete()
    {
        // ARRANGE — altı kişiden yalnızca "ben" dolu
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Noun);
        var json = """{"plural":"kediler","vowelHarmony":"ince","cases":{"nominative":"kedi","accusative":"kediyi","dative":"kediye","locative":"kedide","ablative":"kediden","genitive":"kedinin"},"possessive":{"ben":"kedim"}}""";

        var result = validator.Validate(new WordGrammarInput("kedi", "Katze", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_NOUN_POSSESSIVE_INCOMPLETE");
    }

    [Fact]
    public void Validate_TrNounWithVerbField_ReturnsTrNounVerbFieldsForbidden()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Noun);
        var json = """{"plural":"kediler","vowelHarmony":"ince","cases":{"nominative":"kedi","accusative":"kediyi","dative":"kediye","locative":"kedide","ablative":"kediden","genitive":"kedinin"},"possessive":{"ben":"kedim","sen":"kedin","o":"kedisi","biz":"kedimiz","siz":"kediniz","onlar":"kedileri"},"verbRoot":"gel"}""";

        var result = validator.Validate(new WordGrammarInput("kedi", "Katze", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_NOUN_VERB_FIELDS_FORBIDDEN");
    }

    // --- tr × Verb ---

    [Fact]
    public void Validate_TrVerbValidData_IsValid()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Verb);

        var result = validator.Validate(new WordGrammarInput("gelmek", "kommen", Parse(ValidTrVerbJson())));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TrVerbMissingVerbRoot_ReturnsTrVerbRootRequired()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Verb);
        var json = $$"""{"negativeForm":"gelmemek","conjugation":{{Conjugation(TrVerbTenses, TrPersons)}}}""";

        var result = validator.Validate(new WordGrammarInput("gelmek", "kommen", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_VERB_ROOT_REQUIRED");
    }

    [Fact]
    public void Validate_TrVerbMissingNegativeForm_ReturnsTrVerbNegativeFormRequired()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Verb);
        var json = $$"""{"verbRoot":"gel","conjugation":{{Conjugation(TrVerbTenses, TrPersons)}}}""";

        var result = validator.Validate(new WordGrammarInput("gelmek", "kommen", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_VERB_NEGATIVE_FORM_REQUIRED");
    }

    [Fact]
    public void Validate_TrVerbIncompleteConjugation_ReturnsTrVerbConjugationIncomplete()
    {
        // ARRANGE — beş zamandan yalnızca biri (aorist) dolu
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Verb);
        var json = """{"verbRoot":"gel","negativeForm":"gelmemek","conjugation":{"aorist":{"ben":"gelirim","sen":"gelirsin","o":"gelir","biz":"geliriz","siz":"gelirsiniz","onlar":"gelirler"}}}""";

        var result = validator.Validate(new WordGrammarInput("gelmek", "kommen", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_VERB_CONJUGATION_INCOMPLETE");
    }

    [Fact]
    public void Validate_TrVerbWithNounField_ReturnsTrVerbNounFieldsForbidden()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Verb);
        var json = $$"""{"verbRoot":"gel","negativeForm":"gelmemek","conjugation":{{Conjugation(TrVerbTenses, TrPersons)}},"plural":"gelmekler"}""";

        var result = validator.Validate(new WordGrammarInput("gelmek", "kommen", Parse(json)));

        result.Errors.Should().Contain(e => e.ErrorCode == "TR_VERB_NOUN_FIELDS_FORBIDDEN");
    }

    // --- tr × Diğer ---

    [Fact]
    public void Validate_TrOtherWithNullGrammarData_IsValid()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Conjunction);

        var result = validator.Validate(new WordGrammarInput("ama", "aber, doch", null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TrOtherWithGrammarData_ReturnsGrammarDataMustBeNull()
    {
        var validator = new WordGrammarValidator("tr", PartOfSpeech.Conjunction);

        var result = validator.Validate(new WordGrammarInput("ama", "aber, doch", Parse("""{"plural":"x"}""")));

        result.Errors.Should().Contain(e => e.ErrorCode == "GRAMMAR_DATA_MUST_BE_NULL");
    }
}
