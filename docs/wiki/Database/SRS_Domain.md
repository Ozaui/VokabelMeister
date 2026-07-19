# SRS / İlerleme Domain (Spaced Repetition)

**Özet:** SM-2 algoritmasına dayalı aralıklı tekrar sistemi — sistem kelimeleri (`UserProgress`) ve kişisel kartlar (`UserCardProgress`) için **ayrı** ama şema olarak aynı ilerleme tabloları tutulur. Kod olarak **C-03 (SRS/İlerleme API)**'de yazılacak; `SrsCalculator` yardımcı sınıfı bu domain'in kalbidir.
**Kütüphaneler:** —
**Bağlantılar:** [[Veritabani_Semasi]] · [[Icerik_Domain]] · [[Kisisel_Icerik_Domain]] · [[Kodlama_Standartlari]] · [[Teknik_Ozellikler]]

## UserProgress (sistem kelimesi) / UserCardProgress (kişisel kart)
Aynı alan seti: `CurrentLevel` (0=hiç görülmedi..5=otomatik hatırlama), `Mastery`,
`EasinessFactor` (SM-2, varsayılan 2.5), `TimesCorrect`/`TimesIncorrect`/`TotalAttempts`/
`SuccessRate`, `LastReviewedAt`, `NextReviewAt`, `IntervalDays`, `RepetitionNumber`.
Benzersizlik: `UNIQUE(UserId, WordId)` / `UNIQUE(UserId, UserCardId)`.

**Yön/hedef dil (Yirmi dokuzuncu INGEST):** `WordId` zaten **dile özel** bir satır (bir `WordConcept`'in
`de` karşılığı ve `tr` karşılığı ayrı `Words.Id`'lerdir) — bu yüzden aynı kullanıcı aynı kavramı hem
`de→tr` hem `tr→de` yönünde öğrenirse, iki ayrı `UserProgress` satırına düşer (Almanca `WordId`'sinde
biri, Türkçe `WordId`'sinde diğeri) ve **birbirinden bağımsız ilerler** — kullanıcı profilinde sabit
bir "hedef dil" alanı olmadığı için bu şema **hiç değişmeden** iki yönü de destekliyor. Hangi yönün
aktif olduğu `LearningSessions.TargetLanguageId`'de tutulur (bkz. [[Icerik_Domain]] "Eşleştirme").

### Mastery Formülü (yüzdelik, 0-100)
`Mastery` alanı şemada vardı ama formülü tanımsızdı. Karar: `CurrentLevel` baskın sinyal
(SM-2'nin doğrudan çıktısı), `SuccessRate` ince ayar:
```
Mastery = round((CurrentLevel / 5.0) * 80 + (SuccessRate / 100.0) * 20, 2)
```
Her `SrsCalculator.Calculate()` çağrısından sonra `ProgressService` bu formülle `Mastery`'i
yeniden hesaplar ve kaydeder.

### Mastery Bantları (Zayıf/Orta/İyi)
Bant eşiği `CurrentLevel` değil **`Mastery` yüzdesi** üzerinden — aynı metrik hem bant
gösterimini hem "Bugün Test Ettiklerim" öncesi/sonrası yüzdesini besliyor, tek kaynak.
- 🔴 **Zayıf**: `Mastery` 0-40
- 🟡 **Orta**: `Mastery` 40-70
- 🟢 **İyi**: `Mastery` 70-100
(Eşik sayıları config değeri, ihtiyaca göre ayarlanabilir — yapısal bir karar değil.)

## LearningSessions / LearningHistory
`LearningSessions`: `SessionType` (`Flashcard|MultipleChoice|ArticleQuiz|PluralQuiz|
TranslationQuiz|TrueFalse`), `SourceType` (`SystemWords|UserCards|Mixed`), filtreler (LevelFilter/
CategoryIds/UserCategoryIds JSON), `Status` (`Active|Completed|Abandoned`), XP/başarı özeti.
`LearningHistory`: her cevabın kalıcı kaydı — `WordId`/`UserCardId` (ikisi birden NULL olamaz),
`IsCorrect`, `ResponseTime`, `UserResponse`/`CorrectResponse`, artı yeni alanlar:
`HintUsed` (BIT), `IsExtraPractice` (BIT), `MasteryBefore`/`MasteryAfter` (DECIMAL(5,2), NULL —
yalnızca `IsExtraPractice=0` olan resmi review'larda doldurulur).

### Quiz Formatı — Kullanıcı Seçmez, Backend Rastgele Atar
`POST /learning-sessions` artık istemciden `sessionType` almıyor. Yeni kelime tanıtımı her
zaman `Flashcard` (gösterim, quiz yok). Review/tekrar oturumlarında (due, bant pratiği, karışık)
her soru için backend `MultipleChoice|TranslationQuiz|ArticleQuiz|PluralQuiz|TrueFalse` arasından
**rastgele** bir format seçer ve o sorunun gerçek tipini `LearningHistory.SessionType`'a yazar;
`LearningSessions.SessionType` oturum düzeyinde sabit `Mixed` kalır. Kullanıcıya "hangi tip sınav"
diye sorulmaz.

### Quality (0-5) Nasıl Üretilir
- **Flashcard (yeni kelime, öz-değerlendirme):** Kullanıcı `selfRating` seçer ama seçenekler
  **tavanla sınırlanır**: cevap gecikirse (zaman eşiği) "Çok Kolay" kilitlenir; ipucu (örnek
  cümle) istenirse "İyi" de kilitlenir (sadece Bilmedim/Zor kalır); "Cevabı Göster"e basılırsa
  hiç seçim sorulmaz, `quality` otomatik 0.
- **Objektif tipler (MultipleChoice/TranslationQuiz/ArticleQuiz/PluralQuiz):** `quality`
  kullanıcıya sorulmadan `IsCorrect` + `ResponseTime` + `HintUsed`'dan otomatik türetilir:
  yanlış→0, ipucusuz+hızlı doğru→5, ipucuyla veya yavaş doğru→3-4.
- **TrueFalse:** Şans başarı ihtimali (%50) diğer tiplerden (MultipleChoice %25) yüksek olduğu
  için doğru cevaplarda otomatik `quality` tavanı **4** — asla otomatik 5 verilmez.

### Tek Kelime, Günde Tek Resmi Review Kuralı
Bir kelime bir günde **ilk kez** cevaplandığında (kaynağı due-kuyruğu, bant pratiği veya karışık
fark etmez) SM-2 normal günceller (`NextReviewAt`/`EF`/`Mastery`). Aynı gün aynı kelime tekrar
cevaplanırsa — özellikle "Aynı Kelimelerle Tekrar Et" akışında — SM-2 **bir daha güncellenmez**;
o cevap `LearningHistory`'ye `IsExtraPractice=1` ile sadece istatistik olarak yazılır
(`MasteryBefore`/`MasteryAfter` bu satırlarda NULL kalır). Bu, hem "bant pratiği SM-2'yi etkilesin
mi" hem "tekrar oynama SM-2'yi bozar mı" sorularını tek kuralla çözüyor.

## Yeni Kelime Seçim Algoritması
"Seviyeye göre + database sırasına göre" kuralı, sıfırlanan kelimelerin doğal olarak eski
sırasına geri dönmesini de kapsayacak şekilde:
```sql
SELECT TOP(@dailyWordGoal) w.Id
FROM WordConcepts wc
JOIN Words w ON w.WordConceptId = wc.Id AND w.LanguageId = @targetLanguageId
LEFT JOIN UserProgress up ON up.WordId = w.Id AND up.UserId = @userId
WHERE wc.DifficultyLevel = @userCurrentLevel   -- Users.CurrentLevel
  AND (up.Id IS NULL OR up.NextReviewAt IS NULL)
ORDER BY wc.Id ASC
```
**Kritik nokta:** filtre `CurrentLevel=0` değil **`NextReviewAt IS NULL`** üzerinden. Sebep: normal
bir due-review'da kelime yanlış cevaplanırsa SM-2 zaten `CurrentLevel`'ı 0'a döndürür ama
`NextReviewAt`'i yarına ayarlar — o kelime hâlâ due-kuyruğunda kalmalı, yeni kelime havuzuna
sızmamalı. `CurrentLevel=0` filtresi bunu yanlışlıkla yeni kelime sanardı. `NextReviewAt IS NULL`
yalnızca (a) hiç görülmemiş kelimelerde ve (b) leech "Sıfırla" aksiyonuyla bilinçli olarak
`NULL`'a çekilmiş kelimelerde doğru olur — bu yüzden sıfırlanan bir kelime `WordConceptId` sırasına
göre kendi eski (aradaki) pozisyonundan tekrar çıkar, sona atılmaz.

## Leech Tespiti (Zor Kelime)
Yeni alanlar (`UserProgress`/`UserCardProgress`): `ConsecutiveIncorrect INT` (her `quality<3`'te
artar, `quality>=3`'te 0'a döner), `IsSuspended BIT`.

**Eşik: 5 ardışık yanlış.** (Anki'nin varsayılanı 8 ama o *kümülatif* lapse sayısı, aralarda doğru
gelse de sıfırlanmaz; bizimki *ardışık* — daha katı/hızlı tetiklenen bir metrik olduğu için denk
hassasiyet için daha düşük bir eşik gerekiyor.)

`ConsecutiveIncorrect >= 5` olduğunda kullanıcıya üç seçenek sunulur:
- **Askıya Al** — `IsSuspended=1`. Due sorgusu artık `AND IsSuspended=0` içerir, bu kelime
  kuyruğa girmez. "Askıya Alınanlar" listesinden istediği an geri getirebilir (`IsSuspended=0`).
- **Sıfırla, Yeniden Öğren** — `CurrentLevel=0`, `EasinessFactor=2.5`, `RepetitionNumber=0`,
  `IntervalDays=1`, `LastReviewedAt=NULL`, `NextReviewAt=NULL`, `ConsecutiveIncorrect=0`.
  `TimesCorrect`/`TimesIncorrect`/`TotalAttempts`/`SuccessRate` (geçmiş istatistik) **korunur**,
  yalnızca ilerleme durumu sıfırlanır. `NextReviewAt=NULL` olduğu için kelime yeni kelime
  havuzuna geri döner (bkz. yukarısı).
- **Böyle Devam Et** — hiçbir alan değişmez, sadece bilgilendirme kapatılır.

Bant ekranında (🔴 Zayıf) leech kelimeler `ConsecutiveIncorrect>=5` koşuluyla ayrı bir işaretle (🩹)
gösterilir.

## Achievements — Tetikleme Kuralları ve Görsel
`Achievements.Icon` (mevcut `NVARCHAR(255)`) emoji değil **resim URL'i** olarak kullanılır
(`WordConcepts.ImageUrl` ile aynı desen). Admin CRUD ekranı şimdilik yok (YAGNI) — başlangıç seti
migration seed data ile girilir, ihtiyaç doğarsa admin ekranı sonra eklenir.

Başlangıç seti (`Rarity`: Common/Rare/Epic):
- Streak: 3 gün / 7 gün / 30 gün (Common/Rare/Epic)
- Kelime sayısı: 50 / 200 / 500 kelime öğrenildi (`CurrentLevel>=1` sayısı)
- "İlk Ustalaşma": ilk kez bir kelime `CurrentLevel=5`'e ulaştı
- "Ustalar Kulübü": 100 kelime İyi bantta (`Mastery>=70`)
- "Hatasız Oturum": bir `LearningSession`'da %100 doğru
- "Pes Etmedim": askıya alınmış veya sıfırlanmış bir kelimeyi sonradan İyi bandına taşımak

Tetikleme noktaları: `ProgressService` her cevap işlendikten sonra (mastery/level bazlı olanlar),
`LearningSessionService.CompleteSession()` sonrası (streak/hatasız oturum bazlı olanlar). Basit
if-zinciri yeterli, ayrı bir rule-engine gerekmiyor.

## Bildirim Tetikleyicileri (C-10)
Zamanlama altyapısı: **Hangfire** (SQL Server storage provider) — mevcut MSSQL'e ek bir servis
kurmadan recurring job'ları DB'de persist eder, dashboard'u var, API restart'ta job kaybolmaz.
Quartz.NET/elle `IHostedService` yerine tercih edildi (detay → [[Teknik_Ozellikler]]).

Somut tetikleyiciler (günde bir taranan recurring job'lar):
- **Günlük hatırlatma**: akşam bir saate kadar (config, örn. 20:00) o günkü `dailyWordGoal`
  tamamlanmadıysa
- **Due hatırlatması**: due sayısı bir eşiği (config, örn. 10) geçince günde en fazla 1 kez
- **Streak riski**: streak'i olan kullanıcı gün sonuna yaklaşırken (config, örn. son 2 saat)
  hedefini tamamlamadıysa
- **Achievement bildirimi**: yeni rozet kazanıldığı anda anlık (recurring job değil, event-driven)

## Günlük Akış Kuralları
- **Streak yalnızca günlük yeni kelime hedefine (`dailyWordGoal`, `CurrentLevel=0`→1 geçişi)
  bağlı.** Due review/bant pratiği yapılsın yapılmasın streak etkilenmez.
- **Due rozeti** (`NextReviewAt<=now` sayısı) ana ekranda her zaman pasif görünür, tıklanması
  zorunlu değil.
- Günlük hedef tamamlanınca **opsiyonel** bir teklif çıkar: "X kelimen tekrar bekliyor, tekrar
  edelim mi?" — kabul/red streak'i etkilemez.
- **Oturum boyutu:** Yeni kelime oturumu `dailyWordGoal`'e sabit; due review varsayılan bir
  üst sınırla (config, örn. 20) başlar, biterse "devam?" sorulur; bant pratiğinde kullanıcı
  kendi sayıyı seçer (10/20/30/Tümü).

## Bant Ekranları ve Günlük Özet Listeleri
- **Bant ekranı** (🔴🟡🟢): iki giriş noktası — **İncele** (salt okunur liste: kelime, çeviri,
  son görülme, `Mastery`) ve **Sına** (review akışını o bant kaynağıyla başlatır, günlük hedefe
  saymaz, normal SM-2 güncellemesi olur — bkz. "Tek Kelime, Günde Tek Resmi Review Kuralı").
- **"Bugün Öğrendiklerim":** o gün `CurrentLevel 0→1` olan kelimeler, **seviye/bant gösterilmez**
  (ilk tanışmada bant anlamsız/caydırıcı olur).
- **"Bugün Test Ettiklerim":** o gün resmi review alan (`IsExtraPractice=0`) kelimeler,
  `MasteryBefore → MasteryAfter` yüzdelik gösterimle (örn. "%62 → %78").

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
