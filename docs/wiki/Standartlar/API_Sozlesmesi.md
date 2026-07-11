# API Sözleşmesi (A-03/A-03.1 ✅ — 18 endpoint canlı, diğerleri planlanan)

**Özet:** Tüm endpoint'lerin uyacağı standart response formatı, HTTP kod sözleşmesi ve tam endpoint listesi `docs/REFERENCE/API_ENDPOINTS.md`'de tanımlı. [[WordLearner_API]]'nin `Controllers/` klasöründe artık `AuthController` (13 endpoint) + `QrLoginController` (5 endpoint) var — geri kalan gruplar (Kullanıcı, Sistem Kelimeleri, Kategoriler, ...) henüz yazılmadı, aşağıdaki tablodaki task sırasıyla eklenecek.
**Kütüphaneler:** ASP.NET Core (Controllers, JWT Bearer auth)
**Bağlantılar:** [[WordLearner_API]] · [[Auth_Domain]] · [[Roller_ve_Erisim]] · [[Guvenlik_Politikalari]] · [[Gelistirme_Yol_Haritasi]]

## Temel Kurallar
```
Base URL : https://api.wordlearner.com/api/v1   (dev: http://localhost:5001/api/v1)
Format   : JSON (UTF-8)   ·   Auth: JWT Bearer   ·   Versiyon: URL (/api/v1/)
Rate limiting: Login 5 hatalı/15dk → kilit · Genel 100/dk (auth), 10/dk (anonim)
```

## Standart Yanıt Formatı
```json
// Başarılı
{ "success": true, "data": {}, "message": "İşlem başarılı", "timestamp": "..." }
// Hatalı
{ "success": false, "error": { "code": "INVALID_CREDENTIALS", "message": "E-posta veya şifre hatalı" } }
```
HTTP kodları: 200/201/204 başarı · 400/401/403/404/409/429/500 hata.
Auth seviyeleri: `Anonim` (JWT yok) · `[Authorize]` (JWT) · `[Authorize(Admin)]`.

## Endpoint Grupları (Auth ✅ tamamlandı, diğerleri planlanan — task karşılığı)

| Grup | Endpoint kökü | Task | Wiki |
|------|----------------|------|------|
| Auth | `/auth/*` (13 endpoint) + `/auth/qr/*` (5 endpoint, A-03.1) | A-03 ✅ | [[Auth_Domain]] |
| Kullanıcı | `/users/me*` | C-01 | — |
| Sistem Kelimeleri | `/words*` | A-05 | [[Icerik_Domain]] |
| Kategoriler | `/categories*` | A-06 | [[Icerik_Domain]] |
| Kişisel Kartlar | `/user-cards*` | C-04 | [[Kisisel_Icerik_Domain]] |
| Kişisel Kategoriler | `/user-categories*` | C-02 | [[Kisisel_Icerik_Domain]] |
| Öğrenme/Sınav | `/learning-sessions*` | C-05 | [[SRS_Domain]] |
| İlerleme | `/words/{id}/progress`, `/user-cards/{id}/progress` | C-03 | [[SRS_Domain]] |
| Admin | `/admin/*` (kullanıcı yönetimi, istatistik, log, SMTP) | A-07/A-09 | [[Loglama_Domain]], [[Guvenlik_Politikalari]] |
| Sınıflar | `/classes*` | C-07 | [[Sosyal_Domain]] |
| Arkadaşlar | `/friends*` | C-08 | [[Sosyal_Domain]] |
| Paylaşım | `/shared-contents*` | C-06 | [[Sosyal_Domain]] |

## Önemli Uç Durum Kalıpları
- **Duplikat:** `409` + `?force=true` ile geç (Words, UserCards, ClassWords).
- **learn-system-word:** `POST /user-cards/learn-system-word` → `UserCard` **oluşturmaz**,
  [[SRS_Domain]]'deki `UserProgress` açar.
- **Log görüntüleme (§11.1):** `GET /admin/logs/{activity|application|security}` — filtre+sayfa,
  detay [[Loglama_Domain]].
- **Paylaşım önizleme:** `GET /shared-contents/{token}` **Anonim** — giriş gerekmez.

Tam örnekler (request/response JSON) → `docs/REFERENCE/API_ENDPOINTS.md`.
