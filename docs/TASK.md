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

**Sıradaki task:** `A-08 — Medya / Dosya Yükleme API` ⬜ → `TASK/A_admin_panel_backend.md`
(`A-07 — Admin API` ✅ tamamlandı 2026-07-24: dört dilim — Kullanıcı Yönetimi (`IUserRepository`
genişletmesi + 4 Command/Query + projedeki ilk çift-loglama [`IActivityLogger`+`ISecurityLogger`] +
self-lockout koruması), İstatistik (`GetAdminStatisticsQuery` — toplam/aktif/dondurulmuş kullanıcı,
toplam kelime/kategori, kayıt grafiği; `LoginsByDay` bilinçli olarak yazılmadı), Toplu Kelime Import
(`BulkImportWordsCommand` — her satır bağımsız tek dilli `WordConcept`, best-effort, TEK
`BULK_IMPORT_WORDS` ActivityLog kaydı), Log Görüntüleme (`LogMessages.cs` ile A-04'ten beri bekleyen
`SecurityLog.Detail` çözme borcu kapandı) — `AdminController`'ın 9 endpoint'i, **244/244 yeşil**,
kod denetiminde 2 gerçek düzeltme (tüketicisiz DTO geri alındı, self-lockout koruması eklendi),
Backend Akademi'ye işlendi (7 bölüm), kök karta eklendi. `UserCard` moderasyonu **A-07.1**'e
ertelendi (C-02 bekliyor, bkz. `TASK/A_admin_panel_backend.md` A-07.1))
(`A-06 — Kategori API (Categories)` ✅ tamamlandı: 3 entity+EF config+migration+seed+
ICategoryRepository+5 Command/Query+CategoriesController+21 birim testi + A-05'in `GET /words`
retrofit'i (`categoryId`/`categories[]`, 5 yeni test) + kod denetiminde bulunan 2 hatanın
düzeltilmesi (deferred LINQ audit log hatası, tekrarlanan categoryId→500 riski), toplam
219 birim testi, Backend Akademi'ye işlendi) → `TASK/A_admin_panel_backend.md`

⬜ Başlanmadı · 🔄 Devam ediyor · ✅ Tamamlandı · ⛔ Engellendi
