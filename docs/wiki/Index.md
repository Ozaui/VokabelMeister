# VokabelMeister — Wiki İndeksi (Ana Harita)

**Özet:** VokabelMeister, Almanca-Türkçe kelime öğrenme uygulamasının backend'i (.NET 9) ve planlanan üç istemcisini (Web/Mobil/Admin) haritalayan Obsidian bilgi grafiğinin giriş noktasıdır. Proje şu an **Faz A (Admin Panel Backend)**'in erken adımlarında (A-01 ✅, A-02 ✅, **A-03 ✅ tamamlandı** — Auth API'nin 13 endpoint'i `AuthController` → `IMediator.Send(command)` → `Application/Features/Auth/` altında 13 ayrı Command+Handler (MediatR CQRS) ile yazıldı, gerçek bir sunucu çalıştırılıp curl ile uçtan uca doğrulandı, 72/72 birim testi yeşil; detay → On yedinci INGEST; **A-03.1 ✅ tamamlandı** — QR Kod ile Giriş, 5 MediatR Command+Handler + Controller + 18 birim testi, token üretimi A-03'teki `ILoginCompletionService`'i yeniden kullanıyor; detay → Yirminci INGEST; **A-03.2 ✅ tamamlandı** — Auth başarı mesajlarının lokalizasyonu, [[SuccessMessages]] ([[ErrorMessages]]'ın kardeşi), `MessageResponse` artık `Code+Message`, 7 test dosyasına Almanca senaryo eklendi, toplam 97/97 birim testi yeşil; detay → Yirmi ikinci INGEST. Yirmi dördüncü INGEST'te (2026-07-11, kod kalitesi denetimi) QR ile Giriş akışında 4 gerçek bug (rate-limit self-lockout, boş audit alanları, atlanan soft-delete/hesap-durumu kontrolü, exception mesajına sızan ham token) düzeltildi, **güncel toplam 102/102 birim testi yeşil**). Sırada **A-04 (Loglama Sistemi)** var (bkz. `TASK/A_admin_panel_backend.md`). Her INGEST sonrası bu dosya güncel tutulur (kural kaynağı: `/wiki_schema.md`).

**Kütüphaneler:** —
**Bağlantılar:** [[Sistem_Mimarisi]] · [[Backend_Katmanli_Mimari]] · [[Gelistirme_Yol_Haritasi]] · [[Veritabani_Semasi]]

---

## 1. Mimari

- [[Sistem_Mimarisi]] — 3 istemci + tek API + MSSQL, genel akış
- [[Backend_Katmanli_Mimari]] — Domain ← Application ← Infrastructure ← API bağımlılık zinciri
- [[Roller_ve_Erisim]] — User/Admin rolleri, içerik sahipliği, görünürlük matrisi
- [[Gelistirme_Yol_Haritasi]] — Faz A→F sırası ve güncel ilerleme durumu

## 2. Backend Projeleri (`backend/`)

- [[WordLearner_API]] — HTTP giriş noktası (Controllers, Program.cs, Swagger)
- [[WordLearner_Application]] — İş mantığı katmanı sözleşmeleri (Interfaces, Exceptions)
- [[WordLearner_Infrastructure]] — Veri erişimi (DbContext, Repository, DI extension)
- [[WordLearner_Domain]] — Entity'ler (şu an yalnızca `BaseEntity`)
- [[WordLearner_Tests]] — xUnit test projesi (henüz test yok)

### Yazılmış Kod Düğümleri (A-01/A-02 — A-02 tamamlandı)
- [[Program_cs]] — composition root (**tam yapılandırma:** Serilog, JWT, CORS, MediatR/AutoMapper/FluentValidation, middleware pipeline)
- [[BaseEntity]] — tüm entity'lerin taban sınıfı
- [[IRepository]] / [[Repository]] — generic CRUD sözleşmesi + EF Core implementasyonu
- [[WordLearnerDbContext]] — merkezi DbContext (soft delete filtresi, `UpdatedAt` otomasyonu)
- [[InfrastructureServiceExtensions]] — DI kayıt extension'ı (Infrastructure)
- [[ApplicationServiceExtensions]] — DI kayıt extension'ı (Application — MediatR/AutoMapper/FluentValidation)
- [[EntityNotFoundException]] — özel exception tipi
- [[ApiErrorResponse]] — hata yanıtı zarfı (`ApiResponse<T>`/`PagedResult<T>` YAGNI kuralıyla ertelendi)
- [[Middleware]] — `ExceptionHandlingMiddleware` / `SecurityHeadersMiddleware` / `RequestResponseLoggingMiddleware`
- [[RepositoryTests]] — `Repository<T>` + soft delete filtresi + `userId` audit alanları için 10 birim test (hepsi yeşil, İngilizce isimlendirme)
- [[API_Yol_Haritasi_Sistemi]] — `docs/API_YOL_HARITASI/` HTML rehber sistemi (junior eğitimi)

### Yazılmış Kod Düğümleri (A-03 — tamamlandı)
- `User`/`RefreshToken` entity + `OtpPurpose` enum, `UserConfiguration`/`RefreshTokenConfiguration`
  (Fluent API), `AddUserAndRefreshToken` migration (VokabelMeisterDB'ye uygulandı) — detay bir kod
  sayfası yerine [[Auth_Domain]]'de (şema açıklaması zaten birebir bu koda karşılık geliyor) ve
  `API_YOL_HARITASI/A-03_auth-api.html`'de (birebir kod + junior açıklaması).
- `IPasswordService`/`PasswordService` ve `ITokenService`/`JwtTokenService` yazıldı — ayrı wiki
  sayfası yok (kod zaten [[Auth_Domain]]'in "Referans Kod" bölümünde özetlenmişti), tam hâli
  `API_YOL_HARITASI/A-03_auth-api.html`'de.
- [[AppException]] + [[ErrorMessages]] — Auth exception'ları (`DuplicateEmailException` vb.) için
  yeni taban sınıf + dil sözlüğü (şu an **tr+de** — DE↔TR gerçek hedef kitlesi, İngilizce YAGNI
  gerekçesiyle eklenmedi); [[EntityNotFoundException]] ve [[ApiErrorResponse]] güncellendi,
  [[Middleware]] (`ExceptionHandlingMiddleware`) `Accept-Language`'a göre mesaj çözecek şekilde
  değiştirildi. Bu A-03'e özel değil, mimari bir karar — tüm gelecekteki iş kuralı exception'ları
  bundan türeyecek.
- `IUserRepository`/`UserRepository`, `IRefreshTokenRepository`/`RefreshTokenRepository`,
  `IEmailService`/`DevEmailService`, `IGoogleTokenValidator`/`GoogleTokenValidator`,
  `IAppleTokenValidator`/`AppleTokenValidator` (JWKS tabanlı, elle RS256 doğrulama) — hepsi Auth
  Command Handler'larının bağımlılıkları, ayrı wiki sayfası yok, tam hâli roadmap'te.
- **`IOtpService`/`OtpService`** (OTP üretimi/doğrulanması/temizlenmesi) ve
  **`ILoginCompletionService`/`LoginCompletionService`** (OTP/Google/Apple girişlerinin ortak son
  adımı — grace period kurtarma, giriş istatistikleri, token üretimi) — birden fazla Command
  Handler'ın paylaştığı, MediatR handler'ların birbirini çağıramaması nedeniyle çıkarılan servisler.
- **13 Auth Command+Handler'ı tamamlandı** (`Application/Features/Auth/`, MediatR CQRS —
  register/login 2-adım/Google/Apple/refresh/logout/forgot-reset-password/delete-account). Her
  Command kendi dosyasında Handler'ıyla birlikte (dikey dilim). Öne çıkan kararlar: timing attack
  önlemi (`FakePasswordHashForTiming`, Login ve ConfirmAccountDeletion handler'larında bilinçli
  olarak ikişer kez tekrarlanan küçük bir sabit — paylaşılan servise çıkarılmadı), grace period
  kurtarma (`accountWasRecovered`), Google/Apple account linking, Token Family Pattern replay
  tespiti. `RefreshCommand`/`LogoutCommand` ayrı tipler — eskiden tek bir `RefreshRequest` DTO'sunu
  paylaşırlardı, ama MediatR'da bir `IRequest<T>` tek dönüş tipine bağlı olduğundan (Refresh
  `AuthTokenResponse`, Logout dönüşsüz) ayrılmaları gerekti.
- **FluentValidation validator'ları** (`Application/Validators/Auth/`, Command tiplerini doğrular)
  — her kural hem sabit Türkçe `WithMessage` (log) hem `WithErrorCode` (istemciye giden,
  [[ErrorMessages]]'ten dile göre çözülür) taşır. `ValidationFilter` tip-agnostik çalıştığı için
  (controller action parametresinin runtime tipine göre `IValidator<T>` arar) Command'ların Request
  DTO'larının yerini almasıyla hiçbir değişiklik gerekmedi.
- **`AuthController` (13 endpoint) tamamlandı** — `IMediator.Send(command)` ile çağırır, iş mantığı
  içermez. `ValidationFilter` (global action filter, `API/Filters/`) DI'a kayıtlı `IValidator<T>`'leri
  otomatik çalıştırır; `RequestLanguageResolver` (`API/Common/`) `Accept-Language` çıkarma mantığını
  `Middleware` ile paylaşır. `Program.cs`'e rate limiting eklendi (genel 100/dk, 10/dk anonim —
  "login 5/15dk"/"OTP 3 yanlış" BAŞARISIZ deneme sayaçları SecurityLog'a bağımlı olduğu için A-04
  sonrasına bırakıldı). Gerçek bir sunucu çalıştırılıp curl ile uçtan uca doğrulandı:
  register→verify-email→login→verify-otp→refresh→logout gerçek token/204 döndü. 72/72 birim testi
  yeşil (`WordLearner.Tests/Features/Auth/` — 13 handler test dosyası, `Services/` — `OtpServiceTests`
  + `LoginCompletionServiceTests`). Detay → On yedinci INGEST.
- **A-03.2 (Auth Başarı Mesajlarının Lokalizasyonu) tamamlandı** — yeni [[SuccessMessages]]
  ([[ErrorMessages]]'ın kardeşi, ayrı sözlük çünkü kodlar anlamca farklı kümeler),
  `MessageResponse` artık `record MessageResponse(string Code, string Message)` (`ApiErrorDetail`
  ile simetrik). `MessageResponse` döndüren 7 Command'a (`LoginCommand`/`ResendVerificationCommand`/
  `ResetPasswordCommand`/`ConfirmAccountDeletionCommand`/`RequestAccountDeletionCommand`/
  `VerifyEmailCommand`/`ForgotPasswordCommand`) `Language` init-only property eklendi;
  `AuthController`'a `Language` computed property (`RequestLanguageResolver.Resolve(HttpContext)`,
  `ClientIp` ile aynı desen) eklendi, ilgili 7 endpoint `command with { Language = Language }` ile
  güncellendi. 7 test dosyasına birer `Xxx_GermanLanguage_ReturnsGermanMessage` testi eklendi (7 yeni
  test). Detay → Yirmi ikinci INGEST.

## 3. Veritabanı (planlanan şema — `DATABASE_SCHEMA.md` index + `DATABASE_SCHEMA/` domain dosyaları, henüz migration yok)

- [[Veritabani_Semasi]] — ERD özeti, genel kurallar
- [[Auth_Domain]] — `Users`, `RefreshTokens`
- [[Icerik_Domain]] — `Languages`, `WordConcepts`, `Words`, `WordDetails`, `WordExamples`, `Categories`, `CategoryTranslations`, `WordCategories`
- [[Kisisel_Icerik_Domain]] — `UserCards`, `UserCategories` ve ara tablolar
- [[SRS_Domain]] — `UserProgress`, `UserCardProgress`, `LearningSessions`, `LearningHistory`, `Achievements`
- [[Sosyal_Domain]] — `Classes`, `ClassWords`, `Friendships`, `SharedContents`
- [[Loglama_Domain]] — `ActivityLog`, `ApplicationLog`, `SecurityLog`

## 4. Standartlar ve Kurallar

- [[Kodlama_Standartlari]] — Türkçe yorum kuralı, AMAÇ/NEDEN/NASIL blokları, test standardı
- [[Guvenlik_Politikalari]] — JWT, RBAC, şifreleme, rate limiting
- [[Alman_Dili_Ozellikleri]] — cinsiyet/artikel/hâl/çoğul referansı (`GrammarData` şeması, dil=`de`)
- [[Turkce_Dili_Ozellikleri]] — ünlü uyumu/hâl eki/iyelik referansı (`GrammarData` şeması, dil=`tr`, öncelikli)
- [[Ingilizce_Dili_Ozellikleri]] — çoğul/fiil/karşılaştırma referansı (`GrammarData` şeması, dil=`en`, henüz kullanılmıyor)
- [[Ortam_Degiskenleri]] — ENV değişkenleri listesi
- [[API_Sozlesmesi]] — standart response formatı, endpoint listesi özeti
- [[Teknik_Ozellikler]] — NuGet/npm paket listeleri + JWT/Şifre/SRS/Serilog referans kod örnekleri
- [[Gelistirme_Kurulumu]] — araç kurulumu, `dotnet`/migration komutları, IIS yayınlama

---

## Proje Durumu Özeti

| Faz | Aralık | Başlık | Durum |
|-----|--------|--------|-------|
| A | A-01…A-10 | Admin Panel Backend | 🔄 (A-01 ✅, A-02 ✅, A-03 ✅, A-03.1 ✅, A-03.2 ✅, sıradaki A-04) |
| B | B-01…B-09 | Admin Panel (frontend) | ⬜ |
| C | C-01…C-10 | Kullanıcı Backend | ⬜ |
| D | D-01…D-12 | Web App | ⬜ |
| E | E-01…E-14 | Mobil | ⬜ |
| F | F-01…F-04 | Test & Yayın | ⬜ |

Detay → [[Gelistirme_Yol_Haritasi]].

*On ikinci INGEST (2026-07-07) — SRS/İlerleme tasarım kararları (C-03/C-05 henüz kodlanmadı, saf
tasarım): Kullanıcı "eskiden öğrendiğim kelimeleri unuttum mu, ne zaman test edilecek" sorusundan
başlayarak bir dizi karar netleşti. **(1) Mastery formülü** (şemada alan vardı, formülü yoktu):
`Mastery = (CurrentLevel/5)*80 + (SuccessRate/100)*20` (yüzdelik, 0-100). **(2) Mastery bantları**
`CurrentLevel` değil bu yüzdelik üzerinden: 🔴 Zayıf 0-40 · 🟡 Orta 40-70 · 🟢 İyi 70-100 (aynı
metrik hem bant gösterimini hem günlük özet yüzdesini besliyor). **(3) Streak yalnızca günlük yeni
kelime hedefine (`dailyWordGoal`) bağlı** — due review/bant pratiği yapılsın yapılmasın streak
etkilenmez; due sayısı ana ekranda pasif bir rozet olarak durur, hedef tamamlanınca opsiyonel bir
"tekrar edelim mi" teklifi çıkar. **(4) Bant ekranları** (🔴🟡🟢) iki modlu: İncele (salt okunur
liste) ve Sına (review akışı, kaynak o bant). **(5) Quiz formatı artık istemciden seçilmiyor** —
yeni kelime tanıtımı her zaman `Flashcard`, review'da her soru için backend
`MultipleChoice|TranslationQuiz|ArticleQuiz|PluralQuiz|TrueFalse` (yeni 6. tip) arasından rastgele
seçim yapıyor. **(6) Quality (0-5) üretimi ayrıştı:** Flashcard'da kullanıcı `selfRating` seçer ama
gecikme/ipucu kullanımı üst seçenekleri kilitler (ipucu→"İyi" kapanır, "Cevabı Göster"→otomatik 0);
objektif tiplerde `quality` kullanıcıya sorulmadan `IsCorrect`+`ResponseTime`+`HintUsed`'dan
otomatik hesaplanır; `TrueFalse`'ta şans başarı ihtimali (%50) yüksek olduğu için doğru cevapta
otomatik tavan 4 (asla 5 verilmez — LLM'in kendi önerdiği, kullanıcının onayladığı bir düzeltme).
**(7) "Günde tek resmi review" kuralı:** bir kelime bir günde ilk cevaplandığında SM-2 günceller;
"Aynı Kelimelerle Tekrar Et" ile tekrar oynanırsa `IsExtraPractice=1` ile sadece istatistiğe yazılır,
SM-2/`Mastery` bir daha güncellenmez — bu kural hem bant pratiğinin hem tekrar oynamanın SM-2'yi
bozmasını tek seferde engelliyor. **(8) Günlük özet listeleri:** "Bugün Öğrendiklerim" (seviyesiz,
salt liste) ve "Bugün Test Ettiklerim" (`masteryBefore`→`masteryAfter` yüzdelik, ekstra pratik
satırları hariç). Şema: `LearningHistory`'ye `HintUsed`/`IsExtraPractice`/`MasteryBefore`/
`MasteryAfter` eklendi, `SessionType` enumlarına `TrueFalse` eklendi. API: `POST /learning-sessions`
artık `mode: New|Due|Band|Mixed` alıyor (`sessionType` kalktı), yeni `POST .../hint`,
`POST .../repeat`, `GET /progress/summary`, `GET /progress/words`,
`GET /learning-history/today/learned`, `GET /learning-history/today/tested` endpoint'leri eklendi.
Etkilenen dosyalar: `docs/wiki/Database/SRS_Domain.md` (en detaylı hâli), `docs/DATABASE_SCHEMA/
SRS.md`, `docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md §8`, `docs/REFERENCE/API_ENDPOINTS.md §9-10`,
`docs/TASK/C_kullanici_backend.md` (C-03/C-05 checklist'leri). Henüz hiç kod yazılmadı — C-03/C-05
sıraya geldiğinde bu kararlara göre uygulanacak.*

*On üçüncü INGEST (2026-07-07, aynı gün) — frontend task'ları senkronize edildi: yukarıdaki SRS
kararları backend'e (C-03/C-05) özel işlenmişti, kullanıcı frontend tarafının da bu kararlara göre
güncellenmesini istedi (kod yazılmadan önce, patlamayı önlemek için). `docs/TASK/D_web_app.md`
D-05 (Öğrenme/Sınav Sayfası) → `LearningStartPage` artık `HomePage` (streak, günlük hedef çubuğu,
pasif due rozeti, "Bugün Öğrendiklerim"/"Bugün Test Ettiklerim" listeleri), `sessionType` seçimi
kalktı (`mode: New|Due|Band|Mixed`), `FlashcardScreen`'e ipucu/zaman tavan kilidi, eksik olan
`TranslationQuizScreen` + yeni `TrueFalseScreen` eklendi, `SessionSummaryPage`'e "Aynı Kelimelerle
Tekrar Et" butonu. D-11 (İlerleme Sayfası) → bant kartları (🔴🟡🟢, `Mastery` yüzdesine göre) +
`BandWordListPage` (İncele/Sına iki modlu), yeni route `/progress/band/:band`. `E_mobil.md`'de
E-07/E-13 aynı mantıkla mobil karşılıklarıyla (`HomeScreen`, `BandWordListScreen`) güncellendi.
`B_admin_panel.md`'deki `DashboardPage` (B-07, admin istatistik özeti) kontrol edildi — bu kararlardan
etkilenmiyor çünkü ayrı bir agregat istatistik endpoint'i kullanıyor, `learning-sessions` sözleşmesine
bağımlı değil, değiştirilmedi.*

*On dördüncü INGEST (2026-07-07, aynı gün) — Leech tespiti, achievement tetikleme kuralları,
bildirim tetikleyicileri (yine saf tasarım, kod yok): **(1) Leech:** yeni `ConsecutiveIncorrect`/
`IsSuspended` alanları (`UserProgress`/`UserCardProgress`), eşik **5 ardışık yanlış** (Anki'nin
kümülatif 8'ine göre — bizimki ardışık olduğu için daha düşük). Eşik aşılınca üç aksiyon: Askıya Al
(`IsSuspended=1`, due sorgusundan hariç), Sıfırla (`CurrentLevel`/`EF`/`RepetitionNumber`/
`ConsecutiveIncorrect` sıfırlanır, **`NextReviewAt=NULL`** — geçmiş istatistik `TimesCorrect` vb.
korunur), Devam Et (no-op). **(2) Yeni kelime seçim algoritması netleşti ve bir hata düzeltildi:**
filtre `CurrentLevel=0` değil **`NextReviewAt IS NULL`** olmalı — aksi halde normal bir due-review
başarısızlığı (SM-2 zaten `CurrentLevel`'ı 0'a döndürür ama `NextReviewAt`'i yarına ayarlar)
yanlışlıkla yeni kelime sanılırdı. Bu düzeltme **şema değişikliği gerektirdi**: `NextReviewAt`
`NOT NULL DEFAULT GETUTCDATE()` iken artık **nullable** (`NULL`=zamanlanmadı/sıfırlandı). Sorgu:
`WHERE DifficultyLevel=userLevel AND (UserProgress yok OR NextReviewAt IS NULL) ORDER BY
WordConceptId ASC` — sıfırlanan kelime böylece `WordConceptId` sırasındaki eski (aradaki)
pozisyonundan doğal olarak geri döner. **(3) Achievements:** mevcut `Icon` alanı emoji değil resim
URL'i olarak kullanılacak (admin CRUD yok, seed migration ile — YAGNI); başlangıç seti (streak
3/7/30, kelime sayısı 50/200/500, ilk ustalaşma, 100 kelime İyi bantta, hatasız oturum, leech
kurtarma) ve tetikleme noktaları (`ProgressService`/`LearningSessionService` sonrası) belirlendi.
**(4) Bildirim scheduler'ı için Hangfire seçildi** (SQL Server storage — mevcut MSSQL'e ek altyapı
gerektirmiyor, dashboard'u var, job'lar persist olur) Quartz.NET/elle `IHostedService` yerine;
somut tetikleyiciler: günlük hatırlatma, due hatırlatması, streak riski, achievement bildirimi
(event-driven). Etkilenen dosyalar: `docs/wiki/Database/SRS_Domain.md`, `docs/DATABASE_SCHEMA/
SRS.md` (`NextReviewAt` nullable + yeni kolonlar + `Achievements.Icon` notu),
`docs/REFERENCE/TECHNICAL_SPECIFICATIONS.md` (Hangfire paketleri + leech eşiği), `docs/REFERENCE/
API_ENDPOINTS.md §10` (leech-action, suspended, achievements/me endpoint'leri), `docs/TASK/
C_kullanici_backend.md` (C-03 leech/achievement, C-10 Hangfire+tetikleyiciler), `docs/TASK/
D_web_app.md`/`E_mobil.md` (D-05/E-07 `LeechActionModal`, D-11/E-13 `SuspendedWordsPage/Screen` +
`AchievementsSection`). Henüz hiç kod yazılmadı.*

*On beşinci INGEST (2026-07-07, aynı gün) — `docs/TASK.md` metodolojisine eksik olan tek gerçek
adım eklendi: **git commit/push hiçbir yerde yazılı değildi.** Roadmap'e işleme ve detaylı
açıklama kuralları zaten backend/frontend'de birebir mirror'lanmış durumdaydı (kontrol edildi,
eksik değildi) — asıl boşluk "API/feature bitince ne yapılır" listesinde git adımının hiç
geçmemesiydi. Yeni **⭐ Bir API/Feature Tamamlandığında** bölümü eklendi (commit format örneği,
push'un her seferinde kullanıcı onayıyla yapılacağı notu — git safety protokolü gereği otomatik
değil —, `TASK.md` İlerleme Durumu güncelleme, gerekirse wiki INGEST). Ayrıca "Route/Import"
adımının zaten alt-parçaları birleştiren adım olduğu netleştirildi (ayrı bir "birleştirme" adımına
gerek yok). Backend ⭐ Çalışma Yöntemi ve Frontend ⭐ Çalışma Yöntemi'nin kurallar listelerine bu
yeni bölüme çapraz link eklendi.*

*On altıncı INGEST (2026-07-07, aynı gün) — `docs/FRONTEND_YOL_HARITASI/` üçe bölündü:
`docs/ADMIN_YOL_HARITASI/` (Faz B), `docs/WEB_YOL_HARITASI/` (Faz D), `docs/MOBILE_YOL_HARITASI/`
(Faz E). Sebep: kullanıcı admin panelin ve web app'in (ve zaten planlı olan mobilin) **ayrı
projeler** olarak açılacağını netleştirdi (`/admin`, `/web`, `/mobile` — kod paylaşımı yok, bu
zaten [[Sistem_Mimarisi]]'nde vardı ama roadmap sistemine yansımamıştı); tek bir
`FRONTEND_YOL_HARITASI` + `uygulama` tag'i yaklaşımı yerine üç bağımsız hub tercih edildi. Hiçbir
gerçek feature sayfası yazılmamışken (yalnızca `_TASLAK.html`/`index.html`/`render.js` şablonları
vardı) yapıldığı için içerik kaybı yok — güvenli bir restructuring'di. Her yeni klasör kendi
`_TASLAK.html`/`index.html`/`render.js` kopyasını aldı (render.js değişmeden kopyalandı, zaten
generic). Etkilenen dosyalar: `docs/index.html` (4 roadmap kartı: API/Admin/Web/Mobil),
`docs/API_YOL_HARITASI/index.html` (topbar'daki ölü `FRONTEND_YOL_HARITASI` linki 3 linke
bölündü — kullanıcı bunu ekran görüntüsüyle yakaladı), `docs/API_YOL_HARITASI/_TASLAK.html` (çapraz
link yorumu + örnek `frontendRefs`), `docs/API_YOL_HARITASI/A-03_auth-api.html` (gerçek
`frontendRefs` verisi B-02/D-03/E-05 için doğru klasörlere güncellendi), `docs/TASK.md` (adım 8 +
Çapraz Link Kuralı notu), `docs/00_INDEX.md`, `docs/wiki/Backend/API_Yol_Haritasi_Sistemi.md`
("Frontend Kardeşi" tekil → "Frontend Kardeşleri" üç ayrı sistem). `docs/REFERENCE/ARCHITECTURE.md`
ve [[Sistem_Mimarisi]]'ye dokunulmadı — ikisi zaten "ayrı proje, kod paylaşımı yok" diyordu, yalnızca
roadmap dokümantasyon sistemi bunu yansıtmıyordu.*

## Kaynak Dokümanlar (`/docs` + kök `CLAUDE.md`)
Bu wiki, `docs/` altındaki **tüm** insan-yazımı dokümanların ve kök `CLAUDE.md`'nin taranmasıyla
üretildi. **`/CLAUDE.md` (proje köku, 2026-07-08'den beri) artık tek gerçek kaynak** — dil kuralı,
roller/sahiplik, çoklu dil, veri katmanı, kimlik&güvenlik, test, wiki, backend/frontend yazım sırası,
klasör/namespace ve roadmap kuralları burada toplu; `docs/DATABASE_SCHEMA*` ve `docs/REFERENCE/*`
artık bu kuralları tekrarlamak yerine `CLAUDE.md`'ye referans verir (bkz. Yirmi birinci INGEST).
`docs/` şu klasör yapısındadır (token tasarrufu için bölündü, içerik kaybı yok):
- `docs/00_INDEX.md`, `docs/index.html`, `docs/CONNECTION_STRING.txt` — giriş noktaları (kökte)
- `docs/REFERENCE/` — ARCHITECTURE, API_ENDPOINTS, CODING_STANDARDS, DEVELOPMENT_SETUP, ENV,
  GERMAN_LANGUAGE_FEATURES, TURKISH_LANGUAGE_FEATURES, ENGLISH_LANGUAGE_FEATURES (henüz kullanılmıyor),
  SECURITY, TECHNICAL_SPECIFICATIONS
- `docs/TASK.md` (yöntem/standart + ilerleme) + `docs/TASK/` (faz başına 1 dosya: A_admin_panel_backend,
  B_admin_panel, C_kullanici_backend, D_web_app, E_mobil, F_test_yayin)
- `docs/DATABASE_SCHEMA.md` (index: ERD/seed/genel kurallar) + `docs/DATABASE_SCHEMA/` (domain başına
  1 dosya: Auth, Icerik, Kisisel_Icerik, SRS, Sosyal, Loglama, Sistem)
- `docs/API_YOL_HARITASI/*` (backend) + `docs/ADMIN_YOL_HARITASI/*` / `docs/WEB_YOL_HARITASI/*` /
  `docs/MOBILE_YOL_HARITASI/*` (aynı sistemin frontend kardeşleri — Admin/Web/Mobil ayrı proje
  oldukları için ayrı klasör, henüz hiçbir feature sayfası yazılmadı)

Ayrıca gerçek kaynak kodun (`backend/`, `.csproj`'lar, `.sln`, `.gitignore`, `launchSettings.json`)
taranmasıyla üretildi. Çelişki durumunda `docs/` klasörü otorite kaynağıdır; bu wiki onun bağlantılı
bir haritasıdır.

**Bilinçli olarak wiki içeriğine taşınmadı (okundu ama hassas/düşük değerli):**
- `docs/CONNECTION_STRING.txt` — gerçek DB şifresi içeriyor, **asla** wiki'ye kopyalanmaz
- `backend/WordLearner.API/Properties/launchSettings.json` — dev portları (5001/7001) zaten
  [[WordLearner_API]]'de var; içindeki `AES_ENCRYPTION_KEY` örnek değeri hassas olduğu için atlandı
- `.claude/settings.local.json` — Claude Code araç izinleri, proje mimarisiyle ilgisiz
- `docs/API_YOL_HARITASI/render.js`/`style.css` — sunum kodu, davranışı [[API_Yol_Haritasi_Sistemi]]'nde özetlendi, kod tekrarı gerekmiyor

**Proje dizini şu an %100 taranmış durumda** — okunmamış dosya yok.

*Son INGEST: 2026-07-02 — kapsam: [[BaseEntity]]'ye "kim yaptı" audit alanları eklendi
(`CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId`, `int?`), [[IRepository]]/[[Repository]]
`AddAsync`/`UpdateAsync`/`SoftDeleteAsync`'e opsiyonel `userId` parametresi eklendi,
[[RepositoryTests]] 7→9 teste çıktı; ayrıca [[Kodlama_Standartlari]] §7.2 test metodu adlandırma
kuralı Türkçeden İngilizceye çevrildi (yapı `{Metot}_{Senaryo}_{BeklenenSonuç}` sabit kaldı) ve
mevcut 9 test bu kurala göre yeniden adlandırıldı. `API_YOL_HARITASI/A-02_ortak-altyapi.html`
1/4/5/7. adımlar buna göre güncellendi.*

*İkinci INGEST (aynı gün): `docs/` token tüketimini azaltmak için yeniden klasörlendi — hiçbir
içerik silinmedi, sadece dosyalar/bölümler taşındı. `docs/TASK.md` artık yalnızca yöntem/standart +
ilerleme tablosu; task checklist'leri `docs/TASK/` altında faz başına 1 dosyaya bölündü.
`docs/DATABASE_SCHEMA.md` artık yalnızca ERD/seed/genel kurallar; tam `CREATE TABLE` SQL'leri
`docs/DATABASE_SCHEMA/` altında domain başına 1 dosyaya bölündü. Kökte duran 8 dosya
(ARCHITECTURE, API_ENDPOINTS, CODING_STANDARDS, DEVELOPMENT_SETUP, ENV, GERMAN_LANGUAGE_FEATURES,
SECURITY, TECHNICAL_SPECIFICATIONS) `docs/REFERENCE/` klasörüne taşındı; `00_INDEX.md`/`index.html`/
`CONNECTION_STRING.txt` giriş noktası oldukları için kökte kaldı. Bu wiki'deki tüm `docs/X.md` yol
referansları yeni konumlara göre güncellendi (bu düğüm dahil).*

*Üçüncü INGEST (aynı gün): Frontend için `docs/API_YOL_HARITASI/` ile birebir aynı mantıkta yeni bir
sistem eklendi — `docs/FRONTEND_YOL_HARITASI/` (kendi hub'ı, `_TASLAK.html`'i, bağımsız `render.js`
kopyası; stil `API_YOL_HARITASI/style.css`'i paylaşır, yeni `t-tip/t-api/t-slice/t-hook/t-component/
t-route/t-style` ve `u-web/u-admin/u-mobile` CSS sınıfları o dosyaya eklendi). `docs/TASK.md`'ye
backend'deki **⭐ Çalışma Yöntemi**'nin frontend karşılığı olan **⭐ Frontend Çalışma Yöntemi**
eklendi (adım sırası: tip→api→slice→hook→component→route→test). `docs/TASK/B_admin_panel.md`,
`D_web_app.md`, `E_mobil.md` — önceden tek satırlık başlıklardan ibaretti; artık backend'deki
A/C fazlarıyla aynı granülaritede, her feature için alt-adım checklist'i + roadmap işaretçileriyle
detaylandırıldı. [[API_Yol_Haritasi_Sistemi]] bu yeni sistemi "Frontend Kardeşi" bölümünde anlatıyor.*

*Dördüncü INGEST (aynı gün): İki yol haritası arasında **iki yönlü çapraz link kuralı** eklendi —
bir API'yi bir frontend feature tüketiyorsa, backend sayfası (`frontendRefs`) "buradan sonrası
frontend tarafında", frontend sayfasının `api` adımı (`backendRef`) "buradan sonrası backend
tarafında" bandı gösterip birbirine link verir (`render.js` iki motorda da güncellendi, `.note.xref`
stili `API_YOL_HARITASI/style.css`'e eklendi, her iki `_TASLAK.html` örnek alanla güncellendi).
`docs/TASK.md`'nin her iki ⭐ bölümüne kural yazıldı; `docs/TASK/A_admin_panel_backend.md` ve
`C_kullanici_backend.md`'deki her API task'ının altına **"Frontend karşılığı:"** notu eklenerek
gelecekteki eşleştirmeler (örn. A-03 ↔ B-02/D-03/E-05, C-04 ↔ D-07/E-09) önceden belgelendi.
[[API_Yol_Haritasi_Sistemi]]'ne "Çapraz Link Kuralı" bölümü eklendi.*

*Beşinci INGEST (2026-07-02): [[BaseEntity]].`UpdatedAt` non-nullable `DateTime` (varsayılan
`DateTime.UtcNow`) iken insert anında `CreatedAt`'e eşit bir değer alıyordu — "hiç güncellenmedi"
durumu ayırt edilemiyordu. `UpdatedAt` → `DateTime?` (varsayılansız) yapıldı;
[[WordLearnerDbContext]].`SaveChangesAsync` zaten yalnızca `EntityState.Modified`'ta set ettiği için
davranış değişmedi, sadece insert sonrası artık `null` kalıyor. [[RepositoryTests]]'e
`AddAsync_ValidEntity_LeavesUpdatedAtNull` testi eklendi (9→10 test). `docs/API_YOL_HARITASI/
A-02_ortak-altyapi.html`'deki BaseEntity/WordLearnerDbContext/RepositoryTests adımları (kod +
açıklama) buna göre güncellendi.*

*Altıncı INGEST (2026-07-03): [[EntityNotFoundException]]'a `Type`+key alan bir overload eklendi
(`"{Entity} bulunamadı: Id={key}"` formatını otomatik üretir) — [[Repository]].`SoftDeleteAsync`
artık elle string interpolasyonu yerine bu overload'ı çağırıyor, format tek yerden yönetiliyor.
Yeni `EntityNotFoundExceptionTests` sınıfı (`WordLearner.Tests/Common/Exceptions/`) bu formatı
doğruluyor (10→11 test, bkz. [[WordLearner_Tests]]). `docs/API_YOL_HARITASI/A-02_ortak-altyapi.html`'e
8. adım (`tur:'test'`) olarak işlendi; ayrıca [[WordLearner_Tests]] ve [[Repository]] wiki
sayfalarındaki test sayıları (7/9 gibi eski değerler) güncel duruma (11/10) çekildi.*

*Yedinci INGEST (2026-07-03): **A-02 (Ortak Altyapı) tamamlandı.** Kalan 3 checklist maddesi
yazıldı: (1) `ApiResponse<T>`/`ApiErrorResponse`/`PagedResult<T>` (`Application/Common/Models/`),
REFERENCE/API_ENDPOINTS.md §1'deki "Standart Yanıt" sözleşmesinin kod karşılığı — **NOT: `ApiResponse<T>`
ve `PagedResult<T>` sonradan (Sekizinci INGEST'te) YAGNI kuralıyla geri alındı, yalnızca
[[ApiErrorResponse]] kaldı.** (2) [[Middleware]] — `ExceptionHandlingMiddleware` (exception → `ApiErrorResponse`
JSON, `EntityNotFoundException`→404 diğerleri→500), `SecurityHeadersMiddleware` (5 güvenlik başlığı,
REFERENCE/SECURITY.md §5), `RequestResponseLoggingMiddleware` (Stopwatch + try/finally ile istek/yanıt
logu) — `API/Middleware/` altında. (3) [[Program_cs]] tam yapılandırıldı: Serilog (konsol+dosya;
DB/`ApplicationLog` sink'i A-04'e bırakıldı çünkü tablo migration'ı henüz yok), yeni
[[ApplicationServiceExtensions]] (`AddApplicationServices()` — MediatR/AutoMapper/FluentValidation,
kendi assembly'sinden reflection taraması), JWT Bearer authentication, CORS, ve middleware pipeline
sıralaması (loglama en dışta → güvenlik başlıkları → exception handling en içte). Bu iş için
`WordLearner.Application.csproj`'a 4 NuGet paketi eklendi: `MediatR` 12.1.1, `AutoMapper` 13.0.1,
`FluentValidation`+`FluentValidation.DependencyInjectionExtensions` 11.9.2 (AutoMapper 13.0.1'in
NU1903 güvenlik uyarısı var — REFERENCE/TECHNICAL_SPECIFICATIONS.md §1'de pinlenen sürüm olduğu için
bilinçli olarak korundu). Gerçek bir çalıştırmayla doğrulandı: uygulama ayağa kalkıyor, güvenlik
başlıkları her yanıtta mevcut, Türkçe istek/yanıt logları konsola düşüyor, mevcut 11 test hâlâ yeşil.
`docs/API_YOL_HARITASI/A-02_ortak-altyapi.html`'e 9/10/11. adımlar eklendi (A-02 artık 11 adımlı,
sayfa `durum: 'done'`). `docs/TASK/A_admin_panel_backend.md`'de A-02 ✅ işaretlendi,
`docs/TASK.md`'de sıradaki task A-03 olarak güncellendi. [[WordLearner_API]] ve
[[WordLearner_Application]] sayfaları yeni dosya/klasörleri yansıtacak şekilde güncellendi.*

*Sekizinci INGEST (2026-07-03) — YAGNI düzeltmesi: Kullanıcı, A-02'de `ApiResponse<T>` ve
`PagedResult<T>`'ın **hiçbir controller/endpoint yokken spekülatif olarak** yazıldığını fark etti
("bir yazılımcı AI olmadan bu sıraya göre mi yazardı?" sorusu) — REFERENCE/API_ENDPOINTS.md'deki
somut örnekler (`GET /words`, `POST /auth/register`) zaten bu zarfı kullanmıyor, yani tahmin edilen
şekil dokümanla bile örtüşmüyordu. Karar: `docs/TASK.md`'ye **"Spekülatif ortak tip yazılmaz (YAGNI)"**
kuralı eklendi — bir ortak tip yalnızca gerçek bir tüketicisi (o an yazılmakta olan başka bir kod
parçası) varken yazılır; istisna, o tipin zaten kanıtlanmış bir tüketicisi varsa (`ApiErrorResponse`'un
`ExceptionHandlingMiddleware` tarafından kullanılması gibi). Uygulama: `ApiResponse.cs` ve
`PagedResult.cs` silindi (`ApiErrorResponse.cs` kaldı); `docs/API_YOL_HARITASI/A-02_ortak-altyapi.html`
adım 9 yalnızca `ApiErrorResponse`'u içerecek şekilde daraltıldı (11 adım sayısı değişmedi, içerik
daraldı); `docs/TASK/A_admin_panel_backend.md`'deki A-02 "Ortak tipler" maddesi bu kararı açıklayan
bir nota dönüştürüldü. Wiki: [[ApiResponseModels]] silindi, yerine yalnızca [[ApiErrorResponse]]
düğümü kondu (içinde "YAGNI Düzeltmesi" başlıklı bir bölümle *neden* `ApiResponse<T>`/`PagedResult<T>`
burada olmadığı açıklanıyor — gelecekte biri bunları "zaten yazılmıştı" sanıp yanlışlıkla tekrar
eklemesin diye). [[Middleware]], [[Program_cs]], [[WordLearner_Application]] sayfalarındaki
`[[ApiResponseModels]]` linkleri `[[ApiErrorResponse]]`'a güncellendi. Build + 11 test tekrar
doğrulandı, hiçbir regresyon yok (bu iki tipin hiçbir gerçek tüketicisi olmadığı için silinmeleri
derlemeyi bozmadı — spekülatif olduklarının kanıtı).*

*Dokuzuncu INGEST (2026-07-05) — Çoklu dil altyapısı (WordConcept redesign): Kullanıcı ileride
İngilizce eklemek istediğini belirtti (DE-EN/EN-DE/TR-EN/EN-TR — yön fark etmeksizin); henüz A-05/A-06
kodlanmadığı için `docs/DATABASE_SCHEMA/Icerik.md` şeması koda geçmeden yeniden tasarlandı. **Seçilen
model: WordConcept** — dilden bağımsız bir kavram (`WordConcepts`: `PartOfSpeech`/`DifficultyLevel`/
`ImageUrl`) + her dildeki karşılığı için ayrı bir `Words` satırı (`WordConceptId`+`LanguageId`+`Text`),
yeni `Languages` tablosu (seed: yalnızca `de`+`tr`). Bir kelime tüm dilleriyle (şu an DE+TR) **aynı
işlemde** girilir/düzenlenir, sıralı değil. `WordDetails`'teki Almanca'ya özgü gramer alanları
(`Gender`, artikeller, `ConjugationData` vb.) tek bir `GrammarData` JSON kolonuna taşındı (şekli
`LanguageId`'ye göre değişir — trade-off: `Gender` üzerindeki DB-level `CHECK`/`INDEX` kayboldu).
`WordExamples` basitleştirildi (`SentenceDE`+`SentenceTR` yerine tek `SentenceText`). `Categories` aynı
mantıkla `Categories` (çekirdek) + yeni `CategoryTranslations` (dil başına ad) olarak ayrıldı;
`WordCategories` artık `WordId` yerine `WordConceptId`↔`CategoryId` (kategori kavram üzerinden bir kez
etiketlenir, tüm diller otomatik kapsanır). Bilinçli olarak kapsam dışı bırakıldı: `UserCards`
(kişisel kartlar) ve `ClassWords` (sınıf ad-hoc kelimeleri) — bunlar sistem sözlüğü değil, serbest
metin olarak kalıyor. Etkilenen dosyalar: `docs/DATABASE_SCHEMA.md` (domain haritası/ERD/seed/Kural 4),
`docs/REFERENCE/API_ENDPOINTS.md` §5-6 (`translations[]` şekli), `docs/REFERENCE/
GERMAN_LANGUAGE_FEATURES.md` (artık yalnızca Almanca'nın `GrammarData` şemasını tanımladığı netleşti),
`docs/REFERENCE/ARCHITECTURE.md` (`GermanWord`→`Words.Text`), `docs/00_INDEX.md` §4 ("yalnızca
Almanca-Türkçe" kuralı "şema çoklu dile açık, şu an DE-TR içerik" olarak güncellendi),
`docs/TASK/A_admin_panel_backend.md` (A-05/A-06 entity adımları `Language`/`WordConcept`/
`CategoryTranslation` içerecek şekilde güncellendi). Kontrol edildi: mevcut backend kodunda
(`backend/`) `Word`/`Category` ile ilgili hiçbir entity/alan yoktu, yani bu redesign hiçbir gerçek kodu
etkilemedi — yalnızca A-05/A-06 başlamadan önceki tasarım dokümanı düzeltildi. [[Icerik_Domain]] ve
[[Alman_Dili_Ozellikleri]] bu yeni modele göre güncellendi.*

*Onuncu INGEST (2026-07-05) — Türkçe ve İngilizce dil özellikleri referansları eklendi: Kullanıcı
`GrammarData`'nın yalnızca Almanca için tanımlı olmasının, "bir Almanın Türkçe öğrenmesi" senaryosunda
eksik kalacağını fark etti (önceki tasarımda Türkçe `GrammarData=NULL` varsayılmıştı — bu, Türkçe'nin
her zaman "bilinen dil/çeviri" olacağı, hiç "öğrenilecek hedef" olmayacağı varsayımına dayanıyordu, ki
yön-bağımsız eşleştirme hedefiyle çelişiyordu). Yeni dosyalar: `docs/REFERENCE/
TURKISH_LANGUAGE_FEATURES.md` (öncelikli, aktif içerik — ünlü uyumu, 6 hâl eki, çoğul, iyelik ekleri,
ünsüz yumuşaması, fiil çekimi) ve `docs/REFERENCE/ENGLISH_LANGUAGE_FEATURES.md` (şema hazır, henüz
kullanılmıyor — `Languages`'de `en` satırı yok). `docs/DATABASE_SCHEMA/Icerik.md`'deki `GrammarData`
notu güncellendi (artık üç dilin de kaynağına işaret ediyor). Wiki: yeni düğümler
[[Turkce_Dili_Ozellikleri]] ve [[Ingilizce_Dili_Ozellikleri]] eklendi, [[Icerik_Domain]] ve
[[Alman_Dili_Ozellikleri]]'ndeki çapraz linkler güncellendi. Mimari değişiklik yok — `GrammarData`
zaten `Words.LanguageId`'ye göre şekil değiştiren bir JSON alanıydı (dokuzuncu INGEST'te kuruldu), bu
yalnızca eksik olan dil içeriğini (Türkçe/İngilizce şeması) tamamladı.*

*On birinci INGEST (2026-07-05) — A-03 başladı (`User`/`RefreshToken` entity + migration) + iki yeni
tasarım kararı: **(1) Apple platformlar arası tutarlılık:** Kullanıcı "mobilde Apple ile kayıt olan
biri web'de nasıl giriş yapar?" sorusunu sordu — Apple'ın kullanıcı kimliğini (`sub`) client bazında
(Bundle ID/Services ID) ürettiği, gruplanmazsa aynı kişi için iki hesap açılacağı ortaya çıktı.
Çözüm bir Apple Developer Console ayarı (web Services ID'yi mobil Bundle ID'ye Primary App ID olarak
gruplama) — kod tarafında hiçbir karşılığı yok, `REFERENCE/ENV.md §4`'e ileriye dönük not eklendi.
Bugünkü pratik cevap: (a) `forgot-password` akışı `PasswordHash`'in var olup olmadığına bakmaz, sosyal
girişli hesaplar da şifre belirleyip web'de girebilir (kasıtlı davranış, bozulmamalı) — (b) aşağıdaki
QR girişi. **(2) QR Kod ile Giriş (Steam benzeri) eklendi — yeni task A-03.1:** Ayrı bir kimlik
doğrulama sistemi değil, A-03'teki `ITokenService`'e bağlanan yeni bir "kimliği kanıtlama yöntemi".
Yeni tablo `QrLoginSessions` (`QrTokenHash` SHA-256 + `PairingCode` 4 haneli — ikinci bir savunma
katmanı, DB sızıntısından bağımsız relay/phishing önlemi; `Status`: Pending→Scanned→Confirmed→Consumed
veya Denied/Expired) `DATABASE_SCHEMA/Auth.md`'ye eklendi. 5 yeni endpoint (`REFERENCE/API_ENDPOINTS.md
§3.1`), güvenlik akışı (`REFERENCE/SECURITY.md §1.2-1.3`), `SecurityLog` yeni event tipleri
(`QrLoginConfirmed`/`QrLoginDenied`, `DATABASE_SCHEMA/Loglama.md`), npm paketleri (`qrcode.react` web,
`expo-camera` mobil, `REFERENCE/TECHNICAL_SPECIFICATIONS.md`), `docs/TASK/A_admin_panel_backend.md`
(yeni A-03.1 bölümü), `docs/TASK/D_web_app.md` (D-03.1), `docs/TASK/E_mobil.md` (E-05.1). Henüz hiç
kod yazılmadı (yalnızca tasarım/dokümantasyon) — mevcut `User`/`RefreshToken`/migration kodunda hiçbir
değişiklik gerekmedi, ikisi de tamamen ek/yeni. [[Auth_Domain]], [[Guvenlik_Politikalari]],
[[Sistem_Mimarisi]] güncellendi.*

*On yedinci INGEST (2026-07-07, aynı gün) — A-03'ün MediatR CQRS'e refactor'ü: A-02'de MediatR paketi
kurulup DI'a kaydedilmişti ("ileride lazım olur" varsayımıyla) ama A-03 yazılırken hiç kullanılmadı —
`AuthController` doğrudan `IAuthService`'i çağırdı. Kullanıcı bu tutarsızlığı fark etti ("MediatR'ı
neden yazıp kullanmadık?") ve A-03'ün MediatR best-practice ile en baştan yazılıyormuş gibi retrofit
edilmesini istedi — A-03 çalışıyordu/doğruydu, bu saf bir mimari tutarlılık kararıydı, bug fix değil.
**Tasarım kararları:** (1) 13 endpoint = 13 Command (Auth'ta hepsi state değiştiriyor, Query yok),
her biri kendi dosyasında Command record + Handler class (dikey dilim, `Application/Features/Auth/`).
(2) Paylaşılan mantık iki yeni servise çıktı — handler'lar MediatR üzerinden birbirini çağıramadığı
için: `IOtpService`/`OtpService` (OTP üretimi/doğrulanması/temizlenmesi, 8 handler'ın paylaştığı) ve
`ILoginCompletionService`/`LoginCompletionService` (OTP/Google/Apple girişlerinin ortak son adımı +
`ExpiresInSeconds()` — bu ikincisini `RefreshCommandHandler` da paylaşıyor, `CompleteLoginAsync`
çağırmadan). (3) **En kritik tasarım sorunu:** `RefreshRequest` eskiden hem Refresh (`AuthTokenResponse`
döner) hem Logout (dönüşsüz) tarafından paylaşılıyordu — MediatR'da bir `IRequest<T>` tek dönüş tipine
bağlı olduğu için bu artık mümkün değildi. Çözüm: `RefreshCommand`/`LogoutCommand` ayrı tipler oldu,
ortak validator dosyası (`RefreshRequestValidator.cs`) silinip yerine 2 sınıflı
`RefreshAndLogoutValidators.cs` + paylaşılan kuralı taşıyan yeni `RefreshTokenRuleExtensions.cs`
geldi. (4) `FakePasswordHashForTiming` (Login+ConfirmAccountDeletion, 2 kullanım) ve
`AccountDeletionGraceDays` (1 kullanım) bilinçli olarak paylaşılan servise çıkarılmadı — aşırı
soyutlama olmasın diye, ilgili handler'da kaldı. **Sonuç — sıfır davranış değişikliği:** `IAuthService`/
`AuthService` silindi, 13 Command+Handler dosyası + `OtpService`/`LoginCompletionService` yazıldı,
`AuthController` artık yalnızca `IMediator.Send(command)` çağırıyor (ClientIp/UserId gibi gövdede
olmayan veriler `command with { ... }` ile model binding sonrası ekleniyor). `AuthServiceTests.cs`
(39 test) 13 handler test dosyası + `OtpServiceTests`/`LoginCompletionServiceTests`'e bölündü (72
test toplamda, kapsam kaybı yok — grace period/anonimleştirme testleri artık 3 kez değil tek yerde,
`LoginCompletionServiceTests`'te). `dotnet build`/`dotnet test` (72/72 yeşil) + gerçek sunucuyla
uçtan uca curl doğrulaması (register→verify-email→login→verify-otp→refresh→logout, hepsi eski
davranışla birebir) yapıldı. **Ayrıca fark edilen ayrı bir eksiklik:** `MessageResponse` metinleri
(“OTP gönderildi.” vb.) hardcode Türkçe — hata mesajlarının aksine (`ErrorMessages`+`Accept-Language`)
dile göre çözülmüyor; kullanıcı doğrudan görüyor, bu yüzden gerçek bir eksiklik (log/DB sabiti değil).
Kapsamı büyüteceği için bu refactor'a dahil edilmedi, `A_admin_panel_backend.md`'ye yeni bir
**A-03.2** task'ı olarak eklendi. **Etkilenen dosyalar:** `Application/Features/Auth/*` (13 yeni),
`Application/Services/OtpService.cs`+`LoginCompletionService.cs` (+ arayüzleri, yeni),
`Application/Validators/Auth/RefreshAndLogoutValidators.cs`+`RefreshTokenRuleExtensions.cs` (yeni,
eski `RefreshRequestValidator.cs` silindi), diğer 6 validator dosyası (Command'a retarget),
`Application/DTOs/Auth/*` (6 dosya silindi — Request yarısı Command'a taşındı), `AuthController.cs`,
`ApplicationServiceExtensions.cs`, `IAuthService.cs`+`AuthService.cs` (silindi),
`WordLearner.Tests/Features/Auth/*` (13 yeni test dosyası) + `Services/OtpServiceTests.cs`+
`LoginCompletionServiceTests.cs` (yeni), eski `AuthServiceTests.cs` (silindi). Roadmap:
`API_YOL_HARITASI/_TASLAK.html` (`tur` listesine `handler` eklendi), `A-03_auth-api.html` (36→63 adım,
adım 25-26/27-39/40/41-42/45/49-63 yeni veya güncellendi, geri kalanı — 1-24, 30-31→43-44, 33→46,
34-35→47-48 — birebir korundu, otomatik script ile üretilip diff doğrulandı). Doküman:
`docs/TASK/A_admin_panel_backend.md` (A-03 checkbox'ları değişmedi — davranış regresyonu yok,
yalnızca mimari not eklendi + yeni A-03.2), `docs/wiki/Backend/WordLearner_Application.md`,
`ApplicationServiceExtensions.md`, `API_Yol_Haritasi_Sistemi.md`, `WordLearner_API.md`,
`docs/wiki/Database/Auth_Domain.md`, `docs/wiki/Backend/AppException.md`,
`docs/wiki/Standartlar/Teknik_Ozellikler.md` (MediatR/BCrypt/JWT/Google.Auth paket durumları
güncellendi — "A-03'te eklenecek" artık "kurulu ve kullanımda").*

*On sekizinci INGEST (2026-07-07, aynı gün) — AutoMapper retrofit'i: On yedinci INGEST'teki MediatR
retrofit'iyle birebir aynı desen tekrar ortaya çıktı. A-02'de AutoMapper paketi kurulup DI'a
kaydedilmişti ("ileride Entity↔DTO dönüşümü için Profile sınıfları eklenecek" varsayımıyla) ama
A-03'ün MediatR'a taşınmış hâlinde de (on yedinci INGEST) hiç kullanılmadı — 13 Command Handler'ın
hepsi Entity→DTO dönüşümünü elle yapıyordu. Kullanıcı bunu kontrol edip aynı YAGNI tutarsızlığını
fark etti; farklı olarak burada "kaldır mı, kullan mı" gerçek bir seçimdi (MediatR'da kullanmamak
mimari kayıptı, AutoMapper'da değildi — DTO'lar 3-5 alanlı, elle mapping zaten okunaklıydı). İki
seçenek sunuldu (paket kaldırılsın / gerçekten kullanılsın); kullanıcı ikincisini seçti — paketi
öğrenmek ve kullanmak istediğini belirtti. **Uygulama:** Repo'da gerçek Entity→DTO dönüşümü yalnızca
iki yerde vardı: `RegisterCommandHandler` → `RegisterResponse(user.Id, user.Email, user.FirstName,
user.CurrentLevel)` ve `LoginCompletionService`+`RefreshCommandHandler` → `AuthUserDto(user.Id,
user.CurrentLevel)` (ikisi bağımsız, aynı satırı iki yerde tekrarlıyordu — `RefreshCommandHandler`
`CompleteLoginAsync`'i çağırmadığı için). Geri kalan 9 handler yalnızca sabit `MessageResponse("...")`
string'i döndüğü için (entity kaynaklı değil) AutoMapper'a konu olmadı — `AuthTokenResponse`'un
tamamı da (token'lar+expiresIn+accountWasRecovered) tek kaynaktan gelmediği için elle inşa edilmeye
devam ediyor, yalnızca içindeki `AuthUserDto` alt-nesnesi mapping'e taşındı. Yeni
`Features/Auth/AuthProfile.cs` (`CreateMap<User,RegisterResponse>()`, `CreateMap<User,AuthUserDto>()`
— alan adları birebir eşleştiği için `ForMember` gerekmedi) `ApplicationServiceExtensions`'taki
mevcut `AddAutoMapper(applicationAssembly)` tarafından otomatik bulunuyor, DI değişikliği gerekmedi.
Üç sınıfa (`RegisterCommandHandler`, `LoginCompletionService`, `RefreshCommandHandler`) `IMapper`
enjekte edildi. Testler mock `IMapper` yerine gerçek `AuthProfile`'dan kurulmuş bir `IMapper`
(`new MapperConfiguration(cfg => cfg.AddProfile<AuthProfile>()).CreateMapper()`) kullanacak şekilde
güncellendi — bu hem handler'ı besliyor hem de profil konfigürasyonunun geçerli olduğunu (AutoMapper
`AssertConfigurationIsValid` mantığına paralel) doğruluyor. **Sonuç — sıfır davranış değişikliği:**
`dotnet build` (0 hata) + `dotnet test` (72/72 yeşil, test sayısı değişmedi). **Etkilenen dosyalar:**
`Application/Features/Auth/AuthProfile.cs` (yeni), `RegisterCommand.cs`, `RefreshCommand.cs`,
`Application/Services/LoginCompletionService.cs`, `WordLearner.Tests/Features/Auth/
RegisterCommandHandlerTests.cs`+`RefreshCommandHandlerTests.cs`,
`WordLearner.Tests/Services/LoginCompletionServiceTests.cs` (3 test dosyasının BAĞIMLILIKLAR
yorumuna da AutoMapper eklendi — roadmap'le birebir diff karşılaştırması bu eksikliği ortaya
çıkardı). Doküman: `docs/wiki/Backend/AuthProfile.md` (yeni), `ApplicationServiceExtensions.md`
(AutoMapper artık "henüz kullanılmadı" değil, `AuthProfile`'a link), `Teknik_Ozellikler.md`
(paket tablosu satırı güncellendi). **Roadmap:** Kullanıcı roadmap'in gerçek koddan sapmasını
kabul etmedi ("api yol haritasını güncelle... bizim güncel apilarımız ile aynı olmalı tamamen tam
olarak") — `API_YOL_HARITASI/A-03_auth-api.html` 63→64 adıma çıkarıldı: yeni adım 27 olarak
`AuthProfile.cs` eklendi (27\'den itibaren tüm adımlar +1 kaydı, on yedinci INGEST\'teki
36→63 genişlemesiyle aynı desen), adım 26/28/35/51/52/59'un (LoginCompletionService/
RegisterCommand/RefreshCommand + 3 test dosyası) kod bloklarına AutoMapper notu ve gerçek kod
işlendi. Doğrulama otomatikti: bir Node script `window.API.adimlar` dizisini `eval` ile ayrıştırıp
her ilgili adımın `kod` alanını gerçek dosya içeriğiyle karakter karakter kıyasladı (6/6 `MATCH`)
ve `num` dizisinin 1..64 ardışık olduğunu doğruladı.*

*On dokuzuncu INGEST (2026-07-07, aynı gün) — [[API_Yol_Haritasi_Sistemi]]'ne iki seviyeli adım
gruplama eklendi: A-03'ün 64 adımı tek düz akordiyon listesiydi, kullanıcı bunun okunamaz hâle
geldiğini ve gelecekte A-03.1 (QR Kod ile Giriş, henüz yazılmadı — bkz. `TASK/A_admin_panel_backend.md`)
eklendiğinde daha da büyüyeceğini fark etti. **Karar (kullanıcıya 3 seçenek sunuldu, "ayrı dosya +
gruplama" seçildi):** iki bağımsız mekanizma birleştirildi — (1) **dosyalar arası şişme:** kendi
entity/service/controller'ı olan büyük alt-görevler (ör. A-03.1) `adimlar[]`'a eklenmek yerine
kendi `<faz>.<alt>_<id>.html` dosyasını açar, iki sayfa yeni `relatedRefs` alanıyla (aynı
`frontendRefs` deseninde, çift yönlü) birbirine bağlanır — `_TASLAK.html`'e "✂️ BÜYÜK ALT-GÖREV =
AYRI DOSYA" kuralı olarak yazıldı (ayırt edici soru: yeni entity/controller var mı? A-03.1 evet,
gelecekteki A-03.2 mesaj lokalizasyonu hayır — o yüzden A-03.2 kendi dosyasını AÇMAYACAK, A-03'e
16. bir `grup` olarak eklenecek); (2) **tek dosya içinde şişme:** `render.js`'e opsiyonel
`adim.grup` (adımları bölüm başlığına ayırır) + `API.katmanlar` (`grup`'ları mimari katmana toplayan
üst seviye, `[{ad, gruplar:[...]}]`) eklendi — ikisi de `kod` alanına DOKUNMAZ, yalnızca üstte bir
İçindekiler kutusu ve adımların arasına başlık basar; kullanıcı ilk turda gruplamayı onayladıktan
sonra "daha da kategorilendirebilir miyiz" diye sorunca `katmanlar` (2. seviye) de eklendi.
**Uygulama:** `A-03_auth-api.html`'in 64 adımı 15 `grup`'a (Veri Modeli, Şifre Servisi, Token
Servisi, DTO'lar, Hata Yönetimi, Repository'ler, E-posta & Sosyal Login Doğrulayıcılar, Paylaşılan
Servisler, 13 Command+Handler, DI Kaydı, Validator'lar, Dil Çözümleme & Doğrulama Filtresi,
Controller, Rate Limiting, Birim Testler), bu 15 grup da 4 `katman`'a (Domain & Altyapı,
Application — Servisler, Application — CQRS, API & Test) bölündü — Python regex script'iyle her
adımın `num: N, tur: 'X',` satırının SONUNA `grup: '...'` eklendi (tek satırlık ekleme, yeni satır
açmadı), `katmanlar` dizisi `adimlar[]`'dan önce elle eklendi. **Doğrulama:** `node --check` (JS
sözdizimi bozulmadı — bir `kod:` template literal'ı yanlışlıkla bozulsaydı `eval` anında patlardı),
adım/grup/katman eşlemesi script ile çapraz kontrol edildi (15/15 grup + 15/15 katman eşleşti, ne
eksik ne fazla), dosya 6370→6389 satır (+19, tam olarak eklenen `katmanlar` bloğu kadar — hiçbir
`kod` bloğu satır kaymadı), A-02 (grup/katman kullanmayan sayfa) render çıktısı öncekiyle birebir
aynı kaldı (geriye dönük uyumluluk). **Etkilenen dosyalar:** `API_YOL_HARITASI/render.js` (`slug()`,
İçindekiler + `.step-group`/`.step-katman` render mantığı, `relatedRefs` bandı — hepsi opsiyonel,
alan yoksa davranış değişmez), `style.css` (`.toc`, `.toc-katmanlar`, `.step-group`, `.step-katman`
stilleri), `_TASLAK.html` (🗂️ ADIM GRUPLAMA, 🗃️ KATMAN GRUPLAMA, ✂️ BÜYÜK ALT-GÖREV, 🔗 İLGİLİ API
kuralları eklendi), `A-03_auth-api.html` (`grup`+`katmanlar` eklendi, `kod` alanları değişmedi),
`API_Yol_Haritasi_Sistemi.md` (yeni "Adım Gruplama" ve "İkinci Seviye: Katman Gruplama" bölümleri,
63→64 adım düzeltmesi). A-03.1 henüz yazılmadığı için `relatedRefs` şu an boş — A-03.1 gerçekten
yazıldığında iki yönlü doldurulacak.*

*Yirminci INGEST (2026-07-08) — A-03.1 (QR Kod ile Giriş) tamamlandı ✅: bir önceki oturumda yalnızca
veri modeli (entity+config+migration) vardı, bu oturumda geri kalan her şey yazıldı — `IQrLoginSessionRepository`/
`QrLoginSessionRepository`, `QrSessionGoneException`(410)/`QrSessionForbiddenException`(403) +
`ExceptionHandlingMiddleware`/`ErrorMessages` entegrasyonu, `QrLoginDtos.cs`, paylaşılan
`QrLoginSessionExpiryExtensions` (lazy expire — ayrı temizlik job'ı yok), 5 MediatR Command+Handler
(`GenerateQrLoginCommand`/`ScanQrLoginCommand`/`ConfirmQrLoginCommand`/`DenyQrLoginCommand`/
`GetQrLoginStatusCommand`, `Application/Features/QrLogin/`), `QrLoginController` (4 endpoint,
`/auth/qr/*`), Program.cs'e IP-partitioned `qrGenerate` rate limit policy'si (20/saat), 5 test dosyası
(18 test). Onaylanınca token üretimi A-03'teki AYNI `ILoginCompletionService.CompleteLoginAsync`'i
çağırıyor — QR ayrı bir kimlik doğrulama sistemi değil (bkz. SECURITY.md §1.3). **Tasarım kararı
(kullanıcıyla netleştirildi):** RequesterIp/RequesterDeviceInfo `generate` adımında (web'in isteğinden)
yazılıyor, `scan`'de değil — mobil ekranda "seni İSTEYEN taraf" gösterilip kullanıcı gözle doğruluyor
(relay/phishing önlemi); `GetQrLoginStatusCommand`'daki `ClientIp` de aynı sebeple web'in IP'si, telefonun değil.
**Roadmap:** `A-03.1_qr-login.html` 5→26 adıma çıktı (`grup` eklendi: Veri Modeli/Repository/Hata
Yönetimi & Yardımcılar/DTO'lar/5 QR Command+Handler/DI Kaydı/Controller & Rate Limiting/Birim Testler),
`durum: 'wip'→'done'`, `index.html`'de aynı satır güncellendi, `TASK/A_admin_panel_backend.md` A-03.1 ⬜→✅.
**Yan iş — roadmap'e yeni bir özellik eklendi (kullanıcı isteği):** `adim.sonuclar` alanı — her
`tur:'test'` adımının kodun altına, varsayılan KAPALI bir "Sonuç" dropdown'u basıyor;
`dotnet test --logger trx` çıktısından (GERÇEK çalıştırma, uydurma değil) alınan `{test, durum, sure}`
üçlüleriyle dolduruluyor, opsiyonel `hata` alanı yalnızca Failed'de dolar. Bu alan yalnızca A-03.1'e
değil **geriye dönük olarak A-02 ve A-03'e de** eklendi (kullanıcı "şu ana kadar yazılmış olanlar da
dahil" dedi) — tek bir `dotnet test --logger trx` koşusundan (90/90 yeşil) Python ile trx XML parse
edilip, `tur:'test'` adımının `dosya:` alanındaki sınıf adıyla eşleştirilerek 24 test dosyasının hepsine
otomatik enjekte edildi (elle yazılmadı). **Etkilenen dosyalar:** `render.js` (`renderTestResults()`,
escape-güvenli — `esc()` içeriği önce işler), `style.css` (`.test-results*`, `.tr-hata`),
`_TASLAK.html` (yeni "✅ TEST SONUÇLARI" kuralı), bu dosya + `API_Yol_Haritasi_Sistemi.md` (yeni bölüm),
`A-02_ortak-altyapi.html`/`A-03_auth-api.html`/`A-03.1_qr-login.html` (toplam 24 `sonuclar` bloğu).
**Doğrulama:** `node vm.Script` ile 3 dosyanın da JS sözdizimi bozulmadığı, `adimlar` sayısı ve toplam
test sayısı (11+61+18=90, gerçek `dotnet test` çıktısıyla birebir) script ile çapraz kontrol edildi.
**Sıradaki task:** A-03.2 (Auth başarı mesajlarının lokalizasyonu).*

*Yirmi birinci INGEST (2026-07-08) — İki ayrı kapsam: **(1) Dokümantasyon: ortak kurallar
`CLAUDE.md`'de merkezileştirildi** (kod değişikliği yok, saf dokümantasyon commit'i, `f39d993`).
Her `docs/REFERENCE/*` ve `docs/DATABASE_SCHEMA*` dosyasında tekrar eden "genel kurallar" blokları
(dil kuralı, roller/sahiplik, çoklu dil, veri katmanı, kimlik&güvenlik, test standardı özeti) tek
bir kök `/CLAUDE.md`'ye toplandı; o dosyalar artık yalnızca kendi **özgün** içeriklerini (yorum
şablonları, sistem akışları, ERD, JWT detayları vb.) taşıyıp genel kural için `CLAUDE.md §N`'e
referans veriyor — hiçbir bilgi kaybı yok, yalnızca tekilleştirme (`docs/REFERENCE/*` toplamda
~550 satır küçüldü). Kullanılmayan Obsidian taslak dosyası (`docs/wiki/Başlıksız.base`) kaldırıldı.
Wiki: [[Index]]'in "Kaynak Dokümanlar" bölümüne `CLAUDE.md` kök kaynak olarak eklendi.
**(2) Wiki senkronizasyon açığı kapatıldı** (kullanıcı "son pushladıklarımızdan eksik var mı bak"
diye sordu, kontrol edildi): On yedinci/On sekizinci/Yirminci INGEST'lerin "Etkilenen dosyalar"
listesi bazı Backend wiki düğümlerini "güncellendi" olarak işaretlemişti ama gerçek dosya içeriği
hâlâ A-02/A-03-öncesi durumu yansıtıyordu — INGEST kaydı ile gerçek içerik arasında sapma vardı.
Düzeltilen düğümler: [[WordLearner_Application]] (Klasör Yapısı hâlâ yalnızca A-02'yi gösteriyordu,
`Features/Auth`/`Features/QrLogin`/`Validators/Auth`/`DTOs/Auth`/`Services/` hiç yoktu; BCrypt/JWT/
Google.Apis.Auth hâlâ "planlanan, kurulmamış" deniyordu — hepsi aktif), [[WordLearner_Infrastructure]]
(`QrLoginSessionRepository` eksikti), [[WordLearner_API]] (`QrLoginController`, 5 endpoint, hiç
anılmıyordu — sayfa hâlâ "ilk ve tek controller AuthController" diyordu), [[WordLearner_Tests]]
(toplam 72 diyordu, gerçek 90 — 18 QrLogin testi eksikti), [[Auth_Domain]] (QrLoginSessions bölümü
hâlâ "planlı, henüz kod yok" diyordu, oysa A-03.1 tamamlanmıştı; "Referans Kod" bölümü hâlâ "henüz
yazılmadı" diyordu), [[AppException]]/[[ErrorMessages]] (QR_SESSION_GONE/QR_SESSION_FORBIDDEN kod
listesinde yoktu). Ayrıca **CLAUDE.md commit'inden bağımsız, daha eski bir tutarsızlık** bulundu:
[[Backend_Katmanli_Mimari]] hâlâ "yalnızca A-01/A-02 yazıldı, feature entity/servis/controller
henüz yok" diyordu (A-03/A-03.1'i hiç yansıtmıyordu) ve katman şeması hâlâ eski "Servis Arayüzü/
Servis" desenini gösteriyordu; [[Sistem_Mimarisi]]'nde de `Controllers → Services → Repositories`
satırı MediatR retrofit'ini (on yedinci INGEST) yansıtmıyordu — ikisi de düzeltildi
(`Controllers → (MediatR) Command/Handler → Repositories`). **Kök neden:** geçmiş INGEST'ler
domain'e özgü sayfaları (ör. [[Auth_Domain]] bir istisna dışında) güncellerken, üst-seviye
proje-yapısı/mimari özet sayfalarını (Backend/*, Architecture/*) atlamış — bundan sonraki her
INGEST'te bu iki kategori de kontrol edilmeli. Kod tarafında hiçbir değişiklik yapılmadı, yalnızca
wiki gerçek koda/dokümana yeniden hizalandı.*

*Yirmi ikinci INGEST (2026-07-11) — **A-03.2 (Auth Başarı Mesajlarının Lokalizasyonu) tamamlandı ✅:**
On yedinci INGEST'te (A-03'ün MediatR retrofit'i sırasında) fark edilen bir eksiklik kapatıldı — hata
mesajları [[ErrorMessages]] ile `Accept-Language`'a göre dile göre çözülürken, `MessageResponse`
üreten 7 Auth endpoint'inin başarı metinleri (ör. "OTP gönderildi.") hâlâ hardcode Türkçe'ydi;
kullanıcı bunu doğrudan görüyordu (log/DB sabiti değil), bu yüzden bir eksiklikti. **Uygulama:**
Yeni [[SuccessMessages]] (`Application/Common/Localization/`) — [[ErrorMessages]] ile birebir aynı
`Resolve(code, language)` deseni, 7 kod (`OTP_SENT`, `EMAIL_VERIFIED`, `VERIFICATION_CODE_SENT`,
`PASSWORD_UPDATED`, `PASSWORD_RESET_OTP_SENT`, `ACCOUNT_DELETION_OTP_SENT`,
`ACCOUNT_DELETION_CONFIRMED`, tr+de). Ayrı sözlük (ErrorMessages'a eklenmedi): kodlar anlamca farklı
kümeler (`ACCOUNT_DELETED` orada bir HATA koduyken burada `ACCOUNT_DELETION_CONFIRMED` bir BAŞARI
kodu). `MessageResponse` DTO'su `record MessageResponse(string Message)` iken
`record MessageResponse(string Code, string Message)` oldu — `ApiErrorResponse` içindeki
`ApiErrorDetail(Code, Message)` ile simetrik. `MessageResponse` döndüren 7 Command'a
(`LoginCommand`, `ResendVerificationCommand`, `ResetPasswordCommand`,
`ConfirmAccountDeletionCommand`, `RequestAccountDeletionCommand`, `VerifyEmailCommand`,
`ForgotPasswordCommand`) `UserId`/`ClientIp` ile aynı desende bir `Language` init-only property
eklendi (gövdeden gelmiyor, `Accept-Language` header'ından geliyor); her Handler artık
`SuccessMessages.Resolve(code, request.Language)` çağırıyor. 13'ün geri kalan 6'sı (`RegisterCommand`,
`VerifyLoginOtpCommand`, `LoginWithGoogleCommand`, `LoginWithAppleCommand`, `RefreshCommand`,
`LogoutCommand`) kapsam DIŞINDA kaldı — `AuthTokenResponse`/`RegisterResponse` döndürüyorlar ya da
(Logout) hiç gövde döndürmüyorlar, dile göre değişen bir metin taşımıyorlar. `AuthController`'a
`ClientIp` ile aynı desende bir `Language` computed property eklendi
(`RequestLanguageResolver.Resolve(HttpContext)` — bu yardımcı YENİ DEĞİL, A-03'ten beri
`ValidationFilter`/`ExceptionHandlingMiddleware` tarafından kullanılıyordu, burada yeniden
kullanıldı), ilgili 7 endpoint `command with { Language = Language }` (gövdesiz
`RequestAccountDeletion`'da `new RequestAccountDeletionCommand(CurrentUserId) { Language = Language }`)
ile güncellendi. 7 handler test dosyasına birer `Xxx_GermanLanguage_ReturnsGermanMessage` testi
eklendi (`Code` sabit kaldığını + `Message`'ın Almanca döndüğünü doğrular) — 7 yeni test, toplam
97/97 yeşil (A-03 72 + A-03.1 18 + A-03.2 7; `WordLearner.Tests/TestResults/a032.trx`'te 37 test
GERÇEKTEN çalıştırıldı — bu koşuda A-03.1'in QrLogin testleri de birlikte yer aldı, 24'ü A-03.2'nin
Auth dosyalarına ait). **Roadmap:** A-03.1 gibi kendi entity/controller'ı olmadığı için (bkz.
`_TASLAK.html` "✂️ BÜYÜK ALT-GÖREV = AYRI DOSYA" notu — ayırt edici soru budur) A-03'e 65. bir `grup`
olarak EKLENMEDİ, yine de A-03 sayfası zaten 64 adımken daha da büyütmek okunabilirliği bozacağı için
küçük-orta ölçekli ayrı bir sayfa açıldı: `API_YOL_HARITASI/A-03.2_auth-success-message-localization.html`
(17 adım, 4 grup: Lokalizasyon Altyapısı/7 Handler/Controller/Testler — `sonuclar` alanları
a032.trx'ten gerçek veriyle dolduruldu), `index.html`'e satır eklendi, `A-03_auth-api.html` ve
`A-03.1_qr-login.html`'in `relatedRefs`'i iki yönlü güncellendi (üçü de birbirine bağlı).
`TASK/A_admin_panel_backend.md` A-03.2 ⬜→✅. **Etkilenen dosyalar (kod, benim tarafımdan değil
kullanıcı tarafından zaten tamamlanmış, yalnızca dokümantasyon/roadmap/wiki senkronize edildi):**
`Application/Common/Localization/SuccessMessages.cs` (yeni), `Application/DTOs/Auth/MessageResponse.cs`,
7 Command dosyası (`Application/Features/Auth/*`), `AuthController.cs`, 7 test dosyası
(`WordLearner.Tests/Features/Auth/*`). Doküman: bu dosya (`Index.md`), yeni
`docs/wiki/Backend/SuccessMessages.md`, `ErrorMessages.md` (kardeş linki eklendi),
`WordLearner_Application.md` (özet + Klasör Yapısı + Dosyalar listesi), `Gelistirme_Yol_Haritasi.md`
(GÜNCEL OLMAYAN bir durumdaydı — hâlâ "A-03.1 ⬜", "A-03 61 test", "Sıradaki: A-03.1" yazıyordu;
gerçek duruma göre A-03/A-03.1/A-03.2 ✅, 97 test, sıradaki A-04 olarak düzeltildi).*

*Yirmi üçüncü INGEST (2026-07-11, aynı gün) — **Üç paralel denetim (wiki/md/backend kod) + toplu
düzeltme.** Kullanıcı "her şeyi kontrol ediyoruz" diyerek üç ayrı subagent'a wiki, `docs/*.md` ve
gerçek backend kodunu bağımsız incelettirdi; bulgular çapraz karşılaştırıldı. Kod tarafında **hiçbir
gerçek kural ihlali bulunmadı** (rol sızması yok, SQL injection yok, UserId/soft-delete filtreleri
doğru, ENV/PII kuralına uyumlu) — tüm bulgular dokümantasyonun koddan geride kalmasıydı:
**(1) tr/en↔tr/de çelişkisi:** `CLAUDE.md §1`, `SECURITY.md §1.4`, `API_ENDPOINTS.md §1` "(tr/en)"
diyordu, gerçek kod (`ErrorMessages.cs`/`SuccessMessages.cs`) bilinçli olarak yalnızca **tr/de**
dolu (hedef DE↔TR, `en` YAGNI ile eklenmedi) — mimari zaten dile-göre-anahtarlanan sözlük olduğu
için esnek (yeni dil = sözlüğe sütun, kod değişmez); üç dosya + [[ErrorMessages]] "tr/de + gelecekte
esnek" olarak düzeltildi. **(2) `Standartlar/*` üç düğüm tamamen bayattı:** [[Guvenlik_Politikalari]]
(BCrypt/QR-giriş/güvenlik-başlıkları "planlı" diyordu, hepsi ✅), [[Teknik_Ozellikler]] (JWT/Şifre
servisi/`Program.cs` "planlanan, henüz yok" diyordu, A-02/A-03'te tamamlandı), [[API_Sozlesmesi]]
(başlık "henüz hiç endpoint yok" diyordu, 18 endpoint canlı) — Yirmi birinci INGEST'in bulduğu
"üst-seviye özet sayfası atlanıyor" deseni bu üçüne hiç uygulanmamıştı, şimdi düzeltildi.
**(3) Test sayısı [[WordLearner_Tests]]/[[Backend_Katmanli_Mimari]]'de hâlâ 90 yazıyordu**
(A-03.2'nin +7'si işlenmemiş, Yirmi ikinci INGEST'te bu iki düğüm atlanmış) → 97'ye düzeltildi, A-03
toplamı 72→79. **(4) [[API_Yol_Haritasi_Sistemi]] kendi içinde çelişiyordu** (A-03.1'i hem "henüz
yazılmadı" hem "18 testle tamam" diyordu) ve A-03.2 sayfasından hiç bahsetmiyordu — `Dosyalar`
tablosuna A-03.1/A-03.2 satırları, `LISTE` dizisine dört satır, "A-03.1 henüz yazılmadı" → "✅
tamamlandı" düzeltildi. **(5) [[Loglama_Domain]]** `SecurityLog.EventType`'ı 6 değerle listeliyordu,
`LogEventType` enum'u 10 değer taşıyor (`PasswordReset`/`AccountDeletion`/`QrLoginConfirmed`/
`QrLoginDenied` eksikti) → tamamlandı. **Ayrıca `docs/*.md` tarafında (wiki dışı):**
`DEVELOPMENT_SETUP.md §8` gerçekte hiç kullanılmayan `develop`/`feature/` branch modeli ve
Conventional Commits formatı öneriyordu (gerçek pratik: tek dal `main`, Türkçe+task-no commit,
`CLAUDE.md §7`) → düzeltildi; `DATABASE_SCHEMA.md` üst notu "`Words.CreatedBy`" diyordu, doğrusu
`WordConcepts.CreatedBy` → düzeltildi; `TASK.md`'nin "Sıradaki task" satırı hâlâ A-03.2'yi
gösteriyordu (A-03.2 zaten ✅) → A-04 olarak düzeltildi. **Bilinçli olarak DOKUNULMADI:**
`API_ENDPOINTS.md`'de "## 2." bölüm numarasının atlanmış olması — kozmetik bir boşluk, ama mevcut
numaralandırma (`§3`=Auth, `§4`=Kullanıcı, ...) 40'tan fazla yerde (`TASK/*.md`, wiki, `DATABASE_SCHEMA/
Auth.md`) çapraz referans veriyor; yeniden numaralandırmanın riski kazancından büyük. **Kod tarafında
hiçbir değişiklik yapılmadı**, yalnızca dokümantasyon/wiki gerçek koda yeniden hizalandı. **Etkilenen
dosyalar:** `CLAUDE.md`, `docs/REFERENCE/{SECURITY,API_ENDPOINTS,DEVELOPMENT_SETUP}.md`,
`docs/DATABASE_SCHEMA.md`, `docs/TASK.md`, `docs/wiki/Backend/{ErrorMessages,WordLearner_Tests,
API_Yol_Haritasi_Sistemi}.md`, `docs/wiki/Architecture/Backend_Katmanli_Mimari.md`,
`docs/wiki/Standartlar/{Guvenlik_Politikalari,Teknik_Ozellikler,API_Sozlesmesi}.md`,
`docs/wiki/Database/Loglama_Domain.md`, bu dosya (`Index.md`).*

*Yirmi dördüncü INGEST (2026-07-11, aynı gün) — **Kod kalitesi denetimi + 4 gerçek bug fix
(QR ile Giriş akışı).** Yirmi üçüncü INGEST'in "kod tarafında hiçbir kural ihlali yok" tespitinden
sonra kullanıcı ayrı bir tur olarak saf kod kalitesi/mühendislik pratiği denetimi istedi (3 paralel
subagent: Domain+Infrastructure, Application, API+Testler). Bu kez **4 gerçek fonksiyonel bug**
bulundu ve düzeltildi (dokümantasyon değil, davranış hatası):
**(1) QR polling kendi kendini kilitliyordu:** `GET /auth/qr/{token}/status` (web'in ~2sn'de bir
sorguladığı polling endpoint'i) paylaşımlı `"anonymous"` rate-limit policy'sini (10/dk, TÜM anonim
trafik ortak) kullanıyordu — bu polling hızı (~30/dk) o bütçeyi ~20 saniyede tüketip
register/login/forgot-password dahil TÜM anonim kullanıcıları 429'a düşürüyordu. `qrGenerate`
zaten IP-partitioned yazılmışken `status`'a aynı disiplin uygulanmamıştı. Düzeltme: `Program.cs`'e
yeni IP-partitioned `"qrStatus"` policy'si (40/dk/IP) eklendi, `QrLoginController.GetStatus` buna
geçirildi. **(2) Audit alanları (`CreatedByUserId`/`UpdatedByUserId`) hiçbir Auth/QrLogin
Handler'ında doldurulmuyordu** — `IRepository.cs`'nin kendi "Auth/A-03 tamamlanana kadar null
geçilir" notuna rağmen 22 `AddAsync`/`UpdateAsync` çağrısının TAMAMI `userId`'yi boş bırakıyordu.
Düzeltme: kaydın SAHİBİ kendi eylemiyle güncelliyorsa o kullanıcının Id'si geçiliyor artık; yalnızca
gerçek self-servis KAYIT OLUŞTURMA (Register, Google/Apple yeni-kullanıcı dalı,
GenerateQrLoginCommand) ve sistemin otomatik geçişlerinde (QR oturumu expiry) `null` kalıyor —
`IRepository.cs`'nin NEDEN yorumu bu kurala göre yeniden yazıldı. **(3) QR girişi soft-delete/
hesap-durumu kontrolünü atlıyordu:** `GetQrLoginStatusCommand` kullanıcıyı `GetByIdAsync` (soft-
delete filtreli) ile buluyordu, normal login ise `GetByEmailAsync` ile (filtresiz) — hesabını yeni
silmiş bir kullanıcı QR ile giriş tamamlarken anlamsız bir 404 alıyordu, `ILoginCompletionService`'in
grace-period kurtarma mantığına hiç ulaşamıyordu. Düzeltme: `IUserRepository`'ye
`GetByIdIncludingDeletedAsync` eklendi (`GetByEmailAsync` ile aynı `IgnoreQueryFilters()`
gerekçesi), ayrıca diğer giriş yollarıyla parite için `IsActive` kontrolü eklendi. **(4) Ham QR
token exception mesajına gömülüyordu:** Scan/Confirm/Deny/GetStatus'ta oturum bulunamayınca
`EntityNotFoundException(typeof(QrLoginSession), request.QrToken)` — ham token (bir secret) log'a
gidebilecek bir mesaja yazılıyordu; projede her diğer secret yalnızca hash'i üzerinden ele
alınıyor. Düzeltme: `request.QrToken` → zaten hesaplanmış `tokenHash`. **Testler:** 4 fix için
mevcut 3 test (2 Auth, 1 QrLogin) mock-verify assertion'ları güncellendi, `Features/QrLogin/`'e 5
yeni regresyon testi eklendi (Confirm/Deny'e ilk kez `TokenNotFound` testi, GetQrLoginStatus'a
grace-period kurtarma/inactive/anonymized senaryoları) — 97→102, hepsi yeşil. **Roadmap:**
`API_YOL_HARITASI/{A-02_ortak-altyapi,A-03_auth-api,A-03.1_qr-login}.html`'deki ilgili `adim.kod`
alanları güncel koda senkronize edildi (CLAUDE.md §6 — kod alanı gerçek dosyanın birebir kopyası
olmalı kuralı). **Etkilenen kod dosyaları:** `API/Program.cs`, `API/Controllers/QrLoginController.cs`,
`Application/Interfaces/Repositories/{IRepository,IUserRepository}.cs`,
`Infrastructure/Repositories/UserRepository.cs`, `Application/Features/Auth/{Login,ForgotPassword,
ConfirmAccountDeletion,Logout,Refresh,ResetPassword,RequestAccountDeletion,ResendVerification,
VerifyEmail}Command.cs`, `Application/Services/LoginCompletionService.cs`,
`Application/Features/QrLogin/{Scan,Confirm,Deny,GetQrLoginStatus}QrLoginCommand.cs`, ilgili 6 test
dosyası. **Etkilenen doküman/wiki dosyaları:** `REFERENCE/SECURITY.md`,
`docs/wiki/Backend/WordLearner_Tests.md`, `docs/wiki/Architecture/Backend_Katmanli_Mimari.md`,
`docs/wiki/Database/Auth_Domain.md`, `docs/wiki/Standartlar/Guvenlik_Politikalari.md`, bu dosya
(`Index.md`).*
