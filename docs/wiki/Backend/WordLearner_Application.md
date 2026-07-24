# WordLearner.Application

**Özet:** İş mantığı katmanı — Command+Handler'lar (MediatR CQRS, dikey dilim), paylaşılan servisler, DTO'lar, Validator'lar ve repository/servis arayüzleri burada yaşar. A-02 ✅: [[IRepository]] sözleşmesi, [[EntityNotFoundException]], [[ApiErrorResponse]] ve [[ApplicationServiceExtensions]] (MediatR/AutoMapper/FluentValidation DI kaydı). A-03 ✅ + A-03.1 ✅ + A-03.2 ✅: `Features/Auth/` altında 13 + `Features/QrLogin/` altında 5 Command+Handler — "Servis Arayüzü/Servis" deseni **terk edildi**. A-04 ✅ (Loglama): `IActivityLogger`/`ActivityLogger`, `ISecurityLogger`/`SecurityLogger` (`Services/`, flat) — içerik değiştiren her CRUD `IActivityLogger`'a, admin'e özel hassas işlemler ayrıca `ISecurityLogger`'a yazar (`CLAUDE.md` kuralı). A-05 ✅ (Sistem Kelimesi API): `Features/Words/` altında 7 Command/Query (CRUD + Eşleştirme), `WordGrammarValidator` (dile/türe göre dallanan bağımsız `AbstractValidator`, `Validators/Words/`), `DuplicateWordException`. A-06 ✅ (Kategori API): `Features/Categories/` altında 5 Command/Query, silme koruması için 3 yeni exception (`CategoryHasChildrenException`/`CategoryHasActiveWordsException`/`CategoryParentCycleException`). Paylaşılan mantık küçük servislere çıkar (`OtpService`, `LoginCompletionService`, `ActivityLogger`, ...) — Handler'lar birbirini `_mediator.Send()` ile ASLA çağırmaz. Entity→DTO dönüşümü gerçek bir mapping ise `Profile` (AutoMapper) yazılır (`AuthProfile`), sabitse (`MessageResponse`) veya elle inşa gerekiyorsa (`WordConceptDtoBuilder`, `CategoryDtoBuilder`) DTO builder ile elle inşa edilir (YAGNI koşulu, `CLAUDE.md §3`). Yalnızca [[WordLearner_Domain]]'e bağımlıdır — Infrastructure veya API'yi bilmez.
**Kütüphaneler:** `MediatR` 12.1.1, `AutoMapper` 13.0.1, `FluentValidation` 11.9.2 + `FluentValidation.DependencyInjectionExtensions` 11.9.2, `BCrypt.Net-Next` 4.0.3, `System.IdentityModel.Tokens.Jwt` 7.1.0, `Google.Apis.Auth` 1.67.0 — tam liste [[Teknik_Ozellikler]] §1
**Bağlantılar:** [[WordLearner_Domain]] · [[WordLearner_Infrastructure]] · [[IRepository]] · [[EntityNotFoundException]] · [[ApiErrorResponse]] · [[ApplicationServiceExtensions]] · [[AppException]] · [[ErrorMessages]] · [[SuccessMessages]] · [[AuthProfile]] · [[Auth_Domain]] · [[Loglama_Domain]] · [[Icerik_Domain]] · [[Backend_Katmanli_Mimari]] · [[Teknik_Ozellikler]]

## Proje Referansları
`WordLearner.Application.csproj` → [[WordLearner_Domain]]

## Klasör Yapısı (mevcut, A-06 itibarıyla)
```
Common/Exceptions/    → AppException.cs (taban) + somut alt tipler: Auth (DuplicateEmail,
                        InvalidCredentials, InvalidOtp, AccountNotActive, AccountAnonymized,
                        InvalidRefreshToken, InvalidSocialToken, QrSessionGone, QrSessionForbidden),
                        Words (DuplicateWordException), Categories (CategoryHasChildrenException,
                        CategoryHasActiveWordsException, CategoryParentCycleException) +
                        EntityNotFoundException.cs
Common/Localization/  → ErrorMessages.cs (Code→dil sözlüğü) + SuccessMessages.cs (A-03.2)
Common/Models/        → ApiErrorResponse.cs
DTOs/Auth/            → AuthTokenResponse.cs, MessageResponse.cs, RegisterDtos.cs, QrLoginDtos.cs
DTOs/Words/           → WordDtos.cs (A-05)
DTOs/Categories/      → CategoryDtos.cs (A-06)
DTOs/Admin/           → AdminUserDtos.cs (A-07, henüz commit edilmedi)
Extensions/           → ApplicationServiceExtensions.cs
Features/Auth/        → 13 Command+Handler dosyası + AuthProfile.cs (AutoMapper)
Features/QrLogin/     → 5 Command+Handler dosyası (Generate/Scan/Confirm/Deny/GetStatus) +
                        QrLoginSessionExpiryExtensions.cs (paylaşılan lazy-expire mantığı)
Features/Words/        → 7 Command/Query (Create/Update/Delete/GetById/GetAll/GetUnmatched/Pair) +
                        WordEntityBuilder.cs + WordConceptDtoBuilder.cs + WordMatchSuggestionResolver.cs
Features/Categories/   → 5 Command/Query (Create/Update/Delete/GetAll/GetCategoryWords) +
                        CategoryDtoBuilder.cs
Interfaces/Repositories/ → IRepository.cs, IUserRepository.cs, IRefreshTokenRepository.cs,
                        IQrLoginSessionRepository.cs, IActivityLogRepository.cs,
                        IApplicationLogRepository.cs, ISecurityLogRepository.cs,
                        IWordConceptRepository.cs, ILanguageRepository.cs, ICategoryRepository.cs
Interfaces/Services/  → IPasswordService, ITokenService, IOtpService, ILoginCompletionService,
                        IEmailService, IGoogleTokenValidator, IAppleTokenValidator,
                        IActivityLogger, ISecurityLogger (A-04)
Services/             → PasswordService, JwtTokenService, OtpService, LoginCompletionService,
                        DevEmailService, GoogleTokenValidator, AppleTokenValidator, ActivityLogger,
                        SecurityLogger (flat, feature alt klasörü yok — bkz. [[Kodlama_Standartlari]])
Validators/Auth/      → her Command için FluentValidation validator'ı + paylaşılan
                        Email/Password/Otp/RefreshToken "RuleExtensions" sınıfları
Validators/Words/     → CreateWordCommandValidator, UpdateWordCommandValidator,
                        PairWordConceptsCommandValidator, WordGrammarValidator (bağımsız,
                        Command'a bağlı değil — dile/türe göre dallanır)
Validators/Categories/ → CreateCategoryCommandValidator, UpdateCategoryCommandValidator
```

## Planlanan Genişleme
A-07 (Admin API) `Features/Admin/` altında yeni Command/Query'ler ekliyor (`User` entity'sini
genişletiyor, yeni entity yok). C fazı (Kullanıcı Backend) kendi `Features/<Domain>/`,
`DTOs/<Domain>/`, `Validators/<Domain>/` alt klasörlerini açacak.

## Dosyalar
- [[IRepository]] — generic CRUD sözleşmesi, tüm repository'lerin uyacağı arayüz
- [[EntityNotFoundException]] / [[AppException]] — iki ayrı exception ailesi (dinamik mesaj vs. Code+sözlük)
- [[ErrorMessages]] — `AppException.Code`'un dile göre çözüldüğü merkezi sözlük
- [[SuccessMessages]] — `MessageResponse.Code`'un dile göre çözüldüğü merkezi sözlük (A-03.2, ErrorMessages'ın kardeşi)
- [[ApiErrorResponse]] — hata yanıtı zarfı (`ApiResponse<T>`/`PagedResult<T>` bilinçli olarak yok — YAGNI)
- [[ApplicationServiceExtensions]] — `AddApplicationServices()` DI extension'ı
- [[AuthProfile]] — Entity→DTO AutoMapper profili (`User→RegisterResponse`, `User→AuthUserDto`)
- `Interfaces/Services/IActivityLogger.cs`/`ISecurityLogger.cs`, `Services/ActivityLogger.cs`/`SecurityLogger.cs` — bkz. [[Loglama_Domain]]
- `Features/Words/*`, `Validators/Words/WordGrammarValidator.cs` — bkz. [[Icerik_Domain]]
- `Features/Categories/*` — bkz. [[Icerik_Domain]]
