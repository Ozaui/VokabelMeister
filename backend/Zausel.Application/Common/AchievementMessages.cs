namespace Zausel.Application.Common;

// ErrorMessages/SuccessMessages ile simetrik: Achievement tablosunda Name/Description sütunu YOK —
// rozetin görünen adı/açıklaması Accept-Language'a göre bu sözlükten çözülür. Yeni dil = ErrorMessages
// ile AYNI desen, sözlüğe yeni bir sütun eklemek (migration gerekmez).
public static class AchievementMessages
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<int, (string Name, string Description)>> Messages = new()
    {
        [DefaultLanguage] = new()
        {
            [AchievementIds.Streak3] = ("3 Günlük Seri", "3 gün üst üste çalıştın."),
            [AchievementIds.Streak7] = ("7 Günlük Seri", "7 gün üst üste çalıştın."),
            [AchievementIds.Streak30] = ("30 Günlük Seri", "30 gün üst üste çalıştın."),
            [AchievementIds.WordCount50] = ("50 Kelime", "50 kelime öğrenmeye başladın."),
            [AchievementIds.WordCount200] = ("200 Kelime", "200 kelime öğrenmeye başladın."),
            [AchievementIds.WordCount500] = ("500 Kelime", "500 kelime öğrenmeye başladın."),
            [AchievementIds.FirstMastery] = ("İlk Ustalık", "Bir kelimeyi ilk kez otomatik hatırlama seviyesine (5) taşıdın."),
            [AchievementIds.GoodBand100] = ("100 Kelime İyi Bantta", "100 kelime %70 ve üzeri ustalık seviyesine ulaştı."),
            [AchievementIds.FlawlessSession] = ("Hatasız Oturum", "Bir öğrenme oturumunu hiç hata yapmadan tamamladın."),
            [AchievementIds.LeechRecovery] = ("Leech Kurtarma", "Takılıp kaldığın bir kelimeyi sıfırlayıp yeniden başlattın."),
        },
        ["de"] = new()
        {
            [AchievementIds.Streak3] = ("3-Tage-Serie", "Du hast 3 Tage in Folge gelernt."),
            [AchievementIds.Streak7] = ("7-Tage-Serie", "Du hast 7 Tage in Folge gelernt."),
            [AchievementIds.Streak30] = ("30-Tage-Serie", "Du hast 30 Tage in Folge gelernt."),
            [AchievementIds.WordCount50] = ("50 Wörter", "Du hast begonnen, 50 Wörter zu lernen."),
            [AchievementIds.WordCount200] = ("200 Wörter", "Du hast begonnen, 200 Wörter zu lernen."),
            [AchievementIds.WordCount500] = ("500 Wörter", "Du hast begonnen, 500 Wörter zu lernen."),
            [AchievementIds.FirstMastery] = ("Erste Meisterschaft", "Du hast ein Wort zum ersten Mal auf Stufe 5 (automatisches Erinnern) gebracht."),
            [AchievementIds.GoodBand100] = ("100 Wörter im guten Bereich", "100 Wörter haben einen Beherrschungsgrad von 70% oder mehr erreicht."),
            [AchievementIds.FlawlessSession] = ("Fehlerfreie Sitzung", "Du hast eine Lernsitzung ohne Fehler abgeschlossen."),
            [AchievementIds.LeechRecovery] = ("Leech-Rettung", "Du hast ein feststeckendes Wort zurückgesetzt und neu gestartet."),
        },
    };

    public static (string Name, string Description) Resolve(int achievementId, string? language)
    {
        var resolvedLanguage = language is not null && Messages.ContainsKey(language) ? language : DefaultLanguage;
        return Messages[resolvedLanguage].TryGetValue(achievementId, out var text) ? text : Messages[DefaultLanguage][achievementId];
    }
}
