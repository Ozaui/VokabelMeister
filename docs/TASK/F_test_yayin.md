# FAZ F — Test ve Yayın

> **Yöntem/standart:** Kurallar için → `../TASK.md` (**⭐ Çalışma Yöntemi**, **Her Parça İçin Döngü**)
> — o bölümler değişmez standarttır, burada tekrar edilmez.

### F-01 — Backend Test Konsolidasyonu (Regresyon) ⬜
> **Not:** Bu fazda testler **sıfırdan yazılmaz** — her API kendi task'ında (A-xx/C-xx) zaten birim
> testiyle bitirilmiş olmalı. Burada yapılan: (1) tüm test projesini topluca çalıştır, (2) coverage
> raporuna bak, (3) sadece **eksik kalan** servis/yardımcı sınıflar için test tamamla.
- [ ] Tüm birim testlerini topluca çalıştır (CI script), kırmızı/eksik olanları düzelt
- [ ] Coverage raporu çıkar, kritik servislerde (Auth, SRS, UserCard) eksik dal/senaryo varsa tamamla

### F-02 — Backend Integration Testler ⬜
- [ ] Auth (gerçek/test DB ile uçtan uca login akışı), kelime (rol yetkisi), UserCard (sahiplik),
      paylaşım akışı (anonim önizleme), sınıf görünürlük (üye/sahip)

### F-03 — Frontend Testler ⬜
- [ ] SystemWordCard, PersonalCard, authSlice, Axios interceptor

### F-04 — Deployment ⬜
**Referans:** REFERENCE/SECURITY.md §10
- [ ] IIS publish, production secrets (REFERENCE/ENV.md), güvenlik checklist, DB backup
- [ ] GDPR/KVKK: hesap silme anonimleştirme, `OriginalEmailHash` blok testi, log saklama politikası
