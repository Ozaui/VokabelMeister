// ─────────────────────────────────────────────────────────────────────────────
// LanguageConfiguration.cs
//
// AMAÇ: Language entity'sinin EF Core tablo eşlemesini tanımlar + de/tr seed verisi.
// NEDEN: Code alanı üzerinde UNIQUE index — aynı dil iki kez eklenemesin. Seed
//        veri burada tutulur çünkü Language BaseEntity'den türemediği için
//        (CreatedAt/IsDeleted yok) InfrastructureServiceExtensions'daki gibi bir
//        runtime seed akışına ihtiyaç yok, migration'ın kendisi yeterli
//        (bkz. DATABASE_SCHEMA/Icerik.md "Seed: yalnızca de+tr").
// BAĞIMLILIKLAR: EF Core, Language entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data.Configurations.Words;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.Property(l => l.Code).HasMaxLength(5).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(50).IsRequired();
        builder.Property(l => l.NativeName).HasMaxLength(50).IsRequired();

        // NEDEN: Aynı dil kodunun iki kez eklenmesini DB seviyesinde engeller.
        builder.HasIndex(l => l.Code).IsUnique();

        // NEDEN: Icerik.md'deki INSERT sırasıyla birebir — Id'ler açıkça verilir
        // çünkü HasData migration'da sabit anahtarlar üretir (IDENTITY otomatik artışına bırakılmaz).
        builder.HasData(
            new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch", DisplayOrder = 1 },
            new Language { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe", DisplayOrder = 2 }
        );
    }
}
