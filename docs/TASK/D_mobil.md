# FAZ D — Mobil Uygulama (`/mobile`)

> **Yöntem/standart:** Kurallar için → `TASK.md` (**⭐ Frontend Çalışma Yöntemi**, **Her Parça
> İçin Döngü**) — o bölümler değişmez standarttır, burada tekrar edilmez. Mobil'de adım 6 (Route)
> `React Navigation` ile yapılır (`navigation/*Navigator.tsx`); state/veri katmanı (tip/api/slice/hook)
> Web ile aynı desendedir (axios + `useApiQuery`/`useApiMutation`, Formik+Yup — bkz. `CLAUDE.md` §4.1), mümkünse aynı TS tip tanımları paylaşılır.

> ⚠️ **2026-08-08 — Backend baştan yazım:** `backend/` kodu tamamen sıfırlandı, eski "Admin Panel
> Backend" (A) / "Kullanıcı Backend" (C) ayrımı kaldırıldı — artık TEK, ortak Faz A, A-01…A-20
> (bkz. `A_backend.md`). Bu dosyadaki **"Referans: A-0X"** işaretleri bu YENİ numaralara
> güncellendi (Faz D'nin kendi D-0X task kodları değişmedi). Not: A-11=Öğrenme/Sınav,
> A-09=SRS/İlerleme, A-08=Kişisel Kategori, A-10=Kişisel Kart, A-12=Profil, A-13=Avatar,
> A-14=Paylaşım, A-15=Sınıf, A-16=Arkadaş, A-17=Push — eski C-0X numaralarının karşılığı.

### D-01 — Proje Kurulumu ⬜ *(Expo TS, paketler, klasör yapısı, `.env*`)*
- [ ] Expo TS şablonu, klasör yapısı (`src/{screens,components,navigation,store,hooks,types}`)
- [ ] `.env*` (`EXPO_PUBLIC_API_URL`)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz; ilk feature D-05'ten başlar.)*

### D-02 — Redux Store ⬜
- [ ] `store.ts`, `authSlice`, `uiSlice`, axios `apiClient` + `useApiQuery`/`useApiMutation` hook'u
      (`Authorization`/`Accept-Language` interceptor'ı — Web C-02 ile aynı, dil tercihi olmadan
      backend hata mesajları CLAUDE.md §1 gereği her zaman tr'ye düşer)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz.)*

### D-03 — Axios + Auth Service ⬜ *(Expo Secure Store)*
- [ ] Axios interceptor (401 → refresh token akışı), token saklama `expo-secure-store` (Web'deki `localStorage` yerine)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz.)*

### D-04 — Navigasyon ⬜ *(Auth Stack + Tab + splash)*
- [ ] `RootNavigator` (Auth Stack ↔ Main Tab geçişi, JWT kontrolü), `AuthStackNavigator`, `MainTabNavigator`, splash ekranı
*(Yapısal/altyapı task'ı — D-05+'ta gerçek ekranlar bu navigator'lara eklenecek.)*

### D-05 — Kimlik Doğrulama + Seviye Seçim ⬜ *(Google + Apple iOS)*
**Referans:** A-03 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §3
- [ ] **Tip:** `RegisterRequest`, `LoginRequest`, `VerifyOtpRequest`, `User` (`types/auth.ts` — Web'deki ile aynı şekil)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `authApi` — `register`, `verifyEmail`, `resendVerification`, `login`, `verifyOtp`,
      `loginWithGoogle`, `loginWithApple`, `forgotPassword`, `resetPassword`, `logout`
      (axios + `useApiQuery`/`useApiMutation`) ⚠️ **[2026-08-15]** `forgotPassword`/`resetPassword`/
      `resendVerification`/`logout` Web C-03'te vardı (veya olması gerekiyordu), burada eksikti —
      hepsi anonim/[Authorize] ortak uçlar, mobil'e özgü bir kısıt yok, eklendi
- [ ] ➜ **AKADEMI/mobile'a işle**
> **Not (tema):** `LevelSelectScreen` kendi API çağrısını yazmaz — D-14'teki `profileApi.
> updateProfile` (`PUT /users/me`) çağrılır, `{ currentLevel, themePreference }` birlikte gönderilir.
> Login öncesi ekranlarda tema, cihaz sistem tercihi (`Appearance.getColorScheme()`) ile gösterilir;
> login sonrası `AuthUserDto.themePreference` `authSlice`'a yazılıp senkronlanır.
- [ ] **Slice:** `authSlice` — `user`, `accessToken`, `isAuthenticated`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `RegisterScreen`, `VerifyEmailScreen` (+ "kodu tekrar gönder" —
      `resendVerification`), `LoginScreen` (+ Google/Apple butonları — Apple yalnızca iOS),
      `VerifyOtpScreen`, `ForgotPasswordScreen`, `ResetPasswordScreen`,
      `LevelSelectScreen` (A1-C2 + tema seçimi [Açık/Koyu/Sistem], kayıt sonrası ilk giriş onboarding'i)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `AuthStackNavigator`'a ekran kayıtları (`navigation/AuthStackNavigator.tsx` — `ForgotPasswordScreen`/`ResetPasswordScreen` dahil)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `LoginScreen.test.tsx`, `RegisterScreen.test.tsx` (mutlu yol + validasyon hataları), `authSlice.test.ts`
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-05.1 — QR Kod Tarayıcı (Web/Masaüstü Oturumu Onaylama) ⬜
**Referans:** A-03 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §3.1
> `LoginScreen`'e (veya profil menüsüne) eklenen "QR ile giriş yap" girişi — kullanıcı zaten mobilde
> giriş yapmış olmalı ([Authorize] gerektirir), web/masaüstünde açılan QR'ı okutup onaylar.
- [ ] **Tip:** `QrScanResponse` (`types/auth.ts`'e eklenir)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `authApi`'ye eklenir — `scanQr`, `confirmQr`, `denyQr` (axios + `useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `QrScannerScreen` (`expo-camera` barcode scanning — deep link'ten token çıkarır, `scanQr` çağırır), `QrConfirmScreen` (cihaz bilgisi + `pairingCode` gösterimi, "Onayla"/"Reddet")
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı (profil menüsünden erişim)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `QrConfirmScreen.test.tsx` (pairingCode render, onayla/reddet akışı)
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-06 — Kelime Kartı Komponenti ⬜ *(+ görsel/IPA)*
**Referans:** REFERENCE/GERMAN_LANGUAGE_FEATURES.md §1-6, §8
> Web'deki C-04 `SystemWordCard`/`PersonalCard` ile aynı veri şekli.
> ⚠️ **[2026-08-15] TTS ertelendi:** Sesli okuma (`useTextToSpeech`, `expo-speech`,
> `TtsFallbackNotice`) kullanıcı kararıyla bu task'tan ÇIKARILDI — kullanıcıya ses vermek şimdilik
> kapsam dışı, istenirse ayrı bir task olarak sonra eklenecek (C-04 ile simetrik, CLAUDE.md §4.1'deki
> TTS satırı da aynı notla güncellendi). IPA gösterimi (metin) TTS'e bağlı DEĞİL, kalıyor. Ses
> kayıtlı bir dosyadan hiç okunmuyor zaten — `WordDetails.AudioUrl`/`UserCards.AudioUrl` yok (bkz.
> `DATABASE_SCHEMA/Icerik.md`/`Kisisel_Icerik.md`), backend'e ses yükleme/saklama sorumluluğu binmez.
- [ ] **Tip:** `SystemWordCardProps`, `PersonalCardProps` (`types/card.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `SystemWordCard` (artikel + cinsiyet rengi + 4 hâl + çoğul + IPA),
  `PersonalCard` (flip — `FrontText`/`BackText` düz metin olduğu için admin kürasyonu gerekmez)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `SystemWordCard.test.tsx`, `PersonalCard.test.tsx` (flip)
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-07 — Öğrenme / Sınav Ekranı ⬜
**Referans:** A-11, A-09 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §9
> **Not (2026-07-07 SRS tasarım kararları):** İstemci artık
> `sessionType` seçmiyor — oturum `mode: New|Due|Band|Mixed` ile başlatılıyor, her review sorusunun
> gerçek formatı backend'de rastgele atanıyor. Streak yalnızca `New` (günlük yeni kelime) oturumuna bağlı.
> **Not (yön/hedef dil):** kullanıcı profilinde sabit bir "öğrendiğim dil" yok — aynı hesapla hem
> Almanca hem Türkçe öğrenilebilir (bkz. `A_backend.md` A-11, `DATABASE_SCHEMA/Icerik.md`
> "Eşleştirme"). `targetLanguageId` her oturum başlatmada seçilir (`HomeScreen`'de dil anahtarı),
> `POST /learning-sessions` gövdesine eklenir.
- [ ] **Tip:** `LearningSession`, `AnswerRequest`, `SessionResult`, `SessionMode` (`New|Due|Band|Mixed`),
  `MasteryBand` (`Weak|Medium|Good`), `TargetLanguage` (`de|tr`) (`types/learning.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `learningApi` — `startSession` (mode bazlı), `submitAnswer`, `requestHint`,
  `completeSession`, `abandonSession`, `repeatSession`, `getTodayLearned`, `getTodayTested` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Slice:** `learningSessionSlice` — mevcut soru index'i, oturum durumu, aktif sorunun rastgele
  atanmış tipi, ipucu/zaman bazlı `selfRating` tavan kilidi
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `HomeScreen` (streak, günlük hedef ilerleme çubuğu, pasif due rozeti,
  hedef tamamlanınca opsiyonel "tekrar edelim mi" teklifi, "Bugün Öğrendiklerim"/"Bugün Test
  Ettiklerim" listeleri, **dil anahtarı** [Almanca öğren/Türkçe öğren, `targetLanguageId` seçimi —
  her ikisinin streak/ilerlemesi bağımsız]), `FlashcardScreen` (+ D-06 `SystemWordCard`, ipucu butonu + zaman/ipucu
  bazlı seçenek kilitleme), `MultipleChoiceScreen`, `TranslationQuizScreen`, `ArticleQuizScreen`,
  `PluralQuizScreen`, `TrueFalseScreen` (backend'in rastgele atadığı 5 review formatının tümü —
  Web C-05 ile birebir aynı 6 ekran seti, bkz. `TASK_C_web_app.md` C-05 notu),
  `LeechActionModal` (5 ardışık yanlıştan sonra — Askıya Al/Sıfırla/Devam Et),
  `SessionSummaryScreen` (+ "Aynı Kelimelerle Tekrar Et")
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator` içine `LearningStackNavigator`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `FlashcardScreen.test.tsx` (ipucu/zaman tavan kilidi), `learningSessionSlice.test.ts`,
  `HomeScreen.test.tsx` (streak yalnızca New'e bağlı), `SessionSummaryScreen.test.tsx` (repeat akışı)
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-08 — Kategoriler Ekranı ⬜
**Referans:** A-06, A-08 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §6, §8
- [ ] **Tip:** `Category`, `UserCategory` (`types/category.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `categoriesApi` — `getCategories`, `getUserCategories`, `createUserCategory`,
  `updateUserCategory`, `deleteUserCategory` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `CategoriesScreen` (sistem + kişisel sekmeleri [kart üzerinde kelime
  sayısı], kişisel sekmede düzenle/sil aksiyonu — bir sistem kategorisine dokunmak `DictionaryScreen`'e
  `categoryId` parametresiyle navigate eder, kategorinin kelimelerini KENDİSİ listelemez; bkz. D-08.1
  notu), `UserCategoryFormModal` (ekle **ve** düzenle ortak akışı)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `CategoriesScreen.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-08.1 — Sözlük (Bağımsız Gözat) Ekranı ⬜ ⚠️ **[2026-08-15 — yeni task, kullanıcı isteği]**
**Referans:** A-05, A-06 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §5, §6
> Öğrenme akışından (D-07) BAĞIMSIZ, salt-okunur bir keşif/referans ekranı — Web C-06.1 ile aynı
> veri şekli ve backend uçları (`GET /words`, `GET /categories`), Mobil'e özgü tek fark navigasyon
> (React Navigation) ve stil. Yeni endpoint **gerekmez**. **"Bir kategorinin kelimeleri" akışı da
> buradan geçer:** D-08'deki bir kategori kartına dokunmak `DictionaryScreen`'e `categoryId`
> parametresiyle navigate eder, `dictionaryFilterSlice`'a yazılır, `DictionaryFilterBar` o kategori
> önceden seçili açılır (`GET /categories/{id}/words` YOKTU/kaldırıldı, aynı sonuç
> `GET /words?categoryId=` ile buradan alınır).
- [ ] **Tip:** `Word` (Mobil'in kendi `types/word.ts`'i)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `wordsApi` — `getWords` (filtre+sayfa), `getWordById`, `getLanguages` (`GET /languages`),
  `userCardsApi.learnSystemWord` (`POST /user-cards/learn-system-word`, A-10 — "öğrenmeye başla"
  aksiyonu) (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Slice:** `dictionaryFilterSlice` — arama/level/categoryId/partOfSpeech/sayfa state
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `DictionaryScreen` (üst kapsayıcı), `DictionaryFilterBar` (arama +
  level/kategori/tür select), `DictionaryWordList` (sayfalı/sonsuz kaydırma, kompakt satır render),
  `DictionaryWordDetailScreen` (tıklanınca D-06'nın TAM `SystemWordCard`'ı + "Öğrenmeye Başla"
  butonu — `learnSystemWord` çağırır, zaten öğreniliyorsa buton devre dışı)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `DictionaryScreen.test.tsx` (filtre+sayfalama), `DictionaryFilterBar.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-09 — Kişisel Kartlar Ekranı ⬜
**Referans:** A-10 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §7
- [ ] **Tip:** `UserCard`, `UserCardFormValues` (`types/userCard.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `userCardsApi` — `getUserCards`, `createUserCard`, `updateUserCard`, `deleteUserCard`,
  `uploadCardImage` (`POST /user-cards/{id}/image`, A-10) (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `UserCardsScreen` (+ D-06 `PersonalCard`), `UserCardFormModal` (`expo-image-picker` ile görsel seçimi)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `UserCardFormModal.test.tsx` (duplikat uyarı akışı), `UserCardsScreen.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-10 — Sınıf Ekranı ⬜
**Referans:** A-15 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §12
- [ ] **Tip:** `ClassSummary`, `ClassDetail`, `ClassWord` (`types/class.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `classesApi` — `getClasses`, `createClass`, `joinClass`, `getClassDetail`, `getClassStatistics`,
  `addClassWord`, `updateClassWord`, `deleteClassWord`, `leaveClass`, `deleteClass`,
  `assignCategoryToClass` (`POST /classes/{id}/categories`·`/user-categories`, sahip), `learnClassWord`
  (`POST /classes/{id}/words/{wordConceptId}/learn`, üye — A-15) (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ClassListScreen`, `ClassDetailScreen` (üye+kelime+istatistik sekmeleri —
  sahip "sınıfı sil", üye "ayrıl" aksiyonu görür; kelime sekmesinde sahip için düzenle/sil, üye için
  "öğrenmeye başla" — `learnClassWord`), `JoinClassModal` (davet kodu), `ClassWordFormModal` (sınıf
  kelimesi ekle/düzenle), `ClassCategoryAssignModal` (sahip — sınıfa kategori atama)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `JoinClassModal.test.tsx`, `ClassDetailScreen.test.tsx` (sahip/üye görünürlük farkı)
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-11 — Arkadaş Ekranı ⬜
**Referans:** A-16 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §13
- [ ] **Tip:** `Friendship`, `FriendRequest` (`types/friend.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `friendsApi` — `getFriends`, `getFriendRequests`, `sendRequest`, `acceptRequest`,
  `rejectRequest`, `removeFriend` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `FriendsScreen` (arkadaş listesinde "kaldır" aksiyonu dahil), `SendFriendRequestModal`
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `FriendsScreen.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-12 — Paylaşım Linki Ekranı ⬜
**Referans:** A-14 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §14
- [ ] **Tip:** `SharedContentPreview` (`types/share.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `shareApi` — `createShareLink`, `getSharePreview`, `importSharedContent`, `deleteShareLink`
  (sahip — `ShareModal` linki oluşturduğu `shareToken`'ı zaten bildiği için ayrı bir "linklerim" listesi
  gerekmez) (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ShareModal` (link oluştur + "linki sil" aksiyonu), `SharePreviewScreen` (deep link ile açılır — anonim erişim)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** Deep link config (`app.json` scheme) + `RootNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `SharePreviewScreen.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-13 — İlerleme Ekranı ⬜
**Referans:** A-09 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §10
> **Not (2026-07-07 SRS tasarım kararları):** Bant eşiği `Mastery` yüzdesine göre (🔴 Zayıf 0-40 ·
> 🟡 Orta 40-70 · 🟢 İyi 70-100), `CurrentLevel` değil.
- [ ] **Tip:** `WordProgress`, `UserCardProgress`, `ProgressSummary` (`weak/medium/good/dueNow` sayıları),
  `Achievement`, `SuspendedWord` (`types/progress.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `progressApi` — `getWordProgress`, `getUserCardProgress`, `getProgressSummary`,
  `getBandWords`, `getSuspendedWords`, `applyLeechAction`, `getStatistics` (`GET /users/me/statistics?period=`,
  A-12 — Web C-11 ile aynı gerekçe), `getSessionHistory` (`GET /learning-sessions/history`, A-11 —
  Web C-11 ile aynı gerekçe), `achievementsApi` — `getMyAchievements` (axios + `useApiQuery`/`useApiMutation`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ProgressScreen` (mastery listesi, `getStatistics`'ten beslenen grafik +
  period seçici, bant kartları 🔴🟡🟢), `BandWordListScreen` (tıklanınca — **İncele** salt okunur liste,
  **Sına** butonu D-07 `mode: Band` oturumunu başlatır; leech kelimeler 🩹 işaretli), `SuspendedWordsScreen`
  (geri getir butonu), `SessionHistoryScreen` (`getSessionHistory`'den sayfalı/sonsuz kaydırma liste —
  tarih, mode, sonuç özeti), `AchievementsSection` (rozet grid'i, `Icon` resim URL'i + `Rarity` renk kodu)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `ProgressScreen.test.tsx` (bant kartı sayıları), `BandWordListScreen.test.tsx`
  (İncele/Sına geçişi), `SuspendedWordsScreen.test.tsx` (geri getir akışı), `SessionHistoryScreen.test.tsx`
  (sayfalama), `AchievementsSection.test.tsx` (rozet render)
- [ ] ➜ **AKADEMI/mobile'a işle**

### D-14 — Profil Ekranı ⬜
**Referans:** A-12, A-12.1, A-13, A-17 (`A_backend.md`), REFERENCE/API_ENDPOINTS.md §4
- [ ] **Tip:** `UserProfile`, `UpdateProfileRequest` (`currentLevel`/`themePreference` dahil), `Session`
  (`tokenFamily`/`deviceInfo`/`ipAddress`/`createdAt`/`isCurrent`) (`types/profile.ts`)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **API:** `profileApi` — `getProfile`, `updateProfile`, `uploadAvatar`, `changePassword`
  (`PUT /users/me/password`, A-12), `requestAccountDeletion`, `confirmAccountDeletion`,
  `updateDeviceToken` (OneSignal), `getSessions`, `revokeSession`, `revokeAllSessions`
  (axios + `useApiQuery`/`useApiMutation`); `authApi.logout` (D-05'te tanımlı, burada "Çıkış Yap" butonuna bağlanır)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Component (Ekran):** `ProfileScreen` (`expo-image-picker` ile avatar + tema değiştir seçici
  [Açık/Koyu/Sistem] + oturum sekmesi + "Çıkış Yap" butonu), `ChangePasswordModal`, `DeleteAccountModal`
  (OTP onaylı), `SessionsList` (aktif oturum satırları — `isCurrent` rozeti, tek oturum "çıkış yap"
  butonu, "diğer tüm cihazlardan çıkış" toplu aksiyonu — Web C-12 ile aynı component sözleşmesi)
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Route:** `MainTabNavigator`'a ekran kaydı
- [ ] ➜ **AKADEMI/mobile'a işle**
- [ ] **Birim testleri:** `ProfileScreen.test.tsx`, `DeleteAccountModal.test.tsx`, `SessionsList.test.tsx`
- [ ] ➜ **AKADEMI/mobile'a işle**
