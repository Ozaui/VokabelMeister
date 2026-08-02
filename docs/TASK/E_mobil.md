# FAZ E — Mobil Uygulama (`/mobile`)

> **Yöntem/standart:** Kurallar için → `TASK.md` (**⭐ Frontend Çalışma Yöntemi**, **Her Parça
> İçin Döngü**) — o bölümler değişmez standarttır, burada tekrar edilmez. Mobil'de adım 6 (Route)
> `React Navigation` ile yapılır (`navigation/*Navigator.tsx`); state/veri katmanı (tip/api/slice/hook)
> Web ile aynı desendedir, mümkünse aynı RTK Query tip tanımları paylaşılır.

### E-01 — Proje Kurulumu ⬜ *(Expo TS, paketler, klasör yapısı, `.env*`)*
- [ ] Expo TS şablonu, klasör yapısı (`src/{screens,components,navigation,store,hooks,types}`)
- [ ] `.env*` (`EXPO_PUBLIC_API_URL`)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz; ilk feature E-05'ten başlar.)*

### E-02 — Redux Store ⬜
- [ ] `store.ts`, `authSlice`, RTK Query `api.ts` (baseQuery + `Authorization` header)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz.)*

### E-03 — Axios + Auth Service ⬜ *(Expo Secure Store)*
- [ ] Axios interceptor (401 → refresh token akışı), token saklama `expo-secure-store` (Web'deki `localStorage` yerine)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz.)*

### E-04 — Navigasyon ⬜ *(Auth Stack + Tab + splash)*
- [ ] `RootNavigator` (Auth Stack ↔ Main Tab geçişi, JWT kontrolü), `AuthStackNavigator`, `MainTabNavigator`, splash ekranı
*(Yapısal/altyapı task'ı — E-05+'ta gerçek ekranlar bu navigator'lara eklenecek.)*

### E-05 — Kimlik Doğrulama + Seviye Seçim ⬜ *(Google + Apple iOS)*
**Referans:** A-03 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3
- [ ] **Tip:** `RegisterRequest`, `LoginRequest`, `VerifyOtpRequest`, `User` (`types/auth.ts` — Web'deki ile aynı şekil)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `authApi` — `register`, `verifyEmail`, `login`, `verifyOtp`, `loginWithGoogle`, `loginWithApple`
- [ ] ➜ **AKADEMI/mobile'a işle**
> **Not (tema):** `LevelSelectScreen` kendi RTK Query mutation'ını yazmaz — E-14'teki `profileApi.
> updateProfile` (`PUT /users/me`) çağrılır, `{ currentLevel, themePreference }` birlikte gönderilir.
> Login öncesi ekranlarda tema, cihaz sistem tercihi (`Appearance.getColorScheme()`) ile gösterilir;
> login sonrası `AuthUserDto.themePreference` `authSlice`'a yazılıp senkronlanır.
- [ ] **Slice:** `authSlice` — `user`, `accessToken`, `isAuthenticated`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `RegisterScreen`, `VerifyEmailScreen`, `LoginScreen` (+ Google/Apple butonları — Apple yalnızca iOS), `VerifyOtpScreen`, `LevelSelectScreen` (A1-C2 + tema seçimi [Açık/Koyu/Sistem], kayıt sonrası ilk giriş onboarding'i)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `AuthStackNavigator`'a ekran kayıtları (`navigation/AuthStackNavigator.tsx`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `LoginScreen.test.tsx`, `authSlice.test.ts`
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-05.1 — QR Kod Tarayıcı (Web/Masaüstü Oturumu Onaylama) ⬜
**Referans:** A-03.1 (`A_admin_panel_backend.md`), REFERENCE/API_ENDPOINTS.md §3.1
> `LoginScreen`'e (veya profil menüsüne) eklenen "QR ile giriş yap" girişi — kullanıcı zaten mobilde
> giriş yapmış olmalı ([Authorize] gerektirir), web/masaüstünde açılan QR'ı okutup onaylar.
- [ ] **Tip:** `QrScanResponse` (`types/auth.ts`'e eklenir)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `authApi`'ye eklenir — `scanQr`, `confirmQr`, `denyQr` mutation'ları
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `QrScannerScreen` (`expo-camera` barcode scanning — deep link'ten token çıkarır, `scanQr` çağırır), `QrConfirmScreen` (cihaz bilgisi + `pairingCode` gösterimi, "Onayla"/"Reddet")
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı (profil menüsünden erişim)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `QrConfirmScreen.test.tsx` (pairingCode render, onayla/reddet akışı)
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-06 — Kelime Kartı Komponenti ⬜ *(+ ses/görsel/IPA)*
**Referans:** REFERENCE/GERMAN_LANGUAGE_FEATURES.md §1-6, §8
> Web'deki D-04 `SystemWordCard`/`PersonalCard` ile aynı veri şekli; mobil'e özgü ek: ses çalma
> (`expo-av`) ve IPA telaffuz gösterimi.
- [ ] **Tip:** `SystemWordCardProps`, `PersonalCardProps` (`types/card.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Hook:** `useAudioPlayer` (`expo-av` ile telaffuz sesi çalma)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component:** `SystemWordCard` (artikel + cinsiyet rengi + 4 hâl + çoğul + IPA + ses butonu), `PersonalCard` (flip)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `SystemWordCard.test.tsx`, `useAudioPlayer.test.ts` (mock `expo-av`)
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-07 — Öğrenme / Sınav Ekranı ⬜
**Referans:** C-05, C-03 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §9
> **Not (2026-07-07 SRS tasarım kararları):** İstemci artık
> `sessionType` seçmiyor — oturum `mode: New|Due|Band|Mixed` ile başlatılıyor, her review sorusunun
> gerçek formatı backend'de rastgele atanıyor. Streak yalnızca `New` (günlük yeni kelime) oturumuna bağlı.
> **Not (yön/hedef dil):** kullanıcı profilinde sabit bir "öğrendiğim dil" yok — aynı hesapla hem
> Almanca hem Türkçe öğrenilebilir (bkz. `C_kullanici_backend.md` C-05, `DATABASE_SCHEMA/Icerik.md`
> "Eşleştirme"). `targetLanguageId` her oturum başlatmada seçilir (`HomeScreen`'de dil anahtarı),
> `POST /learning-sessions` gövdesine eklenir.
- [ ] **Tip:** `LearningSession`, `AnswerRequest`, `SessionResult`, `SessionMode` (`New|Due|Band|Mixed`),
  `MasteryBand` (`Weak|Medium|Good`), `TargetLanguage` (`de|tr`) (`types/learning.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `learningApi` — `startSession` (mode bazlı), `submitAnswer`, `requestHint`,
  `completeSession`, `abandonSession`, `repeatSession`, `getTodayLearned`, `getTodayTested`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Slice:** `learningSessionSlice` — mevcut soru index'i, oturum durumu, aktif sorunun rastgele
  atanmış tipi, ipucu/zaman bazlı `selfRating` tavan kilidi
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `HomeScreen` (streak, günlük hedef ilerleme çubuğu, pasif due rozeti,
  hedef tamamlanınca opsiyonel "tekrar edelim mi" teklifi, "Bugün Öğrendiklerim"/"Bugün Test
  Ettiklerim" listeleri, **dil anahtarı** [Almanca öğren/Türkçe öğren, `targetLanguageId` seçimi —
  her ikisinin streak/ilerlemesi bağımsız]), `FlashcardScreen` (+ E-06 `SystemWordCard`, ipucu butonu + zaman/ipucu
  bazlı seçenek kilitleme), `MultipleChoiceScreen`, `TranslationQuizScreen`, `TrueFalseScreen`,
  `LeechActionModal` (5 ardışık yanlıştan sonra — Askıya Al/Sıfırla/Devam Et),
  `SessionSummaryScreen` (+ "Aynı Kelimelerle Tekrar Et")
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator` içine `LearningStackNavigator`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `FlashcardScreen.test.tsx` (ipucu/zaman tavan kilidi), `learningSessionSlice.test.ts`,
  `HomeScreen.test.tsx` (streak yalnızca New'e bağlı), `SessionSummaryScreen.test.tsx` (repeat akışı)
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-08 — Kategoriler Ekranı ⬜
**Referans:** A-06, C-02 (`A_admin_panel_backend.md`, `C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §6, §8
- [ ] **Tip:** `Category`, `UserCategory` (`types/category.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `categoriesApi` — `getCategories`, `getUserCategories`, `createUserCategory`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `CategoriesScreen` (sistem + kişisel sekmeleri), `UserCategoryFormModal`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `CategoriesScreen.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-09 — Kişisel Kartlar Ekranı ⬜
**Referans:** C-04 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §7
- [ ] **Tip:** `UserCard`, `UserCardFormValues` (`types/userCard.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `userCardsApi` — `getUserCards`, `createUserCard`, `updateUserCard`, `deleteUserCard`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `UserCardsScreen` (+ E-06 `PersonalCard`), `UserCardFormModal` (`expo-image-picker` ile görsel seçimi)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `UserCardFormModal.test.tsx` (duplikat uyarı akışı)
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-10 — Sınıf Ekranı ⬜
**Referans:** C-07 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §12
- [ ] **Tip:** `ClassSummary`, `ClassDetail`, `ClassWord` (`types/class.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `classesApi` — `getClasses`, `createClass`, `joinClass`, `getClassDetail`, `getClassStatistics`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ClassListScreen`, `ClassDetailScreen`, `JoinClassModal`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `JoinClassModal.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-11 — Arkadaş Ekranı ⬜
**Referans:** C-08 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §13
- [ ] **Tip:** `Friendship`, `FriendRequest` (`types/friend.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `friendsApi` — `getFriends`, `getFriendRequests`, `sendRequest`, `acceptRequest`, `rejectRequest`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `FriendsScreen`, `SendFriendRequestModal`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `FriendsScreen.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-12 — Paylaşım Linki Ekranı ⬜
**Referans:** C-06 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §14
- [ ] **Tip:** `SharedContentPreview` (`types/share.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `shareApi` — `createShareLink`, `getSharePreview`, `importSharedContent`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ShareModal`, `SharePreviewScreen` (deep link ile açılır — anonim erişim)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** Deep link config (`app.json` scheme) + `RootNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `SharePreviewScreen.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-13 — İlerleme Ekranı ⬜
**Referans:** C-03 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §10
> **Not (2026-07-07 SRS tasarım kararları):** Bant eşiği `Mastery` yüzdesine göre (🔴 Zayıf 0-40 ·
> 🟡 Orta 40-70 · 🟢 İyi 70-100), `CurrentLevel` değil.
- [ ] **Tip:** `WordProgress`, `UserCardProgress`, `ProgressSummary` (`weak/medium/good/dueNow` sayıları),
  `Achievement`, `SuspendedWord` (`types/progress.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `progressApi` — `getWordProgress`, `getUserCardProgress`, `getProgressSummary`,
  `getBandWords`, `getSuspendedWords`, `applyLeechAction`, `achievementsApi` — `getMyAchievements`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ProgressScreen` (mastery listesi, grafik, bant kartları 🔴🟡🟢),
  `BandWordListScreen` (tıklanınca — **İncele** salt okunur liste, **Sına** butonu E-07 `mode: Band`
  oturumunu başlatır; leech kelimeler 🩹 işaretli), `SuspendedWordsScreen` (geri getir butonu),
  `AchievementsSection` (rozet grid'i, `Icon` resim URL'i + `Rarity` renk kodu)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `ProgressScreen.test.tsx` (bant kartı sayıları), `BandWordListScreen.test.tsx`
  (İncele/Sına geçişi), `SuspendedWordsScreen.test.tsx` (geri getir akışı), `AchievementsSection.test.tsx` (rozet render)
- [ ] ➜ **AKADEMI/mobile'a işle**

### E-14 — Profil Ekranı ⬜
**Referans:** C-01, C-09, C-10 (`C_kullanici_backend.md`), REFERENCE/API_ENDPOINTS.md §4
- [ ] **Tip:** `UserProfile`, `UpdateProfileRequest` (`currentLevel`/`themePreference` dahil, `types/profile.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **RTK Query:** `profileApi` — `getProfile`, `updateProfile`, `uploadAvatar`, `changePassword`, `requestAccountDeletion`, `confirmAccountDeletion`, `updateDeviceToken` (OneSignal)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ProfileScreen` (`expo-image-picker` ile avatar + tema değiştir seçici [Açık/Koyu/Sistem]), `ChangePasswordModal`, `DeleteAccountModal` (OTP onaylı)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `ProfileScreen.test.tsx`, `DeleteAccountModal.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**
