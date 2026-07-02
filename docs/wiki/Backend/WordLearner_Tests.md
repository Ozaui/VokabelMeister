# WordLearner.Tests

**Özet:** xUnit birim test projesi; [[WordLearner_Application]] ve [[WordLearner_Infrastructure]]'a referans verir ama şu an hiçbir test dosyası içermez. İlk test sınıfı A-02'nin kalan adımı olan `RepositoryTests` olacak (in-memory EF Core ile generic [[Repository]] CRUD + soft delete filtresi testi).
**Kütüphaneler:** xUnit 2.9.2, xunit.runner.visualstudio 2.8.2, Microsoft.NET.Test.Sdk 17.12.0, coverlet.collector 6.0.2 — planlanan (henüz kurulmadı): `Moq` 4.20.70, `FluentAssertions` 6.12.0, `Microsoft.EntityFrameworkCore.InMemory` 9.0.0 (bkz. [[Kodlama_Standartlari]] §7, tam versiyon listesi [[Teknik_Ozellikler]] §1)
**Bağlantılar:** [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[Repository]] · [[Kodlama_Standartlari]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Tests.csproj` → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## Planlanan Klasör Yapısı
```
Services/      → XxxServiceTests.cs   (her servisin kendi test sınıfı)
Helpers/       → SrsCalculatorTests.cs vb.
Repositories/  → RepositoryTests.cs   (generic taban için, A-02'de bir kez)
```

## Test Felsefesi
Testler Faz F'ye bırakılmaz — her API'nın servis katmanı bitince aynı task içinde birim testi
yazılır. İsimlendirme kalıbı: `{Metot}_{Senaryo}_{BeklenenSonuç}`. AAA deseni (Arrange/Act/Assert),
repository ve dış servisler her zaman mock'lanır. Detay → [[Kodlama_Standartlari]] §7.
