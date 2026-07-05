# Geliştirme Yol Haritası (Faz A→F)

**Özet:** Proje altı fazda ilerler (A: Admin Backend → B: Admin Panel → C: Kullanıcı Backend → D: Web → E: Mobil → F: Test/Yayın) ve her API "dikey dilim" yöntemiyle tüm katmanlarıyla tek seferde bitirilir. Şu an **A-01/A-02/A-03 tamamlandı**; ilk feature (Auth API — `User`/`RefreshToken`, 13 endpoint, 61 birim test) uçtan uca gerçek bir sunucuyla doğrulandı. Sırada **A-03.1 (QR Kod ile Giriş)**. Kaynak otorite: `docs/TASK.md` (yöntem/standart — ⭐ Çalışma Yöntemi, Her Parça İçin Döngü) +
`docs/TASK/` klasörü (faz başına 1 dosya, örn. Faz A → `docs/TASK/A_admin_panel_backend.md`) —
bu düğüm onların özetidir, güncel ilerleme için orijinal dosyalara bakılmalı.
**Kütüphaneler:** —
**Bağlantılar:** [[Backend_Katmanli_Mimari]] · [[Kodlama_Standartlari]] · [[WordLearner_API]] · [[WordLearner_Infrastructure]] · [[API_Yol_Haritasi_Sistemi]] · [[Gelistirme_Kurulumu]]

## Faz Sırası

| Faz | Ne | Durum |
|-----|----|----|
| **A** | Admin panel backend (altyapı + auth + log + içerik + admin) | 🔄 (A-01→A-03 ✅) |
| **B** | Admin panel frontend (`/admin`) | ⬜ |
| **C** | Kullanıcı backend (web+mobil ortak API) | ⬜ |
| **D** | Web app (`/web`) | ⬜ |
| **E** | Mobil (`/mobile`) | ⬜ |
| **F** | Test & deployment | ⬜ |

## Faz A Detayı (mevcut odak)

| Task | Başlık | Durum | Not |
|------|--------|-------|-----|
| A-01 | Proje İskeleti | ✅ | 5 proje + referanslar + NuGet + `Program.cs` temel |
| A-02 | Ortak Altyapı | ✅ | [[BaseEntity]], [[WordLearnerDbContext]], [[IRepository]]/[[Repository]], [[InfrastructureServiceExtensions]], [[EntityNotFoundException]], [[ApiErrorResponse]], [[Middleware]] (Exception/SecurityHeaders/RequestResponseLogging), JWT/CORS/Serilog/FluentValidation kaydı, [[RepositoryTests]] (11 test) |
| A-03 | Auth API (User) | ✅ | `User`/`RefreshToken` entity, [[AppException]]/[[ErrorMessages]] (tr+de, dile göre çözülür — log/DB İngilizce), `AuthController` (13 endpoint), rate limiting, 61 test (toplam, A-02 dahil) — gerçek sunucuyla uçtan uca doğrulandı |
| A-03.1 | QR Kod ile Giriş | ⬜ | `QrLoginSession` + `IQrLoginService` — Steam-tarzı, `PairingCode` ile relay/phishing önlemi |
| A-04 | Loglama Sistemi | ⬜ | `ActivityLog`/`ApplicationLog`/`SecurityLog` |
| A-05 | Sistem Kelimesi API (Words) | ⬜ | |
| A-06 | Kategori API | ⬜ | |
| A-07 | Admin API | ⬜ | Kullanıcı yönetimi + istatistik + log görüntüleme |
| A-08 | Medya/Dosya Yükleme API | ⬜ | |
| A-09 | SMTP Ayarları API | ⬜ | AES-256 şifreli |
| A-10 | E-posta Servisi + Hesap Temizleme | ⬜ | |

## Bir API'ın Yazım Sırası (dikey dilim)

```
1. Entity → 2. EF Konfigürasyon → 3. Migration → 4. Request DTO → 5. Validator →
6. Exception → 7. Repository Arayüzü → 8. Repository → 9. Response DTO →
10. Servis Arayüzü → 11. Servis → 12. Birim Test (Servis) → 13. Controller →
14. DI kaydı → 15. API Yol Haritası'na işle
```

Test felsefesi: birim testler Faz F'ye bırakılmaz — servis katmanı bitince (adım 11) aynı task
içinde test yazılır (adım 12). Detay → [[Kodlama_Standartlari]] §7.

## Sonraki Task
`A-03.1 — QR Kod ile Giriş` (A-03 tamamlandı). Detay ve referans kod → [[Teknik_Ozellikler]],
kurulum komutları → [[Gelistirme_Kurulumu]].

## Rehber Sistemi
Her API'ın yazım adımları `docs/API_YOL_HARITASI/` altındaki HTML sayfalarına işlenir — sistemin
kendisi → [[API_Yol_Haritasi_Sistemi]]. A-02 sayfası 11 adım, A-03 sayfası 36 adım (entity → servisler
→ validator → controller → birim testler) içerir, ikisi de tamamlandı.
