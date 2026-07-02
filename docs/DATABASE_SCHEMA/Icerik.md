# İçerik Domain — Sistem Kelimeleri ve Kategoriler

> Genel kurallar (BaseEntity alanları, soft delete) → `../DATABASE_SCHEMA.md`. FK hedefi `Users` → `Auth.md`.

### Words (Sistem Kelimeleri — yalnızca Admin ekler)
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

### WordDetails (Almanca gramer — 1:1 Words)
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
    CONSTRAINT CK_WordDetails_Gender CHECK (Gender IS NULL OR Gender IN ('Masculine','Feminine','Neuter')),
    INDEX IX_WordDetails_Gender (Gender)
);
```
**ConjugationData JSON:** `{ "present":{"ich":"gehe",...}, "preterite":{...}, "perfect":{...}, "pastParticiple":"gegangen", "auxiliaryVerb":"sein" }`

### WordExamples (seviyeli örnek cümleler — 1:N Words)
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

### Categories (sistem kategorileri — hiyerarşik)
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

### WordCategories (Word ↔ Category M:N)
```sql
CREATE TABLE WordCategories (
    Id INT PRIMARY KEY IDENTITY, WordId INT NOT NULL, CategoryId INT NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WordCategories UNIQUE (WordId, CategoryId)
);
```
