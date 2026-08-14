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
| **A** | Backend (`.NET 9 Web API`, tek/ortak — admin/web/mobil ayrımı yok) | Endpoint'ler önce | `TASK/A_backend.md` |
| **B** | Admin panel (`/admin`) | Kelime/kategori girilir, API gerçek veriyle test edilir | `TASK/TASK_B_admin_panel.md` |
| **C** | Web app (`/web`) | Tarayıcıda hızlı test, mobile referans | `TASK/TASK_C_web_app.md` |
| **D** | Mobil (`/mobile`) | API+içerik+web referansı hazır | `TASK/D_mobil.md` |
| **E** | Test & deployment | Son kontroller, entegrasyon/regresyon | `TASK/E_test_yayin.md` |

> **Not:** Faz E yeniden test yazma fazı değildir — var olan testler her API'da yazılır (`CLAUDE.md §1`), E yalnızca topluca çalıştırıp eksik entegrasyon/regresyon kapsamını tamamlar.
> **Not (2026-08-08 — roadmap değişikliği):** Önceki yapıda backend ikiye bölünmüştü — "Admin
> Panel Backend" (eski Faz A) ve "Kullanıcı Backend" (eski Faz C, Web+Mobil ortak API). Bu ayrım
> kaldırıldı: backend artık kim çağırırsa çağırsın TEK bir Faz A altında, tek seferde, baştan
> tasarlanıp yazılıyor. Bu yüzden eski Web (D) ve Mobil (E) fazları bir harf geriye kaydı (D→C,
> E→D), Test fazı da F→E oldu. `docs/DATABASE_SCHEMA/` ve `docs/REFERENCE/` (API_ENDPOINTS,
> ARCHITECTURE, SECURITY, TECHNICAL_SPECIFICATIONS, ENV, GERMAN/TURKISH_LANGUAGE_FEATURES)
> içindeki tasarım kararları KORUNDU — yeni backend bunları blueprint olarak kullanır.

## İlerleme Durumu

| Faz | Task Aralığı | Başlık | Durum |
|-----|--------------|--------|-------|
| A | A-01…A-20 | Backend | 🔄 |
| B | B-01…B-09 (+B-02.1) | Admin Panel | 🔄 |
| C | C-01…C-12 | Web App | ⬜ |
| D | D-01…D-14 | Mobil | ⬜ |
| E | E-01…E-04 | Test & Yayın | ⬜ |

**Sıradaki task:** `A-04` ⬜ (Loglama Sistemi) → `TASK/A_backend.md`.
Faz A tamamlanınca kaldığı yerden **B-02** ⬜ (Auth Sayfaları) → `TASK/TASK_B_admin_panel.md` devam eder.

(`A-03 — Auth API` ✅ tamamlandı 2026-08-14: `User`/`RefreshToken`/`QrLoginSession` entity'leri,
`IPasswordService`/`ITokenService`/`IOtpService`/`ILoginCompletionService`/`IEmailService` +
Google/Apple doğrulayıcıları, Auth'a özel repository katmanı, 9 `AppException` alt sınıfı, 13 Auth +
5 QR Login Command+Handler, `SuccessMessages`+`ValidationBehavior`+`ApiControllerBase`+15 validator+
5 rate limiting policy'si, `AuthController`[13]+`QrLoginController`[5] = 18 endpoint [canlı test
edildi, 1 gerçek bug bulunup düzeltildi — bkz. A-03 notu], 18 Handler'ın TAMAMI için birim testi
[95/95 yeşil] — `AKADEMI/backend/A-03_auth-api/`e işlendi (43 bölüm).)

(`A-03.2 — İlk Admin Hesabı` ✅ tamamlandı 2026-08-14: `IAdminSeedService`/`AdminSeedService` —
`Program.cs` başlangıcında idempotent seed, `INITIAL_ADMIN_EMAIL`/`INITIAL_ADMIN_PASSWORD`
[`ENV.md` §9], 4/4 birim testi yeşil [99/99 tüm paket], canlı doğrulama yapıldı —
`AKADEMI/backend/A-03.2_ilk-admin-hesabi/`e işlendi (1 bölüm). Sıradaki: A-04 [Loglama Sistemi].)

(`A-01 — Proje İskeleti` ✅ tamamlandı 2026-08-09: 5 proje [Domain/Application/Infrastructure/API/
Tests], CLAUDE.md §5'teki tek yönlü bağımlılık zinciri, TECHNICAL_SPECIFICATIONS.md §1'deki NuGet
paketleri, appsettings.json/appsettings.Development.json ayrımı [ENV.md], başlangıç `Program.cs` —
`AKADEMI/backend/A-01_proje-iskeleti/`e işlendi, bkz. `02_yapilandirma-ozet-sozluk.html`.)

(`A-02 — Ortak Altyapı` ✅ tamamlandı 2026-08-10: BaseEntity, EntityNotFoundException,
WordLearnerDbContext [reflection tabanlı otomatik soft-delete filtresi], IRepository<T>/Repository<T>/
AddInfrastructureServices, ApiErrorResponse+ErrorMessages [Code+dil sözlüğü], 3 middleware
[ExceptionHandling/SecurityHeaders/RequestResponseLogging], Program.cs [Serilog/JWT/CORS/
FluentValidation/MediatR — gerçekten çalıştırılıp Swagger/güvenlik başlıkları/loglama canlı
doğrulandı], RepositoryTests+EntityNotFoundExceptionTests [4/4 yeşil] — `AKADEMI/backend/
A-02_ortak-altyapi/`e işlendi (7 bölüm). Bu görevde 2 düzeltme yapıldı: A-01'de eksik bırakılan
Infrastructure→Application proje referansı, ve bir hata-mesajı dil çelişkisi (EntityNotFoundException'ın
istemciye giden metni de Code+ErrorMessages sözlüğü üzerinden çözülecek şekilde netleştirildi) —
detay `TASK/A_backend.md` A-02 notunda.)

(**2026-08-08 — Backend roadmap'i baştan tasarlandı:** `TASK/A_backend.md` A-01…A-20, eski A
[admin backend] + C [kullanıcı backend] görevlerinin TEK sıraya birleşmiş hâli — git geçmişindeki
eski task içerikleri (kod detayları, SM-2/leech/achievement tasarım kararları, eşleştirme
algoritması, AES şifreleme, vb.) korunarak yeniden yazıldı. **İki bilinçli sıralama düzeltmesi:**
(1) Kullanıcı Profil API (**A-12**) artık SRS/İlerleme API'sinden (**A-09**) SONRA — eski C-01
`/users/me/statistics`'i `UserProgress` yokken planlamıştı, ilk günden gerçek veri dönmüyordu;
(2) Admin API'nin içerik moderasyonu (**A-18**) artık Kişisel Kart API'sinden (**A-10**) SONRA —
eski turda `UserCard` entity'si yokken planlanıp A-07.1'e ertelenmişti, artık ilk seferde tam
yazılıyor, ayrı retrofit task'ı yok. QR ile giriş/tema tercihi/dil tercihi/mesaj lokalizasyonu da
(eskiden 4 ayrı "retrofit" task'ı) **A-03**'e baştan dahil edildi.)

(**2026-08-08 — Backend baştan yazım (kullanıcı kararı):** Önceki backend kodu (`backend/`, 92MB,
A-01…A-10 + A-03.1…A-05.2 retrofit'leri, 296/296 yeşil test) ve onu öğreten `AKADEMI/backend/`
(A-02…A-10, 15 görev klasörü) **tamamen silindi** — git geçmişinde hâlâ duruyor, kayıp değil,
yalnızca çalışma ağacından kaldırıldı. Gerekçe: backend'i "admin panel backend" / "kullanıcı
backend" diye ikiye bölmek yerine TEK, ortak bir backend olarak baştan tasarlamak. Admin panel
frontend'i (`admin/`, `AKADEMI/admin/`, Faz B) bu sıfırlamadan **etkilenmedi** — B-01 (Kurulum) ve
B-01'in 2026-08-06 palet güncellemesi olduğu gibi duruyor. `TASK_B_admin_panel.md`'deki B-02+
maddelerinin "Referans: A-0X" işaretleri artık eski/geçersiz task numaraları — dosya adı
`A_backend.md`'ye güncellendi ama numaralar yeni backend tasarlanınca yeniden verilecek, o ana
kadar API_ENDPOINTS.md bölüm numaralarına (§3, §5, §6, §11 gibi) güvenilir.)

(`B-01 — Kurulum` ✅ tamamlandı 2026-08-05: Vite+React+TS iskeleti + CLAUDE.md §4.1 ortak
kütüphaneleri (oxlint yerine ESLint), tasarım sistemi (index.css `@theme`+`.dark`), Redux store +
authSlice + axios `apiClient`, dil tercihi (`languageSlice`+react-i18next+`LanguageSwitcher`),
`.env`+React Router+`ProtectedRoute`+`AppLayout` (Sidebar/Topbar, mobilde açılır-kapanır panel),
Dark Mode (`themeSlice`+`useThemeSync`+`ThemeSwitcher`+FOUC-önleyici script) — tümü
`AKADEMI/admin/B-01_kurulum/`e işlendi, bkz. `07_ozet-sozluk.html`. 2026-08-06'da palet Apple+
Duolingo temasına (`#FF6B00` turuncu accent, Plus Jakarta Sans) güncellendi, bkz. `TASK_B_admin_panel.md`
B-01 sonu.)

⬜ Başlanmadı · 🔄 Devam ediyor · ✅ Tamamlandı · ⛔ Engellendi
