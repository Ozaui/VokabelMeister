# Sistem Domain — Uygulama Ayarları

> Genel kurallar (BaseEntity alanları) → `../DATABASE_SCHEMA.md`. FK hedefi `Users` → `Auth.md`.

### SmtpSettings (tekil — admin yönetir, AES şifreli)
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
Şifreleme detayı → `REFERENCE/SECURITY.md §3.2`.
