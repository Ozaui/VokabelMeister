namespace Zausel.Application.Common;

// Achievements statik seed/referans tablosu olduğu için (Languages'teki Id=1/Id=2 ile AYNI desen)
// Id'ler burada sabitlenir — AchievementConfiguration'ın HasData'sı, AchievementMessages'ın sözlük
// anahtarları VE AchievementService'in eşik kontrolleri AYNI sabitlere referans verir, birden çok
// yerde "magic number" tekrarlanmaz.
public static class AchievementIds
{
    public const int Streak3 = 1;
    public const int Streak7 = 2;
    public const int Streak30 = 3;
    public const int WordCount50 = 4;
    public const int WordCount200 = 5;
    public const int WordCount500 = 6;
    public const int FirstMastery = 7;
    public const int GoodBand100 = 8;
    public const int FlawlessSession = 9;
    public const int LeechRecovery = 10;
}
