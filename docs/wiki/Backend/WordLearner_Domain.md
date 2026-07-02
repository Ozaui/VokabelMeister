# WordLearner.Domain

**Özet:** En iç katman — Entity'ler ve enum'lar burada yaşar, hiçbir dış pakete veya başka katmana bağımlı değildir. Şu an yalnızca [[BaseEntity]] mevcut; `Word`, `User`, `Category` gibi hiçbir feature entity'si henüz yazılmadı (planlanan tam şema → [[Veritabani_Semasi]]).
**Kütüphaneler:** Saf C# — hiçbir NuGet paketi yok
**Bağlantılar:** [[BaseEntity]] · [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Veritabani_Semasi]] · [[Backend_Katmanli_Mimari]]

## Proje Referansları
`WordLearner.Domain.csproj` → (bağımsız, hiçbir projeye referans vermez)

## Klasör Yapısı (mevcut)
```
Entities/ → BaseEntity.cs
```

## Planlanan Genişleme
Her Faz A/C task'ı kendi entity'sini buraya ekler (`User`, `RefreshToken`, `Word`, `WordDetail`,
`Category`, `UserCard`, `UserProgress`, `Class`, ...) + `Enums/` klasörü (`OtpPurpose`,
`LogEventType` vb.). Tam liste → [[Veritabani_Semasi]].

## Dosyalar
- [[BaseEntity]] — tüm entity'lerin türeyeceği soyut taban sınıf (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
