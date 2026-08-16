using System.Text.Json;

namespace WordLearner.Application.DTOs.Words;

// WordGrammarValidator'ın doğruladığı tek dildeki karşılık — hiçbir Command'a bağlı değil, bu
// yüzden validator Command'lardan bağımsız test edilebiliyor.
public record WordGrammarInput(string Text, string? Definition, JsonElement? GrammarData);
