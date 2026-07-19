# Loglama Domain (ActivityLogs, ApplicationLogs, SecurityLogs) — A-04 ✅ tamamlandı

**Özet:** Üç ayrı, amaç-özel log tablosu — hepsi insert-only (soft delete yok, immutable), `BaseEntity`den TÜREMEZ, ve yalnızca `GET /admin/logs/*` (A-07) ile filtreli+sayfalı olarak Admin'e görüntülenecek. **A-04'te kodlandı**; A-03/A-03.1'in Auth/QR akışlarına `SecurityLog` entegrasyonu (bilinçli olarak A-04 bitene kadar ertelenmişti) 8 Handler'a bağlandı: `LoginCommand`/`ConfirmAccountDeletionCommand` (LoginFailed), `VerifyLoginOtpCommand`/`VerifyEmailCommand`/`ResetPasswordCommand`/`ConfirmAccountDeletionCommand` (OtpFailed), `RefreshCommand` (TokenReplay), `ConfirmQrLoginCommand`/`DenyQrLoginCommand` (QrLoginConfirmed/Denied), artı iki BAŞARI olayı (`ResetPasswordCommand`→PasswordReset, `ConfirmAccountDeletionCommand`→AccountDeletion).
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
| `ApplicationLogs` | Serilog (`_logger`) + MSSqlServer sink (`AutoCreateSqlTable=false`) | Teknik log (hata/uyarı/info) — konsol + dosya + DB |
| `ActivityLogs` | `IActivityLogger`/`ActivityLogger` servisi | Audit: kim ne yaptı (rol değişti, kelime silindi; old/new JSON) |
| `SecurityLogs` | `ISecurityLogger`/`SecurityLogger` servisi | Güvenlik: başarısız giriş, rate-limit, yetkisiz erişim |

> **Tablo adları çoğul** (`ActivityLogs`/`ApplicationLogs`/`SecurityLogs`) — EF Core `DbSet<T>`
> konvansiyonu, `Users`/`RefreshTokens`/`QrLoginSessions` ile aynı desen. Doküman ilk yazıldığında
> (tasarım aşaması) tekil isimlendirilmişti, A-04'te kodlanırken düzeltildi (`DATABASE_SCHEMA/Loglama.md`).

## ActivityLog
`UserId` (anonimse NULL), `ActorRole`, `Action` (serbest `NVARCHAR(100)` — `LOGIN|REGISTER|
CREATE_WORD|DELETE_USER_CARD|CHANGE_ROLE...`, enum DEĞİL çünkü her yeni feature kendi Action
string'ini ekler), `EntityType`/`EntityId`, `OldValue`/`NewValue` (JSON diff, `System.Text.Json`),
`IpAddress`/`UserAgent`. `IActivityLogger.LogAsync(...)` çağrıldığında `ActivityLogger` bu alanları
kurup `IActivityLogRepository.AddAsync`'e devreder.

## ApplicationLog
Serilog `Serilog.Sinks.MSSqlServer` kolon eşlemesi (`ApplicationLogColumnOptions.Build()`,
`WordLearner.API/Logging/`): varsayılan `MessageTemplate`/XML `Properties` kaldırılır, yerine
`LogEvent` standart kolonu "Properties" adıyla JSON üretir; `SourceContext`/`RequestPath`/`UserId`
`AdditionalColumns` ile eklenir. `RequestPath`/`UserId`, Serilog'un STANDART özellikleri değil —
`RequestResponseLoggingMiddleware`'in `Serilog.Context.LogContext.PushProperty(...)` ile elle
pushladığı özel alanlar (UserId, Authentication middleware'den SONRA okunur — pipeline sırası
gereği). `IApplicationLogRepository`'nin `AddAsync`'i **yok** — bu tabloya yalnızca Serilog yazar,
Application katmanı salt okur.

## SecurityLog
`EventType` (`LogEventType` enum, `Domain/Enums/Logging/`, 10 değer: `LoginFailed|OtpFailed|
RateLimitHit|UnauthorizedAccess|TokenReplay|PasswordReset|AccountDeletion|AdminAction|
QrLoginConfirmed|QrLoginDenied` — DB'de `HasConversion<string>` + `CK_SecurityLog_EventType` check
constraint ile iki katmanlı korunur), `EmailHash` (`IPasswordService.HashToken` — SHA-256→Base64,
**44 karakter**; Loglama.md'nin eski `VARCHAR(88)` değeri projenin genelinde 2026-07-11'de düzeltilen
bir hataydı, SecurityLog o düzeltme sırasında henüz kodlanmadığı için gözden kaçmış, A-04'te
düzeltildi — ham e-posta ASLA saklanmaz, PII kuralı), `IpAddress`/`UserAgent`, `Detail`.

## Mimari Karar (A-04): Detail/OldValue/NewValue Bir Code'dur, Serbest Metin Değil
İlk yazımda `SecurityLog.Detail`'e serbest Türkçe cümleler yazılmıştı (ör. "Kullanılmış refresh
token tekrar sunuldu..."). Kullanıcı bunu düzeltti: **admin panel de bir istemci**, kendi dil
tercihine sahip — CLAUDE.md'nin "DB/log/geliştirici İngilizce görür" kuralı burada UYGULANMAZ, çünkü
Detail geliştiricinin değil admin'in `GET /admin/logs/security` (A-07) panelinden okuduğu bir metin.
Ama log satırı YAZILIRKEN (ör. anonim bir isteğin Accept-Language'ıyla) hangi admin'in NE ZAMAN
hangi dille okuyacağı bilinmez — bu yüzden `ErrorMessages`/`SuccessMessages` (A-03.2) ile AYNI
Code-sonra-çöz deseni uygulanır: Detail'e sabit bir Code yazılır (ör. `TOKEN_REPLAY_FAMILY_REVOKED`,
`ACCOUNT_DELETION_PASSWORD_MISMATCH`), tr/de çözümü admin OKURKEN (A-07, admin'in KENDİ
Accept-Language'ıyla) yapılacak yeni bir sözlükle olacak — henüz yazılmadı, `TASK/
A_admin_panel_backend.md` A-07'nin "Log görüntüleme" maddesine not olarak eklendi. Bu kural
`CLAUDE.md`'nin dil bölümüne "İkinci istisna" olarak da eklendi (kalıcı, tüm gelecekteki
ActivityLog/SecurityLog yazımları için geçerli).

## Kodlanan Parçalar (A-04)
`ActivityLog`/`ApplicationLog`/`SecurityLog` entity (Domain/Entities/Logging/) + `LogEventType`
enum (Domain/Enums/Logging/) + 3 EF config (Infrastructure/Data/Configurations/Logging/) +
`AddLoggingTables` migration → Serilog MSSqlServer sink kaydı (`ApplicationLogColumnOptions.cs`,
`RequestResponseLoggingMiddleware` zenginleştirmesi) → `IActivityLogger`/`ActivityLogger`,
`ISecurityLogger`/`SecurityLogger` (Application/Services/, flat) → 3 repository (sayfalı+filtreli,
`PagedResult<T>` — A-02'de YAGNI ile silinmiş, burada gerçek ilk tüketicisiyle yeniden yazıldı) →
8 Handler'a entegrasyon (yukarıda) → `RateLimiterOptions.OnRejected` (Program.cs, herhangi bir
policy 429 dönünce RateLimitHit) → `GET /health` (`HealthController`, MediatR dışı — bilinçli CQRS
sapması, saf altyapı kontrolü). **Log görüntüleme** (`GET /admin/logs/*`) kod olarak A-07'de
yazılacak — bu domain yalnızca yazma tarafını (+ okuma için repository'leri) tamamladı.

## CLAUDE.md'ye Eklenen Genel Kural (A-04)
"İçerik değiştiren her CRUD" (Word/Category/UserCard/Class/SharedContent vb.) `IActivityLogger`'a
yazmalı kuralı CLAUDE.md'nin "Veri katmanı" bölümüne eklendi — bu, kullanıcının "ileride
yazacağımız alanlar da loglama sistemine dahil edilmeli mi" sorusuna yanıt olarak A-05/A-06/A-07/
A-08/A-09/A-10 ve C-02/C-04 task'larına somut `Action` string önerileriyle (ör. `CREATE_WORD`,
`DELETE_USER_CARD`) işlendi (bkz. `TASK/A_admin_panel_backend.md`, `TASK/C_kullanici_backend.md`).
