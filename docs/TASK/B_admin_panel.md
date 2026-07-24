
# FAZ B — Admin Panel (`/admin`)

> **Yöntem/standart:** Bu dosyadaki her feature, `../TASK.md`'deki **⭐ Frontend Çalışma Yöntemi** ve
> **Her Parça İçin Döngü** kurallarına göre yazılır (tip→api→slice→hook→component→route→test, her
> parça yazılır yazılmaz Frontend Yol Haritası'na işlenir). O bölümler değişmez standarttır — burada
> tekrar edilmez, her zaman `../TASK.md`'ye bakılır.

### B-01 — Kurulum ⬜
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §3, REFERENCE/DEVELOPMENT_SETUP.md §6
- [ ] React + Vite + TS, TailwindCSS, RTK Query, React Hook Form kurulumu
- [ ] `store.ts` (Redux store) + boş `authSlice` + RTK Query `api.ts` (baseQuery, `Authorization` header)
- [ ] `.env*` (`VITE_API_URL`), `ProtectedRoute` (JWT yoksa `/login`'e yönlendir), temel layout (Sidebar/Topbar)
*(Kurulum task'ı — dikey dilim/roadmap kuralı A-01 gibi burada uygulanmaz; ilk feature B-02'den başlar.)*

### B-02 — Auth Sayfaları ⬜
**Referans:** A-03 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3
> Yalnızca e-posta + şifre + OTP (2FA); Google/Apple **yok** (Admin panelde asla).
- [ ] **Tip:** `LoginRequest`, `VerifyOtpRequest`, `AdminUser` (`auth.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `authApi` — `login`, `verifyOtp` mutation'ları (`store/api/authApi.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Slice:** `authSlice` — `user`, `accessToken`, `isAuthenticated` (`store/slices/authSlice.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `LoginPage` (e-posta+şifre formu), `OtpVerifyPage` (6 haneli kod)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/login`, `/verify-otp` (`App.tsx`), başarılı girişte `/` yönlendirme
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `LoginPage.test.tsx` (mutlu yol + hatalı şifre), `authSlice.test.ts`
- [ ] ➜ **Frontend Yol Haritası'na işle**

### B-03 — Kelime Yönetimi ⬜
**Referans:** A-05 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §5
- [ ] **Tip:** `Word`, `WordDetail`, `WordFormValues` (`word.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `wordsApi` — `getWords` (filtre/sayfa), `createWord`, `updateWord`, `deleteWord`,
      `getUnmatchedWordConcepts` (`languageId` bazlı, `suggestedMatchConceptId` dahil), `pairWordConcepts`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Slice:** `wordFilterSlice` — liste filtre/sayfa state (arama, level, partOfSpeech)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `WordListPage` (filtre+tablo+sayfalama), `WordFormModal` (RHF — WordDetail + örnek cümle + kategori seçimi, ekle/düzenle ortak;
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
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/words`, `/words/pairing` (`App.tsx`), sidebar linki
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `WordFormModal.test.tsx` (dil+tür bazlı koşullu alan render/validasyon, submit),
      `WordListPage.test.tsx` (filtre), `WordPairingPage.test.tsx` (eşleştirme mutlu yol + önerilen eşleşme render)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### B-04 — Kategori Yönetimi ⬜
**Referans:** A-06 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §6
- [ ] **Tip:** `Category`, `CategoryFormValues` (`category.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `categoriesApi` — `getCategories` (hiyerarşik), `createCategory`, `updateCategory`, `deleteCategory`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `CategoryTreePage` (hiyerarşik ağaç liste), `CategoryFormModal` (üst kategori seçimi, ikon, renk, seviye)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/categories` (`App.tsx`), sidebar linki
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `CategoryTreePage.test.tsx` (hiyerarşik render), `CategoryFormModal.test.tsx`
- [ ] ➜ **Frontend Yol Haritası'na işle**

### B-05 — Kullanıcı Yönetimi ⬜
**Referans:** A-07 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §11
- [ ] **Tip:** `AdminUserListItem`, `UserDetail` (`user.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `adminUsersApi` — `getUsers` (arama/rol filtresi), `getUserDetail`, `changeRole`, `toggleStatus`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `UserListPage` (arama+rol filtresi+tablo), `UserDetailPage` (profil+istatistik+rol/durum aksiyonları)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/users`, `/users/:id` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserListPage.test.tsx` (arama/filtre), `UserDetailPage.test.tsx` (rol değiştir aksiyonu)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### B-06 — Paylaşım/İçerik Moderasyonu ⬜
**Referans:** A-07 (`A_admin_panel_backend.md`)
> **Not:** "Herkese açık + admin onayı" modeli kaldırıldı — DATABASE_SCHEMA/Kisisel_Icerik.md'de `IsPublic`/`IsApproved`
> alanı yok, gerçek mekanizma `SharedContents` (link tabanlı, admin onayı gerektirmez). Bu sayfa onun
> yerine **şikayet edilen** kişisel kartları listeler/siler (`GET/DELETE /admin/user-cards`).
- [ ] **Tip:** `ReportedUserCard` (`moderation.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `moderationApi` — `getReportedUserCards`, `deleteUserCard`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `ModerationPage` (liste + inceleme detayı + sil aksiyonu)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/moderation` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `ModerationPage.test.tsx` (liste + silme akışı)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### B-07 — İstatistik Paneli ⬜
**Referans:** A-07 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §11
- [ ] **Tip:** `AdminStatistics` (`statistics.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `statisticsApi` — `getAdminStatistics`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `DashboardPage` (toplam/aktif kullanıcı kartları, en çok öğrenilen/sorunlu kelimeler tablosu, günlük/haftalık grafik)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/` (ana sayfa, `App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `DashboardPage.test.tsx` (veri render, yükleniyor durumu)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### B-08 — Log Görüntüleme Paneli ⬜
**Referans:** A-04, A-07 (`A_admin_panel_backend.md`)
- [ ] **Tip:** `ActivityLogEntry`, `ApplicationLogEntry`, `SecurityLogEntry` (`log.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `logsApi` — `getActivityLogs`, `getApplicationLogs`, `getSecurityLogs` (filtre+sayfa)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `LogsPage` (3 sekme: Activity/Application/Security — filtre + tarih aralığı + sayfalama tablo)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/logs` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `LogsPage.test.tsx` (sekme geçişi, filtre)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] CSV dışa aktarma (opsiyonel)

### B-09 — SMTP Ayarları Sayfası ⬜
**Referans:** A-09 (`A_admin_panel_backend.md`)
- [ ] **Tip:** `SmtpSettingsFormValues` (`smtp.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `smtpApi` — `getSmtpSettings` (şifre `***`), `updateSmtpSettings`, `testSmtpConnection`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `SmtpSettingsPage` (form: Host/Port/SSL/Kullanıcı/Şifre/From, kaydet, "Test e-postası gönder")
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/settings/smtp` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `SmtpSettingsPage.test.tsx` (form validasyon, test e-postası akışı)
- [ ] ➜ **Frontend Yol Haritası'na işle**
