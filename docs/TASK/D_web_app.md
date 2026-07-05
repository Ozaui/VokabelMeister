# FAZ D — Web Kullanıcı Uygulaması (`/web`)

> **Yöntem/standart:** Kurallar için → `../TASK.md` (**⭐ Frontend Çalışma Yöntemi**, **Her Parça
> İçin Döngü**) — o bölümler değişmez standarttır, burada tekrar edilmez. Her feature
> tip→api→slice→hook→component→route→test sırasıyla yazılır ve Frontend Yol Haritası'na işlenir.

### D-01 — Kurulum ⬜
- [ ] React + Vite + TS, Tailwind, Redux Toolkit + RTK Query, React Router v6, RHF, Axios
- [ ] `.env*` (VITE_API_URL, VITE_GOOGLE_CLIENT_ID), `GoogleOAuthProvider`, ProtectedRoute, temel layout
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz; ilk feature D-03'ten başlar.)*

### D-02 — Redux Store + Auth Service ⬜
- [ ] `store.ts`, `authSlice`, `uiSlice`, RTK Query `api.ts` (baseQuery + `Authorization` header)
- [ ] TS arayüzleri (`types/`), Axios interceptor (401 → refresh token akışı, `localStorage`)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz.)*

### D-03 — Auth Sayfaları ⬜
**Referans:** A-03 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3
- [ ] **Tip:** `RegisterRequest`, `LoginRequest`, `VerifyOtpRequest`, `User` (`auth.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `authApi` — `register`, `verifyEmail`, `login`, `verifyOtp`, `loginWithGoogle`, `forgotPassword`, `resetPassword`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Slice:** `authSlice` — `user`, `accessToken`, `isAuthenticated` güncellemesi
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `RegisterPage`, `VerifyEmailPage` (OTP), `LoginPage` (+ Google butonu), `VerifyOtpPage`, `ForgotPasswordPage`, `ResetPasswordPage`, `LevelSelectPage` (A1-C2 seçimi, kayıt sonrası)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/register`, `/verify-email`, `/login`, `/verify-otp`, `/forgot-password`, `/reset-password`, `/level-select` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `LoginPage.test.tsx`, `RegisterPage.test.tsx` (mutlu yol + validasyon hataları), `authSlice.test.ts`
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-03.1 — QR Kod ile Giriş ⬜
**Referans:** A-03.1 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3.1
> `LoginPage`'e eklenen "QR ile giriş" sekmesi/linki — mobil uygulaması olan ama şifresini
> hatırlamayan ya da yalnızca Google/Apple ile kayıtlı (`PasswordHash` yok) kullanıcılar için.
- [ ] **Tip:** `QrGenerateResponse`, `QrStatusResponse` (`auth.types.ts`'e eklenir)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `authApi`'ye eklenir — `generateQr` (mutation), `getQrStatus` (polling query, `pollingInterval: 2000`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Hook:** `useQrLoginPolling` (durum `Confirmed` olunca `authSlice`'a token yaz + yönlendir; `Expired`/410 olunca QR'ı otomatik yenile)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `QrLoginPage` (`qrcode.react` ile QR görseli + `pairingCode` gösterimi + "süresi doldu, yenile" durumu)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/login/qr` (`App.tsx`, `LoginPage`'den link)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `QrLoginPage.test.tsx` (polling mock — Pending→Confirmed geçişi, Expired yenileme)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-04 — Kelime Kartı Komponenti ⬜
**Referans:** REFERENCE/GERMAN_LANGUAGE_FEATURES.md §1-6, §8
> Yeniden kullanılan ortak component — D-05/D-07'de import edilir; kendi RTK Query/route'u yok,
> yalnızca `component` (+ `tip`) adımları vardır.
- [ ] **Tip:** `SystemWordCardProps`, `PersonalCardProps` (`card.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `SystemWordCard` (artikel + cinsiyet rengi + 4 hâl + çoğul; fiil çekim; ayrılabilir gösterimi), `PersonalCard` (flip animasyonu)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `SystemWordCard.test.tsx` (cinsiyet rengi/artikel render), `PersonalCard.test.tsx` (flip)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-05 — Öğrenme / Sınav Sayfası ⬜
**Referans:** C-05, C-03 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §9
- [ ] **Tip:** `LearningSession`, `AnswerRequest`, `SessionResult` (`learning.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `learningApi` — `startSession`, `submitAnswer`, `completeSession`, `abandonSession`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Slice:** `learningSessionSlice` — mevcut soru index'i, oturum durumu (istemci tarafı ilerleme)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `LearningStartPage` (filtre: seviye/kategori/tür seçimi), `FlashcardScreen` (4'lü öz değerlendirme + D-04 `SystemWordCard`), `MultipleChoiceScreen`, `ArticleQuizScreen`, `PluralQuizScreen`, `SessionSummaryPage` (özet + XP)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/learn`, `/learn/session/:id` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `FlashcardScreen.test.tsx` (öz değerlendirme akışı), `learningSessionSlice.test.ts`
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-06 — Kategoriler Sayfası ⬜
**Referans:** A-06, C-02 (`A_admin_panel_backend.md`, `C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §6, §8
- [ ] **Tip:** `Category`, `UserCategory` (`category.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `categoriesApi` — `getCategories` (hiyerarşik+kelime sayısı), `getUserCategories`, `createUserCategory`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `CategoriesPage` (sistem kategorileri hiyerarşik grid + kişisel kategoriler sekmesi), `UserCategoryFormModal`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/categories` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `CategoriesPage.test.tsx` (sekme geçişi, hiyerarşik render)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-07 — Kişisel Kartlar Sayfası ⬜
**Referans:** C-04 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §7
- [ ] **Tip:** `UserCard`, `UserCardFormValues` (`userCard.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `userCardsApi` — `getUserCards` (filtre/sayfa), `createUserCard` (duplikat 409 handling), `updateUserCard`, `deleteUserCard`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `UserCardsPage` (liste + D-04 `PersonalCard`), `UserCardFormModal` (RHF — sistem kelimesi eşleşme uyarısı gösterimi)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/my-cards` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserCardFormModal.test.tsx` (duplikat uyarı akışı), `UserCardsPage.test.tsx`
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-08 — Sınıf Sayfası ⬜
**Referans:** C-07 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §12
- [ ] **Tip:** `ClassSummary`, `ClassDetail`, `ClassWord` (`class.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `classesApi` — `getClasses`, `createClass`, `joinClass`, `getClassDetail`, `getClassStatistics`, `addClassWord`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `ClassListPage`, `ClassDetailPage` (üye+kelime+istatistik sekmeleri), `JoinClassModal` (davet kodu), `ClassWordFormModal`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/classes`, `/classes/:id` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `JoinClassModal.test.tsx` (davet kodu akışı), `ClassDetailPage.test.tsx` (sahip/üye görünürlük farkı)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-09 — Arkadaş Sayfası ⬜
**Referans:** C-08 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §13
- [ ] **Tip:** `Friendship`, `FriendRequest` (`friend.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `friendsApi` — `getFriends`, `getFriendRequests`, `sendRequest`, `acceptRequest`, `rejectRequest`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `FriendsPage` (arkadaş listesi + gelen/giden istekler sekmeleri), `SendFriendRequestModal`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/friends` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `FriendsPage.test.tsx` (kabul/reddet akışı)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-10 — Paylaşım Linki Sayfası ⬜ *(anonim `/share/{token}`)*
**Referans:** C-06 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §14
- [ ] **Tip:** `SharedContentPreview` (`share.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `shareApi` — `createShareLink`, `getSharePreview` (Anonim), `importSharedContent`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `ShareModal` (link oluştur, uygulama genelinde ortak), `SharePreviewPage` (anonim erişim — giriş yapılmamışsa da render edilir)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/share/:token` (`App.tsx` — `ProtectedRoute` **dışında**, anonim erişilebilir)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `SharePreviewPage.test.tsx` (giriş yapmadan render, "listeme kopyala" akışı)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-11 — İlerleme Sayfası ⬜
**Referans:** C-03 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §10
- [ ] **Tip:** `WordProgress`, `UserCardProgress` (`progress.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `progressApi` — `getWordProgress`, `getUserCardProgress`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `ProgressPage` (mastery seviyesi listesi, sonraki tekrar zamanı, başarı oranı grafiği)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/progress` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `ProgressPage.test.tsx` (veri render)
- [ ] ➜ **Frontend Yol Haritası'na işle**

### D-12 — Profil Sayfası ⬜ *(avatar, şifre değiştir, hesap sil OTP)*
**Referans:** C-01, C-09 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §4
- [ ] **Tip:** `UserProfile`, `UpdateProfileRequest` (`profile.types.ts`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **RTK Query:** `profileApi` — `getProfile`, `updateProfile`, `uploadAvatar`, `changePassword`, `requestAccountDeletion`, `confirmAccountDeletion`
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Component:** `ProfilePage` (profil formu + avatar yükleme), `ChangePasswordModal`, `DeleteAccountModal` (OTP onaylı)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Route:** `/profile` (`App.tsx`)
- [ ] ➜ **Frontend Yol Haritası'na işle**
- [ ] **Birim testleri:** `ProfilePage.test.tsx` (form submit), `DeleteAccountModal.test.tsx` (OTP akışı)
- [ ] ➜ **Frontend Yol Haritası'na işle**
