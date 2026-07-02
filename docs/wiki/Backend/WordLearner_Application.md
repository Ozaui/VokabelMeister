# WordLearner.Application

**Özet:** İş mantığı katmanı — Servisler, DTO'lar, Validator'lar ve repository/servis arayüzleri burada yaşar. Şu an yalnızca paylaşılan iskelet mevcut: [[IRepository]] sözleşmesi ve [[EntityNotFoundException]]; hiçbir feature servisi (Auth, Word, Category, ...) henüz yazılmadı. Yalnızca [[WordLearner_Domain]]'e bağımlıdır — Infrastructure veya API'yi bilmez (bağımlılığı tersine çevirme prensibi).
**Kütüphaneler:** Saf C# — planlanan (henüz kurulmadı): `FluentValidation` 11.9.2, `MediatR` 12.1.1, `AutoMapper` 13.0.1, `BCrypt.Net-Next` 4.0.3, `System.IdentityModel.Tokens.Jwt` 7.1.0, `Google.Apis.Auth` 1.67.0 — tam liste [[Teknik_Ozellikler]] §1
**Bağlantılar:** [[WordLearner_Domain]] · [[WordLearner_Infrastructure]] · [[IRepository]] · [[EntityNotFoundException]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Application.csproj` → [[WordLearner_Domain]]

## Klasör Yapısı (mevcut)
```
Common/Exceptions/  → EntityNotFoundException.cs
Interfaces/Repositories/ → IRepository.cs
```

## Planlanan Genişleme (Faz A-03+)
`Services/`, `DTOs/`, `Validators/`, `Interfaces/Services/` — her feature API'sı kendi dikey diliminde
bu klasörlere ekleme yapacak (bkz. [[Gelistirme_Yol_Haritasi]]).

## Dosyalar
- [[IRepository]] — generic CRUD sözleşmesi, tüm repository'lerin uyacağı arayüz
- [[EntityNotFoundException]] — entity bulunamadığında fırlatılan özel exception
