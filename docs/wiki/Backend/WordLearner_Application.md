# WordLearner.Application

**Özet:** İş mantığı katmanı — Command+Handler'lar (MediatR CQRS, dikey dilim), paylaşılan servisler, DTO'lar, Validator'lar ve repository/servis arayüzleri burada yaşar. A-02 ✅: [[IRepository]] sözleşmesi, [[EntityNotFoundException]], [[ApiErrorResponse]] ve [[ApplicationServiceExtensions]] (MediatR/AutoMapper/FluentValidation DI kaydı). A-03 ✅ + A-03.1 ✅ (ikisi de tamamlandı): `Features/Auth/` altında 13 + `Features/QrLogin/` altında 5 Command+Handler (toplam 18) — "Servis Arayüzü/Servis" deseni **terk edildi**, `IAuthService`/`AuthService` silindi. Paylaşılan mantık küçük servislere çıktı: `OtpService`, `LoginCompletionService`, `PasswordService`, `JwtTokenService`, `GoogleTokenValidator`, `AppleTokenValidator`, `DevEmailService` (hepsi `Application/Services/`, flat). Entity→DTO dönüşümü olan 2 yerde (`RegisterResponse`, `AuthUserDto`) `AuthProfile` (AutoMapper) kullanılıyor — koşul: gerçek bir mapping varsa Profile yazılır, sabit `MessageResponse` ise elle inşa edilir (YAGNI). Word/Category/... feature'ları henüz yazılmadı. Yalnızca [[WordLearner_Domain]]'e bağımlıdır — Infrastructure veya API'yi bilmez (bağımlılığı tersine çevirme prensibi).
**Kütüphaneler:** `MediatR` 12.1.1, `AutoMapper` 13.0.1, `FluentValidation` 11.9.2 + `FluentValidation.DependencyInjectionExtensions` 11.9.2 (A-02'de kurulu, A-03'te ilk kez fiilen kullanılmaya başlandı), `BCrypt.Net-Next` 4.0.3, `System.IdentityModel.Tokens.Jwt` 7.1.0, `Google.Apis.Auth` 1.67.0 (A-03'te eklendi, hepsi **aktif kullanımda**) — tam liste [[Teknik_Ozellikler]] §1
**Bağlantılar:** [[WordLearner_Domain]] · [[WordLearner_Infrastructure]] · [[IRepository]] · [[EntityNotFoundException]] · [[ApiErrorResponse]] · [[ApplicationServiceExtensions]] · [[AppException]] · [[ErrorMessages]] · [[AuthProfile]] · [[Auth_Domain]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Application.csproj` → [[WordLearner_Domain]]

## Klasör Yapısı (mevcut, A-03.1 sonrası)
```
Common/Exceptions/    → AppException.cs (taban) + 9 somut alt tip (DuplicateEmail, InvalidCredentials,
                        InvalidOtp, AccountNotActive, AccountAnonymized, InvalidRefreshToken,
                        InvalidSocialToken, QrSessionGone, QrSessionForbidden) + EntityNotFoundException.cs
Common/Localization/  → ErrorMessages.cs (Code→dil sözlüğü, bkz. [[ErrorMessages]])
Common/Models/        → ApiErrorResponse.cs
DTOs/Auth/            → AuthTokenResponse.cs, MessageResponse.cs, RegisterDtos.cs, QrLoginDtos.cs
Extensions/           → ApplicationServiceExtensions.cs
Features/Auth/        → 13 Command+Handler dosyası + AuthProfile.cs (AutoMapper)
Features/QrLogin/     → 5 Command+Handler dosyası (Generate/Scan/Confirm/Deny/GetStatus) +
                        QrLoginSessionExpiryExtensions.cs (paylaşılan lazy-expire mantığı)
Interfaces/Repositories/ → IRepository.cs, IUserRepository.cs, IRefreshTokenRepository.cs,
                        IQrLoginSessionRepository.cs
Interfaces/Services/  → IPasswordService, ITokenService, IOtpService, ILoginCompletionService,
                        IEmailService, IGoogleTokenValidator, IAppleTokenValidator
Services/             → PasswordService, JwtTokenService, OtpService, LoginCompletionService,
                        DevEmailService, GoogleTokenValidator, AppleTokenValidator (flat, feature
                        alt klasörü yok — bkz. [[Kodlama_Standartlari]])
Validators/Auth/      → her Command için FluentValidation validator'ı + paylaşılan
                        Email/Password/Otp/RefreshToken "RuleExtensions" sınıfları
```

## Planlanan Genişleme (Faz A-05+)
Bir sonraki domain (Vocabulary/Word/Category) kendi `Features/<Domain>/`, `DTOs/<Domain>/`,
`Validators/<Domain>/` alt klasörünü açacak — yukarıdaki Auth/QrLogin ile aynı desende.

## Dosyalar
- [[IRepository]] — generic CRUD sözleşmesi, tüm repository'lerin uyacağı arayüz
- [[EntityNotFoundException]] / [[AppException]] — iki ayrı exception ailesi (dinamik mesaj vs. Code+sözlük)
- [[ErrorMessages]] — `AppException.Code`'un dile göre çözüldüğü merkezi sözlük
- [[ApiErrorResponse]] — hata yanıtı zarfı (`ApiResponse<T>`/`PagedResult<T>` bilinçli olarak yok — YAGNI)
- [[ApplicationServiceExtensions]] — `AddApplicationServices()` DI extension'ı
- [[AuthProfile]] — Entity→DTO AutoMapper profili (`User→RegisterResponse`, `User→AuthUserDto`)
