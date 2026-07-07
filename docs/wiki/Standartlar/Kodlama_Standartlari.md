# Kodlama Standartları

**Özet:** Proje junior eğitimi amaçlı yazılıyor — tüm kod yorumları Türkçe; log mesajları, exception `.Message`'ları ve hata `Code` sabitleri İngilizce (DB/geliştirici tarafı), method/class/property isimleri İngilizce; her dosya başında AMAÇ/NEDEN/BAĞIMLILIKLAR, her public metotta AMAÇ/NEDEN/NASIL bloğu zorunlu. İstemciye giden hata mesajı isteğin diline göre ayrı bir kanaldan ([[ErrorMessages]]) çözülür — bkz. [[AppException]]. Birim testler Faz F'ye bırakılmaz — her servis katmanı bitince aynı task içinde yazılır.
**Kütüphaneler:** xUnit, Moq, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory (yalnızca Repository&lt;T&gt; testinde)
**Bağlantılar:** [[Gelistirme_Yol_Haritasi]] · [[WordLearner_Tests]] · [[Repository]] · [[Backend_Katmanli_Mimari]]

## Dil Kuralı
Türkçe: tüm kod yorumları (AMAÇ/NEDEN/NASIL), XML doc, MD dosyaları, API Yol Haritası.
İngilizce (convention): method/class/property isimleri (C#), test metodu adları, DB kolon adları (SQL),
JS değişkenleri, **log mesajları (`_logger.Log*`), exception `.Message`'ları, hata `Code` sabitleri**
(A-03'te değişti — DB'ye/geliştiriciye giden her şey tek bir gerçek dile kilitlenir).
**İstisna:** istemciye giden hata mesajı isteğin `Accept-Language`'ına göre [[ErrorMessages]]
sözlüğünden (tr+de) çözülür — kullanıcı ne dil seçtiyse onu görür, DB/log İngilizce görür.

## Zorunlu Bloklar
- **Dosya başı:** `AMAÇ / NEDEN / BAĞIMLILIKLAR` — mevcut kod dosyalarında ([[Program_cs]],
  [[BaseEntity]], [[IRepository]] vb.) örnekleri var.
- **Public metot:** `AMAÇ / NEDEN / NASIL` + param/return.
- **Karmaşık bloklar:** `// ADIM N:` + `// NEDEN:` yorumları.

## Katman Şablonları (özet)
- **Entity** — `AMAÇ` + alan başına tek satır Türkçe doc.
- **DTO** — Entity değil çünkü hassas alan gizleme + sözleşme.
- **Validator** — her kuralın üstünde `// NEDEN:` (FluentValidation).
- **Repository** — async + CancellationToken + `Include()` (N+1 önle) + soft delete filtresi.
- **Controller** — ince katman: JWT'den userId al, servisi çağır, DTO döndür; iş mantığı yok.

## Klasör Organizasyonu (Domain Alt-Klasörleri, 2026-07-07 itibarıyla)
`Domain/Entities/`, `Domain/Enums/` ve `Infrastructure/Data/Configurations/` **düz (flat) klasör
olarak kalmaz** — her yeni domain/feature alanı (Auth, Vocabulary, SRS, Sosyal, ...) kendi alt
klasörünü alır: `Entities/<Domain>/`, `Enums/<Domain>/`, `Configurations/<Domain>/` (ör.
`Entities/Auth/User.cs`, `Enums/Auth/OtpPurpose.cs`, `Configurations/Auth/UserConfiguration.cs`).
Bu, `Application/Features/<Domain>/`, `Application/DTOs/<Domain>/`, `Application/Validators/<Domain>/`
için zaten geçerli olan konvansiyonun Domain/Infrastructure katmanına da yayılmış hâlidir — tek bir
domain varken (yalnızca Auth) flat yapı sorun değildi, ama yeni domain'ler eklendikçe (A-05+)
`Entities/`'in içinde onlarca ilgisiz dosyanın karışması ("patlama") önlenir.
- **İstisna — `BaseEntity.cs`:** Domain'e özgü değil, tüm entity'lerin ortak taban sınıfı; her zaman
  `Domain/Entities/` KÖKÜNDE kalır, hiçbir alt klasöre taşınmaz.
- **Namespace klasörle eşleşir:** `Entities/Auth/User.cs` → `namespace WordLearner.Domain.Entities.Auth;`
  (aynı kural `Enums/Auth` → `...Enums.Auth`, `Configurations/Auth` → `...Data.Configurations.Auth`
  için de geçerli) — projenin geri kalanındaki (`Features/Auth`, `DTOs/Auth`, `Validators/Auth`)
  namespace-klasör eşleşmesiyle tutarlı.
- **Yeni bir domain'in ilk entity'si yazılırken** ilgili `Entities/<Domain>/`, `Enums/<Domain>/`
  (enum varsa) ve `Configurations/<Domain>/` klasörleri o an açılır — önceden boş klasör oluşturulmaz
  (YAGNI, bkz. `feedback_yagni_ortak_tipler`).

## Birim Test Standardı (zorunlu, §7)
- Konum: `Services/XxxServiceTests.cs`, `Helpers/`, `Repositories/RepositoryTests.cs`.
- İsimlendirme: `{Metot}_{Senaryo}_{BeklenenSonuç}` — yapı sabit ama **metot adının kendisi İngilizce**
  (örn. `Register_EmailAlreadyRegistered_ThrowsDuplicateException`); test adı da method/property gibi
  kod kimliği sayılır (§Dil Kuralı). Yalnızca AAA yorumları ve AMAÇ/NEDEN doc-comment'i Türkçe kalır.
- AAA deseni (Arrange/Act/Assert), Türkçe yorumla bölünür.
- Repository ve dış servisler (email, OneSignal, Google/Apple) **her zaman mock'lanır** — gerçek
  DB/HTTP unit testte yasak (F-02'nin işi). Yalnızca [[Repository]] taban sınıfı testinde
  in-memory EF Core kullanılır.
- Minimum kapsam her public servis metodu için: mutlu yol, bulunamadı ([[EntityNotFoundException]]),
  yetki/sahiplik ihlali, sınır/uç durum (duplikat 409 vb.).
- Her test API Yol Haritası'nın ayrı "Test" alanına birebir kopyalanır + 3 satırlık
  (Test Adı/Ne Test Edildi/Neden Önemli) açıklama eklenir.

## Genel En İyi Pratikler
`async/await` + `CancellationToken` her I/O metodunda · null kontrolü/guard clauses · SOLID/DRY/KISS
· parametreli sorgular (SQL injection yok, EF Core LINQ tercih).

## API Yol Haritası — `aciklama` Standardı (zorunlu, 2026-07-07)
`docs/API_YOL_HARITASI/*.html`'deki her adımın `aciklama` alanı bir junior'a öğretmek için var —
sadece "ne yapıldığını" anlatmak yetmez, şunları da içermeli:
1. **Kavram tanımı** — junior'ın henüz bilmeyebileceği terimler ("enum nedir", "entity nedir",
   "DI/Scoped-Singleton-Transient nedir", "DbContext/DbSet nedir", "migration nedir", "Fluent
   API / IEntityTypeConfiguration nedir", "bu tasarım deseni ne işe yarar" vb.) en az bir cümleyle
   tanımlanır.
2. **Somut gerekçe** — teknik kararın (index, CASCADE/SET NULL, nullable alan, tek yönlü ilişki,
   Scoped vs Singleton, ...) "şu senaryoda şu sorun çıkardı/çıkarırdı" seviyesinde nedeni.

**Yasak kalıp:** "Bu, `X.md`'deki kurala göre yazıldı/taşındı" gibi dokümana self-referential atıf
yapıp gerçek gerekçeyi atlamak. Kuralın *nerede yazdığı* değil, kuralın *kendisinin neden var
olduğu* açıklanır (ör. "tek domain varken flat klasör sorun değildi, ama yeni domain'ler
eklendikçe bir junior'ın ilgili dosyayı bulması zorlaşacaktı, bu yüzden..." — "Klasör
Organizasyonu kuralına göre" DEĞİL).

**Why:** Kullanıcı 2026-07-07'de roadmap'e yazılan "Not: ... bkz. Kodlama_Standartlari.md" tarzı
notları bu yüzden reddetti (bkz. `feedback_junior_egitici_dokumantasyon` hafıza notu).
**How to apply:** Yeni bir adım eklerken de, var olan bir adımı düzenlerken de yazmadan önce
kontrol et: "bir junior bunu okuyup öğrenebilir mi?" Değilse kavram tanımı veya gerekçe eksiktir.
