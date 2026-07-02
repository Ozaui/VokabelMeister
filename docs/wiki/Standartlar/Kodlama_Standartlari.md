# Kodlama Standartları

**Özet:** Proje junior eğitimi amaçlı yazılıyor — tüm yorum/log/exception mesajları Türkçe, method/class/property isimleri İngilizce; her dosya başında AMAÇ/NEDEN/BAĞIMLILIKLAR, her public metotta AMAÇ/NEDEN/NASIL bloğu zorunlu. Birim testler Faz F'ye bırakılmaz — her servis katmanı bitince aynı task içinde yazılır.
**Kütüphaneler:** xUnit, Moq, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory (yalnızca Repository&lt;T&gt; testinde)
**Bağlantılar:** [[Gelistirme_Yol_Haritasi]] · [[WordLearner_Tests]] · [[Repository]] · [[Backend_Katmanli_Mimari]]

## Dil Kuralı
Türkçe: tüm yorumlar, XML doc, log mesajları, exception mesajları, console çıktısı.
İngilizce (convention): method/class/property isimleri (C#), test metodu adları, DB kolon adları (SQL), JS değişkenleri.

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
