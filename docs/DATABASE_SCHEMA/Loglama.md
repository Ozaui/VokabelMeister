# Loglama Domain — Log Tabloları (admin panelden görüntülenir)

> Üç ayrı tablo, üç farklı amaç. Soft delete **yok**; loglar değişmez (immutable) kayıtlardır.
> Tümü `GET /admin/logs/*` ile filtreli + sayfalı listelenir (yalnızca Admin). FK hedefi `Users` → `Auth.md`.

### ActivityLog (audit — kim ne yaptı)
İş/audit kaydı: kullanıcı ve admin eylemleri. Özel `IActivityLogger` servisi yazar.
```sql
CREATE TABLE ActivityLog (
    Id BIGINT PRIMARY KEY IDENTITY,
    UserId INT NULL,                       -- eylemi yapan (anonim ise NULL)
    ActorRole NVARCHAR(20) NULL,           -- User|Admin (kayıt anındaki rol)
    Action NVARCHAR(100) NOT NULL,         -- LOGIN|REGISTER|CREATE_WORD|DELETE_USER_CARD|CHANGE_ROLE|FREEZE_ACCOUNT...
    EntityType NVARCHAR(50) NULL,          -- Word|UserCard|User|Category...
    EntityId INT NULL,
    OldValue NVARCHAR(MAX) NULL,           -- JSON (değişiklik öncesi)
    NewValue NVARCHAR(MAX) NULL,           -- JSON (değişiklik sonrası)
    IpAddress VARCHAR(45) NULL, UserAgent NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_ActivityLog_UserId (UserId), INDEX IX_ActivityLog_Action (Action),
    INDEX IX_ActivityLog_EntityType (EntityType), INDEX IX_ActivityLog_CreatedAt (CreatedAt DESC)
);
```

### ApplicationLog (teknik log — Serilog DB sink)
Serilog `Serilog.Sinks.MSSqlServer` ile yazar. Kodda `_logger.LogX(...)` → konsol + dosya + bu tablo.
```sql
CREATE TABLE ApplicationLog (
    Id BIGINT PRIMARY KEY IDENTITY,
    Level NVARCHAR(20) NOT NULL,           -- Verbose|Debug|Information|Warning|Error|Fatal
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX) NULL,
    SourceContext NVARCHAR(255) NULL,      -- log'u yazan sınıf
    RequestPath NVARCHAR(500) NULL,
    UserId INT NULL,
    Properties NVARCHAR(MAX) NULL,         -- Serilog yapılandırılmış özellikler (JSON)
    TimeStamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    INDEX IX_ApplicationLog_Level (Level), INDEX IX_ApplicationLog_TimeStamp (TimeStamp DESC)
);
```
> **Not:** Serilog MSSqlServer sink kolon adlarını (`Level`, `Message`, `Exception`, `TimeStamp`,
> `Properties`) `ColumnOptions` ile eşler. `SourceContext`/`RequestPath`/`UserId` ek kolon olarak tanımlanır.

### SecurityLog (güvenlik olayları)
Özel `ISecurityLogger` yazar. Başarısız giriş, rate-limit, yetkisiz erişim, OTP hataları.
```sql
CREATE TABLE SecurityLog (
    Id BIGINT PRIMARY KEY IDENTITY,
    EventType NVARCHAR(50) NOT NULL,       -- LoginFailed|OtpFailed|RateLimitHit|UnauthorizedAccess|TokenReplay|AdminAction
    UserId INT NULL,
    EmailHash VARCHAR(88) NULL,            -- SHA-256(email) — PII saklamadan ilişkilendirme
    IpAddress VARCHAR(45) NULL, UserAgent NVARCHAR(500) NULL,
    Detail NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_SecurityLog_EventType (EventType), INDEX IX_SecurityLog_IpAddress (IpAddress),
    INDEX IX_SecurityLog_CreatedAt (CreatedAt DESC)
);
```

**`LogEventType` enum (Domain/Enums):** `LoginFailed, OtpFailed, RateLimitHit, UnauthorizedAccess,
TokenReplay, PasswordReset, AccountDeletion, AdminAction, QrLoginConfirmed, QrLoginDenied`.
