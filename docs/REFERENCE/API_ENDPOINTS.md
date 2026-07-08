# API ENDPOINTS

```
Base : https://api.wordlearner.com/api/v1  (dev: http://localhost:5001/api/v1)
JSON (UTF-8) · Auth: JWT Bearer · Rate limit: Login 5/15dk → kilit; 100/dk (auth), 10/dk (anonim)
```

## 1. Standart Yanıt

```json
{ "success": true, "data": {}, "message": "İşlem başarılı", "timestamp": "..." }
{ "success": false, "error": { "code": "INVALID_CREDENTIALS", "message": "E-posta veya şifre hatalı" } }
```
`error.code` sabit/dilden bağımsız; `error.message` `Accept-Language`'a göre (tr varsayılan, en). Detay → `SECURITY.md §1.4`.

**HTTP:** 200/201/204 · 400 Geçersiz · 401 Kimlik yok · 403 Yetkisiz · 404 Bulunamadı · 409 Çakışma · 429 Çok istek · 500.
**Auth seviyeleri:** `Anonim` · `[Authorize]` · `[Authorize(Admin)]`.

## 3. Auth

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST | `/auth/register` | Anonim | Kayıt → e-posta doğrulama kodu |
| POST | `/auth/verify-email` | Anonim | OTP ile e-posta doğrula |
| POST | `/auth/resend-verification` | Anonim | Doğrulama kodunu tekrar gönder |
| POST | `/auth/login` | Anonim | Adım 1: şifre doğrula → OTP (token dönmez) |
| POST | `/auth/login/verify-otp` | Anonim | Adım 2: OTP → token |
| POST | `/auth/google` · `/auth/apple` | Anonim | Sosyal giriş (ID/identity token) |
| POST | `/auth/refresh` | Anonim | Refresh → yeni access+refresh |
| POST | `/auth/logout` | [Authorize] | Refresh iptal |
| POST | `/auth/forgot-password` | Anonim | Sıfırlama OTP (kullanıcı yoksa bile 200) |
| POST | `/auth/reset-password` | Anonim | OTP + yeni şifre → tüm cihazlardan çıkış |
| POST | `/auth/delete-account/request` · `/confirm` | [Authorize] | Hesap silme OTP → soft delete + 30 gün |

```json
// POST /auth/register → 201 { "id": 1, "email": "...", "firstName": "Ayşe", "currentLevel": "A1" }
// POST /auth/login → 200 { "message": "OTP gönderildi" }   (token YOK)
// POST /auth/login/verify-otp → 200
{ "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900,
  "user": { "id": 1, "currentLevel": "A1" }, "accountWasRecovered": false }
```

### 3.1 QR ile Giriş

Akış → `SECURITY.md §1.3`. Onaylanınca `/auth/login/verify-otp` ile aynı token akışı.

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST | `/auth/qr/generate` | Anonim | Web çağırır → `{ qrToken, pairingCode, expiresIn }` |
| GET | `/auth/qr/{token}/status` | Anonim | Polling; Confirmed'de tek seferlik token, sonra Consumed (410) |
| POST | `/auth/qr/{token}/scan` | [Authorize] | Mobil okuyunca → `{ requesterDeviceInfo, requesterIp, pairingCode }` |
| POST | `/auth/qr/{token}/confirm` · `/deny` | [Authorize] | Onayla / reddet |

```json
// POST /auth/qr/generate → { "qrToken": "b64-64byte", "pairingCode": "4821", "expiresIn": 120 }
// GET /auth/qr/{token}/status → (Confirmed, TEK SEFERLİK)
{ "status": "Confirmed", "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900, "user": {...} }
```

## 4. Kullanıcı (`/users/me`)

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET/PUT | `/users/me` | Profil / güncelle (firstName, displayName, dailyWordGoal, **currentLevel**) |
| GET | `/users/me/statistics` | period=week\|month\|year |
| POST | `/users/me/avatar` | Avatar (multipart, max 5MB) |
| PUT | `/users/me/device-token` | OneSignal player id |
| DELETE | `/users/me` | Hesap sil (OTP akışı `/auth/delete-account/*`) |

```json
// GET /users/me/statistics → { "totalWordsLearning": 45, "masteredWords": 12, "averageSuccessRate": 78.5,
//   "streakDays": 5, "levelProgress": { "currentLevel": "A1", "xpInLevel": 250, "xpRequiredForNext": 500 } }
```

## 5. Sistem Kelimeleri

> Bir `WordConcept` her zaman **tüm dilleriyle birlikte** oluşturulur/düzenlenir — `POST/PUT` gövdesinde `translations[]` (şu an de+tr). Bkz. `DATABASE_SCHEMA/Icerik.md`.

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| GET | `/words` | [Authorize] | Liste (level, categoryId, partOfSpeech, search, page, pageSize) |
| GET | `/words/{id}` | [Authorize] | Detay (tüm diller `translations` + WordDetail/örnekler) |
| POST | `/words` | Admin | `WordConcept` + `translations[]` tek istekte (aynı dilde Text varsa 409, `?force=true`) |
| PUT | `/words/{id}` | Admin | `translations[]` güncelle / yeni dil ekle |
| DELETE | `/words/{id}` | Admin | Soft delete (kavram + tüm diller) |

```json
// POST /words
{ "partOfSpeech": "Noun", "difficultyLevel": "A1", "imageUrl": "...", "categoryIds": [1],
  "translations": [
    { "languageCode": "de", "text": "Mann",
      "wordDetail": { "grammarData": { "gender": "Masculine", "articleDefiniteNom": "der", "pluralForm": "Männer" } },
      "examples": [ { "sentenceText": "Der Mann ist hier.", "level": "A1" } ] },
    { "languageCode": "tr", "text": "Erkek", "examples": [ { "sentenceText": "Adam burada.", "level": "A1" } ] }
  ] }
// GET /words → data[].{ wordConceptId, partOfSpeech, difficultyLevel, translations[], categories[], userProgress }
//   + pagination { currentPage, totalPages, totalItems }
```

## 6. Kategoriler

> `POST/PUT` gövdesinde `translations: [{languageCode, name}, ...]`.

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| GET | `/categories` | [Authorize] | Hiyerarşik (level, includeWordCount) |
| GET | `/categories/{id}/words` | [Authorize] | Kategorinin kelimeleri (sayfalı) |
| POST/PUT/DELETE | `/categories[/{id}]` | Admin | CRUD (alt kategori/aktif kelime varsa silme 409) |

## 7. Kişisel Kartlar

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/user-cards` | Liste (categoryId, userCategoryId, search, page) — sahibi |
| GET/PUT/DELETE | `/user-cards/{id}` | Detay / güncelle / soft delete (sahibi) |
| POST | `/user-cards` | Oluştur (duplikat 409 + `?force=true`; sistem eşleşmesinde `suggestedSystemWordId`) |
| POST | `/user-cards/learn-system-word` | `{ "wordId": 5 }` → **UserProgress** açar, UserCard OLUŞTURMAZ |

```json
// POST /user-cards/learn-system-word → { "userProgressId": 12, "wordId": 5, "germanWord": "laufen", "alreadyExists": false }
```

## 8. Kişisel Kategoriler

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/user-categories` | Liste (cardCount dahil) |
| POST | `/user-categories` | `{ name, color, icon }` |
| PUT/DELETE | `/user-categories/{id}` | Güncelle / sil (sahibi) |

## 9. Öğrenme / Sınav

> İstemci `sessionType` göndermez. Yeni kelime = Flashcard; review'da her soru backend'de rastgele format (bkz. `TECHNICAL_SPECIFICATIONS.md §8`).

| Metot | Yol | Açıklama |
|-------|-----|----------|
| POST | `/learning-sessions` | Oturum başlat (`mode` bazlı) |
| POST | `/learning-sessions/{id}/answer` | Cevap işle |
| POST | `/learning-sessions/{id}/hint` | İpucu (örnek/şık eleme/ilk harf) — `quality` tavanını düşürür |
| POST | `/learning-sessions/{id}/complete` · `/abandon` | Tamamla (XP+rozet) / bırak |
| POST | `/learning-sessions/{id}/repeat` | Aynı listeyle yeni oturum (`IsExtraPractice=true`, SM-2 güncellenmez) |
| GET | `/learning-sessions/history` | Geçmiş (sayfalı) |

```json
// POST /learning-sessions — mode: New|Due|Band|Mixed
{ "mode": "New", "sourceType": "Mixed" }   // günlük yeni kelime, sayı dailyWordGoal'e sabit, Flashcard
{ "mode": "Due", "sourceType": "Mixed", "wordCount": 20 }   // NextReviewAt<=now; her soru rastgele format
{ "mode": "Band", "band": "Weak", "sourceType": "Mixed", "levelFilter": "A1",
  "categoryIds": [1,3], "userCategoryIds": [1], "wordCount": 10 }
//   → bant pratik (Weak|Medium|Good), günlük hedefe saymaz, resmi review (SM-2 günceller)
//   Mixed: UserProgress+UserCardProgress; FrontText==Words.Text eşleşmesinde UserCard atlanır
// → 201 { "id": 1, "status": "Active", "items": [...], "totalItems": 10 }

// POST /learning-sessions/{id}/hint { "itemId": 1 } → { "hint": "...", "qualityCap": 4 }

// POST /learning-sessions/{id}/answer
// Flashcard: kullanıcı selfRating seçer (gecikme/ipucu tavanı düşürmüşse UI'da kapalı)
{ "itemId": 1, "itemType": "SystemWord", "wordId": 5, "selfRating": 4, "timeSpentSeconds": 4, "hintUsed": false }
// Objektif tipler: selfRating gönderilmez, quality sunucuda isCorrect+responseTime+hintUsed'dan hesaplanır
{ "itemId": 2, "itemType": "SystemWord", "wordId": 8, "userAnswer": "die Katze", "timeSpentSeconds": 6, "hintUsed": true }
// selfRating (SM-2): 0=Bilmedim 2=Zor 4=İyi 5=Çok Kolay (TrueFalse doğru tavanı max 4)
// → 200 { "feedback": {...}, "xpEarned": 10, "progress": {...}, "leechDetected": false }
// leechDetected=true (consecutiveIncorrect>=5) → POST /words|user-cards/{id}/leech-action beklenir

// POST /learning-sessions/{id}/repeat → 201 (aynı kelimeler; her answer IsExtraPractice=true, SM-2/Mastery güncellenmez)
```

## 10. İlerleme

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/words/{wordId}/progress` · `/user-cards/{cardId}/progress` | İlerleme |
| GET | `/progress/summary` | Bant sayıları + due (ana ekran rozetleri) |
| GET | `/progress/words` | Bant bazlı liste (İncele ekranı, quiz'siz) |
| GET | `/progress/suspended` | Askıya alınmış (leech) kelimeler |
| GET | `/learning-history/today/learned` · `/tested` | Bugün öğrenilen / test edilen (`masteryBefore`→`After`) |
| POST | `/words/{wordId}/leech-action` · `/user-cards/{cardId}/leech-action` | Leech aksiyonu |
| GET | `/achievements/me` | Kazanılan rozetler |

```json
// GET /progress/summary → { "weak": 12, "medium": 34, "good": 50, "dueNow": 14 }
// POST /words/{wordId}/leech-action { "action": "Suspend" }   // Suspend|Reset|Continue
//   Suspend → { "isSuspended": true } · Reset → { "currentLevel": 0, "nextReviewAt": null } · Continue → { "acknowledged": true }
// today/learned: currentLevel/mastery YOK (bilinçli). today/tested: IsExtraPractice=1 girmez.
```

## 11. Admin (`[Authorize(Admin)]`)

| Metot | Yol | Açıklama |
|-------|-----|----------|
| GET | `/admin/users` · `/admin/users/{id}` | Liste (page, search, role) / detay+istatistik |
| PUT | `/admin/users/{id}/role` · `/status` | `{ role }` / `{ isActive, reason }` |
| GET/DELETE | `/admin/user-cards[/{id}]` | Moderasyon: liste / sil |
| GET | `/admin/statistics` | Genel istatistik |
| POST | `/admin/words/import` | Toplu kelime (JSON array) |
| GET/PUT | `/admin/smtp-settings` | Şifre `***` maskeli |
| POST | `/admin/smtp-settings/test` | Test e-postası |

### 11.1 Log Görüntüleme (B-08)

| Metot | Yol | Filtre |
|-------|-----|--------|
| GET | `/admin/logs/activity` | userId, action, entityType, from, to, page, pageSize |
| GET | `/admin/logs/application` | level, from, to, search, page, pageSize |
| GET | `/admin/logs/security` | eventType, ip, from, to, page, pageSize |

## 12. Sınıflar

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST/GET | `/classes` | [Authorize] | Oluştur (davet kodu) / üye+sahibi olunanlar |
| GET | `/classes/{id}` | Üye | Detay (kategori, üye, kelime) |
| POST | `/classes/join` | [Authorize] | `{ inviteCode }` |
| POST | `/classes/{id}/categories` · `/user-categories` | Sahip | Kategori ata |
| GET | `/classes/{id}/statistics` | Sahip | Üye başarı, zor kelimeler |
| DELETE | `/classes/{id}` · `/leave` | Sahip / Üye | Sil / ayrıl |
| POST/GET | `/classes/{id}/words` | Sahip / Üye | Sınıf kelimesi ekle (duplikat 409) / liste |
| PUT/DELETE | `/classes/{id}/words/{wordId}` | Sahip | Güncelle / soft delete |

## 13. Arkadaşlar

| Metot | Yol | Açıklama |
|-------|-----|----------|
| POST | `/friends/request` | `{ targetUserId }` veya `{ targetEmail }` |
| GET | `/friends/requests` · `/friends` | İstekler / kabul edilmiş |
| PUT | `/friends/requests/{id}/accept` · `/reject` | Kabul / reddet |
| DELETE | `/friends/{userId}` | Kaldır |

## 14. Paylaşım

| Metot | Yol | Auth | Açıklama |
|-------|-----|------|----------|
| POST | `/shared-contents` | [Authorize] | `{ contentType, contentId, expiresAt }` → ShareToken |
| GET | `/shared-contents/{token}` | **Anonim** | Önizleme |
| POST | `/shared-contents/{token}/import` | [Authorize] | Listene kopyala |
| DELETE | `/shared-contents/{token}` | Sahip | Link sil |

```json
// POST /shared-contents → { "shareToken": "550e8400-...", "shareUrl": "https://app.wordlearner.com/share/550e8400-..." }
```
