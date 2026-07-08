# YAPILACAKLAR — İNDEX

> **Çalışma yöntemi (dikey dilim, MediatR CQRS, roadmap döngüsü, git/durum adımları) → `CLAUDE.md` §3–7.**
> Burada yalnızca kullanım + faz haritası + ilerleme durumu var. Task listeleri `TASK/<faz>.md`'de.

## Nasıl Kullanılır

- Task'lar **faz sırasıyla** yapılır; bir faz bitmeden sonrakine geçilmez.
- Claude'a: **"X-YY task'ını yapalım."** — task'ın hangi faz dosyasında olduğunu aşağıdaki tablodan bul.
- Bir task'a başlarken: önce `CLAUDE.md`, sonra ilgili `TASK/<faz>.md`.

## Faz Haritası

| Faz | Ne | Neden | Dosya |
|-----|----|----|-------|
| **A** | Admin panel backend (altyapı+auth+log+içerik+admin) | Endpoint'ler önce | `TASK/A_admin_panel_backend.md` |
| **B** | Admin panel (`/admin`) | Kelime/kategori girilir, API gerçek veriyle test edilir | `TASK/B_admin_panel.md` |
| **C** | Kullanıcı backend | Kart, SRS, öğrenme, sosyal | `TASK/C_kullanici_backend.md` |
| **D** | Web app (`/web`) | Tarayıcıda hızlı test, mobile referans | `TASK/D_web_app.md` |
| **E** | Mobil (`/mobile`) | API+içerik+web referansı hazır | `TASK/E_mobil.md` |
| **F** | Test & deployment | Son kontroller, entegrasyon/regresyon | `TASK/F_test_yayin.md` |

> **Not:** Faz F yeniden test yazma fazı değildir — var olan testler her API'da yazılır (`CLAUDE.md §1`), F yalnızca topluca çalıştırıp eksik entegrasyon/regresyon kapsamını tamamlar.

## İlerleme Durumu

| Faz | Task Aralığı | Başlık | Durum |
|-----|--------------|--------|-------|
| A | A-01…A-10 | Admin Panel Backend | 🔄 |
| B | B-01…B-09 | Admin Panel | ⬜ |
| C | C-01…C-10 | Kullanıcı Backend | ⬜ |
| D | D-01…D-12 | Web App | ⬜ |
| E | E-01…E-14 | Mobil | ⬜ |
| F | F-01…F-04 | Test & Yayın | ⬜ |

**Sıradaki task:** `A-03.2 — Auth Başarı Mesajlarının Lokalizasyonu` (A-03 ✅, A-03.1 ✅) → `TASK/A_admin_panel_backend.md`

⬜ Başlanmadı · 🔄 Devam ediyor · ✅ Tamamlandı · ⛔ Engellendi
