# VERİTABANI ŞEMASI

> MS SQL Server. Tüm tablolar `BaseEntity` alanlarını (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
> taşır (log tabloları hariç — onlarda soft delete yok). Repository sorgularında **soft delete filtresi**
> ve kişisel içerikte **UserId filtresi** zorunludur.

## 1. İlişki Özeti (ERD)

```
Users ─┬─ RefreshTokens (1:N)
       ├─ UserProgress (1:N) ── Words
       ├─ UserCards (1:N) ─┬─ UserCardProgress (1:N)
       │                   ├─ UserCardExamples (1:N)
       │                   ├─ UserCardCategories (M:N) ── Categories
       │                   └─ UserCardUserCategories (M:N) ── UserCategories
       ├─ UserCategories (1:N)
       ├─ LearningSessions / LearningHistory (1:N)
       ├─ Classes (owner) / ClassMemberships / Friendships / SharedContents
       └─ Activity/Security loglarda UserId (1:N, SET NULL)

Words ─┬─ WordDetails (1:1) ── WordExamples (1:N) ── WordCategories (M:N) ── Categories
Classes ─ ClassWords (1:N) [yalnızca sınıf üyeleri] · ClassCategories / ClassUserCategories (M:N)
Categories ─ self-ref (ParentCategoryId)
```

---

## 2. Tablolar

### 2.1 Users
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
    PendingOtpCodeHash VARCHAR(88) NULL,        -- SHA-256(otp)
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
    OriginalEmailHash VARCHAR(88) NULL,         -- SHA-256(eski email) — silinen e-posta ile tekrar kaydı blokla
    -- Push
    OneSignalPlayerId NVARCHAR(100) NULL,
    -- Rol
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

### 2.2 RefreshTokens
```sql
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    TokenHash VARCHAR(88) NOT NULL,    -- SHA-256
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

### 2.3 Words (Sistem Kelimeleri — yalnızca Admin ekler)
```sql
CREATE TABLE Words (
    Id INT PRIMARY KEY IDENTITY,
    GermanWord NVARCHAR(255) NOT NULL,
    TurkishTranslation NVARCHAR(500) NOT NULL,
    PartOfSpeech NVARCHAR(20) NOT NULL,   -- Noun|Verb|Adjective|Adverb|Conjunction|Preposition|Pronoun|Other
    DifficultyLevel NVARCHAR(2) NOT NULL, -- A1..C2
    Definition NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy INT NULL, UpdatedBy INT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Words_Level CHECK (DifficultyLevel IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_Words_PartOfSpeech CHECK (PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')),
    INDEX IX_Words_GermanWord (GermanWord), INDEX IX_Words_DifficultyLevel (DifficultyLevel)
);
```

### 2.4 WordDetails (Almanca gramer — 1:1 Words)
```sql
CREATE TABLE WordDetails (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL UNIQUE,
    Gender NVARCHAR(20) NULL,                -- Masculine|Feminine|Neuter (fiilde NULL)
    -- Belirli artikeller (4 hâl)
    ArticleDefiniteNom NVARCHAR(10) NULL, ArticleDefiniteAcc NVARCHAR(10) NULL,
    ArticleDefiniteDat NVARCHAR(10) NULL, ArticleDefiniteGen NVARCHAR(10) NULL,
    -- Belirsiz artikeller (4 hâl)
    ArticleIndefiniteNom NVARCHAR(10) NULL, ArticleIndefiniteAcc NVARCHAR(10) NULL,
    ArticleIndefiniteDat NVARCHAR(10) NULL, ArticleIndefiniteGen NVARCHAR(10) NULL,
    -- Çoğul
    PluralForm NVARCHAR(255) NULL, PluralFormDative NVARCHAR(255) NULL,  -- genelde Nom + -n
    -- Fiil çekimleri (JSON: present/preterite/perfect + pastParticiple + auxiliaryVerb)
    ConjugationData NVARCHAR(MAX) NULL,
    IsSeparableVerb BIT NOT NULL DEFAULT 0,
    SeparablePrefix NVARCHAR(50) NULL,
    Pronunciation NVARCHAR(500) NULL,        -- IPA
    AudioUrl NVARCHAR(500) NULL, ImageUrl NVARCHAR(500) NULL,
    Notes NVARCHAR(MAX) NULL, CommonMistakes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,
    INDEX IX_WordDetails_Gender (Gender)
);
```
**ConjugationData JSON:** `{ "present":{"ich":"gehe",...}, "preterite":{...}, "perfect":{...}, "pastParticiple":"gegangen", "auxiliaryVerb":"sein" }`

### 2.5 WordExamples (seviyeli örnek cümleler — 1:N Words)
```sql
CREATE TABLE WordExamples (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL,
    SentenceDE NVARCHAR(MAX) NOT NULL,
    SentenceTR NVARCHAR(MAX) NOT NULL,
    Level NVARCHAR(2) NOT NULL DEFAULT 'A1',
    ExampleType NVARCHAR(20) NOT NULL DEFAULT 'Normal',  -- Normal|Idiom|Formal|Colloquial
    DisplayOrder INT NOT NULL DEFAULT 0, IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,
    CONSTRAINT CK_WordExamples_Level CHECK (Level IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_WordExamples_ExampleType CHECK (ExampleType IN ('Normal','Idiom','Formal','Colloquial')),
    INDEX IX_WordExamples_WordId_Level (WordId, Level)
);
```

### 2.6 Categories (sistem kategorileri — hiyerarşik)
```sql
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,
    NameDE NVARCHAR(100) NOT NULL, NameTR NVARCHAR(100) NOT NULL,
    DescriptionTR NVARCHAR(MAX) NULL,
    ParentCategoryId INT NULL,               -- self-ref
    DisplayOrder INT NOT NULL DEFAULT 0,
    Icon NVARCHAR(100) NULL, Color NVARCHAR(10) NULL,
    MinLevel NVARCHAR(2) NULL, MaxLevel NVARCHAR(2) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id),
    INDEX IX_Categories_ParentCategoryId (ParentCategoryId)
);
```

### 2.7 WordCategories (Word ↔ Category M:N)
```sql
CREATE TABLE WordCategories (
    Id INT PRIMARY KEY IDENTITY, WordId INT NOT NULL, CategoryId INT NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WordCategories UNIQUE (WordId, CategoryId)
);
```

### 2.8 UserCards + UserCardExamples (kişisel kartlar)
```sql
CREATE TABLE UserCards (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,                  -- sahip; sorgularda zorunlu filtre
    FrontText NVARCHAR(500) NOT NULL, BackText NVARCHAR(500) NOT NULL,
    Notes NVARCHAR(MAX) NULL, ImageUrl NVARCHAR(500) NULL, AudioUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_UserCards_UserId (UserId)
);
CREATE TABLE UserCardExamples (
    Id INT PRIMARY KEY IDENTITY, UserCardId INT NOT NULL,
    SentenceFront NVARCHAR(MAX) NOT NULL, SentenceBack NVARCHAR(MAX) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE
);
```

### 2.9 UserCategories + ara tablolar
```sql
CREATE TABLE UserCategories (
    Id INT PRIMARY KEY IDENTITY, UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL, Description NVARCHAR(500) NULL,
    Color NVARCHAR(10) NULL, Icon NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX IX_UserCategories_UserId (UserId)
);
-- UserCard ↔ sistem Category
CREATE TABLE UserCardCategories (
    Id INT PRIMARY KEY IDENTITY, UserCardId INT NOT NULL, CategoryId INT NOT NULL,
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT UQ_UserCardCategories UNIQUE (UserCardId, CategoryId)
);
-- UserCard ↔ kişisel UserCategory
CREATE TABLE UserCardUserCategories (
    Id INT PRIMARY KEY IDENTITY, UserCardId INT NOT NULL, UserCategoryId INT NOT NULL,
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserCategoryId) REFERENCES UserCategories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserCardUserCategories UNIQUE (UserCardId, UserCategoryId)
);
```

### 2.10 UserProgress / UserCardProgress (SRS)
```sql
-- Sistem kelimesi ilerlemesi
CREATE TABLE UserProgress (
    Id INT PRIMARY KEY IDENTITY, UserId INT NOT NULL, WordId INT NOT NULL,
    CurrentLevel INT NOT NULL DEFAULT 0,          -- 0=hiç görülmedi .. 5=otomatik hatırlama
    Mastery DECIMAL(5,2) NOT NULL DEFAULT 0,
    EasinessFactor DECIMAL(4,2) NOT NULL DEFAULT 2.5,  -- SM-2
    TimesCorrect INT NOT NULL DEFAULT 0, TimesIncorrect INT NOT NULL DEFAULT 0,
    TotalAttempts INT NOT NULL DEFAULT 0, SuccessRate DECIMAL(5,2) NOT NULL DEFAULT 0,
    LastReviewedAt DATETIME2 NULL,
    NextReviewAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IntervalDays INT NOT NULL DEFAULT 1, RepetitionNumber INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserProgress UNIQUE (UserId, WordId),
    INDEX IX_UserProgress_UserId_NextReviewAt (UserId, NextReviewAt)
);
-- Kişisel kart ilerlemesi (aynı şema, UserCardId ile)
CREATE TABLE UserCardProgress (
    Id INT PRIMARY KEY IDENTITY, UserId INT NOT NULL, UserCardId INT NOT NULL,
    CurrentLevel INT NOT NULL DEFAULT 0, Mastery DECIMAL(5,2) NOT NULL DEFAULT 0,
    EasinessFactor DECIMAL(4,2) NOT NULL DEFAULT 2.5,
    TimesCorrect INT NOT NULL DEFAULT 0, TimesIncorrect INT NOT NULL DEFAULT 0,
    TotalAttempts INT NOT NULL DEFAULT 0, SuccessRate DECIMAL(5,2) NOT NULL DEFAULT 0,
    LastReviewedAt DATETIME2 NULL, NextReviewAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IntervalDays INT NOT NULL DEFAULT 1, RepetitionNumber INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserCardProgress UNIQUE (UserId, UserCardId),
    INDEX IX_UserCardProgress_UserId_NextReviewAt (UserId, NextReviewAt)
);
```

### 2.11 LearningHistory / LearningSessions
```sql
CREATE TABLE LearningSessions (
    Id INT PRIMARY KEY IDENTITY, UserId INT NOT NULL,
    SessionType NVARCHAR(50) NOT NULL,   -- Flashcard|MultipleChoice|ArticleQuiz|PluralQuiz|TranslationQuiz
    SourceType NVARCHAR(20) NOT NULL,    -- SystemWords|UserCards|Mixed
    LevelFilter NVARCHAR(2) NULL,
    CategoryIds NVARCHAR(MAX) NULL,      -- JSON [1,3,5]
    UserCategoryIds NVARCHAR(MAX) NULL,  -- JSON
    StartedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), CompletedAt DATETIME2 NULL, DurationSeconds INT NULL,
    TotalWords INT NOT NULL DEFAULT 0, CorrectAnswers INT NOT NULL DEFAULT 0, IncorrectAnswers INT NOT NULL DEFAULT 0,
    SuccessRate DECIMAL(5,2) NULL, XPEarned INT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',  -- Active|Completed|Abandoned
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_LearningSessions_Status CHECK (Status IN ('Active','Completed','Abandoned')),
    INDEX IX_LearningSessions_UserId (UserId)
);
CREATE TABLE LearningHistory (
    Id INT PRIMARY KEY IDENTITY, UserId INT NOT NULL,
    WordId INT NULL, UserCardId INT NULL,           -- ikisi birden NULL olamaz (uygulama kontrolü)
    LearningSessionId INT NULL, SessionType NVARCHAR(50) NULL,
    IsCorrect BIT NOT NULL, ResponseTime INT NULL, TimeSpentSeconds INT NULL,
    UserResponse NVARCHAR(500) NULL, CorrectResponse NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (WordId) REFERENCES Words(Id),
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id),
    FOREIGN KEY (LearningSessionId) REFERENCES LearningSessions(Id),
    INDEX IX_LearningHistory_UserId (UserId), INDEX IX_LearningHistory_CreatedAt (CreatedAt DESC)
);
```

### 2.12 Achievements / UserAchievements (gamification)
```sql
CREATE TABLE Achievements (
    Id INT PRIMARY KEY IDENTITY, Name NVARCHAR(100) NOT NULL, Description NVARCHAR(500) NULL,
    Icon NVARCHAR(255) NULL, RewardXP INT NOT NULL DEFAULT 0,
    Rarity NVARCHAR(20) NOT NULL DEFAULT 'Common',  -- Common|Rare|Epic|Legendary
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
CREATE TABLE UserAchievements (
    Id INT PRIMARY KEY IDENTITY, UserId INT NOT NULL, AchievementId INT NOT NULL,
    UnlockedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (AchievementId) REFERENCES Achievements(Id),
    CONSTRAINT UQ_UserAchievements UNIQUE (UserId, AchievementId)
);
```

### 2.13 SmtpSettings (tekil — admin yönetir, AES şifreli)
```sql
CREATE TABLE SmtpSettings (
    Id INT PRIMARY KEY IDENTITY,
    Host NVARCHAR(255) NOT NULL, Port INT NOT NULL DEFAULT 587, EnableSsl BIT NOT NULL DEFAULT 1,
    Username NVARCHAR(255) NOT NULL,
    PasswordEncrypted NVARCHAR(MAX) NOT NULL,    -- Base64(IV + AES-256-CBC cipher)
    FromEmail NVARCHAR(254) NOT NULL, FromName NVARCHAR(100) NOT NULL,
    UpdatedByUserId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id)
);
```

---

## 3. Log Tabloları (admin panelden görüntülenir)

> Üç ayrı tablo, üç farklı amaç. Soft delete **yok**; loglar değişmez (immutable) kayıtlardır.
> Tümü `GET /admin/logs/*` ile filtreli + sayfalı listelenir (yalnızca Admin).

### 3.1 ActivityLog (audit — kim ne yaptı)
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

### 3.2 ApplicationLog (teknik log — Serilog DB sink)
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

### 3.3 SecurityLog (güvenlik olayları)
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
TokenReplay, PasswordReset, AccountDeletion, AdminAction`.

---

## 4. Sosyal Tablolar

### 4.1 Classes
```sql
CREATE TABLE Classes (
    Id INT PRIMARY KEY IDENTITY, OwnerId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL, Description NVARCHAR(500) NULL,
    InviteCode NVARCHAR(20) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    INDEX IX_Classes_OwnerId (OwnerId), INDEX IX_Classes_InviteCode (InviteCode)
);
```

### 4.2 ClassWords (sınıfa özel kelimeler — yalnızca üyeler görür)
```sql
CREATE TABLE ClassWords (
    Id INT PRIMARY KEY IDENTITY, ClassId INT NOT NULL, CreatedBy INT NOT NULL,
    GermanWord NVARCHAR(255) NOT NULL, TurkishTranslation NVARCHAR(500) NOT NULL,
    PartOfSpeech NVARCHAR(20) NULL, Notes NVARCHAR(MAX) NULL,
    Gender NVARCHAR(20) NULL, ArticleDefiniteNom NVARCHAR(10) NULL, PluralForm NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1, IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT CK_ClassWords_Gender CHECK (Gender IS NULL OR Gender IN ('Masculine','Feminine','Neuter')),
    INDEX IX_ClassWords_ClassId (ClassId)
);
```

### 4.3 ClassMemberships / ClassCategories / ClassUserCategories
```sql
CREATE TABLE ClassMemberships (
    Id INT PRIMARY KEY IDENTITY, ClassId INT NOT NULL, UserId INT NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), IsActive BIT NOT NULL DEFAULT 1,
    -- Sınıf içi alt rol YOK; sahiplik Classes.OwnerId ile belirlenir
    FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ClassMemberships UNIQUE (ClassId, UserId)
);
CREATE TABLE ClassCategories (
    Id INT PRIMARY KEY IDENTITY, ClassId INT NOT NULL, CategoryId INT NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT UQ_ClassCategories UNIQUE (ClassId, CategoryId)
);
CREATE TABLE ClassUserCategories (
    Id INT PRIMARY KEY IDENTITY, ClassId INT NOT NULL, UserCategoryId INT NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserCategoryId) REFERENCES UserCategories(Id),
    CONSTRAINT UQ_ClassUserCategories UNIQUE (ClassId, UserCategoryId)
);
```

### 4.4 Friendships
```sql
CREATE TABLE Friendships (
    Id INT PRIMARY KEY IDENTITY, RequesterId INT NOT NULL, ReceiverId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending|Accepted|Rejected|Blocked
    RequestedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (RequesterId) REFERENCES Users(Id), FOREIGN KEY (ReceiverId) REFERENCES Users(Id),
    CONSTRAINT UQ_Friendships UNIQUE (RequesterId, ReceiverId),
    CONSTRAINT CK_Friendships_Self CHECK (RequesterId <> ReceiverId),
    CONSTRAINT CK_Friendships_Status CHECK (Status IN ('Pending','Accepted','Rejected','Blocked'))
);
```

### 4.5 SharedContents / SharedContentImports
```sql
CREATE TABLE SharedContents (
    Id INT PRIMARY KEY IDENTITY, OwnerId INT NOT NULL,
    ShareToken NVARCHAR(36) NOT NULL UNIQUE,         -- UUID
    ContentType NVARCHAR(30) NOT NULL,               -- UserCard|UserCategory|Class
    ContentId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1, ExpiresAt DATETIME2 NULL, ViewCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_SharedContents_ContentType CHECK (ContentType IN ('UserCard','UserCategory','Class')),
    INDEX IX_SharedContents_ShareToken (ShareToken)
);
CREATE TABLE SharedContentImports (
    Id INT PRIMARY KEY IDENTITY, SharedContentId INT NOT NULL, ImportedByUserId INT NOT NULL,
    ImportedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (SharedContentId) REFERENCES SharedContents(Id),
    FOREIGN KEY (ImportedByUserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_SharedContentImports UNIQUE (SharedContentId, ImportedByUserId)
);
```

---

## 5. Seed Data (başlangıç kategorileri)
```sql
INSERT INTO Categories (NameDE, NameTR, DisplayOrder, MinLevel, Color, Icon) VALUES
('Menschen','İnsanlar',1,'A1','#FF6B6B','people'), ('Familie','Aile',2,'A1','#FF8C42','family'),
('Essen','Yemek',3,'A1','#95E1D3','food'), ('Haus','Ev',4,'A1','#4ECDC4','house'),
('Schule','Okul',5,'A1','#AA96DA','school'), ('Zahlen','Sayılar',6,'A1','#FCBAD3','numbers'),
('Farben','Renkler',7,'A1','#A8EDEA','colors'), ('Zeit','Zaman',8,'A1','#FFD89B','time'),
('Körperteile','Vücut',9,'A1','#FB7D5B','body'), ('Tiere','Hayvanlar',10,'A1','#84DCC6','animal'),
('Arbeit','İş',11,'A2','#F38181','work'), ('Reisen','Seyahat',12,'A2','#C7CEEA','travel');
```

---

## 6. Önemli Kurallar (uygulama katmanında zorunlu)

1. **UserProgress** (sistem) ve **UserCardProgress** (kişisel) ayrı tablolardır.
2. **LearningHistory.WordId** ve **UserCardId** ikisi birden NULL olamaz (uygulama kontrolü).
3. **UserCard** yalnızca sahibi tarafından görülür → her sorguda `UserId` filtresi.
4. **Sistem kelimesi eşleşmesi:** UserCard.FrontText, Words.GermanWord ile eşleşir ve kullanıcının
   o kelime için `UserProgress` kaydı varsa, Mixed sınav oturumunda o UserCard **atlanır** (UserProgress önceliklidir).
5. **ClassWords** yalnızca sınıf üyelerine görünür: `WHERE ClassId IN (kullanıcının üye olduğu sınıflar)`.
6. **Log tabloları** değişmezdir (insert-only); soft delete yok, güncellenmez.
