# Geliştirme Yol Haritası (Faz A→F)

**Özet:** Proje altı fazda ilerler (A: Admin Backend → B: Admin Panel → C: Kullanıcı Backend → D: Web → E: Mobil → F: Test/Yayın) ve her API "dikey dilim" yöntemiyle tüm katmanlarıyla tek seferde bitirilir. Şu an **A-01→A-06 tamamlandı**: Auth API (13 endpoint) + QR Kod ile Giriş (5 endpoint) + mesaj lokalizasyonu (A-03.2) + tema tercihi (A-03.3), Loglama Sistemi (A-04 — `ActivityLog`/`ApplicationLog`/`SecurityLog`, Serilog sink), Sistem Kelimesi API (A-05 — `Word`/`WordConcept` çoklu dil modeli, ilk Admin-only endpoint), Kategori API (A-06 — hiyerarşi + çoklu dil + kelime eşleştirme). Toplam **219/219 birim testi yeşil**. Sırada **A-07 (Admin API — Kullanıcı Yönetimi/İstatistik/Log Görüntüleme)**; ilk kapsamındaki "UserCard moderasyonu" maddesi `UserCard` entity'si henüz yazılmadığı için **A-07.1**'e ertelendi (C-02 sonrası). Kaynak otorite: `docs/TASK.md` (yöntem/standart — ⭐ Çalışma Yöntemi, Her Parça İçin Döngü) +
`docs/TASK/` klasörü (faz başına 1 dosya, örn. Faz A → `docs/TASK/A_admin_panel_backend.md`) —
bu düğüm onların özetidir, güncel ilerleme için orijinal dosyalara bakılmalı.
**Kütüphaneler:** —
**Bağlantılar:** [[Backend_Katmanli_Mimari]] · [[Kodlama_Standartlari]] · [[WordLearner_API]] · [[WordLearner_Infrastructure]] · [[API_Yol_Haritasi_Sistemi]] · [[Gelistirme_Kurulumu]]

## Faz Sırası

| Faz | Ne | Durum |
|-----|----|----|
| **A** | Admin panel backend (altyapı + auth + log + içerik + admin) | 🔄 (A-01→A-06 ✅, sıradaki A-07) |
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
| A-03 | Auth API (User) | ✅ | `User`/`RefreshToken` entity, [[AppException]]/[[ErrorMessages]] (tr+de, dile göre çözülür — log/DB İngilizce), `AuthController` (13 endpoint), rate limiting, 72 test — gerçek sunucuyla uçtan uca doğrulandı |
| A-03.1 | QR Kod ile Giriş | ✅ | `QrLoginSession` + 5 MediatR Command+Handler — Steam-tarzı, `PairingCode` ile relay/phishing önlemi, A-03'ün `ILoginCompletionService`'ini yeniden kullanır, 18 test |
| A-03.2 | Auth Başarı Mesajlarının Lokalizasyonu | ✅ | [[SuccessMessages]] ([[ErrorMessages]]'ın kardeşi), `MessageResponse` artık `Code+Message`, 7 Command'a `Language` init-property, 7 yeni test (toplam A-03+A-03.1+A-03.2 = 97 test) |
| A-03.3 | Kullanıcı Tema Tercihi | ✅ | `ThemePreference` alanı, tek alanlık retrofit |
| A-04 | Loglama Sistemi | ✅ | `ActivityLog`/`ApplicationLog`/`SecurityLog` (insert-only) + Serilog MSSqlServer sink + 8 Handler entegrasyonu, `GET /health`, 144 test (toplam 144) |
| A-05 | Sistem Kelimesi API (Words) | ✅ | `WordConcept`/`Word` çoklu dil modeli, `WordGrammarValidator`, 7 Command/Query (CRUD+Eşleştirme), ilk `[Authorize(Roles="Admin")]`, 193 test |
| A-06 | Kategori API | ✅ | `Category` self-ref hiyerarşi + `CategoryTranslation` + `WordCategory` (M:N), silme koruması, `GET /words` retrofit, 219 test |
| A-07 | Admin API | ⬜ | Kullanıcı yönetimi + istatistik + log görüntüleme (kapsamı 2026-07-23'te düzeltildi — MediatR Command+Handler'a çevrildi, UserCard moderasyonu **A-07.1**'e ertelendi) |
| A-08 | Medya/Dosya Yükleme API | ⬜ | |
| A-09 | SMTP Ayarları API | ⬜ | AES-256 şifreli |
| A-10 | E-posta Servisi + Hesap Temizleme | ⬜ | |

## Bir API'ın Yazım Sırası (dikey dilim)

```
1. Entity → 2. EF Konfigürasyon → 3. Migration → 4. Request DTO → 5. Validator →
6. Exception → 7. Repository Arayüzü → 8. Repository → 9. Response DTO →
10. Servis Arayüzü → 11. Servis → 12. Birim Test (Servis) → 13. Controller →
14. DI kaydı → 15. BACKEND_AKADEMI'ye işle
```

Test felsefesi: birim testler Faz F'ye bırakılmaz — servis katmanı bitince (adım 11) aynı task
içinde test yazılır (adım 12). Detay → [[Kodlama_Standartlari]] §7.

## Sonraki Task
`A-07 — Admin API (Kullanıcı Yönetimi + İstatistik + Log Görüntüleme)` (A-01→A-06 tamamlandı).
Detay ve referans kod → [[Teknik_Ozellikler]], kurulum komutları → [[Gelistirme_Kurulumu]].

## Rehber Sistemi
Her API'ın yazım adımları kök `BACKEND_AKADEMI/` altındaki HTML slayt bölümlerine işlenir —
sistemin kendisi → [[Backend_Akademi_Sistemi]]. A-02 9 bölüm, A-03 25 bölüm (entity → servisler
→ validator → controller → birim testler), A-03.1 13 bölüm (kendi entity/controller'ı olduğu
için ayrı klasör), A-03.2 8 bölüm (lokalizasyon altyapısı → 7 handler → controller → testler,
kendi entity/controller'ı olmadığı için A-03'e değil ayrı bir küçük klasöre işlendi), A-03.3
3 bölüm (tek alanlık retrofit), A-04 12 bölüm (Loglama Sistemi), A-05 10 bölüm (ilk kez bir
`ozet-sozluk` kapanışı aldı), A-06 8 bölüm (yeni `kod-degisiklik` slayt türü eklendi) içerir —
hepsi tamamlandı, zincir kesintisiz A-06'nın son bölümüne kadar bağlı.
