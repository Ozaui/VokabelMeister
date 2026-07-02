# SRS / İlerleme Domain (Spaced Repetition)

**Özet:** SM-2 algoritmasına dayalı aralıklı tekrar sistemi — sistem kelimeleri (`UserProgress`) ve kişisel kartlar (`UserCardProgress`) için **ayrı** ama şema olarak aynı ilerleme tabloları tutulur. Kod olarak **C-03 (SRS/İlerleme API)**'de yazılacak; `SrsCalculator` yardımcı sınıfı bu domain'in kalbidir.
**Kütüphaneler:** —
**Bağlantılar:** [[Veritabani_Semasi]] · [[Icerik_Domain]] · [[Kisisel_Icerik_Domain]] · [[Kodlama_Standartlari]] · [[Teknik_Ozellikler]]

## UserProgress (sistem kelimesi) / UserCardProgress (kişisel kart)
Aynı alan seti: `CurrentLevel` (0=hiç görülmedi..5=otomatik hatırlama), `Mastery`,
`EasinessFactor` (SM-2, varsayılan 2.5), `TimesCorrect`/`TimesIncorrect`/`TotalAttempts`/
`SuccessRate`, `LastReviewedAt`, `NextReviewAt`, `IntervalDays`, `RepetitionNumber`.
Benzersizlik: `UNIQUE(UserId, WordId)` / `UNIQUE(UserId, UserCardId)`.

## LearningSessions / LearningHistory
`LearningSessions`: `SessionType` (`Flashcard|MultipleChoice|ArticleQuiz|PluralQuiz|
TranslationQuiz`), `SourceType` (`SystemWords|UserCards|Mixed`), filtreler (LevelFilter/
CategoryIds/UserCategoryIds JSON), `Status` (`Active|Completed|Abandoned`), XP/başarı özeti.
`LearningHistory`: her cevabın kalıcı kaydı — `WordId`/`UserCardId` (ikisi birden NULL olamaz),
`IsCorrect`, `ResponseTime`, `UserResponse`/`CorrectResponse`.

## Achievements / UserAchievements
Basit gamification: `Name`/`Description`/`RewardXP`/`Rarity` (`Common|Rare|Epic|Legendary`) +
kullanıcı-başarı eşleştirme tablosu.

## Planlanan Kod (C-03)
`UserProgress`/`UserCardProgress`/`LearningHistory` entity → `SrsCalculator` (SM-2: interval,
easiness factor, mastery 0-5; quality<3 → sıfırlama, EF alt sınır 1.3) → `IProgressService`/
`ProgressService` (XP, streak) → `ProgressController`. Birim test önceliği: `SrsCalculatorTests`.

## SM-2 Öz Değerlendirme Skalası
`selfRating`: 🔴0=Bilmedim · 🟠2=Zor · 🟢4=İyi · 🔵5=Çok Kolay — bkz. `docs/REFERENCE/API_ENDPOINTS.md §9`.

## SrsCalculator — Referans Kod (henüz yazılmadı, `docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md §8`'den)
```csharp
public static class SrsCalculator
{
    public static (int intervalDays, int newLevel, decimal newEF) Calculate(
        int currentLevel, int repetitionNumber, decimal easinessFactor, int quality)
    {
        if (quality < 3)   // yanlış/çok zor → başa dön, EF 1.3 altına inmez
            return (1, 0, Math.Max(1.3m, easinessFactor - 0.2m));

        int interval = repetitionNumber == 0 ? 1
                     : repetitionNumber == 1 ? 3
                     : (int)Math.Round((repetitionNumber - 1) * easinessFactor);

        decimal newEF = easinessFactor + (0.1m - (5 - quality) * (0.08m + (5 - quality) * 0.02m));
        return (interval, Math.Min(currentLevel + 1, 5), Math.Max(1.3m, newEF));
    }
}
```
Tam bağlam ve diğer referans kod örnekleri (JWT, Şifre servisi, Serilog) → [[Teknik_Ozellikler]].
