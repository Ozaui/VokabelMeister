# Sosyal Domain — Sınıflar, Arkadaşlar, Paylaşım

> Genel kurallar (BaseEntity alanları, soft delete) → `../DATABASE_SCHEMA.md`. FK hedefi `Users` →
> `Auth.md`, `Categories` → `Icerik.md`, `UserCategories` → `Kisisel_Icerik.md`.

### Classes
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

### ClassWords (sınıfa özel kelimeler — yalnızca üyeler görür)
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

### ClassMemberships / ClassCategories / ClassUserCategories
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

### Friendships
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

### SharedContents / SharedContentImports
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
