using System.Text.Json;

namespace Zausel.Application.DTOs.Words;

public record WordDetailResponse(string? Pronunciation, string? Notes, string? CommonMistakes, JsonElement? GrammarData);
