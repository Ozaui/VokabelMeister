# Auth Domain — Kimlik Doğrulama Tabloları

> Genel kurallar → `../CLAUDE.md §1`. QR giriş akışının tamamı → `REFERENCE/SECURITY.md §1.3` + `REFERENCE/API_ENDPOINTS.md §3.1`.

### Users
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Email NVARCHAR(254) NOT NULL UNIQUE,
    PasswordHash VARCHAR(60) NULL,              -- bcrypt; sosyal girişte NULL
    GoogleId NVARCHAR(255) NULL,
    AppleId NVARCHAR(255) NULL,
    AuthProvider NVARCHAR(20) NOT NULL DEFAULT 'Local',  -- Local|Google|Apple
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    DisplayName NVARCHAR(100) NULL,
    AvatarUrl NVARCHAR(500) NULL,
    -- Öğrenme hedefleri
    DailyWordGoal INT NOT NULL DEFAULT 10,
    DailyNewWordLimit INT NOT NULL DEFAULT 5,   -- geri kalanı SRS tekrarı
    -- İstatistikler
    CurrentLevel NVARCHAR(2) NOT NULL DEFAULT 'A1',
    TotalXP INT NOT NULL DEFAULT 0,
    LifetimeXP INT NOT NULL DEFAULT 0,
    StreakDays INT NOT NULL DEFAULT 0,
    LastStreakDate DATETIME2 NULL,
    -- OTP (tek set, purpose ile ayrılır)
    PendingOtpCodeHash VARCHAR(44) NULL,        -- SHA-256(otp)→Base64, sabit 44 karakter
    PendingOtpCodeExpiresAt DATETIME2 NULL,
    PendingOtpCodePurpose NVARCHAR(20) NULL,    -- EmailVerification|LoginOtp|PasswordReset|AccountDeletion
    -- Hesap durumu
    IsOnboardingCompleted BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    EmailVerifiedAt DATETIME2 NULL,
    LastLoginAt DATETIME2 NULL,
    LastLoginIP VARCHAR(45) NULL,
    LoginCount INT NOT NULL DEFAULT 0,
    -- Hesap silme (30 gün grace + kalıcı blok)
    ScheduledDeletionAt DATETIME2 NULL,
    IsAnonymized BIT NOT NULL DEFAULT 0,
    OriginalEmailHash VARCHAR(44) NULL,         -- SHA-256(eski email)→Base64, sabit 44 karakter — silinen e-posta ile tekrar kaydı blokla
    OneSignalPlayerId NVARCHAR(100) NULL,       -- Push
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',  -- User|Admin
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Users_Level CHECK (CurrentLevel IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('User','Admin')),
    CONSTRAINT CK_Users_AuthProvider CHECK (AuthProvider IN ('Local','Google','Apple')),
    INDEX IX_Users_Email (Email), INDEX IX_Users_Role (Role),
    INDEX IX_Users_GoogleId (GoogleId), INDEX IX_Users_AppleId (AppleId)
);
```

### RefreshTokens
```sql
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    TokenHash VARCHAR(44) NOT NULL,    -- SHA-256→Base64, sabit 44 karakter
    TokenFamily NVARCHAR(36) NOT NULL, -- GUID — replay tespiti (Token Family Pattern)
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME2 NULL,
    DeviceInfo NVARCHAR(500) NULL,
    IpAddress VARCHAR(45) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_RefreshTokens_TokenHash (TokenHash), INDEX IX_RefreshTokens_TokenFamily (TokenFamily)
);
```

### QrLoginSessions

> Steam benzeri QR ile giriş: mobilde zaten giriş yapmış kullanıcı, web'de gösterilen QR'ı okutup onaylar.
> `QrTokenHash` DB sızıntısına karşı (ham token yalnızca QR/URL'de). `PairingCode` relay/phishing'e karşı:
> web ekranındaki 4 hane mobil onay ekranındakiyle eşleşmezse kullanıcı fark eder. Süre 2dk.

```sql
CREATE TABLE QrLoginSessions (
    Id INT PRIMARY KEY IDENTITY,
    QrTokenHash VARCHAR(44) NOT NULL,        -- SHA-256(token)→Base64, sabit 44 karakter — ham token DB'de asla saklanmaz
    PairingCode CHAR(4) NOT NULL,            -- web + mobil onay ekranında gösterilir, gözle karşılaştırılır
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending|Scanned|Confirmed|Consumed|Denied|Expired
    UserId INT NULL,                         -- taranana kadar boş; QR'ı okutan kullanıcı ile doldurulur
    RequesterIp VARCHAR(45) NULL,
    RequesterDeviceInfo NVARCHAR(500) NULL,  -- QR'ı isteyen tarafın User-Agent'ı
    ScannedAt DATETIME2 NULL,
    ConfirmedAt DATETIME2 NULL,
    ExpiresAt DATETIME2 NOT NULL,            -- +2 dakika
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedByUserId INT NULL, UpdatedByUserId INT NULL, DeletedByUserId INT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT CK_QrLoginSessions_Status CHECK (Status IN ('Pending','Scanned','Confirmed','Consumed','Denied','Expired')),
    INDEX IX_QrLoginSessions_QrTokenHash (QrTokenHash), INDEX IX_QrLoginSessions_ExpiresAt (ExpiresAt)
);
```
> `QrLoginStatus` enum (Domain/Enums): `Pending, Scanned, Confirmed, Consumed, Denied, Expired`.
> Süresi dolan/kullanılan kayıt silinmez (audit); `ExpiresAt` geçmişse okuma anında `Expired` yorumlanır (ayrı temizlik görevi yok — YAGNI).
