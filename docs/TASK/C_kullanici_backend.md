# FAZ C — Kullanıcı Backend (Web + Mobil Ortak API)

> **Yöntem/standart:** Her task = bir API'ı dikey dilim olarak bitir + `API_YOL_HARITASI/` rehberine
> işle. Kurallar için → `../TASK.md` (**⭐ Çalışma Yöntemi**, **Her Parça İçin Döngü**) — o bölümler
> değişmez standarttır, burada tekrar edilmez.

### C-01 — User Profil API (`/users/me`) ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §4
**Frontend karşılığı:** D-12 (Web — Profil Sayfası), E-14 (Mobil — Profil Ekranı)
> 🧩 `frontendRefs` ↔ D-12/E-14 `backendRef` (iki yönlü).
- [ ] `UserController`: `GET /users/me`, `PUT /users/me` (CurrentLevel, **ThemePreference** dahil), `GET /users/me/statistics`, `DELETE /users/me`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserServiceTests` (profil güncelleme, istatistik hesaplama)
- [ ] ➜ **API Yol Haritası'na işle**
> **Not (A-03.3, `ThemePreference`):** `PUT /users/me` bu görevin **gerçek toplama noktası** —
> `LevelSelectPage`/`LevelSelectScreen` (D-03/E-05) kayıt sonrası ilk-login onboarding'inde
> kullanıcının seçtiği `CurrentLevel` ile birlikte `ThemePreference`'ı da (`Light|Dark|System`)
> buraya gönderir. Validator'a `RuleFor(x => x.ThemePreference)` eklenmeli — izin verilen
> değerler dışında bir şey gelirse `WithErrorCode("INVALID_THEME_PREFERENCE")` → `ErrorMessages.cs`'e
> (tr/de) o zaman eklenir (`RegisterCommandValidator` ile birebir aynı desen). DB `CK_Users_
> ThemePreference` zaten son savunma hattı olarak var (A-03.3'te eklendi).

### C-02 — Kişisel Kategori API ⬜
**Frontend karşılığı:** D-06 (Web — Kategoriler Sayfası, kişisel kategoriler sekmesi), E-08 (Mobil — Kategoriler Ekranı)
> 🧩 `frontendRefs` ↔ D-06/E-08 `backendRef` (iki yönlü).
> **Not:** Sıra değişti (eski C-03). C-04'ün ihtiyaç duyduğu `UserCategory` entity'si önce hazır
> olmalı (`UserCardUserCategories` ara tablosu buna FK verir) → dikey dilim bütünlüğü için öne çekildi.
- [ ] **Entity:** `UserCategory` + migration, `IUserCategoryService` + `UserCategoryController` (yalnızca sahibi)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserCategoryServiceTests` (sahiplik filtresi, CRUD)
- [ ] ➜ **API Yol Haritası'na işle**

### C-03 — SRS / İlerleme API (UserProgress) ⬜
**Referans:** REFERENCE/TECHNICAL_SPECIFICATIONS.md §8
**Frontend karşılığı:** D-11 (Web — İlerleme Sayfası), E-13 (Mobil — İlerleme Ekranı) — ayrıca D-05/E-07
(Öğrenme/Sınav) bu API'nin sonuçlarını dolaylı kullanır (bkz. C-05)
> 🧩 `frontendRefs` ↔ D-11/E-13 `backendRef` (iki yönlü).
> **Not:** Sıra değişti (eski C-04). `POST /user-cards/learn-system-word` (C-04'te yazılacak) bu
> entity'yi (`UserProgress`) kullanır; o yüzden Kişisel Kart API'sından **önce** bitirilmesi gerekir.
- [ ] **Entity:** `UserProgress`, `UserCardProgress` (`NextReviewAt` **nullable** — NULL=yeni kelime
  havuzu, + `ConsecutiveIncorrect`/`IsSuspended` leech alanları), `LearningHistory` (+ `HintUsed`/
  `IsExtraPractice`/`MasteryBefore`/`MasteryAfter` alanları), `Achievements`/`UserAchievements` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `SrsCalculator` (SM-2: interval, easiness factor, mastery 0-5 + `CalculateMastery` yüzdelik formülü)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `SrsCalculatorTests` (quality<3 sıfırlama, EF alt sınır 1.3, interval hesapları, Mastery formülü)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IProgressService` + `ProgressService` (XP, streak **yalnızca günlük yeni kelime hedefine bağlı**,
  Mastery bantları Zayıf/Orta/İyi 0-40/40-70/70-100, yeni kelime seçim sorgusu — `DifficultyLevel` +
  `WordConceptId ASC` + `NextReviewAt IS NULL`, leech tespiti `ConsecutiveIncorrect>=5` →
  Suspend/Reset/Continue aksiyonları), `ProgressController` (`GET /progress/summary`,
  `GET /progress/words`, `GET /progress/suspended`, `POST /words/{id}/leech-action`,
  `POST /user-cards/{id}/leech-action`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IAchievementService` + `AchievementService` (başlangıç seti: streak 3/7/30, kelime sayısı
  50/200/500, ilk `CurrentLevel=5`, 100 kelime İyi bantta, hatasız oturum, leech kurtarma —
  tetikleme `ProgressService`/`LearningSessionService` sonrası basit kural kontrolü),
  `GET /achievements/me` (seed data migration ile, admin CRUD yok — YAGNI)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `ProgressServiceTests` (XP/streak güncelleme, NextReviewAt hesaplama, bant
  eşikleri, yeni kelime seçim sorgusu — sıfırlanan kelimenin geri dönmesi, leech eşiği/aksiyonları),
  `AchievementServiceTests` (her kural için tetiklenme senaryosu)
- [ ] ➜ **API Yol Haritası'na işle**

### C-04 — Kişisel Kart API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §7
**Frontend karşılığı:** D-07 (Web — Kişisel Kartlar Sayfası), E-09 (Mobil — Kişisel Kartlar Ekranı)
> 🧩 `frontendRefs` ↔ D-07/E-09 `backendRef` (iki yönlü).
> **Not:** Sıra değişti (eski C-02). `UserCategory` (C-02) ve `UserProgress` (C-03) artık hazır;
> bu sayede aşağıdaki entity/endpoint'ler **eksiksiz** tek seferde yazılabilir (dikey dilim bozulmaz).
- [ ] **Entity:** `UserCard`, `UserCardExample` + ara tablolar (`UserCardCategory`, `UserCardUserCategory`) + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IUserCardService` + `UserCardService` (liste/detay/CRUD — yalnızca sahibi, UserId filtresi zorunlu)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Duplikat uyarısı (409 + `?force=true`), sistem kelimesi eşleşme uyarısı (`suggestedSystemWordId`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `POST /user-cards/learn-system-word` → UserCard değil **UserProgress** açar, `UserCardController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `UserCardServiceTests` (sahiplik filtresi, duplikat 409, learn-system-word akışı)
- [ ] ➜ **API Yol Haritası'na işle**

### C-05 — Öğrenme / Sınav API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §9
**Frontend karşılığı:** D-05 (Web — Öğrenme/Sınav Sayfası), E-07 (Mobil — Öğrenme/Sınav Ekranı)
> 🧩 `frontendRefs` ↔ D-05/E-07 `backendRef` (iki yönlü).
- [ ] **Entity:** `LearningSession` (+ `TrueFalse` dahil 6 `SessionType`) + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ILearningSessionService` + `LearningSessionService` (başlat — `mode: New|Due|Band|Mixed`,
  kelime seçim önceliği, Mixed dedup —, her review sorusu için rastgele format seçimi
  [`sessionType` istemciden gelmez], ipucu → quality tavanı düşürme, cevap işleme
  [Flashcard=selfRating, objektif tipler=otomatik quality, TrueFalse max tavan 4], "günde tek
  resmi review" kuralı [`IsExtraPractice`], tamamla, bırak, `repeat` [aynı kelimelerle SM-2
  güncellemeden tekrar])
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `LearningSessionController` (+ `GET /learning-history/today/learned`, `GET /learning-history/today/tested`)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `LearningSessionServiceTests` (Mixed dedup, SRS önceliği, tamamla/bırak,
  rastgele format ataması, ipucu/zaman bazlı quality tavanı, TrueFalse tavanı, repeat'in SM-2'yi
  etkilememesi)
- [ ] ➜ **API Yol Haritası'na işle**

### C-06 — Paylaşım API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §14
**Frontend karşılığı:** D-10 (Web — Paylaşım Linki Sayfası), E-12 (Mobil — Paylaşım Linki Ekranı)
> 🧩 `frontendRefs` ↔ D-10/E-12 `backendRef` (iki yönlü).
- [ ] **Entity:** `SharedContent`, `SharedContentImport` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IShareService` + `ShareService` (UUID link, anonim önizleme, listene kopyala, sil), `SharedContentController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `ShareServiceTests` (link üretimi, expiresAt kontrolü, anonim önizleme)
- [ ] ➜ **API Yol Haritası'na işle**

### C-07 — Sınıf API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §12
**Frontend karşılığı:** D-08 (Web — Sınıf Sayfası), E-10 (Mobil — Sınıf Ekranı)
> 🧩 `frontendRefs` ↔ D-08/E-10 `backendRef` (iki yönlü).
- [ ] **Entity:** `Class`, `ClassMembership`, `ClassWord`, `ClassCategory`, `ClassUserCategory` + migration
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IClassService` + `ClassService` (oluştur+davet kodu, katıl, kategori ekle, istatistik, ayrıl/sil)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `IClassWordService` + `ClassWordService` (yalnızca sahibi ekler/düzenler/siler, üyeler görür; duplikat + sistem uyarısı)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] `ClassController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `ClassServiceTests` (davet kodu, katılım, sahiplik), `ClassWordServiceTests` (üye görünürlüğü)
- [ ] ➜ **API Yol Haritası'na işle**

### C-08 — Arkadaş API ⬜
**Referans:** REFERENCE/API_ENDPOINTS.md §13
**Frontend karşılığı:** D-09 (Web — Arkadaş Sayfası), E-11 (Mobil — Arkadaş Ekranı)
> 🧩 `frontendRefs` ↔ D-09/E-11 `backendRef` (iki yönlü).
- [ ] **Entity:** `Friendship` + migration, `IFriendshipService` + `FriendshipService`, `FriendshipController`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `FriendshipServiceTests` (istek/kabul/reddet, self-friendship engeli)
- [ ] ➜ **API Yol Haritası'na işle**

### C-09 — Avatar Yükleme API ⬜
**Frontend karşılığı:** D-12 (Web — Profil Sayfası, avatar yükleme), E-14 (Mobil — Profil Ekranı, avatar yükleme)
> 🧩 `frontendRefs` ↔ D-12/E-14 `backendRef` (iki yönlü).
- [ ] `POST /users/me/avatar` (multipart, max 5MB, jpg/png/webp, benzersiz ad, eski avatar silinir)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** boyut/uzantı reddi, eski dosyanın silindiğinin doğrulanması
- [ ] ➜ **API Yol Haritası'na işle**

### C-10 — Push Notification (OneSignal) ⬜
**Referans:** REFERENCE/ENV.md §6, REFERENCE/TECHNICAL_SPECIFICATIONS.md §1 (Hangfire)
**Frontend karşılığı:** E-14 (Mobil — Profil Ekranı, device token kaydı; Web'de push yok)
> 🧩 `frontendRefs` ↔ E-14 `backendRef` (iki yönlü).
> **Not (2026-07-07):** Zamanlama altyapısı **Hangfire** (SQL Server storage) — Quartz.NET/elle
> `IHostedService` yerine tercih edildi (gerekçe → `wiki/Database/SRS_Domain.md` "Bildirim
> Tetikleyicileri (C-10)").
- [ ] `INotificationService` + `OneSignalNotificationService`, `User.OneSignalPlayerId` + migration, `PUT /users/me/device-token`
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] Hangfire kurulumu (`AddHangfire`/`AddHangfireServer`, SQL Server storage, dashboard) +
  recurring job'lar: günlük hatırlatma (hedef tamamlanmadıysa, config saat), due hatırlatması
  (due sayısı eşiği geçince günde 1 kez), streak riski (gün sonuna yaklaşırken hedef eksikse);
  achievement bildirimi event-driven (`AchievementService` tetikleyince anlık)
- [ ] ➜ **API Yol Haritası'na işle**
- [ ] **Birim testleri:** `OneSignalNotificationServiceTests` (HTTP client mock'lanır, hata yönetimi),
  `NotificationTriggerJobTests` (her tetikleyici koşulunun doğru kullanıcıları seçmesi)
- [ ] ➜ **API Yol Haritası'na işle**
