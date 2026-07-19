# Loglama Domain — Log Tabloları (admin panelden görüntülenir)

> Üç tablo, üç amaç. Soft delete **yok**, loglar değişmez (insert-only). Tümü `GET /admin/logs/*` ile
> filtreli+sayfalı (yalnızca Admin). FK: `Users`→`Auth.md`. Mimari → `REFERENCE/ARCHITECTURE.md §6`, `REFERENCE/SECURITY.md §6`.
> **Tablo adları çoğul** (`ActivityLogs`/`ApplicationLogs`/`SecurityLogs`) — EF Core'un `DbSet<T>`
> adlandırma konvansiyonu, `Users`/`RefreshTokens`/`QrLoginSessions` ile aynı desen (A-04'te kodlandı).

### ActivityLogs (audit — kim ne yaptı; `IActivityLogger` yazar)
```sql
CREATE TABLE ActivityLogs (
    Id BIGINT PRIMARY KEY IDENTITY,
    UserId INT NULL,                       -- eylemi yapan (anonim ise NULL)
    ActorRole NVARCHAR(20) NULL,           -- User|Admin (kayıt anındaki rol)
    Action NVARCHAR(100) NOT NULL,         -- LOGIN|REGISTER|CREATE_WORD|DELETE_USER_CARD|CHANGE_ROLE...
    EntityType NVARCHAR(50) NULL,          -- Word|UserCard|User|Category...
    EntityId INT NULL,
    OldValue NVARCHAR(MAX) NULL, NewValue NVARCHAR(MAX) NULL,   -- JSON (öncesi/sonrası)
    IpAddress VARCHAR(45) NULL, UserAgent NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_ActivityLogs_UserId (UserId), INDEX IX_ActivityLogs_Action (Action),
    INDEX IX_ActivityLogs_EntityType (EntityType), INDEX IX_ActivityLogs_CreatedAt (CreatedAt DESC)
);
```

### ApplicationLogs (teknik log — Serilog MSSqlServer sink; kurulum → `REFERENCE/TECHNICAL_SPECIFICATIONS.md §9`)
```sql
CREATE TABLE ApplicationLogs (
    Id BIGINT PRIMARY KEY IDENTITY,
    Level NVARCHAR(20) NOT NULL,           -- Verbose|Debug|Information|Warning|Error|Fatal
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX) NULL,
    SourceContext NVARCHAR(255) NULL,      -- log'u yazan sınıf
    RequestPath NVARCHAR(500) NULL,
    UserId INT NULL,
    Properties NVARCHAR(MAX) NULL,         -- Serilog yapılandırılmış özellikler (JSON)
    TimeStamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    INDEX IX_ApplicationLogs_Level (Level), INDEX IX_ApplicationLogs_TimeStamp (TimeStamp DESC)
);
```
> Serilog sink kolonları (`Level`, `Message`, `Exception`, `TimeStamp`, `Properties`) `ColumnOptions` ile eşlenir; `SourceContext`/`RequestPath`/`UserId` ek kolon. FK **yok** — sink User tablosuna join/kontrol yapmaz, ham `UserId` int'i yazar.

### SecurityLogs (güvenlik olayları — `ISecurityLogger` yazar)
```sql
CREATE TABLE SecurityLogs (
    Id BIGINT PRIMARY KEY IDENTITY,
    EventType NVARCHAR(50) NOT NULL,       -- LoginFailed|OtpFailed|RateLimitHit|UnauthorizedAccess|TokenReplay|AdminAction
    UserId INT NULL,
    EmailHash VARCHAR(44) NULL,            -- SHA-256(email)→Base64, sabit 44 karakter — PII saklamadan ilişkilendirme
    IpAddress VARCHAR(45) NULL, UserAgent NVARCHAR(500) NULL,
    Detail NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_SecurityLogs_EventType (EventType), INDEX IX_SecurityLogs_IpAddress (IpAddress),
    INDEX IX_SecurityLogs_CreatedAt (CreatedAt DESC)
);
```
> `LogEventType` enum (Domain/Enums/Logging): `LoginFailed, OtpFailed, RateLimitHit, UnauthorizedAccess, TokenReplay, PasswordReset, AccountDeletion, AdminAction, QrLoginConfirmed, QrLoginDenied`. DB'de `CK_SecurityLog_EventType` check constraint ile aynı küme zorunlu kılınır (okunabilir string, `HasConversion<string>`).
