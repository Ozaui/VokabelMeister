# FAZ D — Web Kullanıcı Uygulaması (`/web`)

> **Yöntem/standart:** Kurallar için → `TASK.md` (**⭐ Frontend Çalışma Yöntemi**, **Her Parça
> İçin Döngü**) — o bölümler değişmez standarttır, burada tekrar edilmez. Her feature
> tip→api→slice→hook→component→route→test sırasıyla yazılır ve `AKADEMI/web/`'e işlenir.

> **"Component:" maddeleri özettir, atomik değildir** (`../../CLAUDE.md` §4.1'deki 2026-08-05
> notu) — gerçek yazımda her isimlendirilmiş component kendi alt-component'lerine bölünür ve
> roadmap'e her biri **ayrı `[ ]` satırı** olarak işlenir. **D-05 aşağıda bu bölünmenin örneği
> olarak önceden alt maddelere ayrılmıştır** — yeni bir sayfaya başlarken şablon olarak kullanılır.

> **2026-08-05 — Tasarım sistemi eklendi:** D-01'e, Admin ile ortak `REFERENCE/DESIGN_SYSTEM.md`
> uygulama adımı eklendi — önceden Web'in kendi ayrı tasarım kararı alması bekleniyordu, bu ayrım
> kaldırıldı (bkz. `DESIGN_SYSTEM.md` kapsam notu).

### D-01 — Kurulum ⬜
- [ ] React + Vite + TS, Tailwind, Redux Toolkit (yalnızca local/UI state — bkz. `CLAUDE.md` §4.1), React Router v6, Formik + Yup, Axios
- [ ] Tasarım sistemi uygulaması — `REFERENCE/DESIGN_SYSTEM.md`'deki Admin ile ortak palet
      (Primary/accent `#5B54F0` light · `#8A83FF` dark), Inter fontu, §4 radius skalası
      (buton/input 8px, kart 16px, modal 20px, badge 999px) ve §5 gölge skalası Tailwind
      `@theme`'e + `.dark` override'ına işlenir — Admin'deki `index.css` token'larıyla birebir
      aynı değerler, ayrı bir palet icat edilmez
- [ ] `.env*` (VITE_API_URL, VITE_GOOGLE_CLIENT_ID), `GoogleOAuthProvider`, ProtectedRoute, temel layout
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz; ilk feature D-03'ten başlar.)*

### D-02 — Redux Store + Auth Service ⬜
- [ ] `store.ts`, `authSlice`, `uiSlice`, axios `apiClient` + `useApiQuery`/`useApiMutation` hook'u (`Authorization`/`Accept-Language` interceptor'ı)
- [ ] TS arayüzleri (`types/`), Axios interceptor (401 → refresh token akışı, `localStorage`)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz.)*

### D-03 — Auth Sayfaları ⬜
**Referans:** A-03 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3
- [ ] **Tip:** `RegisterRequest`, `LoginRequest`, `VerifyOtpRequest`, `User` (`auth.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `authApi` — `register`, `verifyEmail`, `login`, `verifyOtp`, `loginWithGoogle`, `forgotPassword`, `resetPassword` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
> **Not (tema):** `LevelSelectPage` kendi API çağrısını yazmaz — D-12'deki `profileApi.
> updateProfile` (`PUT /users/me`) çağrılır, `{ currentLevel, themePreference }` birlikte gönderilir.
> Login öncesi (bu sayfadan önceki ekranlarda) tema, local cihaz tercihi/`prefers-color-scheme`
> ile gösterilir; login sonrası `AuthUserDto.themePreference` `authSlice`'a yazılıp senkronlanır.
- [ ] **Slice:** `authSlice` — `user`, `accessToken`, `isAuthenticated` güncellemesi
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `RegisterPage`, `VerifyEmailPage` (OTP), `LoginPage` (+ Google butonu), `VerifyOtpPage`, `ForgotPasswordPage`, `ResetPasswordPage`, `LevelSelectPage` (A1-C2 + tema seçimi [Açık/Koyu/Sistem], kayıt sonrası ilk giriş onboarding'i — `PUT /users/me` ile C-01'e gönderilir, bkz. `C_kullanici_backend.md` C-01 notu)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/register`, `/verify-email`, `/login`, `/verify-otp`, `/forgot-password`, `/reset-password`, `/level-select` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `LoginPage.test.tsx`, `RegisterPage.test.tsx` (mutlu yol + validasyon hataları), `authSlice.test.ts`
- [ ] ➜ **AKADEMI/web'e işle**

### D-03.1 — QR Kod ile Giriş ⬜
**Referans:** A-03.1 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3.1
> `LoginPage`'e eklenen "QR ile giriş" sekmesi/linki — mobil uygulaması olan ama şifresini
> hatırlamayan ya da yalnızca Google/Apple ile kayıtlı (`PasswordHash` yok) kullanıcılar için.
- [ ] **Tip:** `QrGenerateResponse`, `QrStatusResponse` (`auth.types.ts`'e eklenir)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `authApi`'ye eklenir — `generateQr`, `getQrStatus` (axios + `useApiMutation`/`useApiQuery`, polling `useQrLoginPolling` hook'unda ~2sn aralıkla tekrar çağrılır)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Hook:** `useQrLoginPolling` (durum `Confirmed` olunca `authSlice`'a token yaz + yönlendir; `Expired`/410 olunca QR'ı otomatik yenile)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `QrLoginPage` (`qrcode.react` ile QR görseli + `pairingCode` gösterimi + "süresi doldu, yenile" durumu)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/login/qr` (`App.tsx`, `LoginPage`'den link)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `QrLoginPage.test.tsx` (polling mock — Pending→Confirmed geçişi, Expired yenileme)
- [ ] ➜ **AKADEMI/web'e işle**

### D-04 — Kelime Kartı Komponenti ⬜
**Referans:** REFERENCE/GERMAN_LANGUAGE_FEATURES.md §1-6, §8
> Yeniden kullanılan ortak component — D-05/D-07'de import edilir; kendi API çağrısı/route'u yok,
> yalnızca `component` (+ `tip`) adımları vardır.
- [ ] **Tip:** `SystemWordCardProps`, `PersonalCardProps` (`card.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `SystemWordCard` (artikel + cinsiyet rengi + 4 hâl + çoğul; fiil çekim; ayrılabilir gösterimi), `PersonalCard` (flip animasyonu)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `SystemWordCard.test.tsx` (cinsiyet rengi/artikel render), `PersonalCard.test.tsx` (flip)
- [ ] ➜ **AKADEMI/web'e işle**

### D-05 — Öğrenme / Sınav Sayfası ⬜
**Referans:** C-05, C-03 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §9
> **Not (2026-07-07 SRS tasarım kararları):** İstemci artık
> `sessionType` seçmiyor — oturum `mode: New|Due|Band|Mixed` ile başlatılıyor, her review sorusunun
> gerçek formatı (MultipleChoice/TranslationQuiz/ArticleQuiz/PluralQuiz/TrueFalse) backend'de
> rastgele atanıyor. Streak yalnızca `New` (günlük yeni kelime) oturumuna bağlı.
> **Not (yön/hedef dil):** kullanıcı profilinde sabit bir "öğrendiğim dil" yok — aynı hesapla hem
> Almanca hem Türkçe öğrenilebilir (bkz. `C_kullanici_backend.md` C-05, `DATABASE_SCHEMA/Icerik.md`
> "Eşleştirme"). `targetLanguageId` her oturum başlatmada seçilir (`HomePage`'de bir dil anahtarı/
> sekmesi — `de`/`tr`), `POST /learning-sessions` gövdesine eklenir.
> **Component detaylandırma örneği:** aşağıdaki alt maddeler `CLAUDE.md` §4.1'deki granülerlik
> kuralının uygulanmış hâlidir — bu sayfa projedeki en karmaşık ekran olduğu için (6 farklı soru
> tipi + özet + leech modalı) şablon olarak seçildi.
- [ ] **Tip:** `LearningSession`, `AnswerRequest`, `SessionResult`, `SessionMode` (`New|Due|Band|Mixed`),
  `MasteryBand` (`Weak|Medium|Good`), `TargetLanguage` (`de|tr`) (`learning.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `learningApi` — `startSession` (mode bazlı), `submitAnswer`, `requestHint`,
  `completeSession`, `abandonSession`, `repeatSession`, `getTodayLearned`, `getTodayTested` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Slice:** `learningSessionSlice` — mevcut soru index'i, oturum durumu (istemci tarafı ilerleme),
  aktif sorunun rastgele atanmış tipi, ipucu/zaman bazlı `selfRating` tavan kilidi
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component — `HomePage`:**
  - [ ] `HomePage` (üst kapsayıcı)
  - [ ] `LanguageTabSwitcher` (Almanca/Türkçe öğren sekmesi — `targetLanguageId` seçimi, her ikisinin streak/ilerlemesi bağımsız)
  - [ ] `StreakBadge` (streak sayacı — yalnızca `New` oturumuna bağlı)
  - [ ] `DailyGoalProgressBar` (günlük hedef ilerleme çubuğu + tamamlanınca opsiyonel "tekrar edelim mi" teklifi)
  - [ ] `DueBadge` (pasif due rozeti)
  - [ ] `TodayLearnedList` / `TodayTestedList` ("Bugün Öğrendiklerim"/"Bugün Test Ettiklerim" — seviyesiz vs. `masteryBefore→masteryAfter` yüzdelik gösterimi ortak bir `MasteryDeltaRow` alt component'iyle)
  - [ ] ➜ **AKADEMI/web'e işle** (her alt component kendi bölümü)
- [ ] **Component — Soru ekranları (ortak `QuizLayout` kapsayıcısı + 6 tip):**
  - [ ] `QuizLayout` (ortak kapsayıcı — ilerleme çubuğu, D-04 `SystemWordCard`, alt aksiyon barı; 6 ekran de bunu sarar)
  - [ ] `FlashcardScreen` (4'lü öz değerlendirme + ipucu butonu + zaman/ipucu bazlı seçenek kilitleme)
  - [ ] `MultipleChoiceScreen`
  - [ ] `TranslationQuizScreen`
  - [ ] `ArticleQuizScreen`
  - [ ] `PluralQuizScreen`
  - [ ] `TrueFalseScreen`
  - [ ] `SelfRatingButtons` (4'lü değerlendirme kontrolü — `FlashcardScreen` içinde kullanılan, ama tek başına test edilebilir alt component)
  - [ ] ➜ **AKADEMI/web'e işle** (her alt component kendi bölümü)
- [ ] **Component — Diğer:**
  - [ ] `LeechActionModal` (5 ardışık yanlıştan sonra — Askıya Al/Sıfırla/Devam Et)
  - [ ] `SessionSummaryPage` (özet + XP), `RepeatSessionButton` ("Aynı Kelimelerle Tekrar Et" — kendi mutation çağrısı olan ayrı alt component)
  - [ ] ➜ **AKADEMI/web'e işle** (her alt component kendi bölümü)
- [ ] **Route:** `/learn`, `/learn/session/:id` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `FlashcardScreen.test.tsx` (öz değerlendirme akışı + ipucu/zaman tavan
  kilidi), `learningSessionSlice.test.ts`, `HomePage.test.tsx` (streak yalnızca New'e bağlı,
  due rozeti render), `SessionSummaryPage.test.tsx` (repeat akışı)
- [ ] ➜ **AKADEMI/web'e işle**

### D-06 — Kategoriler Sayfası ⬜
**Referans:** A-06, C-02 (`A_admin_panel_backend.md`, `C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §6, §8
- [ ] **Tip:** `Category`, `UserCategory` (`category.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `categoriesApi` — `getCategories` (hiyerarşik+kelime sayısı), `getUserCategories`, `createUserCategory` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `CategoriesPage` (sistem kategorileri hiyerarşik grid + kişisel kategoriler sekmesi), `UserCategoryFormModal`
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/categories` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `CategoriesPage.test.tsx` (sekme geçişi, hiyerarşik render)
- [ ] ➜ **AKADEMI/web'e işle**

### D-07 — Kişisel Kartlar Sayfası ⬜
**Referans:** C-04 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §7
- [ ] **Tip:** `UserCard`, `UserCardFormValues` (`userCard.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `userCardsApi` — `getUserCards` (filtre/sayfa), `createUserCard` (duplikat 409 handling), `updateUserCard`, `deleteUserCard` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `UserCardsPage` (liste + D-04 `PersonalCard`), `UserCardFormModal` (Formik+Yup — sistem kelimesi eşleşme uyarısı gösterimi)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/my-cards` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `UserCardFormModal.test.tsx` (duplikat uyarı akışı), `UserCardsPage.test.tsx`
- [ ] ➜ **AKADEMI/web'e işle**

### D-08 — Sınıf Sayfası ⬜
**Referans:** C-07 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §12
- [ ] **Tip:** `ClassSummary`, `ClassDetail`, `ClassWord` (`class.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `classesApi` — `getClasses`, `createClass`, `joinClass`, `getClassDetail`, `getClassStatistics`, `addClassWord` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `ClassListPage`, `ClassDetailPage` (üye+kelime+istatistik sekmeleri), `JoinClassModal` (davet kodu), `ClassWordFormModal`
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/classes`, `/classes/:id` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `JoinClassModal.test.tsx` (davet kodu akışı), `ClassDetailPage.test.tsx` (sahip/üye görünürlük farkı)
- [ ] ➜ **AKADEMI/web'e işle**

### D-09 — Arkadaş Sayfası ⬜
**Referans:** C-08 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §13
- [ ] **Tip:** `Friendship`, `FriendRequest` (`friend.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `friendsApi` — `getFriends`, `getFriendRequests`, `sendRequest`, `acceptRequest`, `rejectRequest` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `FriendsPage` (arkadaş listesi + gelen/giden istekler sekmeleri), `SendFriendRequestModal`
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/friends` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `FriendsPage.test.tsx` (kabul/reddet akışı)
- [ ] ➜ **AKADEMI/web'e işle**

### D-10 — Paylaşım Linki Sayfası ⬜ *(anonim `/share/{token}`)*
**Referans:** C-06 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §14
- [ ] **Tip:** `SharedContentPreview` (`share.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `shareApi` — `createShareLink`, `getSharePreview` (Anonim), `importSharedContent` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `ShareModal` (link oluştur, uygulama genelinde ortak), `SharePreviewPage` (anonim erişim — giriş yapılmamışsa da render edilir)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/share/:token` (`App.tsx` — `ProtectedRoute` **dışında**, anonim erişilebilir)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `SharePreviewPage.test.tsx` (giriş yapmadan render, "listeme kopyala" akışı)
- [ ] ➜ **AKADEMI/web'e işle**

### D-11 — İlerleme Sayfası ⬜
**Referans:** C-03 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §10
> **Not (2026-07-07 SRS tasarım kararları):** Bant eşiği `Mastery` yüzdesine göre (🔴 Zayıf 0-40 ·
> 🟡 Orta 40-70 · 🟢 İyi 70-100), `CurrentLevel` değil.
- [ ] **Tip:** `WordProgress`, `UserCardProgress`, `ProgressSummary` (`weak/medium/good/dueNow` sayıları),
  `Achievement`, `SuspendedWord` (`progress.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `progressApi` — `getWordProgress`, `getUserCardProgress`, `getProgressSummary`,
  `getBandWords` (İncele listesi), `getSuspendedWords`, `applyLeechAction`, `achievementsApi` — `getMyAchievements` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `ProgressPage` (mastery seviyesi listesi, sonraki tekrar zamanı, başarı oranı
  grafiği), `BandCard` (🔴🟡🟢 bant kartı — tıklanınca `BandWordListPage`'e gider, leech kelimeler 🩹
  işaretli), `BandWordListPage` (**İncele** salt okunur liste ve **Sına** butonu ile D-05
  `mode: Band` oturumunu başlatma), `SuspendedWordsPage` (askıya alınmışlar, geri getir butonu),
  `AchievementsSection` (rozet grid'i), `AchievementBadge` (tek rozet — `Icon` resim URL'i +
  `Rarity` renk kodu, `AchievementsSection` içinde tekrarlı kullanılır)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/progress`, `/progress/band/:band`, `/progress/suspended` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `ProgressPage.test.tsx` (veri render, bant kartı sayıları), `BandWordListPage.test.tsx`
  (İncele/Sına geçişi), `SuspendedWordsPage.test.tsx` (geri getir akışı), `AchievementsSection.test.tsx` (rozet render)
- [ ] ➜ **AKADEMI/web'e işle**

### D-12 — Profil Sayfası ⬜ *(avatar, şifre değiştir, hesap sil OTP)*
**Referans:** C-01, C-09 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §4
- [ ] **Tip:** `UserProfile`, `UpdateProfileRequest` (`currentLevel`/`themePreference` dahil, `profile.types.ts`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **API:** `profileApi` — `getProfile`, `updateProfile`, `uploadAvatar`, `changePassword`, `requestAccountDeletion`, `confirmAccountDeletion` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Component:** `ProfilePage` (profil formu + avatar yükleme + tema değiştir seçici [Açık/Koyu/Sistem]), `ChangePasswordModal`, `DeleteAccountModal` (OTP onaylı)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Route:** `/profile` (`App.tsx`)
- [ ] ➜ **AKADEMI/web'e işle**
- [ ] **Birim testleri:** `ProfilePage.test.tsx` (form submit), `DeleteAccountModal.test.tsx` (OTP akışı)
- [ ] ➜ **AKADEMI/web'e işle**
