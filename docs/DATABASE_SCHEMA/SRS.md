# SRS Domain — İlerleme, Öğrenme Geçmişi, Başarımlar

> Genel kurallar (BaseEntity alanları, soft delete, UserId filtresi) → `../DATABASE_SCHEMA.md`.
> FK hedefi `Users` → `Auth.md`, `Words` → `Icerik.md`, `UserCards` → `Kisisel_Icerik.md`.

### UserProgress / UserCardProgress (SRS)
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
    NextReviewAt DATETIME2 NULL,                  -- NULL = hiç zamanlanmadı (yeni kelime havuzunda) — bkz. SRS_Domain.md "Yeni Kelime Seçim Algoritması"
    IntervalDays INT NOT NULL DEFAULT 1, RepetitionNumber INT NOT NULL DEFAULT 0,
    ConsecutiveIncorrect INT NOT NULL DEFAULT 0,  -- leech tespiti, quality>=3'te 0'a döner
    IsSuspended BIT NOT NULL DEFAULT 0,           -- leech "Askıya Al" — due sorgusundan hariç tutulur
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
    LastReviewedAt DATETIME2 NULL, NextReviewAt DATETIME2 NULL,
    IntervalDays INT NOT NULL DEFAULT 1, RepetitionNumber INT NOT NULL DEFAULT 0,
    ConsecutiveIncorrect INT NOT NULL DEFAULT 0, IsSuspended BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserCardProgress UNIQUE (UserId, UserCardId),
    INDEX IX_UserCardProgress_UserId_NextReviewAt (UserId, NextReviewAt)
);
```

### LearningHistory / LearningSessions
```sql
CREATE TABLE LearningSessions (
    Id INT PRIMARY KEY IDENTITY, UserId INT NOT NULL,
    SessionType NVARCHAR(50) NOT NULL,   -- Flashcard|MultipleChoice|ArticleQuiz|PluralQuiz|TranslationQuiz|TrueFalse
                                          -- Yeni kelime oturumu = Flashcard sabit; review oturumları = Mixed sabit
                                          -- (her sorunun gerçek formatı backend'de rastgele seçilir, bkz. LearningHistory.SessionType)
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
    LearningSessionId INT NULL, SessionType NVARCHAR(50) NULL,  -- bu sorunun rastgele atanan gerçek formatı
    IsCorrect BIT NOT NULL, ResponseTime INT NULL, TimeSpentSeconds INT NULL,
    UserResponse NVARCHAR(500) NULL, CorrectResponse NVARCHAR(500) NULL,
    HintUsed BIT NOT NULL DEFAULT 0,                -- ipucu (örnek cümle/harf/şık eleme) istendi mi — quality tavanını düşürür
    IsExtraPractice BIT NOT NULL DEFAULT 0,          -- "Aynı Kelimelerle Tekrar Et" cevabı mı — SM-2'yi güncellemez, sadece istatistik
    MasteryBefore DECIMAL(5,2) NULL, MasteryAfter DECIMAL(5,2) NULL,  -- yalnızca IsExtraPractice=0 iken doldurulur ("Bugün Test Ettiklerim" için)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (WordId) REFERENCES Words(Id),
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id),
    FOREIGN KEY (LearningSessionId) REFERENCES LearningSessions(Id),
    INDEX IX_LearningHistory_UserId (UserId), INDEX IX_LearningHistory_CreatedAt (CreatedAt DESC)
);
```

### Achievements / UserAchievements (gamification)
```sql
CREATE TABLE Achievements (
    Id INT PRIMARY KEY IDENTITY, Name NVARCHAR(100) NOT NULL, Description NVARCHAR(500) NULL,
    Icon NVARCHAR(255) NULL,   -- resim URL'i (WordConcepts.ImageUrl ile aynı desen), emoji değil
    RewardXP INT NOT NULL DEFAULT 0,
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
