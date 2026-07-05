# İçerik Domain — Sistem Kelimeleri ve Kategoriler

> Genel kurallar (BaseEntity alanları, soft delete) → `../DATABASE_SCHEMA.md`. FK hedefi `Users` → `Auth.md`.

> **Sistem kelimesi vs. kullanıcı kelimesi:** Bu dosyadaki tüm tablolar (`WordConcepts`, `Words`,
> `WordDetails`, `WordExamples`, `Categories`, `CategoryTranslations`, `WordCategories`) **yalnızca
> Admin tarafından yazılır**, tüm giriş yapmış kullanıcılar salt-okunur erişir. Kullanıcının kendi
> oluşturduğu kişisel kartlar (`UserCards`) tamamen ayrı bir tablodur ve bu dosyanın kapsamı dışındadır
> → `Kisisel_Icerik.md`. Bu redesign yalnızca sistem kelimelerini (Admin içeriği) kapsar.

> **Çoklu dil altyapısı:** Bir kelime, dilden bağımsız bir `WordConcepts` kaydına (kavram) ve o kavrama
> bağlı, her biri kendi dilindeki bir `Languages` satırına işaret eden N adet `Words` satırına ayrılır.
> Şu an yalnızca `de`+`tr` içerik yazılıyor (bir kelime oluşturulurken/düzenlenirken Almanca ve Türkçe
> karşılığı — kendi gramer/örnek cümleleriyle birlikte — **aynı işlemde** girilir/güncellenir). Yeni bir
> dil (örn. `en`) eklemek yalnızca `Languages`'e bir satır + ilgili kavramlara birer `Words` satırı
> eklemek demektir — **şema/migration değişmez**. Eşleştirme yönsüzdür: DE-TR, DE-EN, TR-EN hepsi aynı
> `WordConceptId` üzerinden otomatik geçerlidir.

### Languages (desteklenen diller)
```sql
CREATE TABLE Languages (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(5) NOT NULL,           -- ISO 639-1: 'de','tr','en'...
    Name NVARCHAR(50) NOT NULL,          -- İngilizce ad: 'German','Turkish'
    NativeName NVARCHAR(50) NOT NULL,    -- Kendi dilinde ad: 'Deutsch','Türkçe'
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Languages_Code UNIQUE (Code)
);
-- Seed: yalnızca de+tr. Üçüncü dil eklenmesi tek satırlık bir INSERT'tir, migration gerekmez.
INSERT INTO Languages (Code, Name, NativeName, DisplayOrder) VALUES
('de', 'German', 'Deutsch', 1), ('tr', 'Turkish', 'Türkçe', 2);
```

### WordConcepts (dilden bağımsız kavram — kategori/seviye burada tutulur, yalnızca Admin ekler)
```sql
CREATE TABLE WordConcepts (
    Id INT PRIMARY KEY IDENTITY,
    PartOfSpeech NVARCHAR(20) NOT NULL,   -- Noun|Verb|Adjective|Adverb|Conjunction|Preposition|Pronoun|Other
    DifficultyLevel NVARCHAR(2) NOT NULL, -- A1..C2
    ImageUrl NVARCHAR(500) NULL,          -- görsel dilden bağımsızdır (bir "masa" resmi tüm dillerde aynı)
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy INT NULL, UpdatedBy INT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_WordConcepts_Level CHECK (DifficultyLevel IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_WordConcepts_PartOfSpeech CHECK (PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')),
    INDEX IX_WordConcepts_DifficultyLevel (DifficultyLevel)
);
```

### Words (bir kavramın tek bir dildeki karşılığı — yalnızca Admin ekler)
```sql
CREATE TABLE Words (
    Id INT PRIMARY KEY IDENTITY,
    WordConceptId INT NOT NULL,
    LanguageId INT NOT NULL,
    Text NVARCHAR(255) NOT NULL,          -- örn. 'Tisch' (de) / 'masa' (tr)
    Definition NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordConceptId) REFERENCES WordConcepts(Id) ON DELETE CASCADE,
    FOREIGN KEY (LanguageId) REFERENCES Languages(Id),
    CONSTRAINT UQ_Words_Concept_Language UNIQUE (WordConceptId, LanguageId),
    INDEX IX_Words_LanguageId_Text (LanguageId, Text)   -- duplikat kontrolü + dil bazlı arama
);
```

### WordDetails (dile özel gramer — 1:1 Words)
```sql
CREATE TABLE WordDetails (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL UNIQUE,
    Pronunciation NVARCHAR(500) NULL,        -- IPA, dile özel
    AudioUrl NVARCHAR(500) NULL,
    Notes NVARCHAR(MAX) NULL, CommonMistakes NVARCHAR(MAX) NULL,
    GrammarData NVARCHAR(MAX) NULL,          -- JSON — şekli Words.LanguageId'ye göre değişir (bkz. alt not)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE
);
```
**GrammarData JSON (dil bazında farklı şema):** Almanca (`de`) için şekil ve alan kaynağı →
`REFERENCE/GERMAN_LANGUAGE_FEATURES.md` (`gender`, 4 hâlde 8 artikel, `pluralForm`/`pluralFormDative`,
`conjugationData`, `isSeparableVerb`/`separablePrefix`). Türkçe (`tr`) için →
`REFERENCE/TURKISH_LANGUAGE_FEATURES.md` (`vowelHarmony`, 6 hâl eki, `pluralForm`, `possessive`,
`consonantMutation`, fiil çekimi) — yön fark etmeksizin (bir Almanın Türkçe öğrenmesi dâhil) gramer
içeriği sağlanır. İngilizce (`en`) için şema tanımlı ama **henüz kullanılmıyor** →
`REFERENCE/ENGLISH_LANGUAGE_FEATURES.md` (`article`, `pluralForm`, `verbForms`,
`comparative`/`superlative`); `Languages`'e satır eklenip gerçek `Words` girilene kadar bu şekil boşta
bekler — yeni bir sütun gerekmez.
**Trade-off:** Önceki tasarımdaki `Gender` üzerindeki `CHECK`/`INDEX` (DB-seviyesi) kayboldu; "tüm
maskülin isimleri getir" gibi filtreler artık uygulama katmanında (`JSON_VALUE`) yapılır. Bu ölçekteki
bir kelime dağarcığı için performans sorunu yaratmaz.

### WordExamples (seviyeli örnek cümleler — 1:N Words, dile özel)
```sql
CREATE TABLE WordExamples (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL,
    SentenceText NVARCHAR(MAX) NOT NULL,   -- bu Word'ün dilinde tek bir örnek cümle
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

### Categories (sistem kategorileri — hiyerarşik, dilden bağımsız çekirdek)
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

### CategoryTranslations (kategori adı/açıklaması — dil başına 1 satır)
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

### WordCategories (WordConcept ↔ Category M:N — kavram üzerinden bir kez etiketlenir)
```sql
CREATE TABLE WordCategories (
    Id INT PRIMARY KEY IDENTITY, WordConceptId INT NOT NULL, CategoryId INT NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordConceptId) REFERENCES WordConcepts(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WordCategories UNIQUE (WordConceptId, CategoryId)
);
```
