# VERİTABANI ŞEMASI

## 1. Entity İlişki Diyagramı (ERD)

```
Users
 ├── RefreshTokens          (1:N)
 ├── UserProgress           (1:N) ── Words
 ├── LearningHistory        (1:N) ── Words
 ├── LearningSessions       (1:N)
 ├── UserAchievements       (1:N) ── Achievements
 ├── UserCards              (1:N) ── UserCardProgress (1:N)
 │                                ── UserCardExamples (1:N)
 │                                ── UserCardCategories (M:N) ── Categories
 │                                ── UserCardUserCategories (M:N) ── UserCategories
 └── UserCategories         (1:N)

Words
 ├── WordDetails            (1:1)
 ├── WordExamples           (1:N)
 └── WordCategories         (M:N) ── Categories

Categories
 └── Self-referencing (ParentCategoryId)
```

---

## 2. Tablolar

### 2.1 Users

```sql
CREATE TABLE Users (
    Id                    INT           PRIMARY KEY IDENTITY(1,1),

    -- Kimlik Doğrulama
    Email                 NVARCHAR(254) NOT NULL UNIQUE,
    PasswordHash          VARCHAR(60)   NULL,     -- bcrypt (60 karakter) — sosyal girişte NULL olabilir

    -- Sosyal Giriş
    GoogleId              NVARCHAR(255) NULL,      -- Google ile girişte kullanıcı ID'si
    AppleId               NVARCHAR(255) NULL,      -- Apple ile girişte kullanıcı ID'si
    AuthProvider          NVARCHAR(20)  NOT NULL DEFAULT 'Local',  -- Local | Google | Apple

    -- Profil
    FirstName             NVARCHAR(50)  NOT NULL,
    LastName              NVARCHAR(50)  NOT NULL,
    DisplayName           NVARCHAR(100) NULL,
    AvatarUrl             NVARCHAR(500) NULL,

    -- Dil Tercihleri
    PreferredLanguagePair NVARCHAR(10)  NOT NULL DEFAULT 'TR-DE',  -- TR-DE, EN-DE vb.
    PreferredUILanguage   NVARCHAR(5)   NOT NULL DEFAULT 'tr',

    -- Öğrenme İstatistikleri
    CurrentLevel          NVARCHAR(2)   NOT NULL DEFAULT 'A1',
    TotalXP               INT           NOT NULL DEFAULT 0,
    LifetimeXP            INT           NOT NULL DEFAULT 0,
    TotalLearningMinutes  INT           NOT NULL DEFAULT 0,
    StreakDays            INT           NOT NULL DEFAULT 0,
    LastStreakDate        DATETIME2     NULL,

    -- Hesap Durumu
    IsActive              BIT           NOT NULL DEFAULT 1,
    IsEmailVerified       BIT           NOT NULL DEFAULT 0,
    EmailVerifiedAt       DATETIME2     NULL,
    LastLoginAt           DATETIME2     NULL,
    LastLoginIP           VARCHAR(45)   NULL,
    LoginCount            INT           NOT NULL DEFAULT 0,

    -- Rol
    Role                  NVARCHAR(20)  NOT NULL DEFAULT 'User',  -- User, Instructor, Admin

    -- Soft Delete
    IsDeleted             BIT           NOT NULL DEFAULT 0,
    DeletedAt             DATETIME2     NULL,
    CreatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT CK_Users_Level CHECK (CurrentLevel IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_Users_Role  CHECK (Role IN ('User','Instructor','Admin')),
    CONSTRAINT CK_Users_AuthProvider CHECK (AuthProvider IN ('Local','Google','Apple')),

    INDEX IX_Users_Email      (Email),
    INDEX IX_Users_IsDeleted  (IsDeleted),
    INDEX IX_Users_Role       (Role),
    INDEX IX_Users_GoogleId   (GoogleId),
    INDEX IX_Users_AppleId    (AppleId)
);
```

---

### 2.2 RefreshTokens

```sql
CREATE TABLE RefreshTokens (
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    UserId       INT           NOT NULL,
    TokenHash    VARCHAR(88)   NOT NULL,   -- SHA-256 hash (güvenlik)
    TokenFamily  NVARCHAR(36)  NOT NULL,   -- GUID — Token Family Pattern
    ExpiresAt    DATETIME2     NOT NULL,
    IsUsed       BIT           NOT NULL DEFAULT 0,
    RevokedAt    DATETIME2     NULL,
    DeviceInfo   NVARCHAR(500) NULL,
    IpAddress    VARCHAR(45)   NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,

    INDEX IX_RefreshTokens_UserId     (UserId),
    INDEX IX_RefreshTokens_TokenHash  (TokenHash),
    INDEX IX_RefreshTokens_TokenFamily(TokenFamily)
);
```

---

### 2.3 Words (Sistem Kelimeleri — Admin Ekler)

```sql
CREATE TABLE Words (
    Id                  INT           PRIMARY KEY IDENTITY(1,1),
    GermanWord          NVARCHAR(255) NOT NULL,
    TurkishTranslation  NVARCHAR(500) NOT NULL,
    EnglishTranslation  NVARCHAR(500) NULL,       -- Gelecekteki dil desteği için
    PartOfSpeech        NVARCHAR(20)  NOT NULL,    -- Noun, Verb, Adjective, Adverb, vb.
    DifficultyLevel     NVARCHAR(2)   NOT NULL,    -- A1, A2, B1, B2, C1, C2
    Definition          NVARCHAR(MAX) NULL,
    IsActive            BIT           NOT NULL DEFAULT 1,
    ApprovedBy          INT           NULL,
    ApprovedAt          DATETIME2     NULL,
    CreatedBy           INT           NULL,
    UpdatedBy           INT           NULL,

    -- Soft Delete
    IsDeleted           BIT           NOT NULL DEFAULT 0,
    DeletedAt           DATETIME2     NULL,
    CreatedAt           DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (ApprovedBy) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy)  REFERENCES Users(Id),
    FOREIGN KEY (UpdatedBy)  REFERENCES Users(Id),

    CONSTRAINT CK_Words_Level       CHECK (DifficultyLevel IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_Words_PartOfSpeech CHECK (PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')),

    INDEX IX_Words_GermanWord      (GermanWord),
    INDEX IX_Words_DifficultyLevel (DifficultyLevel),
    INDEX IX_Words_PartOfSpeech    (PartOfSpeech),
    INDEX IX_Words_IsActive        (IsActive)
);
```

---

### 2.4 WordDetails (Almanca Gramer Bilgisi — 1:1 Words)

```sql
CREATE TABLE WordDetails (
    Id                    INT           PRIMARY KEY IDENTITY(1,1),
    WordId                INT           NOT NULL UNIQUE,  -- 1:1 ilişki

    -- Cinsiyet
    Gender                NVARCHAR(20)  NULL,  -- Masculine, Feminine, Neuter (fiillerde NULL)

    -- Belirli Artikeller (4 Hâl)
    ArticleDefiniteNom    NVARCHAR(10)  NULL,  -- der / die / das
    ArticleDefiniteAcc    NVARCHAR(10)  NULL,  -- den / die / das
    ArticleDefiniteDat    NVARCHAR(10)  NULL,  -- dem / der / dem
    ArticleDefiniteGen    NVARCHAR(10)  NULL,  -- des / der / des

    -- Belirsiz Artikeller (4 Hâl)
    ArticleIndefiniteNom  NVARCHAR(10)  NULL,  -- ein / eine / ein
    ArticleIndefiniteAcc  NVARCHAR(10)  NULL,  -- einen / eine / ein
    ArticleIndefiniteDat  NVARCHAR(10)  NULL,  -- einem / einer / einem
    ArticleIndefiniteGen  NVARCHAR(10)  NULL,  -- eines / einer / eines

    -- Tekil Hâl Formları (İsimler)
    FormNominative        NVARCHAR(255) NULL,
    FormAccusative        NVARCHAR(255) NULL,
    FormDative            NVARCHAR(255) NULL,
    FormGenitive          NVARCHAR(255) NULL,

    -- Çoğul Formlar
    PluralForm            NVARCHAR(255) NULL,
    PluralFormNominative  NVARCHAR(255) NULL,
    PluralFormAccusative  NVARCHAR(255) NULL,
    PluralFormDative      NVARCHAR(255) NULL,  -- Genellikle Nominative + -n
    PluralFormGenitive    NVARCHAR(255) NULL,

    -- Fiil Çekimleri (JSON — tüm zamanlar ve kişiler)
    -- Yapı: { "present": { "ich":"gehe", "du":"gehst", ... }, "preterite": {...}, ... }
    ConjugationData       NVARCHAR(MAX) NULL,

    -- Ayrılabilir Fiiller
    IsSeparableVerb       BIT           NOT NULL DEFAULT 0,
    SeparablePrefix       NVARCHAR(50)  NULL,  -- an, auf, ein, mit, vb.

    -- Telaffuz ve Medya
    Pronunciation         NVARCHAR(500) NULL,  -- IPA notasyonu
    AudioUrl              NVARCHAR(500) NULL,
    ImageUrl              NVARCHAR(500) NULL,

    -- Notlar
    Notes                 NVARCHAR(MAX) NULL,
    CommonMistakes        NVARCHAR(MAX) NULL,

    CreatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt             DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,

    INDEX IX_WordDetails_Gender          (Gender),
    INDEX IX_WordDetails_IsSeparableVerb (IsSeparableVerb)
);
```

**ConjugationData JSON Yapısı:**
```json
{
  "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
  "preterite":  { "ich":"ging", "du":"gingst", "erSieEs":"ging", "wir":"gingen", "ihr":"gingt", "sie":"gingen" },
  "perfect":    { "ich":"bin gegangen", "du":"bist gegangen", "erSieEs":"ist gegangen", "wir":"sind gegangen", "ihr":"seid gegangen", "sie":"sind gegangen" },
  "pastParticiple": "gegangen",
  "auxiliaryVerb":  "sein"
}
```

---

### 2.5 WordExamples (Seviyeli Örnek Cümleler — 1:N Words)

```sql
CREATE TABLE WordExamples (
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    WordId       INT           NOT NULL,

    SentenceDE   NVARCHAR(MAX) NOT NULL,  -- Almanca cümle
    SentenceTR   NVARCHAR(MAX) NOT NULL,  -- Türkçe çeviri
    SentenceEN   NVARCHAR(MAX) NULL,      -- İngilizce çeviri (opsiyonel)

    -- Kullanıcı seviyesine göre doğru cümle gösterilir
    Level        NVARCHAR(2)   NOT NULL DEFAULT 'A1',

    -- Cümle türü
    -- Normal     : standart kullanım
    -- Idiom      : deyimsel kullanım (z.B. "Das geht mir auf die Nerven")
    -- Formal     : resmi dil
    -- Colloquial : günlük konuşma
    ExampleType  NVARCHAR(20)  NOT NULL DEFAULT 'Normal',

    DisplayOrder INT           NOT NULL DEFAULT 0,  -- Sıralama
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedBy    INT           NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (WordId)    REFERENCES Words(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),

    CONSTRAINT CK_WordExamples_Level       CHECK (Level IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_WordExamples_ExampleType CHECK (ExampleType IN ('Normal','Idiom','Formal','Colloquial')),

    INDEX IX_WordExamples_WordId       (WordId),
    INDEX IX_WordExamples_Level        (Level),
    INDEX IX_WordExamples_WordId_Level (WordId, Level)
);
```

---

### 2.6 Categories (Sistem Kategorileri — Admin Ekler)

```sql
CREATE TABLE Categories (
    Id               INT           PRIMARY KEY IDENTITY(1,1),
    NameDE           NVARCHAR(100) NOT NULL,
    NameTR           NVARCHAR(100) NOT NULL,
    NameEN           NVARCHAR(100) NULL,
    DescriptionTR    NVARCHAR(MAX) NULL,
    ParentCategoryId INT           NULL,   -- Hiyerarşi için kendi kendine referans
    DisplayOrder     INT           NOT NULL DEFAULT 0,
    Icon             NVARCHAR(100) NULL,
    Color            NVARCHAR(10)  NULL,   -- Hex renk (#FF5733)
    MinLevel         NVARCHAR(2)   NULL,   -- Bu kategori en az hangi seviyede gösterilsin
    MaxLevel         NVARCHAR(2)   NULL,
    IsActive         BIT           NOT NULL DEFAULT 1,
    IsDeleted        BIT           NOT NULL DEFAULT 0,
    DeletedAt        DATETIME2     NULL,
    CreatedAt        DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id),

    INDEX IX_Categories_ParentCategoryId (ParentCategoryId),
    INDEX IX_Categories_IsActive         (IsActive)
);
```

---

### 2.7 WordCategories (Word ↔ Category M:N)

```sql
CREATE TABLE WordCategories (
    Id           INT       PRIMARY KEY IDENTITY(1,1),
    WordId       INT       NOT NULL,
    CategoryId   INT       NOT NULL,
    DisplayOrder INT       NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (WordId)     REFERENCES Words(Id)      ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_WordCategories UNIQUE (WordId, CategoryId),

    INDEX IX_WordCategories_CategoryId (CategoryId),
    INDEX IX_WordCategories_WordId     (WordId)
);
```

---

### 2.8 UserCards (Kişisel Kartlar — Kullanıcı Oluşturur)

```sql
CREATE TABLE UserCards (
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    UserId       INT           NOT NULL,   -- Kart sahibi

    -- Kart İçeriği (Serbest format — dil kısıtı yok)
    FrontText    NVARCHAR(500) NOT NULL,   -- Ön yüz (örn: Almanca kelime)
    BackText     NVARCHAR(500) NOT NULL,   -- Arka yüz (örn: Türkçe çeviri)
    Notes        NVARCHAR(MAX) NULL,       -- Ek notlar

    -- İsteğe bağlı medya
    ImageUrl     NVARCHAR(500) NULL,
    AudioUrl     NVARCHAR(500) NULL,

    IsActive     BIT           NOT NULL DEFAULT 1,
    IsDeleted    BIT           NOT NULL DEFAULT 0,
    DeletedAt    DATETIME2     NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,

    INDEX IX_UserCards_UserId    (UserId),
    INDEX IX_UserCards_IsDeleted (IsDeleted)
);
```

---

### 2.9 UserCardExamples (Kişisel Kart Örnek Cümleleri)

```sql
CREATE TABLE UserCardExamples (
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    UserCardId   INT           NOT NULL,
    SentenceFront NVARCHAR(MAX) NOT NULL,  -- Örnek cümle (ön dil)
    SentenceBack  NVARCHAR(MAX) NOT NULL,  -- Örnek cümle (arka dil)
    DisplayOrder INT           NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE,

    INDEX IX_UserCardExamples_UserCardId (UserCardId)
);
```

---

### 2.10 UserCategories (Kişisel Kategoriler — Kullanıcı Oluşturur)

```sql
CREATE TABLE UserCategories (
    Id           INT           PRIMARY KEY IDENTITY(1,1),
    UserId       INT           NOT NULL,
    Name         NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(500) NULL,
    Color        NVARCHAR(10)  NULL,
    Icon         NVARCHAR(100) NULL,
    IsDeleted    BIT           NOT NULL DEFAULT 0,
    DeletedAt    DATETIME2     NULL,
    CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,

    INDEX IX_UserCategories_UserId (UserId)
);
```

---

### 2.11 UserCardCategories (UserCard ↔ Category M:N — Sistem Kategorisi)

```sql
CREATE TABLE UserCardCategories (
    Id           INT       PRIMARY KEY IDENTITY(1,1),
    UserCardId   INT       NOT NULL,
    CategoryId   INT       NOT NULL,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserCardId)  REFERENCES UserCards(Id)   ON DELETE CASCADE,
    FOREIGN KEY (CategoryId)  REFERENCES Categories(Id),

    CONSTRAINT UQ_UserCardCategories UNIQUE (UserCardId, CategoryId),

    INDEX IX_UserCardCategories_UserCardId  (UserCardId),
    INDEX IX_UserCardCategories_CategoryId  (CategoryId)
);
```

---

### 2.12 UserCardUserCategories (UserCard ↔ UserCategory M:N — Kişisel Kategori)

```sql
CREATE TABLE UserCardUserCategories (
    Id               INT       PRIMARY KEY IDENTITY(1,1),
    UserCardId       INT       NOT NULL,
    UserCategoryId   INT       NOT NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserCardId)     REFERENCES UserCards(Id)      ON DELETE CASCADE,
    FOREIGN KEY (UserCategoryId) REFERENCES UserCategories(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_UserCardUserCategories UNIQUE (UserCardId, UserCategoryId),

    INDEX IX_UserCardUserCategories_UserCardId     (UserCardId),
    INDEX IX_UserCardUserCategories_UserCategoryId (UserCategoryId)
);
```

---

### 2.13 UserProgress (Sistem Kelimesi İlerlemesi — SRS)

```sql
CREATE TABLE UserProgress (
    Id                INT           PRIMARY KEY IDENTITY(1,1),
    UserId            INT           NOT NULL,
    WordId            INT           NOT NULL,

    -- Mastery (0=Hiç görülmedi, 5=Otomatik hatırlama)
    CurrentLevel      INT           NOT NULL DEFAULT 0,
    Mastery           DECIMAL(5,2)  NOT NULL DEFAULT 0,

    -- İstatistikler
    TimesCorrect      INT           NOT NULL DEFAULT 0,
    TimesIncorrect    INT           NOT NULL DEFAULT 0,
    TotalAttempts     INT           NOT NULL DEFAULT 0,
    SuccessRate       DECIMAL(5,2)  NOT NULL DEFAULT 0,

    -- SRS (Spaced Repetition System)
    LastReviewedAt    DATETIME2     NULL,
    NextReviewAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IntervalDays      INT           NOT NULL DEFAULT 1,
    RepetitionNumber  INT           NOT NULL DEFAULT 0,

    CreatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_UserProgress UNIQUE (UserId, WordId),

    INDEX IX_UserProgress_UserId                  (UserId),
    INDEX IX_UserProgress_WordId                  (WordId),
    INDEX IX_UserProgress_UserId_NextReviewAt     (UserId, NextReviewAt),
    INDEX IX_UserProgress_CurrentLevel            (CurrentLevel)
);
```

---

### 2.14 UserCardProgress (Kişisel Kart İlerlemesi — SRS)

```sql
CREATE TABLE UserCardProgress (
    Id                INT           PRIMARY KEY IDENTITY(1,1),
    UserId            INT           NOT NULL,
    UserCardId        INT           NOT NULL,

    CurrentLevel      INT           NOT NULL DEFAULT 0,
    Mastery           DECIMAL(5,2)  NOT NULL DEFAULT 0,
    TimesCorrect      INT           NOT NULL DEFAULT 0,
    TimesIncorrect    INT           NOT NULL DEFAULT 0,
    TotalAttempts     INT           NOT NULL DEFAULT 0,
    SuccessRate       DECIMAL(5,2)  NOT NULL DEFAULT 0,

    LastReviewedAt    DATETIME2     NULL,
    NextReviewAt      DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IntervalDays      INT           NOT NULL DEFAULT 1,
    RepetitionNumber  INT           NOT NULL DEFAULT 0,

    CreatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId)     REFERENCES Users(Id)     ON DELETE CASCADE,
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_UserCardProgress UNIQUE (UserId, UserCardId),

    INDEX IX_UserCardProgress_UserId              (UserId),
    INDEX IX_UserCardProgress_UserCardId          (UserCardId),
    INDEX IX_UserCardProgress_UserId_NextReviewAt (UserId, NextReviewAt)
);
```

---

### 2.15 LearningHistory (Tüm Girişim Kayıtları)

```sql
CREATE TABLE LearningHistory (
    Id                INT           PRIMARY KEY IDENTITY(1,1),
    UserId            INT           NOT NULL,

    -- Hangi kelime veya kart? (İkisi birden NULL olamaz, biri dolu olmalı)
    WordId            INT           NULL,      -- Sistem kelimesi
    UserCardId        INT           NULL,      -- Kişisel kart

    LearningSessionId INT           NULL,
    SessionType       NVARCHAR(50)  NULL,      -- Flashcard, MultipleChoice, ArticleQuiz, vb.
    IsCorrect         BIT           NOT NULL,
    ResponseTime      INT           NULL,      -- Milisaniye
    TimeSpentSeconds  INT           NULL,
    UserResponse      NVARCHAR(500) NULL,
    CorrectResponse   NVARCHAR(500) NULL,
    CreatedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId)            REFERENCES Users(Id)            ON DELETE CASCADE,
    FOREIGN KEY (WordId)            REFERENCES Words(Id),
    FOREIGN KEY (UserCardId)        REFERENCES UserCards(Id),
    FOREIGN KEY (LearningSessionId) REFERENCES LearningSessions(Id),

    INDEX IX_LearningHistory_UserId            (UserId),
    INDEX IX_LearningHistory_WordId            (WordId),
    INDEX IX_LearningHistory_UserCardId        (UserCardId),
    INDEX IX_LearningHistory_CreatedAt         (CreatedAt DESC),
    INDEX IX_LearningHistory_LearningSessionId (LearningSessionId)
);
```

---

### 2.16 LearningSessions (Oturum Özetleri)

```sql
CREATE TABLE LearningSessions (
    Id              INT           PRIMARY KEY IDENTITY(1,1),
    UserId          INT           NOT NULL,

    -- Kapsam Filtreleri (kullanıcının seçtiği)
    SessionType     NVARCHAR(50)  NOT NULL,  -- Flashcard, MultipleChoice, ArticleQuiz, PluralQuiz, TranslationQuiz
    SourceType      NVARCHAR(20)  NOT NULL,  -- SystemWords, UserCards, Mixed
    LevelFilter     NVARCHAR(2)   NULL,      -- A1, A2, vb. (NULL = tüm seviyeler)
    CategoryIds     NVARCHAR(MAX) NULL,      -- JSON dizi: [1, 3, 5] (birden fazla kategori)
    UserCategoryIds NVARCHAR(MAX) NULL,      -- JSON dizi: kişisel kategoriler

    -- Zaman
    StartedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt     DATETIME2     NULL,
    DurationSeconds INT           NULL,

    -- Sonuçlar
    TotalWords      INT           NOT NULL DEFAULT 0,
    CorrectAnswers  INT           NOT NULL DEFAULT 0,
    IncorrectAnswers INT          NOT NULL DEFAULT 0,
    SuccessRate     DECIMAL(5,2)  NULL,
    XPEarned        INT           NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Active',  -- Active, Completed, Abandoned

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,

    CONSTRAINT CK_LearningSessions_Status CHECK (Status IN ('Active','Completed','Abandoned')),

    INDEX IX_LearningSessions_UserId    (UserId),
    INDEX IX_LearningSessions_StartedAt (StartedAt DESC),
    INDEX IX_LearningSessions_Status    (Status)
);
```

---

### 2.17 Achievements ve UserAchievements

```sql
CREATE TABLE Achievements (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Icon        NVARCHAR(255) NULL,
    RewardXP    INT           NOT NULL DEFAULT 0,
    Rarity      NVARCHAR(20)  NOT NULL DEFAULT 'Common',  -- Common, Rare, Epic, Legendary
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE UserAchievements (
    Id            INT       PRIMARY KEY IDENTITY(1,1),
    UserId        INT       NOT NULL,
    AchievementId INT       NOT NULL,
    UnlockedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId)        REFERENCES Users(Id)        ON DELETE CASCADE,
    FOREIGN KEY (AchievementId) REFERENCES Achievements(Id),

    CONSTRAINT UQ_UserAchievements UNIQUE (UserId, AchievementId),

    INDEX IX_UserAchievements_UserId (UserId)
);
```

---

### 2.18 AuditLog

```sql
CREATE TABLE AuditLog (
    Id         BIGINT        PRIMARY KEY IDENTITY(1,1),
    UserId     INT           NULL,
    Action     NVARCHAR(100) NOT NULL,  -- LOGIN, REGISTER, CREATE_WORD, DELETE_USER, vb.
    TableName  NVARCHAR(50)  NULL,
    RecordId   INT           NULL,
    OldValue   NVARCHAR(MAX) NULL,
    NewValue   NVARCHAR(MAX) NULL,
    IpAddress  VARCHAR(45)   NULL,
    UserAgent  NVARCHAR(500) NULL,
    Status     NVARCHAR(20)  NULL,      -- Success, Failure
    CreatedAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,

    INDEX IX_AuditLog_UserId    (UserId),
    INDEX IX_AuditLog_Action    (Action),
    INDEX IX_AuditLog_CreatedAt (CreatedAt DESC)
);
```

---

## 3. Başlangıç Verileri (Seed Data)

```sql
-- Sistem kategorileri
INSERT INTO Categories (NameDE, NameTR, NameEN, DisplayOrder, MinLevel, Color, Icon)
VALUES
('Menschen',      'İnsanlar',        'People',      1,  'A1', '#FF6B6B', 'people'),
('Familie',       'Aile',            'Family',      2,  'A1', '#FF8C42', 'family'),
('Essen',         'Yemek',           'Food',        3,  'A1', '#95E1D3', 'food'),
('Haus',          'Ev',              'House',       4,  'A1', '#4ECDC4', 'house'),
('Schule',        'Okul',            'School',      5,  'A1', '#AA96DA', 'school'),
('Zahlen',        'Sayılar',         'Numbers',     6,  'A1', '#FCBAD3', 'numbers'),
('Farben',        'Renkler',         'Colors',      7,  'A1', '#A8EDEA', 'colors'),
('Zeit',          'Zaman',           'Time',        8,  'A1', '#FFD89B', 'time'),
('Körperteile',   'Vücut Bölümleri', 'Body Parts',  9,  'A1', '#FB7D5B', 'body'),
('Tiere',         'Hayvanlar',       'Animals',     10, 'A1', '#84DCC6', 'animal'),
('Arbeit',        'İş',              'Work',        11, 'A2', '#F38181', 'work'),
('Reisen',        'Seyahat',         'Travel',      12, 'A2', '#C7CEEA', 'travel');
```

---

## 4. Önemli Kısıtlamalar

```
1. UserProgress ve UserCardProgress ayrı tablolardır.
   - Sistem kelimeleri → UserProgress
   - Kişisel kartlar   → UserCardProgress

2. LearningHistory.WordId ve LearningHistory.UserCardId
   ikisi birden NULL olamaz — uygulama katmanında kontrol edilir.

3. UserCard sadece sahibi tarafından görülebilir.
   - Repository sorguları her zaman UserId filtresi içerir.

4. Bir UserCard hem sistem kategorisine (Categories)
   hem de kişisel kategoriye (UserCategories) bağlanabilir.

5. LearningSessions.CategoryIds JSON dizi olarak saklanır
   çünkü kullanıcı aynı anda birden fazla kategori seçebilir.
```

---

## 5. Sosyal Özellikler — Yeni Tablolar

### 5.1 Classes (Sınıflar)

```sql
CREATE TABLE Classes (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    OwnerId     INT           NOT NULL,         -- Sınıfı oluşturan kullanıcı
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    InviteCode  NVARCHAR(20)  NOT NULL UNIQUE,  -- Katılım kodu (örn: "ABC123")
    IsActive    BIT           NOT NULL DEFAULT 1,
    IsDeleted   BIT           NOT NULL DEFAULT 0,
    DeletedAt   DATETIME2     NULL,
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (OwnerId) REFERENCES Users(Id),

    INDEX IX_Classes_OwnerId    (OwnerId),
    INDEX IX_Classes_InviteCode (InviteCode)
);
```

---

### 5.2 ClassMemberships (Sınıf Üyelikleri)

```sql
CREATE TABLE ClassMemberships (
    Id        INT           PRIMARY KEY IDENTITY(1,1),
    ClassId   INT           NOT NULL,
    UserId    INT           NOT NULL,
    Role      NVARCHAR(20)  NOT NULL DEFAULT 'Student',  -- Student, Teacher
    JoinedAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    IsActive  BIT           NOT NULL DEFAULT 1,

    FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId)  REFERENCES Users(Id)   ON DELETE CASCADE,

    CONSTRAINT UQ_ClassMemberships UNIQUE (ClassId, UserId),

    INDEX IX_ClassMemberships_ClassId (ClassId),
    INDEX IX_ClassMemberships_UserId  (UserId)
);
```

---

### 5.3 ClassCategories (Sınıf ↔ Sistem Kategorisi M:N)

```sql
CREATE TABLE ClassCategories (
    Id           INT       PRIMARY KEY IDENTITY(1,1),
    ClassId      INT       NOT NULL,
    CategoryId   INT       NOT NULL,
    DisplayOrder INT       NOT NULL DEFAULT 0,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (ClassId)    REFERENCES Classes(Id)    ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),

    CONSTRAINT UQ_ClassCategories UNIQUE (ClassId, CategoryId),

    INDEX IX_ClassCategories_ClassId    (ClassId),
    INDEX IX_ClassCategories_CategoryId (CategoryId)
);
```

---

### 5.4 ClassUserCategories (Sınıf ↔ Kişisel Kategori M:N)

```sql
CREATE TABLE ClassUserCategories (
    Id               INT       PRIMARY KEY IDENTITY(1,1),
    ClassId          INT       NOT NULL,
    UserCategoryId   INT       NOT NULL,
    DisplayOrder     INT       NOT NULL DEFAULT 0,
    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (ClassId)        REFERENCES Classes(Id)        ON DELETE CASCADE,
    FOREIGN KEY (UserCategoryId) REFERENCES UserCategories(Id),

    CONSTRAINT UQ_ClassUserCategories UNIQUE (ClassId, UserCategoryId),

    INDEX IX_ClassUserCategories_ClassId        (ClassId),
    INDEX IX_ClassUserCategories_UserCategoryId (UserCategoryId)
);
```

---

### 5.5 Friendships (Arkadaşlıklar)

```sql
CREATE TABLE Friendships (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    RequesterId INT           NOT NULL,    -- İsteği gönderen
    ReceiverId  INT           NOT NULL,    -- İsteği alan
    Status      NVARCHAR(20)  NOT NULL DEFAULT 'Pending',  -- Pending, Accepted, Rejected, Blocked
    RequestedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (RequesterId) REFERENCES Users(Id),
    FOREIGN KEY (ReceiverId)  REFERENCES Users(Id),

    CONSTRAINT UQ_Friendships      UNIQUE (RequesterId, ReceiverId),
    CONSTRAINT CK_Friendships_Self CHECK  (RequesterId <> ReceiverId),
    CONSTRAINT CK_Friendships_Status CHECK (Status IN ('Pending','Accepted','Rejected','Blocked')),

    INDEX IX_Friendships_RequesterId (RequesterId),
    INDEX IX_Friendships_ReceiverId  (ReceiverId),
    INDEX IX_Friendships_Status      (Status)
);
```

---

### 5.6 SharedContents (Paylaşım Linkleri)

```sql
CREATE TABLE SharedContents (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    OwnerId     INT           NOT NULL,
    ShareToken  NVARCHAR(36)  NOT NULL UNIQUE,  -- UUID — link'te kullanılır
    ContentType NVARCHAR(30)  NOT NULL,          -- UserCard | UserCategory | Class
    ContentId   INT           NOT NULL,          -- İlgili kaydın ID'si
    IsActive    BIT           NOT NULL DEFAULT 1,
    ExpiresAt   DATETIME2     NULL,              -- NULL = sonsuz
    ViewCount   INT           NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE CASCADE,

    CONSTRAINT CK_SharedContents_ContentType
        CHECK (ContentType IN ('UserCard','UserCategory','Class')),

    INDEX IX_SharedContents_OwnerId    (OwnerId),
    INDEX IX_SharedContents_ShareToken (ShareToken)
);
```

**Paylaşım Linki Örneği:**
```
https://app.wordlearner.com/share/550e8400-e29b-41d4-a716-446655440000
                                        └── ShareToken (UUID)
```

---

### 5.7 SharedContentImports (Paylaşım Kopyalamaları)

```sql
-- Bir kullanıcı paylaşım linkinden içerik eklediğinde kaydedilir
CREATE TABLE SharedContentImports (
    Id               INT       PRIMARY KEY IDENTITY(1,1),
    SharedContentId  INT       NOT NULL,
    ImportedByUserId INT       NOT NULL,
    ImportedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    FOREIGN KEY (SharedContentId)  REFERENCES SharedContents(Id),
    FOREIGN KEY (ImportedByUserId) REFERENCES Users(Id) ON DELETE CASCADE,

    CONSTRAINT UQ_SharedContentImports UNIQUE (SharedContentId, ImportedByUserId),

    INDEX IX_SharedContentImports_SharedContentId  (SharedContentId),
    INDEX IX_SharedContentImports_ImportedByUserId (ImportedByUserId)
);
```

---

## 6. Görünürlük Kuralları (Uygulama Katmanında Zorunlu)

```
Sistem Kelimeleri (Words):
  → Tüm aktif kullanıcılar görebilir

Kullanıcı Kartı (UserCard):
  → Varsayılan: Sadece sahibi
  → SharedContent kaydı varsa: Linki olanlar önizleyebilir
  → Sınıfa eklenirse: Sınıf üyeleri görebilir
  → Kabul edilen arkadaşlar görebilir (sahibi "arkadaşlarla paylaş" seçtiyse)

Kullanıcı Kategorisi (UserCategory):
  → Aynı kurallar

Sınıf (Class):
  → ClassMembership kaydı olanlar görebilir
  → InviteCode veya SharedContent linki ile katılınabilir

Repository katmanında her sorguda UserId filtresi ZORUNLUDUR.
```
