# Backend Katmanlı Mimari

**Özet:** Backend, klasik katmanlı (layered) mimariyle dört .NET projesine ayrılır ve bağımlılık yönü tek yönlüdür: `Domain ← Application ← Infrastructure ← API`. Her katman yalnızca kendi altındaki katmana referans verir; Domain hiçbir şeye bağımlı değildir. Çalışma yöntemi "dikey dilim"dir — bir API tüm katmanlarıyla bitirilip sonra diğerine geçilir. **A-03'te (17. INGEST) mimari kanonik desen değişti:** Controller → Servis Arayüzü/Servis yerine artık Controller → `IMediator.Send(command)` → MediatR Command+Handler (CQRS); "Servis Arayüzü/Servis" deseni tamamen terk edildi, yalnızca birden çok Handler'ın paylaştığı küçük yardımcı servisler (`OtpService`, `LoginCompletionService` gibi) kalıyor. Detay → `CLAUDE.md §3`.
**Kütüphaneler:** .NET 9, EF Core 9, ASP.NET Core, MediatR 12.1.1, AutoMapper 13.0.1, FluentValidation 11.9.2
**Bağlantılar:** [[WordLearner_API]] · [[WordLearner_Application]] · [[WordLearner_Infrastructure]] · [[WordLearner_Domain]] · [[WordLearner_Tests]] · [[Gelistirme_Yol_Haritasi]] · [[Sistem_Mimarisi]] · [[Auth_Domain]]

## Katmanlar

```
WordLearner.API            → HTTP (Controllers, Middleware, Program.cs)
WordLearner.Application    → İş mantığı (Features/<Domain>/ — Command+Handler, DTOs, Validators,
                              Interfaces, Services — paylaşılan yardımcı servisler)
WordLearner.Infrastructure → Veri erişimi (DbContext, Repositories, Configurations, Serilog sink)
WordLearner.Domain         → Entities, Enums
```

Proje referansları (`.csproj`):
- [[WordLearner_API]] → [[WordLearner_Application]], [[WordLearner_Infrastructure]]
- [[WordLearner_Infrastructure]] → [[WordLearner_Application]], [[WordLearner_Domain]]
- [[WordLearner_Application]] → [[WordLearner_Domain]]
- [[WordLearner_Domain]] → (bağımsız)
- [[WordLearner_Tests]] → [[WordLearner_Application]], [[WordLearner_Infrastructure]]

## Dikey Dilim Çalışma Yöntemi

Bir API'ın tüm parçaları (Entity → EF Config → Migration → Command/Query → Validator → Repository →
Handler → Birim Test → Controller → DI kaydı) **tek task içinde** tamamlanır, ardından
`BACKEND_AKADEMI/`'ye işlenir. Katman katman ilerleme (önce tüm entity'ler, sonra tüm
DTO'lar) yasaktır. Handler'lar birbirini `_mediator.Send()` ile ASLA çağırmaz (döngüsel bağımlılık) —
paylaşılan mantık gerekiyorsa küçük bir servise çıkarılır. Detay ve gerekçe → `CLAUDE.md §3`,
[[Gelistirme_Yol_Haritasi]].

## Şu Ana Kadar Yazılan Kod

**A-01/A-02 ✅ (ortak altyapı):**
- [[Program_cs]] (API katmanı — tam yapılandırılmış: Serilog, JWT, CORS, MediatR/AutoMapper/FluentValidation, middleware, rate limiting)
- [[BaseEntity]] (Domain)
- [[IRepository]] / [[Repository]], [[WordLearnerDbContext]], [[InfrastructureServiceExtensions]] (Infrastructure)
- [[EntityNotFoundException]] / [[AppException]] / [[ErrorMessages]] / [[ApiErrorResponse]] (Application)
- [[Middleware]] — `ExceptionHandlingMiddleware`/`SecurityHeadersMiddleware`/`RequestResponseLoggingMiddleware`

**A-03 ✅ + A-03.1 ✅ + A-03.2 ✅ (Auth + QR ile Giriş + mesaj lokalizasyonu — ilk gerçek feature domain'i):**
- Domain: `User`, `RefreshToken`, `QrLoginSession` entity + `OtpPurpose`, `QrLoginStatus` enum (`Entities/Auth/`, `Enums/Auth/`)
- Infrastructure: `UserConfiguration`/`RefreshTokenConfiguration`/`QrLoginSessionConfiguration`, `UserRepository`/`RefreshTokenRepository`/`QrLoginSessionRepository`
- Application: 13 (Auth) + 5 (QrLogin) Command+Handler (`Features/Auth/`, `Features/QrLogin/`), `AuthProfile` (AutoMapper), paylaşılan servisler (`OtpService`, `LoginCompletionService`, `PasswordService`, `JwtTokenService`, `GoogleTokenValidator`, `AppleTokenValidator`), FluentValidation validator'ları (`Validators/Auth/`), **A-03.2:** [[ErrorMessages]]'ın başarı-mesajı kardeşi `SuccessMessages` (`Common/Localization/`) — 7 `MessageResponse` döndüren Handler'ın tamamı artık `SuccessMessages.Resolve(code, language)` kullanıyor, hardcode Türkçe metin kalmadı
- API: `AuthController` (13 endpoint) + `QrLoginController` (5 endpoint), ikisi de yalnızca `IMediator.Send(command)` çağırır
- Test: 117 birim test (79 Auth [A-03.2'de +7 dil senaryosu] + 23 QrLogin [2026-07-11 bugfix turunda +5 regresyon testi] + 15 yeni `Repositories/` testi [2026-07-12, User/RefreshToken/QrLoginSession repository'lerinin IgnoreQueryFilters kullanan sorguları]), hepsi yeşil
- Detay → [[Auth_Domain]], `BACKEND_AKADEMI/A-03_auth-register/`, `A-03.1_qr-login/`, `A-03.2_mesaj-lokalizasyonu/`

**A-04 ✅ (Loglama Sistemi, 2026-07-19):**
- Domain: `ActivityLog`/`ApplicationLog`/`SecurityLog` (`Entities/Logging/`, hiçbiri `BaseEntity`den türemiyor — insert-only) + `LogEventType` enum
- Infrastructure: 3 Configuration + 3 Repository (`Repositories/`) + Serilog MSSqlServer sink (`ApplicationLogColumnOptions.cs`, API katmanında)
- Application: `IActivityLogger`/`ActivityLogger`, `ISecurityLogger`/`SecurityLogger` (`Services/`, flat) — 8 mevcut Auth/QrLogin Handler'a entegre edildi (A-03/A-03.1'den beri bekleyen borç kapandı)
- API: `HealthController` (`GET /health`, MediatR dışı bilinçli sapma) + `RateLimiterOptions.OnRejected`→SecurityLog
- **Mimari karar:** `SecurityLog.Detail`/`ActivityLog.OldValue`/`NewValue` serbest metin değil bir **Code** — admin panel de bir istemci, tr/de çözümü A-07'de `GET /admin/logs/*` OKUNURKEN yapılır (`CLAUDE.md` "İkinci istisna")
- Test: 144/144 yeşil. Detay → [[Loglama_Domain]], `BACKEND_AKADEMI/A-04_loglama-sistemi/`

**A-05 ✅ (Sistem Kelimesi API — Words, 2026-07-21, ilk içerik domain'i):**
- Domain: `Language` (`BaseEntity`siz istisna, statik seed), `WordConcept`/`Word`/`WordDetail`/`WordExample` (`Entities/Words/`)
- Infrastructure: 5 Configuration + `WordConceptRepository` (aggregate root — Word/WordDetail/WordExample için ayrı repository açılmadı) + `LanguageRepository`
- Application: `WordGrammarValidator` (Command'dan bağımsız, `LanguageId`→dil, `PartOfSpeech`→tür dallanması), 7 Command/Query (`Features/Words/`) — CRUD + Eşleştirme (`GetUnmatchedWordConceptsQuery`/`PairWordConceptsCommand`), `DuplicateWordException`
- API: `WordsController` — projedeki **İLK** `[Authorize(Roles="Admin")]` kullanımı, 7 endpoint
- `IActivityLogger`: `CREATE_WORD`/`UPDATE_WORD`/`DELETE_WORD`/`PAIR_WORD_CONCEPTS`
- Test: 193/193 yeşil. Detay → [[Icerik_Domain]], `BACKEND_AKADEMI/A-05_sistem-kelimesi-api/`

**A-06 ✅ (Kategori API — Categories, 2026-07-23):**
- Domain: `Category` (self-ref hiyerarşi), `CategoryTranslation`, `WordCategory` (M:N, `Entities/Categories/`)
- Infrastructure: 3 Configuration + `CategoryRepository`, seed (12 kategori + 24 çeviri)
- Application: 5 Command/Query (`Features/Categories/`), silme koruması (alt kategori/aktif kelime/döngü → 3 yeni exception), `GetWordsQuery`'ye retrofit (`categoryId` filtresi + `categories[]` alanı)
- API: `CategoriesController`
- `IActivityLogger` entegrasyonu + kod denetiminde bulunan 2 hatanın düzeltilmesi (deferred LINQ audit log, duplikat categoryId→500 riski)
- Test: 219/219 yeşil. Detay → [[Icerik_Domain]], `BACKEND_AKADEMI/A-06_kategori-api/`

Sırada **A-07 (Admin API — Kullanıcı Yönetimi + İstatistik + Log Görüntüleme)** var, yeni entity
eklemiyor (mevcut `User`'ı genişletiyor). Detay → [[Gelistirme_Yol_Haritasi]].
