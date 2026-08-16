# FAZ B — Admin Panel (`/admin`)

> **Yöntem/standart:** Bu dosyadaki her feature, `../../CLAUDE.md` §4/§6 kurallarına göre yazılır
> (tip→api→slice→hook→component→route→test, her parça yazılır yazılmaz `AKADEMI/admin/`'e
> işlenir — backend'deki `AKADEMI/backend/` ile aynı disiplin, `postman` yerine `onizleme` slaytı).
> O bölümler değişmez standarttır — burada tekrar edilmez, her zaman `../../CLAUDE.md`'ye bakılır.
> **Ortak kütüphaneler (state/routing/form/i18n/HTTP/ikon/QR/grafik/tarih) → `../../CLAUDE.md` §4.1.**

> **"Component:" maddeleri özettir, atomik değildir** (`../../CLAUDE.md` §4.1'deki 2026-08-05
> notu) — gerçek yazımda her isimlendirilmiş component kendi alt-component'lerine bölünür ve
> roadmap'e her biri **ayrı `[ ]` satırı** olarak işlenir. **B-03 aşağıda bu bölünmenin örneği
> olarak önceden alt maddelere ayrılmıştır** — yeni bir sayfaya başlarken şablon olarak kullanılır.

> **2026-08-05 — Baştan yazım:** Önceki `/admin` kodu (B-01/B-02/B-03) ve onu öğreten
> `AKADEMI/admin/` tamamen silindi, bu dosyadaki ilerleme ⬜'e sıfırlandı — kullanıcı kararıyla
> admin frontend'i sıfırdan yeniden yazılacak. Aşağıdaki maddeler önceki turdan çıkan geçerli
> tasarım kararlarını (paleti, dark mode yaklaşımı, dosya deseni) korur; yalnızca "tamamlandı"
> anlatıları ve kod-özel detaylar kaldırıldı.

> **2026-08-05 — Tasarım sistemi yenilendi:** B-01'deki palet/font referansı, `REFERENCE/
> DESIGN_SYSTEM.md`'nin baştan yazımına göre güncellendi (Turkuaz+Mercan/Nunito+DM Sans →
> yeni nötr palet/Inter). Tasarım sistemi artık Admin+Web ortak — ayrıntı `DESIGN_SYSTEM.md`.

> **2026-08-16 — Yeniden baştan yazım:** `/admin` kodu (B-01) ve onu öğreten `AKADEMI/admin/`
> tekrar tamamen silindi (kullanıcı kararı), B-01 ilerlemesi ⬜'e sıfırlandı.

### B-01 — Kurulum ⬜
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §2, REFERENCE/DEVELOPMENT_SETUP.md §6, REFERENCE/DESIGN_SYSTEM.md
- [ ] React + Vite + TS, TailwindCSS, Axios + Formik/Yup + React Router DOM + Redux Toolkit kurulumu (bkz. `CLAUDE.md` §4.1 — ortak kütüphane listesi)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] Tasarım sistemi uygulaması — `DESIGN_SYSTEM.md`'deki ortak palet (Primary/accent `#5B54F0`
      light · `#8A83FF` dark), Inter fontu, §4 radius skalası (buton/input 8px, kart 16px, modal
      20px, badge 999px) ve §5 gölge skalası Tailwind `@theme`'e + `.dark` override'ına işlenir
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] `store.ts` (Redux store) + `authSlice` (yalnızca `accessToken`/`isAuthenticated` — `ProtectedRoute`'un ihtiyaç duyduğu asgari alan) + `store/api.ts` (axios `apiClient`, `Authorization`/`Accept-Language` interceptor'ı)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Dil tercihi (i18n)** — `languageSlice` (tr/de, localStorage persist, varsayılan tr — `ErrorMessages`/`SuccessMessages` ile aynı "desteklenmiyorsa tr'ye düş" kuralı), `react-i18next` ile frontend statik metinleri (buton/etiket) tr/de, `api.ts`'e `Accept-Language` header'ı, Topbar'da dil değiştirici (`aria-pressed` + `<html lang>` senkronu dahil)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] `.env*` (`VITE_API_URL`), `ProtectedRoute` (JWT yoksa `/login`'e yönlendir, `state`'te nereden geldiğini taşır), temel layout (Sidebar/Topbar)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Dark Mode** — dark tema paleti `DESIGN_SYSTEM.md`'de (light paletin yanına, §2), `index.css`'te `.dark` seçicisiyle token override (Tailwind v4 `@custom-variant dark`), `themeSlice` (Light/Dark/System — `languageSlice` ile aynı desen), `useThemeSync` hook (canlı değişiklik + System modunda OS dinleyicisi), `ThemeSwitcher` (Topbar), `index.html`'e FOUC-önleyici senkron script
- [ ] ➜ **Admin Akademi'ye işle**
*(Kurulum task'ı — dikey dilim/roadmap kuralı A-01 gibi burada uygulanmaz (tek "feature" değil,
paylaşılan altyapı), AMA A-02 (Ortak Altyapı) emsaliyle aynı şekilde akademiye işlenir.)*

⚠️ **2026-08-08 — Backend baştan yazım:** `backend/` kodu ve `AKADEMI/backend/` tamamen
sıfırlandı (kullanıcı kararı), `A_backend.md`'de A-01…A-20 olarak yeniden tasarlandı (tek/ortak
Faz A — admin/kullanıcı backend ayrımı yok). Aşağıdaki madde başlarındaki **"Referans: A-0X"**
işaretleri bu YENİ numaralara güncellendi — Auth=A-03 (QR dahil), Words=A-05, Categories=A-06,
Loglama=A-04 hiç değişmedi; **Admin API eski A-07'den A-18'e, SMTP eski A-09'dan A-19'a kaydı**
(Admin API artık Kişisel Kart API'sinden [A-10] SONRA — içerik moderasyonu ilk seferde tam
yazılıyor, eski "A-07.1 ertelendi" retrofit'i artık yok, bkz. `A_backend.md` başlık notu).

🔄 **2026-08-06 — Palet güncellemesi:** `DESIGN_SYSTEM.md`'deki renk/tipografi/radius/gölge sistemi
"Turkuaz+Mor" temelinden "Apple + Duolingo" temeline (accent `#FF6B00` Canlı Turuncu, font Plus
Jakarta Sans, radius skalası genişletildi — ör. `--radius-control` yerine ayrı `--radius-input`/
`--radius-button`) yeniden yazıldı. `admin/src/index.css`, `App.tsx`, `LanguageSwitcher`/
`ThemeSwitcher` (`rounded-control`→`rounded-button`) senkronlandı, `AKADEMI/admin/B-01_kurulum/`
bölümleri (02, 04, 05, 06) yeni token değerleriyle güncellendi. B-01 tekrar ⬜'e alınmadı — bu
yalnızca bir token senkronizasyonu.

### B-02 — Auth Sayfaları ⬜
**Referans:** A-03 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §3
> E-posta + şifre + OTP (2FA) + **QR ile giriş** (B-02.1); Google/Apple **yok** (Admin panelde asla).
- [ ] **Tip:** `LoginRequest`, `VerifyOtpRequest`, `AdminUser` (`auth.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `authApi` — `login`, `verifyOtp`, `logout` (axios + `useApiMutation`, `store/api/authApi.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Slice:** `authSlice` — `user`, `accessToken`, `isAuthenticated` (`store/slices/authSlice.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `LoginPage` (e-posta+şifre formu, "QR ile giriş" linki), `OtpVerifyPage` (6 haneli
  kod) — `logout` çağrısı B-01'in Topbar'ındaki "Çıkış Yap" aksiyonuna bağlanır (B-01 ✅ zaten kurulu
  Topbar'a bu task'ta retrofit edilir)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/login`, `/verify-otp` (`App.tsx`), başarılı girişte `/` yönlendirme
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `LoginPage.test.tsx` (mutlu yol + hatalı şifre), `authSlice.test.ts`
- [ ] ➜ **Admin Akademi'ye işle**

### B-02.1 — QR Kod ile Giriş ⬜
**Referans:** A-03 (`A_backend.md`) — QR ile giriş, ayrı bir alt-task değil, A-03'ün parçası, REFERENCE/API_ENDPOINTS.md §3.1, REFERENCE/SECURITY.md §1.3
> `LoginPage`'e eklenen "QR ile giriş" sekmesi/linki — Steam benzeri akış: admin panelde QR
> gösterilir, admin'in kendi hesabıyla mobilde (Faz D tamamlanınca) zaten giriş yapmış olan taraf
> okutup onaylar. Backend endpoint'leri (`/auth/qr/*`) C-03.1 (Web) / D-05.1 (Mobil) ile
> **birebir aynı** — istemciye göre dallanmaz, yalnızca admin tarafına yeni bir frontend ekranı
> eklenir. Onaylanan mobil tarafın `Admin` rolünde olması **gerekmez** (`/auth/qr/generate` kimin
> çağırdığını bilmez) — güvenlik zaten normal login'deki AYNI token akışından gelir.
- [ ] **Hook:** `useQrLoginPolling` (durum `Confirmed` olunca `authSlice`'a token yaz + yönlendir; `Expired`/410 olunca QR'ı otomatik yenile)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `QrLoginPage` (`qrcode.react` ile QR görseli + `pairingCode` gösterimi + "süresi doldu, yenile" durumu)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/login/qr` (`App.tsx`), `LoginPage`'den link
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `QrLoginPage.test.tsx` (polling durum geçişleri, expired→yenile)
- [ ] ➜ **Admin Akademi'ye işle**

### B-03 — Kelime Yönetimi ⬜
**Referans:** A-05 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §5
> **Component detaylandırma örneği:** aşağıdaki alt maddeler `CLAUDE.md` §4.1'deki granülerlik
> kuralının uygulanmış hâlidir — `WordFormModal` ve `WordPairingPage` tek satırlık birer madde
> değil, her biri kendi dosyasına çıkan alt component'lerin listesidir. Yeni bir sayfaya
> başlarken bu bölüm şablon olarak kopyalanır.
- [ ] **Tip:** `Word`, `WordDetail`, `WordFormValues` (`word.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `wordsApi` — `getWords` (filtre/sayfa), `createWord`, `updateWord`, `deleteWord`,
      `getUnmatchedWordConcepts` (`languageId` bazlı, `suggestedMatchConceptId` dahil), `pairWordConcepts`,
      `getLanguages` (`GET /languages` — `LanguageAndTypeStep`'in dil seçimi), `uploadWordImage`
      (`POST /media/images/upload`, A-07 — `WordConcept.ImageUrl`'i doldurur)
      (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Slice:** `wordFilterSlice` — liste filtre/sayfa state (arama, level, partOfSpeech)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component — `WordListPage`:**
  - [ ] `WordListPage` (üst kapsayıcı — filtre çubuğu + tablo + sayfalamayı bir araya getirir, kendi mantığı yok)
  - [ ] `WordFilterBar` (arama input + level/partOfSpeech select — `wordFilterSlice`'a yazar)
  - [ ] `WordTable` (satır render, sıralama başlıkları)
  - [ ] `WordTableRow` (tek satır — düzenle/sil aksiyon butonları)
  - [ ] `Pagination` (sayfa numarası, genel amaçlı — başka listelerde de kullanılacak, `components/common/`'a konur)
  - [ ] ➜ **Admin Akademi'ye işle** (her alt component kendi bölümü)
- [ ] **Component — `WordFormModal`:**
  - [ ] `WordFormModal` (üst kapsayıcı — Formik context + Yup şema + submit/ekle-düzenle ortak akış)
  - [ ] `LanguageAndTypeStep` (önce dil [`de`/`tr`] sonra Tür seçimi — gramer bölümünün hangi alt component'i göstereceğine karar verir)
  - [ ] `GermanGrammarFields` (`de` + Noun/Verb/Diğer — `GERMAN_LANGUAGE_FEATURES.md §10`'un TS karşılığı)
  - [ ] `TurkishGrammarFields` (`tr` + Noun/Verb/Diğer — `TURKISH_LANGUAGE_FEATURES.md §9`'un TS karşılığı; backend `WordGrammarValidator` ile aynı mantık, kod paylaşımı yok — bkz. A-05 notu)
  - [ ] `ConjugationGrid` (fiil çekim tablosu — Almanca/Türkçe form'unun ikisinde de kullanılan ortak alt component)
  - [ ] `ExampleSentenceField` (örnek cümle + kategori seçimi)
  - [ ] `WordImageUploadField` (`uploadWordImage` ile görsel yükle/önizle/kaldır — `WordConcept.ImageUrl`)
  - [ ] ➜ **Admin Akademi'ye işle** (her alt component kendi bölümü)
- [ ] **Component — `WordPairingPage`:**
  - [ ] `WordPairingPage` (üst kapsayıcı — iki sütun layout + eşleştirme state'i)
  - [ ] `UnmatchedWordColumn` (tek dilin eşleşmemiş liste sütunu — `de`/`tr` için aynı component iki kez kullanılır)
  - [ ] `SuggestedMatchBadge` (`suggestedMatchConceptId` varsa öne çıkaran rozet)
  - [ ] `PrimarySideToggle` ("birincil tarafı değiştir" kontrolü — varsayılan: işlemi başlatan taraf `primaryId`)
  - [ ] `PairingConfirmModal` (onay öncesi özet — Tür/Seviye/Kategori farkı yalnızca bilgilendirme, force gerektirmez)
  - [ ] ➜ **Admin Akademi'ye işle** (her alt component kendi bölümü)
- [ ] **Route:** `/words`, `/words/pairing` (`App.tsx`), sidebar linki
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `WordFormModal.test.tsx` (dil+tür bazlı koşullu alan render/validasyon, submit),
      `WordListPage.test.tsx` (filtre), `WordPairingPage.test.tsx` (eşleştirme mutlu yol + önerilen eşleşme render)
- [ ] ➜ **Admin Akademi'ye işle**

### B-03.1 — Toplu Kelime İçe Aktarma ⬜ ⚠️ **[2026-08-15 — yeni task, kullanıcı isteği]**
**Referans:** A-18 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §11
> A-18'in `POST /admin/words/import`'u planlanmıştı ama admin panelde bunu çağıran hiçbir sayfa/
> component yoktu — bu boşluğu kapatır. Endpoint bir CSV değil, JSON `{ rows: [...] }` bekler; admin
> bir CSV dosyası seçer, istemci tarafında JSON'a çevrilip gönderilir. **Yeni kütüphane:**
> `papaparse` (CLAUDE.md §4.1 "duruma göre eklenen" deseni — yalnızca bu task'ta, CSV parse için).
- [ ] **Tip:** `BulkImportRow`, `BulkImportResult` (`{ totalRows, importedCount, skippedCount, results[] }`) (`wordImport.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `wordsApi` — `bulkImportWords` (`POST /admin/words/import`) (axios + `useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `WordImportPage` (üst kapsayıcı), `CsvFileDropzone` (dosya seç/sürükle-bırak,
  `papaparse` ile satırları `BulkImportRow[]`'a çevirir — hatalı sütun eşlemesi istemci tarafında
  erken uyarır), `ImportResultTable` (satır bazlı başarı/hata — `errorCode` insan-okur mesaja çevrilir)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/words/import` (`App.tsx`, `WordListPage`'den link)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `CsvFileDropzone.test.tsx` (parse + hatalı sütun uyarısı), `ImportResultTable.test.tsx` (best-effort karışık sonuç render)
- [ ] ➜ **Admin Akademi'ye işle**

### B-04 — Kategori Yönetimi ⬜
**Referans:** A-06 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §6
- [ ] **Tip:** `Category`, `CategoryFormValues` (`category.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `categoriesApi` — `getCategories` (hiyerarşik), `createCategory`, `updateCategory`, `deleteCategory` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `CategoryTreePage` (hiyerarşik ağaç liste), `CategoryTreeNode` (tek düğüm — özyinelemeli render), `CategoryFormModal` (üst kategori seçimi, ikon, renk, seviye)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/categories` (`App.tsx`), sidebar linki
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `CategoryTreePage.test.tsx` (hiyerarşik render), `CategoryFormModal.test.tsx`
- [ ] ➜ **Admin Akademi'ye işle**

### B-05 — Kullanıcı Yönetimi ⬜
**Referans:** A-18 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §11
- [ ] **Tip:** `AdminUserListItem`, `UserDetail` (`user.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `adminUsersApi` — `getUsers` (arama/rol filtresi), `getUserDetail`, `changeRole`, `toggleStatus` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `UserListPage` (arama+rol filtresi+tablo — `WordListPage` ile aynı `Pagination` ortak component'i kullanılır), `UserDetailPage` (profil+istatistik+rol/durum aksiyonları), `RoleBadge`/`StatusBadge` (küçük, tekrar kullanılan rozet component'leri)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/users`, `/users/:id` (`App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `UserListPage.test.tsx` (arama/filtre), `UserDetailPage.test.tsx` (rol değiştir aksiyonu)
- [ ] ➜ **Admin Akademi'ye işle**

### B-06 — Paylaşım/İçerik Moderasyonu ⬜
**Referans:** A-18 (`A_backend.md`)
> **Not:** "Herkese açık + admin onayı" modeli kaldırıldı — DATABASE_SCHEMA/Kisisel_Icerik.md'de `IsPublic`/`IsApproved`
> alanı yok, gerçek mekanizma `SharedContents` (link tabanlı, admin onayı gerektirmez). Bu sayfa onun
> yerine **şikayet edilen** kişisel kartları listeler/siler (`GET/DELETE /admin/user-cards`).
> **Backend bağımlılığı (2026-08-08 roadmap'inde çözüldü):** `GET/DELETE /admin/user-cards`
> **A-18**'in parçası — eski turda `UserCard` entity'si (**A-10**) yokken planlanıp ayrı bir
> "A-07.1 ertelendi" retrofit task'ına bölünmüştü, yeni roadmap'te Admin API bilinçli olarak
> Kişisel Kart API'sinden SONRAYA alındığı için bu artık gerekmiyor — Faz A tamamlandığında
> A-18 de dahil tüm admin endpoint'leri hazır olacak, B-06'nın atlanması/retrofit edilmesi gerekmez.
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
**Referans:** A-18 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §11
- [ ] **Tip:** `AdminStatistics` (`statistics.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `statisticsApi` — `getAdminStatistics` (axios + `useApiQuery`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `DashboardPage` (üst kapsayıcı), `StatCard` (tekil metrik kartı — toplam/aktif kullanıcı, tekrar kullanılır), `TopWordsTable` (en çok öğrenilen/sorunlu kelimeler), `ActivityChart` (günlük/haftalık grafik — grafik kütüphanesi bu task'ta seçilir, henüz karar verilmedi)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/` (ana sayfa, `App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `DashboardPage.test.tsx` (veri render, yükleniyor durumu)
- [ ] ➜ **Admin Akademi'ye işle**

### B-08 — Log Görüntüleme Paneli ⬜
**Referans:** A-04, A-18 (`A_backend.md`)
- [ ] **Tip:** `ActivityLogEntry`, `ApplicationLogEntry`, `SecurityLogEntry` (`log.types.ts`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **API:** `logsApi` — `getActivityLogs`, `getApplicationLogs`, `getSecurityLogs` (filtre+sayfa, axios + `useApiQuery`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Component:** `LogsPage` (üst kapsayıcı — sekme state'i), `LogTabs` (3 sekme: Activity/Application/Security), `LogFilterBar` (filtre + tarih aralığı [`date-fns` ile biçimlendirme]), `LogTable` (sayfalama tablo — `Pagination` ortak component'i kullanılır)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Route:** `/logs` (`App.tsx`)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] **Birim testleri:** `LogsPage.test.tsx` (sekme geçişi, filtre)
- [ ] ➜ **Admin Akademi'ye işle**
- [ ] CSV dışa aktarma (opsiyonel)

### B-09 — SMTP Ayarları Sayfası ⬜
**Referans:** A-19 (`A_backend.md`)
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
