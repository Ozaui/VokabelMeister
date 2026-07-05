# API ENDPOINTS

```
Base URL : https://api.wordlearner.com/api/v1   (dev: http://localhost:5001/api/v1)
Format   : JSON (UTF-8)   ·   Auth: JWT Bearer   ·   Versiyon: URL (/api/v1/)
```

**Rate limiting:** Login 5 hatalı/15dk → kilit · Genel 100/dk (auth), 10/dk (anonim).

## 1. Standart Yanıt

```json
// Başarılı
{ "success": true, "data": {}, "message": "İşlem başarılı", "timestamp": "2026-01-15T10:30:00Z" }
// Hatalı
{ "success": false, "error": { "code": "INVALID_CREDENTIALS", "message": "E-posta veya şifre hatalı" } }
```

**Hata mesajı dil desteği:** `error.code` sabit/dilden bağımsızdır (frontend bu koda göre özel
davranış tetikleyebilir); `error.message` ise **`Accept-Language` header'ına göre** değişir (`tr`
varsayılan, `en` destekleniyor — yeni dil eklemek `Application/Common/Localization/ErrorMessages.cs`
sözlüğüne bir sütun eklemekle olur). Bu yalnızca **istemciye giden** mesajdır; sunucu loglarındaki
(`ApplicationLog`) exception mesajı her zaman Türkçe kalır (`REFERENCE/CODING_STANDARDS.md §1`) —
ikisi birbirinden bağımsız kanallardır. Detay → `REFERENCE/SECURITY.md §1.4`.

## 2. HTTP Kodları ve Auth Seviyeleri

**HTTP kodları:** 200 OK · 201 Created · 204 No Content · 400 Geçersiz · 401 Kimlik yok ·
403 Yetkisiz · 404 Bulunamadı · 409 Çakışma · 429 Çok istek · 500 Sunucu.

**Auth seviyeleri:** `Anonim` (JWT gerekmez) · `[Authorize]` (JWT) · `[Authorize(Admin)]`.

---

## 3. Auth

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST | `/auth/register` | Anonim | Kayıt → e-posta doğrulama kodu gönderilir |
| POST | `/auth/verify-email` | Anonim | OTP ile e-posta doğrula |
| POST | `/auth/resend-verification` | Anonim | Doğrulama kodunu tekrar gönder |
| POST | `/auth/login` | Anonim | **Adım 1**: şifre doğrula → OTP gönder (token dönmez) |
| POST | `/auth/login/verify-otp` | Anonim | **Adım 2**: OTP doğrula → token döner |
| POST | `/auth/google` | Anonim | Google ID token ile giriş |
| POST | `/auth/apple` | Anonim | Apple identity token ile giriş |
| POST | `/auth/refresh` | Anonim | Refresh token → yeni access+refresh |
| POST | `/auth/logout` | [Authorize] | Refresh token iptal |
| POST | `/auth/forgot-password` | Anonim | Şifre sıfırlama OTP (kullanıcı yoksa bile 200) |
| POST | `/auth/reset-password` | Anonim | OTP + yeni şifre → tüm cihazlardan çıkış |
| POST | `/auth/delete-account/request` | [Authorize] | Hesap silme OTP (15dk) |
| POST | `/auth/delete-account/confirm` | [Authorize] | OTP + şifre → soft delete + 30 gün zamanla |

```json
// POST /auth/register
{ "email": "user@example.com", "password": "Sifre123!@#", "firstName": "Ayşe", "lastName": "Yılmaz" }
// → 201 { "id": 1, "email": "...", "firstName": "Ayşe", "currentLevel": "A1" }

// POST /auth/login → 200 { "message": "OTP gönderildi" }   (token YOK)
// POST /auth/login/verify-otp { "email": "...", "otpCode": "123456" }
// → 200 { "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900,
//         "user": { "id": 1, "currentLevel": "A1" }, "accountWasRecovered": false }
```

### 3.1 QR Kod ile Giriş

> Steam benzeri akış: web/masaüstü tarafı QR üretir, zaten mobilde giriş yapmış kullanıcı okutup
> onaylar. **Ayrı bir kimlik doğrulama mekanizması değildir** — onaylanınca `/auth/login/verify-otp`
> ile aynı `ITokenService` çalışır, aynı `RefreshTokens` tablosuna yazılır (bkz. `REFERENCE/SECURITY.md §1.3`).

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST | `/auth/qr/generate` | Anonim | Web/masaüstü çağırır → `{ qrToken, pairingCode, expiresIn }` |
| GET | `/auth/qr/{token}/status` | Anonim | Polling (~2sn); `Confirmed` ilk okunduğunda tek seferlik `{ accessToken, refreshToken, user }` döner, sonra `Consumed` |
| POST | `/auth/qr/{token}/scan` | [Authorize] | Mobil kamerayla okuyunca çağırır → `{ requesterDeviceInfo, requesterIp, pairingCode }` (onay ekranı için) |
| POST | `/auth/qr/{token}/confirm` | [Authorize] | Kullanıcı mobilde "Onayla" der |
| POST | `/auth/qr/{token}/deny` | [Authorize] | Kullanıcı mobilde "Reddet" der |

```json
// POST /auth/qr/generate → 200
{ "qrToken": "b64-rastgele-64byte", "pairingCode": "4821", "expiresIn": 120 }
// Web bu qrToken'ı bir deep-link'e gömüp QR görseline çevirir: vokabelmeister://qr-login?token=...

// POST /auth/qr/{token}/scan → 200 (mobil onay ekranını doldurur)
{ "requesterDeviceInfo": "Chrome 126 / Windows", "requesterIp": "88.12.34.56", "pairingCode": "4821" }
// Mobil ekranda "4821" gösterilir — kullanıcı web ekranındaki kodla KARŞILAŞTIRIR, eşleşmiyorsa reddeder.

// GET /auth/qr/{token}/status → 200 (Confirmed olduğunda, TEK SEFERLİK)
{ "status": "Confirmed", "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900,
  "user": { "id": 1, "currentLevel": "A1" } }
// Sonraki sorguda: 410 Gone (zaten tüketildi)
```

---

## 4. Kullanıcı (`/users/me`)

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/users/me` | Profil |
| PUT | `/users/me` | Güncelle (firstName, displayName, dailyWordGoal, **currentLevel**) |
| GET | `/users/me/statistics` | İstatistik (period=week\|month\|year) |
| POST | `/users/me/avatar` | Avatar yükle (multipart, max 5MB) |
| PUT | `/users/me/device-token` | OneSignal player id kaydet |
| DELETE | `/users/me` | Hesap sil (şifre onayı — OTP akışı `/auth/delete-account/*`) |

```json
// GET /users/me/statistics → 200
{ "totalWordsLearning": 45, "masteredWords": 12, "averageSuccessRate": 78.5, "streakDays": 5,
  "levelProgress": { "currentLevel": "A1", "xpInLevel": 250, "xpRequiredForNext": 500 } }
```

---

## 5. Sistem Kelimeleri

> **Çoklu dil:** Bir kelime (`WordConcept`) her zaman **tüm dilleriyle birlikte** oluşturulur/düzenlenir
> — `POST`/`PUT` gövdesinde `translations[]` dizisi olur (şu an `de`+`tr` birlikte gönderilir; ayrı ayrı
> "önce Almanca ekle" endpoint'i yoktur). İngilizce eklendiğinde `translations[]`'a yeni bir eleman
> (`languageCode: "en"`) eklenerek mevcut kelime `PUT` ile güncellenir. Bkz. `DATABASE_SCHEMA/Icerik.md`.

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| GET | `/words` | [Authorize] | Liste (level, categoryId, partOfSpeech, search, page, pageSize) |
| GET | `/words/{id}` | [Authorize] | Detay (tüm dillerdeki `translations` + her birinin WordDetail/örnekleri) |
| POST | `/words` | Admin | Oluştur (`WordConcept` + `translations[]` tek istekte; aynı dilde `Text` varsa 409, `?force=true` ile geç) |
| PUT | `/words/{id}` | Admin | Güncelle (`translations[]` — mevcut diller güncellenir, yeni dil eklenirse o dile yeni satır açılır) |
| DELETE | `/words/{id}` | Admin | Soft delete (`WordConcept` + tüm dillerdeki `Words` satırları) |

```json
// POST /words
{ "partOfSpeech": "Noun", "difficultyLevel": "A1", "imageUrl": "...", "categoryIds": [1],
  "translations": [
    { "languageCode": "de", "text": "Mann",
      "wordDetail": { "grammarData": { "gender": "Masculine", "articleDefiniteNom": "der", "pluralForm": "Männer" } },
      "examples": [ { "sentenceText": "Der Mann ist hier.", "level": "A1" } ] },
    { "languageCode": "tr", "text": "Erkek",
      "examples": [ { "sentenceText": "Adam burada.", "level": "A1" } ] }
  ] }

// GET /words → 200
{ "data": [ { "wordConceptId": 1, "partOfSpeech": "Noun", "difficultyLevel": "A1",
  "translations": [
    { "languageCode": "de", "text": "Mann", "wordDetail": { "grammarData": { "gender": "Masculine", "articleDefiniteNom": "der", "pluralForm": "Männer" } } },
    { "languageCode": "tr", "text": "Erkek" }
  ],
  "categories": [ { "id": 1, "name": "İnsanlar" } ],
  "userProgress": { "currentLevel": 2, "successRate": 75.0, "nextReviewAt": "2026-01-18T10:00:00Z" } } ],
  "pagination": { "currentPage": 1, "totalPages": 10, "totalItems": 200 } }
```

---

## 6. Kategoriler

> Kategori adı da çoklu dile açık: `Categories` (çekirdek) + `CategoryTranslations` (dil başına ad).
> `POST`/`PUT` gövdesinde aynı şekilde `translations: [{languageCode, name}, ...]` gönderilir.

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| GET | `/categories` | [Authorize] | Hiyerarşik liste (level, includeWordCount) |
| GET | `/categories/{id}/words` | [Authorize] | Kategoriye ait kelimeler (sayfalı) |
| POST/PUT/DELETE | `/categories[/{id}]` | Admin | CRUD (`translations[]` ile; alt kategori/aktif kelime varsa silme 409) |

---

## 7. Kişisel Kartlar

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/user-cards` | Liste (categoryId, userCategoryId, search, page) — yalnızca sahibi |
| GET | `/user-cards/{id}` | Detay (sahibi) |
| POST | `/user-cards` | Oluştur (duplikat 409 + `?force=true`; sistem eşleşmesinde `suggestedSystemWordId`) |
| PUT/DELETE | `/user-cards/{id}` | Güncelle / soft delete (sahibi) |
| POST | `/user-cards/learn-system-word` | `{ "wordId": 5 }` → **UserProgress** açar, UserCard OLUŞTURMAZ |

```json
// POST /user-cards/learn-system-word → 200
{ "userProgressId": 12, "wordId": 5, "germanWord": "laufen", "alreadyExists": false }
```

---

## 8. Kişisel Kategoriler

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/user-categories` | Liste (cardCount dahil) |
| POST | `/user-categories` | Oluştur `{ name, color, icon }` |
| PUT/DELETE | `/user-categories/{id}` | Güncelle / sil (sahibi) |

---

## 9. Öğrenme / Sınav

| Metot | Yol | Açıklama |
|-------|-----|----------|
| POST | `/learning-sessions` | Oturum başlat |
| POST | `/learning-sessions/{id}/answer` | Cevap işle |
| POST | `/learning-sessions/{id}/complete` | Tamamla (XP + rozet) |
| POST | `/learning-sessions/{id}/abandon` | Bırak |
| GET | `/learning-sessions/history` | Geçmiş (sayfalı) |

```json
// POST /learning-sessions
{ "sessionType": "MultipleChoice", "sourceType": "Mixed", "levelFilter": "A1",
  "categoryIds": [1,3], "userCategoryIds": [1], "wordCount": 10 }
// Mixed: UserProgress + UserCardProgress sorgulanır; FrontText==Words.Text eşleşmesinde UserCard atlanır.
// → 201 { "id": 1, "status": "Active", "items": [...], "totalItems": 10 }

// POST /learning-sessions/{id}/answer
{ "itemId": 1, "itemType": "SystemWord", "wordId": 5, "selfRating": 4, "timeSpentSeconds": 4 }
// selfRating (SM-2 quality): 0=Bilmedim 2=Zor 4=İyi 5=Çok Kolay
// → 200 { "feedback": {...}, "xpEarned": 10, "progress": { "currentLevel": 2, "nextReviewAt": "..." } }
```

---

## 10. İlerleme

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/words/{wordId}/progress` | Sistem kelimesi ilerlemesi |
| GET | `/user-cards/{cardId}/progress` | Kişisel kart ilerlemesi (sahibi) |

---

## 11. Admin (`[Authorize(Admin)]`)

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/admin/users` | Liste (page, search, role) |
| GET | `/admin/users/{id}` | Detay + istatistik |
| PUT | `/admin/users/{id}/role` | `{ "role": "Admin" }` |
| PUT | `/admin/users/{id}/status` | `{ "isActive": false, "reason": "..." }` |
| GET | `/admin/user-cards` | Moderasyon: kart listesi |
| DELETE | `/admin/user-cards/{id}` | Kart sil |
| GET | `/admin/statistics` | Genel istatistik |
| POST | `/admin/words/import` | Toplu kelime ekleme (JSON array) |
| GET/PUT | `/admin/smtp-settings` | SMTP ayarları (şifre `***` maskeli) |
| POST | `/admin/smtp-settings/test` | Test e-postası gönder |

### 11.1 Log Görüntüleme (admin panel B-08)

| Metot | Yol | Filtre query |
|-------|-----|--------------|
| GET | `/admin/logs/activity` | userId, action, entityType, from, to, page, pageSize |
| GET | `/admin/logs/application` | level, from, to, search, page, pageSize |
| GET | `/admin/logs/security` | eventType, ip, from, to, page, pageSize |

```json
// GET /admin/logs/activity?action=CHANGE_ROLE&page=1 → 200
{ "data": [ { "id": 1024, "userId": 3, "actorRole": "Admin", "action": "CHANGE_ROLE",
  "entityType": "User", "entityId": 87, "oldValue": "{\"role\":\"User\"}", "newValue": "{\"role\":\"Admin\"}",
  "ipAddress": "1.2.3.4", "createdAt": "2026-06-25T09:00:00Z" } ],
  "pagination": { "currentPage": 1, "totalPages": 5, "totalItems": 92 } }

// GET /admin/logs/security?eventType=LoginFailed → 200
{ "data": [ { "id": 5, "eventType": "LoginFailed", "emailHash": "a1b2c3d4",
  "ipAddress": "1.2.3.4", "detail": "Hatalı şifre (3. deneme)", "createdAt": "..." } ], "pagination": {...} }
```

---

## 12. Sınıflar

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST | `/classes` | [Authorize] | Oluştur → davet kodu döner |
| GET | `/classes` | [Authorize] | Üye + sahibi olunan sınıflar |
| GET | `/classes/{id}` | Sınıf üyesi | Detay (kategori, üye, kelime) |
| POST | `/classes/join` | [Authorize] | `{ "inviteCode": "ABC123" }` |
| POST | `/classes/{id}/categories` | Sahip | Sistem kategorisi ekle |
| POST | `/classes/{id}/user-categories` | Sahip | Kişisel kategori ekle |
| GET | `/classes/{id}/statistics` | Sahip | Üye başarı oranları, zor kelimeler |
| DELETE | `/classes/{id}` | Sahip | Sınıfı sil |
| DELETE | `/classes/{id}/leave` | Üye | Sınıftan ayrıl |
| POST | `/classes/{id}/words` | Sahip | Sınıfa özel kelime (duplikat 409, sistem eşleşmesinde uyarı) |
| GET | `/classes/{id}/words` | Üye | Sınıf kelimeleri (sayfalı) |
| PUT/DELETE | `/classes/{id}/words/{wordId}` | Sahip | Güncelle / soft delete |

---

## 13. Arkadaşlar

| Metot | Yol | Açıklama |
|-------|-----|----------|
| POST | `/friends/request` | `{ "targetUserId": 15 }` veya `{ "targetEmail": "..." }` |
| GET | `/friends/requests` | Gelen + giden istekler |
| PUT | `/friends/requests/{id}/accept` · `/reject` | Kabul / reddet |
| GET | `/friends` | Kabul edilmiş arkadaşlar |
| DELETE | `/friends/{userId}` | Arkadaşı kaldır |

---

## 14. Paylaşım

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST | `/shared-contents` | [Authorize] | Link oluştur `{ contentType, contentId, expiresAt }` → ShareToken |
| GET | `/shared-contents/{token}` | **Anonim** | Önizleme (giriş gerekmez) |
| POST | `/shared-contents/{token}/import` | [Authorize] | Kendi listene kopyala |
| DELETE | `/shared-contents/{token}` | İçerik sahibi | Link sil |

```json
// POST /shared-contents → 201
{ "shareToken": "550e8400-...", "shareUrl": "https://app.wordlearner.com/share/550e8400-..." }
```
