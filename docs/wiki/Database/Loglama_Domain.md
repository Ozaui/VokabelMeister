# Loglama Domain (ActivityLog, ApplicationLog, SecurityLog)

**Özet:** Üç ayrı, amaç-özel log tablosu — hepsi insert-only (soft delete yok, immutable) ve yalnızca `GET /admin/logs/*` ile filtreli+sayfalı olarak Admin'e görüntülenir. Kod olarak **A-04 (Loglama Sistemi)**'nde yazılacak; A-03'teki Auth akışına `SecurityLog` entegrasyonu bilinçli olarak A-04 bitince eklenecek (tek istisna, dikey dilim kuralının bilinçli ihlali).
**Kütüphaneler:** Serilog.AspNetCore, Serilog.Sinks.MSSqlServer, Serilog.Sinks.Console, Serilog.Sinks.File
**Bağlantılar:** [[Veritabani_Semasi]] · [[Guvenlik_Politikalari]] · [[Auth_Domain]] · [[API_Sozlesmesi]] · [[BaseEntity]]

## ActivityLog ile BaseEntity Audit Alanları Farkı
[[BaseEntity]]'ye A-02'de eklenen `CreatedByUserId`/`UpdatedByUserId`/`DeletedByUserId` bu domain'in
**yerine geçmez**, tamamlar: `BaseEntity` alanları yalnızca "kaydın şu anki hâlini en son kim
etkiledi" sorusunu (tek değer, log'a JOIN atmadan) cevaplar; `ActivityLog` ise her işlemin **tam
geçmişini** (`OldValue`/`NewValue` JSON diff, zaman damgalı, silinmeyen) tutar. Örn. bir kelime 3 kez
güncellenmişse, `Word.UpdatedByUserId` yalnızca son güncelleyeni gösterir, `ActivityLog` ise üç
`UPDATE_WORD` kaydının hepsini ayrı ayrı saklar.

## Üç Tablo, Üç Amaç

| Tablo | Kim yazar | İçerik |
|-------|-----------|--------|
| `ApplicationLog` | Serilog (`_logger`) + MSSqlServer sink | Teknik log (hata/uyarı/info) — konsol + dosya + DB |
| `ActivityLog` | `IActivityLogger` servisi | Audit: kim ne yaptı (rol değişti, kelime silindi; old/new JSON) |
| `SecurityLog` | `ISecurityLogger` servisi | Güvenlik: başarısız giriş, rate-limit, yetkisiz erişim |

## ActivityLog
`UserId` (anonimse NULL), `ActorRole`, `Action` (`LOGIN|REGISTER|CREATE_WORD|DELETE_USER_CARD|
CHANGE_ROLE|FREEZE_ACCOUNT...`), `EntityType`/`EntityId`, `OldValue`/`NewValue` (JSON diff),
`IpAddress`/`UserAgent`.

## ApplicationLog
Serilog `Serilog.Sinks.MSSqlServer` kolon eşlemesi: `Level`/`Message`/`Exception`/`TimeStamp`/
`Properties` (Serilog standardı) + ek kolonlar `SourceContext`/`RequestPath`/`UserId`.

## SecurityLog
`EventType` (`LoginFailed|OtpFailed|RateLimitHit|UnauthorizedAccess|TokenReplay|AdminAction`),
`EmailHash` (SHA-256 — **ham e-posta asla saklanmaz**, PII kuralı), `IpAddress`/`UserAgent`, `Detail`.

## Planlanan Kod (A-04)
`ActivityLog`/`ApplicationLog`/`SecurityLog` entity + `LogEventType` enum → Serilog MSSqlServer
sink kaydı → `IActivityLogger`/`ISecurityLogger` implementasyonları → sayfalı+filtreli
repository'ler → Auth akışlarına entegrasyon (`LoginFailed`, `OtpFailed`, `RateLimitHit`) →
`GET /health` → admin panel B-08'de görüntüleme.
