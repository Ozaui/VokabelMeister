# WordLearner.Domain

**Özet:** En iç katman — Entity'ler ve enum'lar burada yaşar, hiçbir dış pakete veya başka katmana bağımlı değildir. [[BaseEntity]] kökte, domain'e özgü entity/enum'lar (Auth: `User`, `RefreshToken`, `QrLoginSession`, `OtpPurpose`, `QrLoginStatus`) kendi alt klasöründe (planlanan tam şema → [[Veritabani_Semasi]]).
**Kütüphaneler:** Saf C# — hiçbir NuGet paketi yok
**Bağlantılar:** [[BaseEntity]] · [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Veritabani_Semasi]] · [[Backend_Katmanli_Mimari]] · [[Auth_Domain]]

## Proje Referansları
`WordLearner.Domain.csproj` → (bağımsız, hiçbir projeye referans vermez)

## Klasör Yapısı (mevcut, 2026-07-07 itibarıyla domain alt klasörlerine geçildi)
```
Entities/
  BaseEntity.cs          ← domain'e özgü değil, her zaman kökte kalır
  Auth/
    User.cs
    RefreshToken.cs
    QrLoginSession.cs
Enums/
  Auth/
    OtpPurpose.cs
    QrLoginStatus.cs
```
**Kural (bkz. `wiki/Standartlar/Kodlama_Standartlari.md` "Klasör Organizasyonu"):** `Entities/` ve
`Enums/` flat kalmaz — her yeni domain (Vocabulary, SRS, Sosyal, ...) kendi `<Domain>/` alt
klasörünü ve namespace'ini (`WordLearner.Domain.Entities.<Domain>`, `...Enums.<Domain>`) alır.
Yalnızca `BaseEntity.cs` istisna, her zaman `Entities/` kökünde kalır. Bu konvansiyon
`Application/Features/<Domain>/`, `Application/DTOs/<Domain>/` ile tutarlıdır.

## Planlanan Genişleme
Her Faz A/C task'ı kendi entity'sini kendi domain alt klasörüne ekler (`Word`, `WordDetail`,
`Category`, `UserCard`, `UserProgress`, `Class`, ...). Tam liste → [[Veritabani_Semasi]].

## Dosyalar
- [[BaseEntity]] — tüm entity'lerin türeyeceği soyut taban sınıf (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
- `Entities/Auth/User.cs`, `RefreshToken.cs`, `QrLoginSession.cs` — bkz. [[Auth_Domain]]
- `Enums/Auth/OtpPurpose.cs`, `QrLoginStatus.cs` — bkz. [[Auth_Domain]]
