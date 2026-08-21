namespace Zausel.Application.Services;

public static class SrsCalculator
{
    // SM-2 — quality: 0-5 (kullanıcı öz değerlendirmesi)
    public static (int intervalDays, int newLevel, decimal newEF) Calculate(
        int currentLevel, int repetitionNumber, decimal easinessFactor, int quality)
    {
        // quality < 3 → yanlış/çok zor → başa dön (EF düşer ama 1.3'ün altına inmez)
        if (quality < 3)
            return (1, 0, Math.Max(1.3m, easinessFactor - 0.2m));

        int interval = repetitionNumber == 0 ? 1
                     : repetitionNumber == 1 ? 3
                     : (int)Math.Round((repetitionNumber - 1) * easinessFactor);

        // EF = EF + (0.1 - (5-q)*(0.08 + (5-q)*0.02))
        decimal newEF = easinessFactor + (0.1m - (5 - quality) * (0.08m + (5 - quality) * 0.02m));
        newEF = Math.Max(1.3m, newEF);
        return (interval, Math.Min(currentLevel + 1, 5), newEF);
    }

    // Mastery: yüzdelik (0-100), CurrentLevel baskın + SuccessRate ince ayar
    public static decimal CalculateMastery(int currentLevel, decimal successRate) =>
        Math.Round((currentLevel / 5.0m) * 80 + (successRate / 100.0m) * 20, 2);
}
