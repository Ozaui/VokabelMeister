namespace Zausel.Application.DTOs.Progress;

// ProgressWordResponse'tan farkı: NextReviewAt YOK (askıya alınmış bir kelimenin zaten
// zamanlanmış bir tekrarı yoktur), ConsecutiveIncorrect VAR (kullanıcı "bu kelime kaç kez
// üst üste yanlış gitti" bilgisini görür — leech-action kararını bu sayı üzerinden verir).
public record SuspendedWordResponse(
    int WordId, string Text, string? Definition, int CurrentLevel, decimal Mastery, int ConsecutiveIncorrect);
