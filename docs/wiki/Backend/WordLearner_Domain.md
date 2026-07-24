# WordLearner.Domain

**Özet:** En iç katman — Entity'ler ve enum'lar burada yaşar, hiçbir dış pakete veya başka katmana bağımlı değildir. [[BaseEntity]] kökte, dört domain kendi alt klasöründe: Auth (`User`/`RefreshToken`/`QrLoginSession`), Logging (`ActivityLog`/`ApplicationLog`/`SecurityLog` — **`BaseEntity`den TÜREMEZ**, insert-only), Words (`Language`/`WordConcept`/`Word`/`WordDetail`/`WordExample` — `Language` de `BaseEntity`siz istisna), Categories (`Category`/`CategoryTranslation`/`WordCategory`). Tam şema → [[Veritabani_Semasi]].
**Kütüphaneler:** Saf C# — hiçbir NuGet paketi yok
**Bağlantılar:** [[BaseEntity]] · [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Veritabani_Semasi]] · [[Backend_Katmanli_Mimari]] · [[Auth_Domain]] · [[Loglama_Domain]] · [[Icerik_Domain]]

## Proje Referansları
`WordLearner.Domain.csproj` → (bağımsız, hiçbir projeye referans vermez)

## Klasör Yapısı (mevcut, A-06 itibarıyla)
```
Entities/
  BaseEntity.cs          ← domain'e özgü değil, her zaman kökte kalır
  Auth/
    User.cs
    RefreshToken.cs
    QrLoginSession.cs
  Logging/                ← BaseEntity'den TÜREMEZ (insert-only, soft delete yok)
    ActivityLog.cs
    ApplicationLog.cs
    SecurityLog.cs
  Words/
    Language.cs           ← BaseEntity'siz istisna (statik seed/referans tablosu)
    WordConcept.cs
    Word.cs
    WordDetail.cs
    WordExample.cs
  Categories/
    Category.cs            ← self-referencing (ParentCategoryId)
    CategoryTranslation.cs
    WordCategory.cs         ← M:N ara tablo (Word↔Category)
Enums/
  Auth/
    OtpPurpose.cs
    QrLoginStatus.cs
  Logging/
    LogEventType.cs         ← HasConversion<string> + DB check constraint, iki katmanlı korunur
```
**Kural (bkz. `wiki/Standartlar/Kodlama_Standartlari.md` "Klasör Organizasyonu"):** `Entities/` ve
`Enums/` flat kalmaz — her yeni domain kendi `<Domain>/` alt klasörünü ve namespace'ini
(`WordLearner.Domain.Entities.<Domain>`, `...Enums.<Domain>`) alır. Yalnızca `BaseEntity.cs`
istisna, her zaman `Entities/` kökünde kalır. Bu konvansiyon `Application/Features/<Domain>/`,
`Application/DTOs/<Domain>/` ile tutarlıdır.

## Planlanan Genişleme
Sıradaki A-07 (Admin API) yeni bir entity eklemiyor (mevcut `User`'ı genişletiyor); C fazı
(Kullanıcı Backend) `UserCard`, `UserCategory`, `UserProgress`, `Class`, ... ile kendi domain alt
klasörlerini ekleyecek. Tam liste → [[Veritabani_Semasi]].

## Dosyalar
- [[BaseEntity]] — tüm entity'lerin türeyeceği soyut taban sınıf (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
- `Entities/Auth/User.cs`, `RefreshToken.cs`, `QrLoginSession.cs` — bkz. [[Auth_Domain]]
- `Enums/Auth/OtpPurpose.cs`, `QrLoginStatus.cs` — bkz. [[Auth_Domain]]
- `Entities/Logging/ActivityLog.cs`, `ApplicationLog.cs`, `SecurityLog.cs` + `Enums/Logging/LogEventType.cs` — bkz. [[Loglama_Domain]]
- `Entities/Words/Language.cs`, `WordConcept.cs`, `Word.cs`, `WordDetail.cs`, `WordExample.cs` — bkz. [[Icerik_Domain]]
- `Entities/Categories/Category.cs`, `CategoryTranslation.cs`, `WordCategory.cs` — bkz. [[Icerik_Domain]]
