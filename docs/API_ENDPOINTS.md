# API ENDPOINTS

## 1. Genel Bilgiler

```
Base URL   : https://api.wordlearner.com/api/v1
Protokol   : REST / HTTPS
Format     : JSON (UTF-8)
Auth       : JWT Bearer Token
Versiyonlama: URL tabanlı (/api/v1/)
```

**Rate Limiting:**
- Giriş denemeleri: 5 hatalı/15 dakika → hesap geçici kilitlenir
- Genel API: 100 istek/dakika (kimlik doğrulamalı)
- Genel API: 10 istek/dakika (anonim)

---

## 2. Standart Yanıt Formatı

**Başarılı:**
```json
{ "success": true, "data": {}, "message": "İşlem başarılı", "timestamp": "2024-01-15T10:30:00Z" }
```

**Hatalı:**
```json
{ "success": false, "error": { "code": "GECERSIZ_KIMLIK", "message": "E-posta veya şifre hatalı", "details": {} }, "timestamp": "2024-01-15T10:30:00Z" }
```

**HTTP Durum Kodları:**

| Kod | Anlam |
|-----|-------|
| 200 | Başarılı |
| 201 | Oluşturuldu |
| 204 | İçerik yok (silme) |
| 400 | Geçersiz istek |
| 401 | Kimlik doğrulanmamış |
| 403 | Yetkisiz |
| 404 | Bulunamadı |
| 409 | Çakışma |
| 429 | Çok fazla istek |
| 500 | Sunucu hatası |

---

## 3. Auth Endpoints

### POST /auth/register
```json
// İstek
{ "email": "user@example.com", "password": "Sifre123!@#", "firstName": "Ayşe", "lastName": "Yılmaz" }

// Yanıt 201
{ "success": true, "data": { "id": 1, "email": "user@example.com", "firstName": "Ayşe", "currentLevel": "A1" } }
```

### POST /auth/login
```json
// İstek
{ "email": "user@example.com", "password": "Sifre123!@#" }

// Yanıt 200
{ "success": true, "data": { "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900, "user": { "id": 1, "email": "...", "firstName": "Ayşe", "currentLevel": "A1", "totalXP": 250 } } }
```

### POST /auth/google
```json
// İstek — Google SDK'dan alınan ID token gönderilir
{ "idToken": "eyJ..." }

// Yanıt 200 — Kendi JWT'mizi üretiriz, Google token'ı saklamayız
{ "success": true, "data": { "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900, "user": { "id": 1, "email": "...", "firstName": "Ayşe" }, "isNewUser": false } }
```

### POST /auth/apple
```json
// İstek — Apple SDK'dan alınan identity token gönderilir
{ "identityToken": "eyJ...", "firstName": "Ayşe", "lastName": "Yılmaz" }
// Not: firstName/lastName sadece ilk girişte Apple tarafından gönderilir

// Yanıt 200
{ "success": true, "data": { "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900, "user": { "id": 1, "email": "..." }, "isNewUser": true } }
```

### POST /auth/refresh
```json
// İstek
{ "refreshToken": "eyJ..." }

// Yanıt 200
{ "success": true, "data": { "accessToken": "eyJ...", "refreshToken": "eyJ...", "expiresIn": 900 } }
```

### POST /auth/logout
```json
// İstek
{ "refreshToken": "eyJ..." }
// Yanıt 200 — token iptal edilir
```

### POST /auth/password-reset-request
```json
// İstek
{ "email": "user@example.com" }
// Yanıt her zaman 200 (e-posta sıralaması güvenliği)
```

### POST /auth/password-reset-confirm
```json
// İstek
{ "token": "xxx", "newPassword": "YeniSifre456!@#" }
// Yanıt 200
```

### POST /auth/change-password  [Authorize]
```json
// İstek
{ "currentPassword": "Eski123!@#", "newPassword": "Yeni456!@#" }
// Yanıt 200 — tüm cihazlardan çıkış yapılır
```

---

## 4. Kullanıcı Endpoints

### GET /users/me  [Authorize]
```json
// Yanıt 200
{ "id": 1, "email": "...", "firstName": "Ayşe", "currentLevel": "A1", "totalXP": 250, "streakDays": 5 }
```

### PUT /users/me  [Authorize]
```json
// İstek
{ "firstName": "Ayşe", "displayName": "Ayşe Y.", "preferredUILanguage": "tr" }
```

### GET /users/me/statistics  [Authorize]
```
Query Params: period=week|month|year
```
```json
// Yanıt 200
{
  "totalWordsLearning": 45, "masteredWords": 12, "totalUserCards": 8,
  "averageSuccessRate": 78.5, "streakDays": 5,
  "levelProgress": { "currentLevel": "A1", "xpInLevel": 250, "xpRequiredForNext": 500 }
}
```

### DELETE /users/me  [Authorize]
```json
// İstek — şifre onayı gerekir
{ "password": "Sifre123!@#", "reason": "Hesabımı silmek istiyorum" }
// Yanıt 204
```

---

## 5. Sistem Kelimesi Endpoints

### GET /words  [Authorize]
```
Query Params:
  level        = A1|A2|B1|B2|C1|C2
  categoryId   = int
  partOfSpeech = Noun|Verb|Adjective|...
  search       = string (Almanca veya Türkçe)
  page         = int (varsayılan: 1)
  pageSize     = int (varsayılan: 20, maks: 100)
```
```json
// Yanıt 200
{
  "data": [
    {
      "id": 1, "germanWord": "Mann", "turkishTranslation": "Erkek",
      "partOfSpeech": "Noun", "difficultyLevel": "A1",
      "wordDetail": { "gender": "Masculine", "articleDefiniteNom": "der", "pluralForm": "Männer" },
      "categories": [ { "id": 1, "nameTR": "İnsanlar" } ],
      "userProgress": { "currentLevel": 2, "successRate": 75.0, "nextReviewAt": "2024-01-18T10:00:00Z" }
    }
  ],
  "pagination": { "currentPage": 1, "totalPages": 10, "totalItems": 200 }
}
```

### GET /words/{id}  [Authorize]
```json
// Yanıt 200 — tüm detaylar dahil
{
  "id": 1, "germanWord": "gehen", "turkishTranslation": "Gitmek",
  "wordDetail": {
    "isSeparableVerb": false,
    "conjugationData": { "present": { "ich": "gehe", "du": "gehst" }, "pastParticiple": "gegangen", "auxiliaryVerb": "sein" }
  },
  "examples": [
    { "id": 1, "sentenceDE": "Ich gehe in die Schule.", "sentenceTR": "Okula gidiyorum.", "level": "A1", "exampleType": "Normal" },
    { "id": 2, "sentenceDE": "Das geht mir auf die Nerven.", "sentenceTR": "Bu sinirlerimi bozuyor.", "level": "B1", "exampleType": "Idiom" }
  ]
}
```

### POST /words  [Authorize — Admin]
```json
// İstek
{
  "germanWord": "Haus", "turkishTranslation": "Ev", "partOfSpeech": "Noun", "difficultyLevel": "A1",
  "wordDetail": { "gender": "Neuter", "articleDefiniteNom": "das", "pluralForm": "Häuser" },
  "categoryIds": [4],
  "examples": [
    { "sentenceDE": "Das Haus ist groß.", "sentenceTR": "Ev büyük.", "level": "A1", "exampleType": "Normal" }
  ]
}
// Yanıt 201
```

### PUT /words/{id}  [Authorize — Admin]
### DELETE /words/{id}  [Authorize — Admin]  (Soft delete)

---

## 6. Kategori Endpoints

### GET /categories  [Authorize]
```
Query Params: level=A1, includeWordCount=true
```
```json
// Yanıt 200
[
  {
    "id": 1, "nameTR": "İnsanlar", "nameDE": "Menschen",
    "icon": "people", "color": "#FF6B6B",
    "wordCount": 25, "userProgress": { "masteredCount": 5, "averageSuccessRate": 70.0 },
    "children": [ { "id": 2, "nameTR": "Aile" } ]
  }
]
```

### GET /categories/{id}/words  [Authorize]
```
Query Params: level=A1, page=1, pageSize=20
```

### POST /categories  [Authorize — Admin]
### PUT /categories/{id}  [Authorize — Admin]
### DELETE /categories/{id}  [Authorize — Admin]

---

## 7. Kişisel Kart Endpoints

### GET /user-cards  [Authorize]
```
Query Params:
  categoryId     = int (sistem kategorisi)
  userCategoryId = int (kişisel kategori)
  search         = string
  page           = int
  pageSize       = int
```
```json
// Yanıt 200
{
  "data": [
    {
      "id": 1, "frontText": "der Hund", "backText": "Köpek",
      "notes": "Çoğul: die Hunde",
      "categories": [ { "id": 1, "nameTR": "Hayvanlar" } ],
      "userCategories": [ { "id": 1, "name": "Favori Kelimelerim" } ],
      "progress": { "currentLevel": 1, "successRate": 60.0, "nextReviewAt": "2024-01-17T10:00:00Z" }
    }
  ],
  "pagination": { "currentPage": 1, "totalPages": 1, "totalItems": 5 }
}
```

### GET /user-cards/{id}  [Authorize — sadece kart sahibi]

### POST /user-cards  [Authorize]
```json
// İstek
{
  "frontText": "der Hund",
  "backText": "Köpek",
  "notes": "Çoğul: die Hunde",
  "categoryIds": [10],
  "userCategoryIds": [1],
  "examples": [
    { "sentenceFront": "Der Hund bellt.", "sentenceBack": "Köpek havlıyor." }
  ]
}
// Yanıt 201
```

### PUT /user-cards/{id}  [Authorize — sadece kart sahibi]
### DELETE /user-cards/{id}  [Authorize — sadece kart sahibi]  (Soft delete)

### POST /user-cards/learn-system-word  [Authorize]
```json
// Sistem kelimesini öğrenme listesine ekle — UserCard OLUŞTURULMAZ
// İstek
{ "wordId": 5 }

// Yanıt 200 — mevcut kayıt varsa aynı kaydı döner, yoksa yeni açar
{ "success": true, "data": { "userProgressId": 12, "wordId": 5, "germanWord": "laufen", "alreadyExists": false } }
```

> **Not:** Bu endpoint `POST /user-cards` ile `FrontText` sistem Words tablosundaki
> bir `GermanWord` ile eşleştiğinde yanıtla birlikte gelen `suggestedSystemWordId`
> kullanılarak çağrılır. Kullanıcı "Sisteme ekle" seçeneğini seçerse bu endpoint;
> "Hayır, kendi kartımı oluşturayım" seçeneğini seçerse normal `POST /user-cards`
> devam eder.

---

## 8. Kişisel Kategori Endpoints

### GET /user-categories  [Authorize]
```json
// Yanıt 200
[ { "id": 1, "name": "Favori Kelimelerim", "color": "#FF5733", "cardCount": 12 } ]
```

### POST /user-categories  [Authorize]
```json
{ "name": "Favori Kelimelerim", "color": "#FF5733", "icon": "star" }
```

### PUT /user-categories/{id}  [Authorize — sadece sahibi]
### DELETE /user-categories/{id}  [Authorize — sadece sahibi]

---

## 9. Öğrenme / Sınav Endpoints

### POST /learning-sessions  [Authorize]
```json
// İstek — Sınav başlat
{
  "sessionType": "MultipleChoice",   // Flashcard | MultipleChoice | ArticleQuiz | PluralQuiz | TranslationQuiz
  "sourceType": "SystemWords",       // SystemWords | UserCards | Mixed
  "levelFilter": "A1",              // Opsiyonel
  "categoryIds": [1, 3],            // Opsiyonel — birden fazla kategori
  "userCategoryIds": [1],           // Opsiyonel — kişisel kategoriler
  "wordCount": 10                    // Kaç kelime
}
// Mixed sourceType notu: Hem UserProgress hem UserCardProgress sorgulanır.
// Bir UserCard'ın FrontText'i kullanıcının aktif UserProgress'indeki bir
// GermanWord ile eşleşiyorsa UserCard oturuma dahil edilmez; UserProgress
// sürümü kullanılır (çift görünme önlenir).

// Yanıt 201
{
  "id": 1,
  "sessionType": "MultipleChoice",
  "status": "Active",
  "items": [
    { "id": 1, "type": "SystemWord", "wordId": 5, "germanWord": "Mann", "options": ["der","die","das","ein"] },
    { "id": 2, "type": "UserCard",   "cardId": 3, "frontText": "der Hund" }
  ],
  "totalItems": 10
}
```

### POST /learning-sessions/{id}/answer  [Authorize]
```json
// İstek
{ "itemId": 1, "itemType": "SystemWord", "wordId": 5, "isCorrect": true, "userResponse": "der", "timeSpentSeconds": 4 }

// Yanıt 200
{
  "feedback": { "isCorrect": true, "correctAnswer": "der Mann", "explanation": "Mann eril cinsiyet (Maskulin) — der." },
  "xpEarned": 10,
  "progress": { "currentLevel": 2, "successRate": 75.0, "nextReviewAt": "2024-01-18T10:00:00Z" }
}
```

### POST /learning-sessions/{id}/complete  [Authorize]
```json
// Yanıt 200
{
  "summary": { "totalItems": 10, "correctAnswers": 8, "successRate": 80.0, "durationSeconds": 300, "xpEarned": 80 },
  "rewards": { "xpEarned": 80, "newAchievements": [] }
}
```

### POST /learning-sessions/{id}/abandon  [Authorize]

### GET /learning-sessions/history  [Authorize]
```
Query Params: page=1, pageSize=20
```

---

## 10. İlerleme Endpoints

### GET /words/{wordId}/progress  [Authorize]
```json
// Yanıt 200
{ "currentLevel": 2, "successRate": 75.0, "timesCorrect": 9, "timesIncorrect": 3, "nextReviewAt": "2024-01-18T10:00:00Z" }
```

### GET /user-cards/{cardId}/progress  [Authorize — sadece kart sahibi]

---

## 11. Admin Endpoints

Tüm admin endpoint'leri `Admin` veya uygun rol gerektirir.

### GET /admin/users  [Admin]
```
Query Params: page=1, pageSize=20, search=string, role=User|Instructor|Admin
```

### PUT /admin/users/{id}/role  [Admin]
```json
{ "role": "Instructor" }
```

### PUT /admin/users/{id}/status  [Admin]
```json
{ "isActive": false, "reason": "Kural ihlali" }
```

### GET /admin/statistics  [Admin]
```json
{
  "totalUsers": 1500, "activeUsers": 320,
  "totalWords": 850,  "totalUserCards": 4200,
  "topWords": [ { "germanWord": "gehen", "learnerCount": 800 } ]
}
```

---

## 12. Sınıf Endpoints

### POST /classes  [Authorize]
```json
// İstek
{ "name": "A1 Türkçe-Almanca", "description": "Başlangıç grubu" }

// Yanıt 201
{ "id": 1, "name": "A1 Türkçe-Almanca", "inviteCode": "ABC123", "ownerName": "Ayşe Y." }
```

### GET /classes  [Authorize]
```json
// Kullanıcının üye olduğu ve sahibi olduğu sınıflar
[
  { "id": 1, "name": "A1 Türkçe-Almanca", "memberCount": 12, "role": "Teacher" },
  { "id": 2, "name": "Arkadaş Grubu",     "memberCount": 4,  "role": "Student" }
]
```

### GET /classes/{id}  [Authorize — Sınıf üyesi]
```json
{
  "id": 1, "name": "A1 Türkçe-Almanca", "memberCount": 12,
  "categories": [ { "id": 1, "nameTR": "Aile" } ],
  "userCategories": [ { "id": 3, "name": "Öğretmenin Listesi" } ],
  "members": [ { "userId": 5, "displayName": "Mehmet", "role": "Student" } ]
}
```

### POST /classes/join  [Authorize]
```json
// İstek — davet koduyla katıl
{ "inviteCode": "ABC123" }
// Yanıt 200 — sınıf bilgileri döner
```

### POST /classes/{id}/categories  [Authorize — Sınıf sahibi]
```json
// Sınıfa sistem kategorisi ekle
{ "categoryId": 3 }
```

### POST /classes/{id}/user-categories  [Authorize — Sınıf sahibi]
```json
// Sınıfa kişisel kategori ekle
{ "userCategoryId": 7 }
```

### GET /classes/{id}/statistics  [Authorize — Sınıf sahibi/öğretmen]
```json
{
  "members": [
    { "userId": 5, "displayName": "Mehmet", "totalWordsLearned": 45, "averageSuccessRate": 72.0 }
  ],
  "hardestWords": [ { "germanWord": "Schmetterlingseffekt", "avgSuccessRate": 23.0 } ]
}
```

### DELETE /classes/{id}  [Authorize — Sınıf sahibi]
### DELETE /classes/{id}/leave  [Authorize — Sınıf üyesi]

### POST /classes/{id}/words  [Authorize — Sınıf sahibi (Instructor/Admin)]
```json
// Sınıfa özel kelime ekle — YALNIZCA sınıf üyeleri görebilir
// İstek
{
  "germanWord": "der Hund",
  "turkishTranslation": "Köpek",
  "partOfSpeech": "Noun",
  "gender": "Masculine",
  "articleDefiniteNom": "der",
  "pluralForm": "Hunde",
  "notes": "Evcil hayvan"
}
// Yanıt 201
{ "id": 1, "germanWord": "der Hund", "turkishTranslation": "Köpek", "classId": 5 }
```

### GET /classes/{id}/words  [Authorize — Sınıf üyesi]
```
Query Params: page=1, pageSize=20, search=string
```
```json
// Yanıt 200 — yalnızca bu sınıfa özel kelimeler
{
  "data": [
    { "id": 1, "germanWord": "der Hund", "turkishTranslation": "Köpek", "gender": "Masculine", "articleDefiniteNom": "der", "pluralForm": "Hunde" }
  ],
  "pagination": { "currentPage": 1, "totalPages": 1, "totalItems": 1 }
}
```

### PUT /classes/{id}/words/{wordId}  [Authorize — Sınıf sahibi]
```json
{ "germanWord": "der Hund", "turkishTranslation": "Köpek", "notes": "Düzenlenmiş not" }
```

### DELETE /classes/{id}/words/{wordId}  [Authorize — Sınıf sahibi]  (Soft delete)

---

## 13. Arkadaş Endpoints

### POST /friends/request  [Authorize]
```json
{ "targetUserId": 15 }
// veya
{ "targetEmail": "ahmet@example.com" }
```

### GET /friends/requests  [Authorize]
```json
// Gelen ve giden istekler
{
  "incoming": [ { "id": 3, "requesterId": 15, "displayName": "Ahmet", "requestedAt": "..." } ],
  "outgoing": [ { "id": 4, "receiverId": 20, "displayName": "Zeynep", "status": "Pending" } ]
}
```

### PUT /friends/requests/{id}/accept  [Authorize]
### PUT /friends/requests/{id}/reject  [Authorize]

### GET /friends  [Authorize]
```json
// Kabul edilmiş arkadaşlar
[ { "userId": 15, "displayName": "Ahmet", "currentLevel": "A2", "streakDays": 7 } ]
```

### DELETE /friends/{userId}  [Authorize]

---

## 14. Paylaşım Endpoints

### POST /shared-contents  [Authorize]
```json
// Paylaşım linki oluştur
{ "contentType": "UserCategory", "contentId": 7, "expiresAt": null }

// Yanıt 201
{ "shareToken": "550e8400-e29b-41d4-a716-446655440000", "shareUrl": "https://app.wordlearner.com/share/550e8400..." }
```

### GET /shared-contents/{token}  [Herkes — giriş gerekmez]
```json
// Paylaşım önizlemesi
{
  "contentType": "UserCategory",
  "ownerName": "Ayşe Y.",
  "preview": { "name": "Favori Kelimelerim", "cardCount": 15 }
}
```

### POST /shared-contents/{token}/import  [Authorize]
```json
// Paylaşılan içeriği kendi listeme ekle
// Yanıt 200 — oluşturulan UserCard/UserCategory ID'leri döner
{ "importedIds": [101, 102, 103] }
```

### DELETE /shared-contents/{token}  [Authorize — içerik sahibi]
