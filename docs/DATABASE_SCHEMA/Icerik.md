# İçerik Domain — Sistem Kelimeleri ve Kategoriler

> Genel + çoklu dil kuralları → `../CLAUDE.md §1`. Bu dosyadaki tüm tablolar **yalnızca Admin yazar**,
> giriş yapmış herkes salt-okur. Kullanıcının kendi kartları ayrı → `Kisisel_Icerik.md`.
> Bir kelime `WordConcept` (kavram) + her dile bir `Words` satırıdır; `de`+`tr` **aynı işlemde** girilir.

### Languages
```sql
CREATE TABLE Languages (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(5) NOT NULL,           -- ISO 639-1: 'de','tr','en'...
    Name NVARCHAR(50) NOT NULL,          -- 'German','Turkish'
    NativeName NVARCHAR(50) NOT NULL,    -- 'Deutsch','Türkçe'
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Languages_Code UNIQUE (Code)
);
-- Seed: yalnızca de+tr. Üçüncü dil = tek satırlık INSERT, migration gerekmez.
INSERT INTO Languages (Code, Name, NativeName, DisplayOrder) VALUES
('de','German','Deutsch',1), ('tr','Turkish','Türkçe',2);
```

### WordConcepts (dilden bağımsız kavram — kategori/seviye burada)
```sql
CREATE TABLE WordConcepts (
    Id INT PRIMARY KEY IDENTITY,
    PartOfSpeech NVARCHAR(20) NOT NULL,   -- Noun|Verb|Adjective|Adverb|Conjunction|Preposition|Pronoun|Other
    DifficultyLevel NVARCHAR(2) NOT NULL, -- A1..C2
    ImageUrl NVARCHAR(500) NULL,          -- görsel dilden bağımsız
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy INT NULL, UpdatedBy INT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_WordConcepts_Level CHECK (DifficultyLevel IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_WordConcepts_PartOfSpeech CHECK (PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')),
    INDEX IX_WordConcepts_DifficultyLevel (DifficultyLevel)
);
```

### Words (bir kavramın tek dildeki karşılığı)
```sql
CREATE TABLE Words (
    Id INT PRIMARY KEY IDENTITY,
    WordConceptId INT NOT NULL,
    LanguageId INT NOT NULL,
    Text NVARCHAR(255) NOT NULL,          -- 'Tisch' (de) / 'masa' (tr)
    Definition NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordConceptId) REFERENCES WordConcepts(Id) ON DELETE CASCADE,
    FOREIGN KEY (LanguageId) REFERENCES Languages(Id),
    CONSTRAINT UQ_Words_Concept_Language UNIQUE (WordConceptId, LanguageId),
    INDEX IX_Words_LanguageId_Text (LanguageId, Text)
);
```

### WordDetails (dile özel gramer — 1:1 Words)
```sql
CREATE TABLE WordDetails (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL UNIQUE,
    Pronunciation NVARCHAR(500) NULL,        -- IPA
    AudioUrl NVARCHAR(500) NULL,
    Notes NVARCHAR(MAX) NULL, CommonMistakes NVARCHAR(MAX) NULL,
    GrammarData NVARCHAR(MAX) NULL,          -- JSON — şekli Words.LanguageId'ye göre değişir
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE
);
```
> **GrammarData JSON şeması dil bazında:** de → `REFERENCE/GERMAN_LANGUAGE_FEATURES.md`, tr → `REFERENCE/TURKISH_LANGUAGE_FEATURES.md`, en → `REFERENCE/ENGLISH_LANGUAGE_FEATURES.md` (tanımlı ama henüz kullanılmıyor).
> **Trade-off:** `Gender` üzerinde DB `CHECK`/`INDEX` yok; "tüm maskülinleri getir" gibi filtreler uygulama katmanında (`JSON_VALUE`). Bu ölçekte sorun değil.

### WordExamples (seviyeli örnek cümleler — 1:N Words)
```sql
CREATE TABLE WordExamples (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL,
    SentenceText NVARCHAR(MAX) NOT NULL,   -- bu Word'ün dilinde tek örnek cümle
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

### Categories (hiyerarşik, dilden bağımsız çekirdek)
```sql
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,
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

### CategoryTranslations (dil başına 1 satır)
```sql
CREATE TABLE CategoryTranslations (
    Id INT PRIMARY KEY IDENTITY,
    CategoryId INT NOT NULL, LanguageId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL, Description NVARCHAR(MAX) NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    FOREIGN KEY (LanguageId) REFERENCES Languages(Id),
    CONSTRAINT UQ_CategoryTranslations_Category_Language UNIQUE (CategoryId, LanguageId)
);
```

### WordCategories (WordConcept ↔ Category M:N)
```sql
CREATE TABLE WordCategories (
    Id INT PRIMARY KEY IDENTITY, WordConceptId INT NOT NULL, CategoryId INT NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordConceptId) REFERENCES WordConcepts(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WordCategories UNIQUE (WordConceptId, CategoryId)
);
```
