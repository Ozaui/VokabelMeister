
# FAZ B — Admin Panel (`/admin`)

> **Yöntem/standart:** Bu dosyadaki her feature, `../../CLAUDE.md` §4/§6 kurallarına göre yazılır
> (tip→api→slice→hook→component→route→test, her parça yazılır yazılmaz `AKADEMI/admin/`'e
> işlenir — backend'deki `AKADEMI/backend/` ile aynı disiplin, `postman` yerine `onizleme` slaytı).
> O bölümler değişmez standarttır — burada tekrar edilmez, her zaman `../../CLAUDE.md`'ye bakılır.

### B-01 — Kurulum ✅
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §3, REFERENCE/DEVELOPMENT_SETUP.md §6, REFERENCE/DESIGN_SYSTEM.md
- [x] React + Vite + TS, TailwindCSS, Axios + Formik/Yup kurulumu (RTK Query/React Hook Form ile başlanmıştı, B-03 sonrası axios + `useApiQuery`/`useApiMutation` + Formik/Yup'a geriye dönük geçirildi)
- [x] Tasarım sistemi uygulaması — `DESIGN_SYSTEM.md`'deki Turkuaz+Mercan paleti/Nunito+DM Sans/16px-12px radius Tailwind `@theme`'e işlendi (Primary rengi B-01 sırasında `#6D5DFC`'den `#4E93BC`'ye düzeltildi, dokümana not edildi)
- [x] `store.ts` (Redux store) + `authSlice` (yalnızca `accessToken`/`isAuthenticated` — `ProtectedRoute`'un ihtiyaç duyduğu asgari alan; `user` nesnesi B-02'de eklenir) + `store/api.ts` (axios `apiClient`, `Authorization`/`Accept-Language` interceptor'ı)
- [x] **Dil tercihi (i18n)** — `languageSlice` (tr/de, localStorage persist, varsayılan tr — `ErrorMessages`/`SuccessMessages` ile aynı "desteklenmiyorsa tr'ye düş" kuralı), `react-i18next` ile frontend statik metinleri (buton/etiket) tr/de, `api.ts`'e `Accept-Language` header'ı (backend'den gelen mesajlar da seçili dile göre gelsin), Topbar'da dil değiştirici (`aria-pressed` + `<html lang>` senkronu dahil). **Backend'de bu ihtiyaç A-03.4 (`Users.LanguagePreference`) retrofit'ini doğurdu** — yazma ucu C-01'e bırakıldı, bkz. `C_kullanici_backend.md` C-01 notu.
- [x] `.env*` (`VITE_API_URL`), `ProtectedRoute` (JWT yoksa `/login`'e yönlendir, `state`'te nereden geldiğini taşır), temel layout (Sidebar/Topbar)
- [x] **Dark Mode** — `DESIGN_SYSTEM.md`'ye koyu tema paleti eklendi (Primary/Accent/Background/
      Surface/Text/Muted/Border/Success/Warning/Destructive'ın dark varyantları — mevcut
      Turkuaz+Mercan light paleti DEĞİŞMEDİ, yanına eklendi), `index.css`'te `.dark` seçicisiyle
      token override (Tailwind v4 `@custom-variant dark`), `themeSlice` (Light/Dark/System —
      `languageSlice` ile AYNI desen: localStorage persist, varsayılan `System`, DB'ye YAZMIYOR
      henüz), `useThemeSync` hook (canlı değişiklik + System modunda OS dinleyicisi), `ThemeSwitcher`
      (Topbar, `LanguageSwitcher`'ın yanına — `lucide-react` Sun/Moon/Monitor ikonlarıyla, admin
      panelin İLK ikon kütüphanesi kararı, `DESIGN_SYSTEM.md`'ye işlendi), `index.html`'e
      FOUC-önleyici senkron script (sayfa ilk boyanmadan `.dark` class'ı localStorage/OS tercihine
      göre erkenden uygulanır)
- [x] ➜ **Admin Akademi'ye işle** (`AKADEMI/admin/B-01_kurulum/06_dark-mode.html`)
**296+ backend testi hâlâ yeşil (A-03.4 sonrası 297/297), admin projesi `npm run build` ile doğrulandı, `localStorage.setItem('accessToken', ...)` ile ProtectedRoute→AppLayout→dil değiştirme→çıkış akışı VE Light/Dark/System tema geçişi tarayıcıda uçtan uca test edildi (kullanıcı bizzat test etti).**
*(Kurulum task'ı — dikey dilim/roadmap kuralı A-01 gibi burada uygulanmaz (tek "feature" değil,
paylaşılan altyapı), AMA A-02 (Ortak Altyapı) emsaliyle aynı şekilde akademiye işlenir — kullanıcı
kararı: kurulum adımları da atlanmadan `AKADEMI/admin/B-01_kurulum/`'a yazılır.)*

> **Dark Mode — kaldığı yerden (2026-07-26):** İlk kararda dark
> mode AskUserQuestion ile **ertelenmişti** ("henüz B-0X numarası yok" notuyla) — kullanıcı hemen
> ardından fikrini değiştirdi ve B-01 içinde (henüz commit edilmediği için ayrı bir B-0X.Y retrofit
> AÇILMADI, aynı görevin devamı sayıldı) eklenmesini istedi. Yaklaşım `LanguagePreference` ile
> BİREBİR aynı: `Users.ThemePreference` (A-03.3) DB'de zaten var, yazma ucu (`PUT /users/me`) HÂLÂ
> C-01'de — bu yüzden `themeSlice` de `languageSlice` gibi ŞİMDİLİK yalnızca `localStorage`'a
> yazıyor, backend'de YENİ bir değişiklik GEREKMEDİ (kullanıcının kendi sorusunun cevabı: hayır,
> gerekmiyor, çünkü DB alanı zaten oradaydı). C-01 tamamlandığında `themeSlice` de `languageSlice`
> ile AYNI anda gerçek API'ye bağlanmalı (bkz. `C_kullanici_backend.md` C-01 notu, güncellenecek).

### B-02 — Auth Sayfaları ✅
**Referans:** A-03 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3
> Yalnızca e-posta + şifre + OTP (2FA); Google/Apple **yok** (Admin panelde asla).
- [x] **Tip:** `LoginRequest`, `VerifyOtpRequest`, `AdminUser` (`auth.types.ts`)
- [x] ➜ **Admin Akademi'ye işle**
- [x] **API:** `authApi` — `login`, `verifyOtp` (axios + `useApiMutation`, `store/api/authApi.ts`)
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Slice:** `authSlice` — `user`, `accessToken`, `isAuthenticated` (`store/slices/authSlice.ts`)
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Component:** `LoginPage` (e-posta+şifre formu), `OtpVerifyPage` (6 haneli kod)
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Route:** `/login`, `/verify-otp` (`App.tsx`), başarılı girişte `/` yönlendirme
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Birim testleri:** `LoginPage.test.tsx` (mutlu yol + hatalı şifre), `authSlice.test.ts`
- [x] ➜ **Admin Akademi'ye işle**

**Tamamlandı 2026-07-27:** `lib/apiError.ts` (backend'in `{error:{code,message}}` gövdesini okuyan
paylaşılan yardımcı, planlanmamış ama `getApiErrorMessage` her iki sayfada da gerekli çıktı) da
yazıldı. Admin panelde **ilk kez** test altyapısı kuruldu (Vitest + React Testing Library +
jsdom — `vite.config.ts`'e `test` bloğu, `src/test/setup.ts`); Node.js 22+'nin kendi deneysel
`localStorage`'ının jsdom'unkiyle çakışması `"test": "NODE_OPTIONS=--no-experimental-webstorage
vitest run"` ile çözüldü. **Bağımsız bir kod denetiminde** 2 gerçek düzeltme yapıldı: (1) login
sonrası yönlendirme yalnızca `pathname` taşıyordu, `/words?page=3` gibi bir sorgu dizesinden
atılan bir admin girişten sonra filtrelerini kaybediyordu — `from` artık `search`/`hash`'i de
taşıyor; (2) `authSlice`'ın `readStoredUser()`'ı `try/catch` olmadan `JSON.parse` yapıyordu, bozuk
bir `authUser` değeri (DevTools'tan elle değiştirme vb.) modül yüklenirken (React mount olmadan
ÖNCE) tüm admin panelini beyaz ekranda çökertebilirdi — artık kendi kendini onarıyor (bozuk değeri
silip `null` dönüyor). **İki bilinçli kapsam kararı** (denetimde bulunup ERTELENDİ, unutularak
değil): (1) backend `VerifyOtpResponse`'ta bir `refreshToken` döndürüyor ama `authSlice` bunu
SAKLAMIYOR — 15 dakikalık access token süresi dolunca otomatik yenileme (silent refresh) henüz
yok, ileride ayrı bir görevde ele alınacak; (2) `AdminUser` tipinde `role` alanı yok (backend
gövdede döndürmüyor, yalnızca JWT'nin içinde) — LoginPage/OtpVerifyPage giriş yapanın gerçekten
Admin olup olmadığını client tarafında KONTROL ETMİYOR, sıradan bir `User` de bu iki sayfayı
geçebilir (güvenlik açığı değil — B-03'ten itibaren yazılacak gerçek admin endpoint'lerinin hepsi
backend'de `[Authorize(Roles="Admin")]` ile zaten korunuyor — ama erken bir "bu hesapla admin
paneline giremezsin" uyarısı YOK). Gerçek backend'e (dotnet run) karşı Chrome'da uçtan uca
doğrulandı (yanlış şifre → 401 + doğru hata mesajı, doğru şifre+OTP → gerçek JWT). Bu test
sırasında DB'nin A-09/A-03.4 migration'larının henüz UYGULANMAMIŞ olduğu ortaya çıktı (`dotnet ef
database update` ile düzeltildi — bu B-02'ye özel değil, herhangi bir geliştirici ortamının ilk
kurulumunda atlanabilecek standart bir adım, not olarak düşülüyor). `AKADEMI/admin/
B-02_auth-sayfalari/` (7 bölüm), kök `AKADEMI/admin/index.html`'e kart eklendi, B-01'in kapanışı
buraya zincirlendi.

### B-03 — Kelime Yönetimi ✅
**Referans:** A-05 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §5
- [x] **Tip:** `Word`, `WordDetail`, `WordFormValues` (`word.types.ts`)
- [x] ➜ **Admin Akademi'ye işle**
- [x] **API:** `wordsApi` — `getWords` (filtre/sayfa), `createWord`, `updateWord`, `deleteWord`,
      `getUnmatchedWordConcepts` (`languageId` bazlı, `suggestedMatchConceptId` dahil), `pairWordConcepts`
      (axios + `useApiQuery`/`useApiMutation`)
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Slice:** `wordFilterSlice` — liste filtre/sayfa state (arama, level, partOfSpeech)
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Component:** `WordListPage` (filtre+tablo+sayfalama), `WordFormModal` (Formik+Yup — WordDetail + örnek cümle + kategori seçimi, ekle/düzenle ortak;
      önce dil (`de`/`tr`) sonra `Tür` seçilir, gramer bölümü ikisine göre koşullu render edilir —
      `de` + Noun/Verb/Diğer → `GERMAN_LANGUAGE_FEATURES.md §10`; `tr` + Noun/Verb/Diğer →
      `TURKISH_LANGUAGE_FEATURES.md §9`; backend `WordGrammarValidator`'ın TS karşılığı, aynı mantık
      iki ayrı katmanda tekrar yazılır — kod paylaşımı yok), `WordPairingPage` (iki sütun — solda `de`
      eşleşmemiş liste, sağda `tr` eşleşmemiş liste, her satırda varsa **önerilen eşleşme**
      [`suggestedMatchConceptId`] öne çıkarılmış — admin onaylar veya manuel seçip "Eşleştir" der;
      onay öncesi açık bir **"birincil tarafı değiştir"** kontrolü [varsayılan: işlemi başlattığın
      taraf `primaryId` olur, istersen karşı tarafa çevirebilirsin — Tür/Seviye/Kategori bilgisi
      birincil olandan alınır]; `PartOfSpeech`/kategori farkı yalnızca **bilgilendirme** amaçlı
      gösterilir, onay/force gerektirmez — diller arası tür kayması normal, bkz. `Icerik.md`
      "Eşleştirme")
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Route:** `/words`, `/words/pairing` (`App.tsx`), sidebar linki
- [x] ➜ **Admin Akademi'ye işle**
- [x] **Birim testleri:** `WordFormModal.test.tsx` (dil+tür bazlı koşullu alan render/validasyon, submit),
      `WordListPage.test.tsx` (filtre), `WordPairingPage.test.tsx` (eşleştirme mutlu yol + önerilen eşleşme render)
- [x] ➜ **Admin Akademi'ye işle**

**Tamamlandı 2026-07-27:** Şimdiye kadarki en büyük admin feature'ı. `WordFormModal` önce dil sonra
tür seçtirir; gramer bölümü ikisine göre koşullu render edilir (`GermanGrammarFields`/
`TurkishGrammarFields`, ortak bir `ConjugationGrid` — DE 3×6, TR 5×6 hücre — paylaşılır). Zorunlu
alanlar formda kırmızı `*` ile işaretlenir (admin backend validator'ın hangi alanı reddedeceğini
tahmin etmek zorunda kalmaz). Aynı ekranın hazırlığı sırasında iki küçük backend eksiği fark edildi
ve ayrı retrofit olarak kapatıldı: (1) `languageId`'nin tek kaynağı migration seed'iydi — yeni bir
`GET /languages` endpoint'i (`languagesApi.ts`) bu boşluğu kapattı; (2) Türkçe isimlerin ünlü uyumu
ve iyelik eki alanları backend validator'da hiç zorunlu değildi (kart tasarımı dokümanıyla
tutarsızdı) — ikisi de zorunlu yapıldı, `TurkishGrammarFields`'a işlendi. `WordPairingPage` iki
sütunu (Almanca/Türkçe eşleşmemiş) TEK bir paylaşılan, saf-render sütun component'iyle gösterir —
veri çekme sorumluluğu üst bileşende toplanır (ilk taslakta her sütunun kendi verisini çekip
diğerine side-effect'le sızdırdığı bir tasarım fark edilip düzeltildi). `lib/apiError.ts`'e
`getApiErrorCode` eklendi (409 `WORD_TEXT_ALREADY_EXISTS` → "yine de ekle" akışı, dile göre değişen
mesaj yerine sabit koda göre dallanır). **i18n düzeltmesi:** ilk taslakta hâl alanlarının küçük
etiketleri (nominative/accusative/...) ham İngilizce JSON alan adı olarak sızmıştı, ve ayrıca örnek
cümle türü (`Normal`/`Idiom`/`Formal`/`Colloquial`) seçimi hiç çevrilmemişti — ikisi de düzeltildi
(`words.grammar.*.caseLabels`/`personLabels`/`tenseLabels` ve `words.examples.type` i18n anahtarları
eklendi); ayrıca gerçek Almanca/Türkçe dilbilgisi terimlerinin (Nominativ/Akkusativ vb.) kasıtlı
olarak İKİ dilde de AYNI kalması gerektiği netleşti — bunlar arayüz metni değil, öğretilen dilin
kendi terminolojisi. **24/24 frontend testi yeşil**, gerçek bir backend'e (`dotnet run`) karşı
Chrome'da uçtan uca doğrulandı: Almanca isim (tüm gramer) + Türkçe isim (vowelHarmony/possessive
dahil) oluşturuldu, Eşleştirme ekranında birleştirildi, Düzenle'de her iki dilin verisi doğru
şekilde geri yüklendiği görüldü, admin'in kendi arayüz dili TR↔DE canlı değiştirilerek tüm
etiketlerin (backend'den gelen kategori adları dahil) doğru dilde geldiği doğrulandı. `AKADEMI/admin/
B-03_kelime-yonetimi/` işlendi, kök `AKADEMI/admin/index.html`'e kart eklendi.

### B-04 — Kategori Yönetimi ⬜
**Referans:** A-06 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §6
- [ ] **Tip:** `Category`, `CategoryFormValues` (`category.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `categoriesApi` — `getCategories` (hiyerarşik), `createCategory`, `updateCategory`, `deleteCategory` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `CategoryTreePage` (hiyerarşik ağaç liste), `CategoryFormModal` (üst kategori seçimi, ikon, renk, seviye)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/categories` (`App.tsx`), sidebar linki
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `CategoryTreePage.test.tsx` (hiyerarşik render), `CategoryFormModal.test.tsx`
- [ ] ➜ **Admin Akademi'ye işle**

### B-05 — Kullanıcı Yönetimi ⬜
**Referans:** A-07 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §11
- [ ] **Tip:** `AdminUserListItem`, `UserDetail` (`user.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `adminUsersApi` — `getUsers` (arama/rol filtresi), `getUserDetail`, `changeRole`, `toggleStatus` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `UserListPage` (arama+rol filtresi+tablo), `UserDetailPage` (profil+istatistik+rol/durum aksiyonları)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/users`, `/users/:id` (`App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `UserListPage.test.tsx` (arama/filtre), `UserDetailPage.test.tsx` (rol değiştir aksiyonu)
- [ ] ➜ **Admin Akademi'ye işle**

### B-06 — Paylaşım/İçerik Moderasyonu ⬜
**Referans:** A-07 (`A_admin_panel_backend.md`)
> **Not:** "Herkese açık + admin onayı" modeli kaldırıldı — DATABASE_SCHEMA/Kisisel_Icerik.md'de `IsPublic`/`IsApproved`
> alanı yok, gerçek mekanizma `SharedContents` (link tabanlı, admin onayı gerektirmez). Bu sayfa onun
> yerine **şikayet edilen** kişisel kartları listeler/siler (`GET/DELETE /admin/user-cards`).
- [ ] **Tip:** `ReportedUserCard` (`moderation.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `moderationApi` — `getReportedUserCards`, `deleteUserCard` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `ModerationPage` (liste + inceleme detayı + sil aksiyonu)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/moderation` (`App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `ModerationPage.test.tsx` (liste + silme akışı)
- [ ] ➜ **Admin Akademi'ye işle**

### B-07 — İstatistik Paneli ⬜
**Referans:** A-07 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §11
- [ ] **Tip:** `AdminStatistics` (`statistics.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `statisticsApi` — `getAdminStatistics` (axios + `useApiQuery`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `DashboardPage` (toplam/aktif kullanıcı kartları, en çok öğrenilen/sorunlu kelimeler tablosu, günlük/haftalık grafik)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/` (ana sayfa, `App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `DashboardPage.test.tsx` (veri render, yükleniyor durumu)
- [ ] ➜ **Admin Akademi'ye işle**

### B-08 — Log Görüntüleme Paneli ⬜
**Referans:** A-04, A-07 (`A_admin_panel_backend.md`)
- [ ] **Tip:** `ActivityLogEntry`, `ApplicationLogEntry`, `SecurityLogEntry` (`log.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `logsApi` — `getActivityLogs`, `getApplicationLogs`, `getSecurityLogs` (filtre+sayfa, axios + `useApiQuery`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `LogsPage` (3 sekme: Activity/Application/Security — filtre + tarih aralığı + sayfalama tablo)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/logs` (`App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `LogsPage.test.tsx` (sekme geçişi, filtre)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] CSV dışa aktarma (opsiyonel)

### B-09 — SMTP Ayarları Sayfası ⬜
**Referans:** A-09 (`A_admin_panel_backend.md`)
- [ ] **Tip:** `SmtpSettingsFormValues` (`smtp.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `smtpApi` — `getSmtpSettings` (şifre `***`), `updateSmtpSettings`, `testSmtpConnection` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `SmtpSettingsPage` (form: Host/Port/SSL/Kullanıcı/Şifre/From, kaydet, "Test e-postası gönder")
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/settings/smtp` (`App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `SmtpSettingsPage.test.tsx` (form validasyon, test e-postası akışı)
- [ ] ➜ **Admin Akademi'ye işle**
