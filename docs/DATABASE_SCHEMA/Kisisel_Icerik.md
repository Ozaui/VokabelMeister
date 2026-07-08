# Kişisel İçerik Domain — Kullanıcı Kartları ve Kategorileri

> Genel kurallar (BaseEntity, soft delete, UserId filtresi) → `../CLAUDE.md §1`. FK: `Users`→`Auth.md`, `Categories`→`Icerik.md`.

### UserCards + UserCardExamples
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

### UserCategories + ara tablolar
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
-- NOT: UserCategoryId FK bilinçli NO ACTION — Users silindiğinde hem UserCards hem UserCategories
-- üzerinden CASCADE ulaşılsaydı "multiple cascade paths" hatası olurdu. UserCardId cascade zincirini taşır.
CREATE TABLE UserCardUserCategories (
    Id INT PRIMARY KEY IDENTITY, UserCardId INT NOT NULL, UserCategoryId INT NOT NULL,
    FOREIGN KEY (UserCardId) REFERENCES UserCards(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserCategoryId) REFERENCES UserCategories(Id),
    CONSTRAINT UQ_UserCardUserCategories UNIQUE (UserCardId, UserCategoryId)
);
```
