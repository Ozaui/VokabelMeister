namespace Zausel.Application.DTOs.Progress;

public record LearnSystemWordResponse(int UserProgressId, int WordId, string GermanWord, bool AlreadyExists);
