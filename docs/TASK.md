# YAPILACAKLAR — İNDEX

> **Çalışma yöntemi (dikey dilim, MediatR CQRS, roadmap döngüsü, git/durum adımları) → `CLAUDE.md` §3–7.**
> Burada yalnızca kullanım + faz haritası + ilerleme durumu var. Task listeleri `TASK/<faz>.md`'de.

## Nasıl Kullanılır

- Task'lar **faz sırasıyla** yapılır; bir faz bitmeden sonrakine geçilmez.
- Claude'a: **"X-YY task'ını yapalım."** — task'ın hangi faz dosyasında olduğunu aşağıdaki tablodan bul.
- Bir task'a başlarken: önce `CLAUDE.md`, sonra ilgili `TASK/<faz>.md`.

## Faz Haritası

| Faz | Ne | Neden | Dosya |
|-----|----|----|-------|
| **A** | Admin panel backend (altyapı+auth+log+içerik+admin) | Endpoint'ler önce | `TASK/A_admin_panel_backend.md` |
| **B** | Admin panel (`/admin`) | Kelime/kategori girilir, API gerçek veriyle test edilir | `TASK/B_admin_panel.md` |
| **C** | Kullanıcı backend | Kart, SRS, öğrenme, sosyal | `TASK/C_kullanici_backend.md` |
| **D** | Web app (`/web`) | Tarayıcıda hızlı test, mobile referans | `TASK/D_web_app.md` |
| **E** | Mobil (`/mobile`) | API+içerik+web referansı hazır | `TASK/E_mobil.md` |
| **F** | Test & deployment | Son kontroller, entegrasyon/regresyon | `TASK/F_test_yayin.md` |

> **Not:** Faz F yeniden test yazma fazı değildir — var olan testler her API'da yazılır (`CLAUDE.md §1`), F yalnızca topluca çalıştırıp eksik entegrasyon/regresyon kapsamını tamamlar.

## İlerleme Durumu

| Faz | Task Aralığı | Başlık | Durum |
|-----|--------------|--------|-------|
| A | A-01…A-10 | Admin Panel Backend | ✅ |
| B | B-01…B-09 | Admin Panel | 🔄 |
| C | C-01…C-10 | Kullanıcı Backend | ⬜ |
| D | D-01…D-12 | Web App | ⬜ |
| E | E-01…E-14 | Mobil | ⬜ |
| F | F-01…F-04 | Test & Yayın | ⬜ |

**Sıradaki task:** `B-03` ⬜ (Kelime Yönetimi) → `TASK/B_admin_panel.md` (B-02 tamamlandı)
(`B-02 — Auth Sayfaları` ✅ tamamlandı 2026-07-27: `auth.types.ts` (backend'in gerçek DTO'larını
birebir yansıtan LoginRequest/LoginResponse/VerifyOtpRequest/AdminUser/VerifyOtpResponse),
`lib/apiError.ts` (backend'in `{error:{code,message}}` hata gövdesini okuyan, planlanmamış ama
gerekli çıkan paylaşılan yardımcı), `authApi.ts` (B-01'in temel `api`'sine `injectEndpoints` ile
eklenen İLK gerçek endpoint'ler — `login`/`verifyOtp`, admin panelin backend'e attığı İLK gerçek
HTTP istekleri), `authSlice`'a `user: AdminUser | null` eklendi (bozuk `localStorage` verisine karşı
`try/catch` ile kendi kendini onarıyor), `LoginPage`/`OtpVerifyPage` (react-hook-form'un İLK gerçek
kullanımı, iki adımlı giriş — şifre doğrulama → OTP → JWT), `App.tsx`'teki yer tutucular gerçek
sayfalarla değişti + yeni `/verify-otp` route'u. Admin panelde **İLK KEZ** test altyapısı kuruldu
(Vitest+React Testing Library+jsdom — Node 22+'nin kendi deneysel `localStorage`'ının jsdom'unkiyle
çakışması `NODE_OPTIONS=--no-experimental-webstorage` ile çözüldü), `authSlice.test.ts` (3) +
`LoginPage.test.tsx` (mutlu yol + hatalı şifre, 2) yeşil. **Bağımsız bir kod denetiminde** 2 gerçek
düzeltme: login sonrası yönlendirme yalnızca `pathname` taşıyordu (`?page=3` gibi sorgu dizeleri
kayboluyordu) — `search`/`hash` de taşınacak şekilde düzeltildi; `readStoredUser()`'ın `JSON.parse`'ı
`try/catch`'siz modül yükleme anında (React mount OLMADAN ÖNCE) tüm admin panelini çökertebilirdi —
düzeltildi. **İki bilinçli kapsam kararı** (ertelendi, unutulmadı — detay `B_admin_panel.md` B-02
notunda): (1) `refreshToken` backend'den dönüyor ama `authSlice` SAKLAMIYOR, silent refresh henüz
YOK; (2) `AdminUser`'da `role` yok, client tarafında admin rolü KONTROLÜ yapılmıyor (backend zaten
`[Authorize(Roles="Admin")]` ile koruyor, güvenlik açığı değil). Gerçek backend'e karşı Chrome'da
uçtan uca doğrulandı — bu sırada DB'nin A-09/A-03.4 migration'larının UYGULANMAMIŞ olduğu ortaya
çıktı (`dotnet ef database update` ile düzeltildi, ortam kurulumuna özel bir not). Admin Akademi'ye
işlendi (`AKADEMI/admin/B-02_auth-sayfalari/`, 7 bölüm).)
(`B-01 — Kurulum` ✅ tamamlandı 2026-07-26: `/admin` React 19 + Vite 8 + TS + Tailwind v4 iskeleti,
tasarım sistemi (`DESIGN_SYSTEM.md`'nin `@theme`'e işlenmesi — Primary rengi gerçek tarayıcıda
görülünce `#6D5DFC`'den `#4E93BC`'ye [Turkuaz] düzeltildi, karar dokümana kalıcı not edildi),
`store.ts` + `authSlice` (accessToken/isAuthenticated, localStorage) + RTK Query `api.ts`
(Authorization + Accept-Language header'ları, henüz endpoint yok), **dil tercihi altyapısı**
(`languageSlice` tr/de + `react-i18next` — admin panelin İKİ bağımsız dil kanalı: backend
mesajları Accept-Language'la, admin'in kendi statik metinleri react-i18next'le), `ProtectedRoute`
(JWT yoksa `/login`, `state`'te nereden geldiğini taşır) + `AppLayout` (Sidebar/Topbar) + routing —
hepsi tarayıcıda uçtan uca (JWT yok→login, JWT var→layout→dil değiştir→çıkış) doğrulandı, 2
subagent kod denetimi (redirect state eksikliği, `aria-pressed`/`<html lang>` senkronu eklendi),
Admin Akademi'ye işlendi (`AKADEMI/admin/B-01_kurulum/`, 6 bölüm). **Backend'e sıçrayan bir
retrofit:** admin panelin kendi dil tercihinin DB'ye bağlı olması gerektiği (localStorage yetmez)
netleşince **A-03.4 — Admin Dil Tercihi (`LanguagePreference`)** doğdu, `ThemePreference` (A-03.3)
ile birebir aynı desen, yazma ucu C-01'e bırakıldı, **297/297 backend testi yeşil** (296'dan +1),
Backend Akademi'ye de işlendi. Dark mode (`ThemePreference`) admin panelde bilinçli olarak
ERTELENDİ — henüz B-0X numarası yok, B_admin_panel.md B-01 notunda ve C_kullanici_backend.md
C-01 notunda çapraz işaretlendi, unutulmaması için.)
(`A-10 — E-posta Servisi + Hesap Temizleme Görevi` ✅ tamamlandı 2026-07-25: `EmailTemplates.cs`
(6 şablon × tr/de, `ErrorMessages`/`SuccessMessages`'ın kardeşi, ortak `Layout` + inline stil —
`string.Format` ve e-posta istemcisi uyumluluğu aynı çözümü gerektiriyor), `IEmailService`'in 6
metoduna **zorunlu** `string? language` (opsiyonel DEĞİL: derleyici 7 çağrı noktasını zorlasın diye)
+ yeni `SendAccountRecoveredNotificationAsync` (`LoginCompletionService`'in grace period kurtarma
akışına bağlandı), `RegisterCommand`/`VerifyLoginOtpCommand`/`LoginWithGoogleCommand`/
`LoginWithAppleCommand`/`GetQrLoginStatusCommand`'a `Language` alanı (`QrLoginController`
`RequestLanguageResolver`'ı kullanan İKİNCİ controller oldu), `SmtpEmailService` (A-09'un DB'deki
şifreli ayarlarını HER gönderimde okur — admin panelden değiştirilebilsin diye önbellek YOK),
`MailKitSender` (gönderim adımları `MailKitSmtpTestService` ile paylaşıldı, try/catch bilinçli
olarak paylaşılmadı: mekanizma ortak, politika ayrı), `EmailSendFailedException` (503),
`IAccountCleanupService`/`AccountCleanupService` + `AccountCleanupBackgroundService` (projedeki
İLK `IHostedService`, günde 1 03:00 UTC), `IUserRepository.GetPendingAnonymizationAsync`.
**A-10'un merkezî tasarım kararı — kritik/bilgilendirme ayrımı:** OTP e-postaları gönderilemezse
hata fırlatılır (kod eline geçmeyen kullanıcı akışı tamamlayamaz), bildirim e-postaları
gönderilemezse hata yutulup loglanır (şifre zaten değişti, hesap zaten kurtarıldı — e-posta hatası
onları geri almaz). **İki kapsam genişlemesi:** (1) şablonlar çok dilli yazıldı (kullanıcı kararı;
CLAUDE.md §1 "istemciye giden mesaj" kuralı e-postaları da kapsıyor), bu da yukarıdaki imza
değişikliklerini gerektirdi; (2) anonimleştirme SECURITY.md §9'un ilk listesinden GENİŞ —
`DisplayName`/`AvatarUrl`/`LastLoginIP`/`OneSignalPlayerId`/bekleyen OTP alanları + `IsActive=false`
de temizlenir (avatar bir fotoğraf, IP ve cihaz kimliği de kişisel veri; `/uploads` A-08'de public
yapılmıştı), SECURITY.md §9 bu kapsamla güncellendi. `OriginalEmailHash` GERÇEK adresten, `Email`
üzerine yazılmadan ÖNCE üretilir — bu satır sırası tekrar kayıt engelinin tamamıdır, ters olsaydı
kod derlenir/testler yeşil kalır ama engel sessizce çalışmazdı. **296/296 birim testi yeşil**
(265'ten +31), Backend Akademi'ye işlendi (4 bölüm), kök karta eklendi. **Faz A tamamlandı** —
A-07.1 (`UserCard` moderasyonu) bilinçli olarak C-02'yi bekliyor, kendi notundaki karar gereği
Faz A'nın "bitti" sayılmasını engellemez.)
(`A-09 — SMTP Ayarları API` ✅ tamamlandı 2026-07-24: `SmtpSettings` (BaseEntity, tekil/singleton
satır — DATABASE_SCHEMA.md'nin "ad-hoc `UpdatedByUserId` → BaseEntity" birleştirme notu burada
uygulandı, ayrı bir `UpdatedBy` alanı EKLENMEDİ), `IEncryptionService`/`AesEncryptionService`
(AES-256-CBC, rastgele IV, Base64(IV+cipher), anahtar `AES_ENCRYPTION_KEY`'in tam 32 bayta
çözüldüğü constructor'da doğrulanır), `ISmtpSettingsRepository` (tek satır, `GetCurrentAsync`),
`Features/Smtp/` (`GetSmtpSettingsQuery` — şifre `***` maskeli, `UpdateSmtpSettingsCommand` —
upsert + "***" gönderilirse eski şifreyi koruma sözleşmesi + `IActivityLogger`/`ISecurityLogger`
çift loglama [UpdateUserRoleCommand ile aynı desen], `TestSmtpSettingsCommand` — kayıtlı
ayarlarla admin'in kendi e-postasına test gönderir), `ISmtpTestService`/`MailKitSmtpTestService`
(projeye MailKit eklendi, A-10'dan önce), `SmtpSettingsController` (`api/v1/admin/smtp-settings`,
WordsController/CategoriesController/MediaController ile aynı "ayrı domain controller'ı" deseni,
AdminController'a EKLENMEDİ), Backend Akademi'ye işlendi (4 bölüm), kök karta eklendi. **Kod
denetimi (2 subagent — kod + Backend Akademi), 5 gerçek düzeltme:** (1) ENV.md/launchSettings.json'daki
örnek `AES_ENCRYPTION_KEY` 29 bayta çözülüyordu (32 DEĞİL) — GERÇEKTEN 32 baytlık yeni bir anahtarla
düzeltildi; (2) hiç ayar kaydedilmemişken maske literal'i ("***") gerçek şifre olarak sessizce
şifrelenebiliyordu — yeni `SmtpPasswordRequiredException` ile kapatıldı; (3) `GetCurrentAsync`'e
`OrderBy(Id)` eklendi (eşzamanlı PUT'ların DB'de birden fazla satır oluşturabileceği kabul edilmiş
riskte deterministik okuma için); (4) MailKit'in bilinen bir CVE'si olan 4.3.0 sürümü 4.17.0'a
yükseltildi; (5) Backend Akademi'de "Tam Dosya" etiketli 5 slayt gerçek dosyalardan eksik satırlar
içeriyordu, programatik diff ile birebir eşitlendi. **265/265 birim testi yeşil.**)
(`A-08 — Medya / Dosya Yükleme API` ✅ tamamlandı 2026-07-24: `IFileStorageService`/
`LocalFileStorageService` (uzantı jpg/jpeg/png/webp + 5 MB boyut + İÇERİK [magic bytes]
doğrulaması, `Guid` tabanlı benzersiz ad üretimi), `MediaController` (`POST /media/images/upload`,
projedeki İLK `multipart/form-data`/`IFormFile` uç noktası — HealthController ile aynı desende
MediatR DIŞINDA), `app.UseStaticFiles()` (`/uploads` herkese açık), `IActivityLogger`
(`UPLOAD_MEDIA`, `EntityType=Word`/`EntityId=NULL`) — **252/252 yeşil**, Backend Akademi'ye işlendi
(3 bölüm), kök karta eklendi. **Kapsam düzeltmesi:** `Word.ImageUrl` için yeni migration GEREKMEDİ
— bu alan (`WordConcept.ImageUrl`) A-05'te zaten yazılmıştı, TASK maddesinin ilk hâli yanlış
okunabilirdi. **Kod denetimi (2 subagent — kod + Backend Akademi), 2 gerçek düzeltme:** yalnızca
uzantı kontrolü yeterli değildi (magic-byte doğrulaması eklendi — bir `.exe`, adı `.png` yapılarak
yüklenebiliyordu), eksik dosya ASP.NET Core'un ham hata şekliyle dönüyordu (`IFormFile?` +
`FileRequiredException` ile projenin standart `ApiErrorResponse` sözleşmesine alındı).)
(`A-07 — Admin API` ✅ tamamlandı 2026-07-24: dört dilim — Kullanıcı Yönetimi (`IUserRepository`
genişletmesi + 4 Command/Query + projedeki ilk çift-loglama [`IActivityLogger`+`ISecurityLogger`] +
self-lockout koruması), İstatistik (`GetAdminStatisticsQuery` — toplam/aktif/dondurulmuş kullanıcı,
toplam kelime/kategori, kayıt grafiği; `LoginsByDay` bilinçli olarak yazılmadı), Toplu Kelime Import
(`BulkImportWordsCommand` — her satır bağımsız tek dilli `WordConcept`, best-effort, TEK
`BULK_IMPORT_WORDS` ActivityLog kaydı), Log Görüntüleme (`LogMessages.cs` ile A-04'ten beri bekleyen
`SecurityLog.Detail` çözme borcu kapandı) — `AdminController`'ın 9 endpoint'i, **244/244 yeşil**,
kod denetiminde 2 gerçek düzeltme (tüketicisiz DTO geri alındı, self-lockout koruması eklendi),
Backend Akademi'ye işlendi (7 bölüm), kök karta eklendi. `UserCard` moderasyonu **A-07.1**'e
ertelendi (C-02 bekliyor, bkz. `TASK/A_admin_panel_backend.md` A-07.1)) → `TASK/A_admin_panel_backend.md`

⬜ Başlanmadı · 🔄 Devam ediyor · ✅ Tamamlandı · ⛔ Engellendi
