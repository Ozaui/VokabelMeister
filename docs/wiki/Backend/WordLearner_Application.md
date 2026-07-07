# WordLearner.Application

**Özet:** İş mantığı katmanı — Servisler, DTO'lar, Validator'lar, Command+Handler'lar ve repository/servis arayüzleri burada yaşar. A-02 tamamlandı: [[IRepository]] sözleşmesi, [[EntityNotFoundException]], [[ApiErrorResponse]] ve [[ApplicationServiceExtensions]] (MediatR/AutoMapper/FluentValidation DI kaydı) mevcut. A-03 (Auth) tamamlandı: `Features/Auth/` altında 13 Command+Handler (MediatR CQRS — `Application/Services/`de `IAuthService`/`AuthService` bir servis olarak DEĞİL, `OtpService`/`LoginCompletionService` gibi paylaşılan yardımcı servisler olarak var); Word/Category/... feature'ları henüz yazılmadı. Yalnızca [[WordLearner_Domain]]'e bağımlıdır — Infrastructure veya API'yi bilmez (bağımlılığı tersine çevirme prensibi). **Not:** `ApiResponse<T>`/`PagedResult<T>` bilinçli olarak burada değil — YAGNI kuralı (bkz. [[ApiErrorResponse]] "Düzeltme" bölümü), ilk gerçek controller'a ertelendi.
**Kütüphaneler:** `FluentValidation` 11.9.2 + `FluentValidation.DependencyInjectionExtensions` 11.9.2, `MediatR` 12.1.1, `AutoMapper` 13.0.1 (hepsi kurulu, A-02) — planlanan (henüz kurulmadı): `BCrypt.Net-Next` 4.0.3, `System.IdentityModel.Tokens.Jwt` 7.1.0, `Google.Apis.Auth` 1.67.0 (A-03) — tam liste [[Teknik_Ozellikler]] §1
**Bağlantılar:** [[WordLearner_Domain]] · [[WordLearner_Infrastructure]] · [[IRepository]] · [[EntityNotFoundException]] · [[ApiErrorResponse]] · [[ApplicationServiceExtensions]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Application.csproj` → [[WordLearner_Domain]]

## Klasör Yapısı (mevcut)
```
Common/Exceptions/  → EntityNotFoundException.cs
Common/Models/       → ApiErrorResponse.cs
Interfaces/Repositories/ → IRepository.cs
Extensions/          → ApplicationServiceExtensions.cs
```

## Planlanan Genişleme (Faz A-03+)
`Services/`, `DTOs/`, `Validators/`, `Interfaces/Services/` — her feature API'sı kendi dikey diliminde
bu klasörlere ekleme yapacak (bkz. [[Gelistirme_Yol_Haritasi]]). `ApiResponse<T>`/`PagedResult<T>`
da ilk gerçek ihtiyaç doğduğunda (muhtemelen A-03 veya A-05) buraya eklenecek — bkz. [[ApiErrorResponse]].

## Dosyalar
- [[IRepository]] — generic CRUD sözleşmesi, tüm repository'lerin uyacağı arayüz
- [[EntityNotFoundException]] — entity bulunamadığında fırlatılan özel exception
- [[ApiErrorResponse]] — hata yanıtı zarfı (`ApiResponse<T>`/`PagedResult<T>` bilinçli olarak yok — YAGNI)
- [[ApplicationServiceExtensions]] — `AddApplicationServices()` DI extension'ı
