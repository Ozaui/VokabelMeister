using FluentAssertions;
using Zausel.Application.Services;

namespace Zausel.Tests.Services;

public class SrsCalculatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Calculate_QualityBelowThree_ResetsIntervalAndLevelAndDecreasesEF(int quality)
    {
        // ARRANGE — 0/1/2'nin hepsi "başarısız" sayılır, hangisi olursa olsun AYNI sonucu üretmeli
        // ACT
        var (intervalDays, newLevel, newEF) = SrsCalculator.Calculate(
            currentLevel: 3, repetitionNumber: 4, easinessFactor: 2.5m, quality);

        // ASSERT — kelime "yeni öğreniliyormuş" gibi baştan başlar, EF 0.2 düşer
        intervalDays.Should().Be(1);
        newLevel.Should().Be(0);
        newEF.Should().Be(2.3m);
    }

    [Fact]
    public void Calculate_QualityBelowThree_EasinessFactorNeverDropsBelowFloor()
    {
        // ARRANGE — EF zaten tabana yakın (1.35), 0.2 düşse 1.15 olur ama 1.3'ün altına inmemeli
        // ACT
        var (_, _, newEF) = SrsCalculator.Calculate(
            currentLevel: 2, repetitionNumber: 3, easinessFactor: 1.35m, quality: 0);

        // ASSERT
        newEF.Should().Be(1.3m);
    }

    [Fact]
    public void Calculate_FirstRepetition_ReturnsOneDayInterval()
    {
        // ARRANGE — repetitionNumber=0, henüz hiç başarılı tekrar yok
        // ACT
        var (intervalDays, _, _) = SrsCalculator.Calculate(
            currentLevel: 0, repetitionNumber: 0, easinessFactor: 2.5m, quality: 4);

        // ASSERT — EF'ten BAĞIMSIZ, sabit 1 gün
        intervalDays.Should().Be(1);
    }

    [Fact]
    public void Calculate_SecondRepetition_ReturnsThreeDayInterval()
    {
        // ARRANGE — repetitionNumber=1, SM-2'nin sabit ikinci aralığı
        // ACT
        var (intervalDays, _, _) = SrsCalculator.Calculate(
            currentLevel: 1, repetitionNumber: 1, easinessFactor: 2.5m, quality: 4);

        // ASSERT — EF'ten BAĞIMSIZ, sabit 3 gün
        intervalDays.Should().Be(3);
    }

    [Fact]
    public void Calculate_ThirdRepetitionOnward_MultipliesPriorRepetitionsByEasinessFactor()
    {
        // ARRANGE — repetitionNumber=3 → (3-1)*2.36 = 4.72
        // ACT
        var (intervalDays, _, _) = SrsCalculator.Calculate(
            currentLevel: 2, repetitionNumber: 3, easinessFactor: 2.36m, quality: 4);

        // ASSERT — 4.72 normal yuvarlamayla 5'e gider
        intervalDays.Should().Be(5);
    }

    [Fact]
    public void Calculate_HalfwayIntervalResult_RoundsToNearestEven()
    {
        // ARRANGE — repetitionNumber=2 → (2-1)*2.5 = 2.5 TAM ORTADA, Math.Round bankacı yuvarlaması kullanır
        // ACT
        var (intervalDays, _, _) = SrsCalculator.Calculate(
            currentLevel: 2, repetitionNumber: 2, easinessFactor: 2.5m, quality: 4);

        // ASSERT — 2.5, en yakın ÇİFT sayı olan 2'ye yuvarlanır (3'e DEĞİL) — .NET'in decimal
        // Math.Round varsayılanı MidpointRounding.ToEven, "her zaman yukarı" DEĞİL
        intervalDays.Should().Be(2);
    }

    [Theory]
    [InlineData(5, 2.6)] // çok kolay → EF artar
    [InlineData(4, 2.5)] // iyi → EF değişmez
    [InlineData(3, 2.36)] // zar zor doğru → EF azalır
    public void Calculate_SuccessfulQuality_AdjustsEasinessFactorPerSm2Formula(int quality, decimal expectedEF)
    {
        // ARRANGE — repetitionNumber=5 (interval formülüyle karışmasın diye 0/1 dışında sabit bir değer)
        // ACT
        var (_, _, newEF) = SrsCalculator.Calculate(
            currentLevel: 2, repetitionNumber: 5, easinessFactor: 2.5m, quality);

        // ASSERT
        newEF.Should().Be(expectedEF);
    }

    [Fact]
    public void Calculate_SuccessfulAnswer_EasinessFactorFloorAppliesOnSuccessToo()
    {
        // ARRANGE — quality=3'ün EF deltası -0.14, EF zaten 1.35 → 1.21 olur, 1.3'ün altına inmemeli
        // ACT
        var (_, _, newEF) = SrsCalculator.Calculate(
            currentLevel: 1, repetitionNumber: 5, easinessFactor: 1.35m, quality: 3);

        // ASSERT — taban yalnızca quality<3 dalında DEĞİL, başarılı dalda da geçerli
        newEF.Should().Be(1.3m);
    }

    [Fact]
    public void Calculate_SuccessfulAnswer_IncreasesLevelByOne()
    {
        // ARRANGE + ACT
        var (_, newLevel, _) = SrsCalculator.Calculate(
            currentLevel: 2, repetitionNumber: 5, easinessFactor: 2.5m, quality: 4);

        // ASSERT
        newLevel.Should().Be(3);
    }

    [Fact]
    public void Calculate_LevelAtMaximum_DoesNotExceedFive()
    {
        // ARRANGE — currentLevel zaten 5 (otomatik hatırlama, en üst seviye)
        // ACT
        var (_, newLevel, _) = SrsCalculator.Calculate(
            currentLevel: 5, repetitionNumber: 5, easinessFactor: 2.5m, quality: 5);

        // ASSERT — 6'ya değil, 5'te KALIR
        newLevel.Should().Be(5);
    }

    [Theory]
    [InlineData(5, 100, 100.00)] // en üst seviye + kusursuz başarı → tam ustalık
    [InlineData(0, 0, 0.00)] // hiç ilerlememiş + hiç başarı yok → sıfır
    [InlineData(3, 50, 58.00)] // orta seviye + orta başarı
    [InlineData(2, 73.5, 46.70)] // level baskın (32) + successRate ince ayar (14.7)
    public void CalculateMastery_VariousLevelsAndSuccessRates_ReturnsWeightedPercentage(
        int currentLevel, decimal successRate, decimal expectedMastery)
    {
        // ARRANGE + ACT
        var mastery = SrsCalculator.CalculateMastery(currentLevel, successRate);

        // ASSERT — %80 level + %20 successRate ağırlıklı ortalama, 2 ondalığa yuvarlı
        mastery.Should().Be(expectedMastery);
    }
}
