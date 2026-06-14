/// <summary>
/// WordLearnerDbContext.cs
///
/// AMAÇ:
///   EF Core'un merkezi veritabanı bağlam sınıfı.
///   Tüm entity'leri DbSet olarak tanımlar ve model yapılandırmalarını uygular.
///
/// NEDEN:
///   Tüm konfigürasyonlar ayrı IEntityTypeConfiguration<T> dosyalarında tutulur.
///   ApplyConfigurationsFromAssembly ile hepsi tek seferde yüklenir.
///   SaveChangesAsync override'ı UpdatedAt alanını otomatik günceller.
///
/// BAĞIMLILIKLAR:
///   - WordLearner.Domain.Entities (tüm entity'ler)
///   - WordLearner.Domain.Common (BaseEntity)
///   - Data/Configurations/** (tüm IEntityTypeConfiguration sınıfları)
/// </summary>
using Microsoft.EntityFrameworkCore;
using WordLearner.Domain.Common;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data;

/// <summary>
/// EF Core veritabanı bağlam sınıfı.
///
/// AMAÇ: Tüm entity'lere DbSet üzerinden erişim sağlamak ve model yapılandırmalarını yönetmek.
/// NEDEN: Repository'ler bu context üzerinden çalışır; konfigürasyonlar assembly taramasıyla yüklenir.
/// </summary>
public class WordLearnerDbContext : DbContext
{
    public WordLearnerDbContext(DbContextOptions<WordLearnerDbContext> options)
        : base(options) { }

    // ─── Auth ────────────────────────────────────────────────────────────────
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // ─── Sistem Kelimeleri ────────────────────────────────────────────────────
    public DbSet<Word> Words { get; set; }
    public DbSet<WordDetail> WordDetails { get; set; }
    public DbSet<WordExample> WordExamples { get; set; }

    // ─── Kategoriler ─────────────────────────────────────────────────────────
    public DbSet<Category> Categories { get; set; }
    public DbSet<WordCategory> WordCategories { get; set; }

    // ─── Kişisel Kartlar ──────────────────────────────────────────────────────
    public DbSet<UserCard> UserCards { get; set; }
    public DbSet<UserCardExample> UserCardExamples { get; set; }
    public DbSet<UserCategory> UserCategories { get; set; }
    public DbSet<UserCardCategory> UserCardCategories { get; set; }
    public DbSet<UserCardUserCategory> UserCardUserCategories { get; set; }

    // ─── Öğrenme ─────────────────────────────────────────────────────────────
    public DbSet<UserProgress> UserProgresses { get; set; }
    public DbSet<UserCardProgress> UserCardProgresses { get; set; }
    public DbSet<LearningHistory> LearningHistories { get; set; }
    public DbSet<LearningSession> LearningSessions { get; set; }

    // ─── Gamification ─────────────────────────────────────────────────────────
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }

    // ─── Sosyal ──────────────────────────────────────────────────────────────
    public DbSet<Class> Classes { get; set; }
    public DbSet<ClassMembership> ClassMemberships { get; set; }

    // Instructor'ın sınıfına özel kelimeleri — sistem Words tablosundan bağımsız
    public DbSet<ClassWord> ClassWords { get; set; }
    public DbSet<ClassCategory> ClassCategories { get; set; }
    public DbSet<ClassUserCategory> ClassUserCategories { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<SharedContent> SharedContents { get; set; }
    public DbSet<SharedContentImport> SharedContentImports { get; set; }

    // ─── Denetim ─────────────────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs { get; set; }

    /// <summary>
    /// AMAÇ: Tüm entity konfigürasyonlarını assembly'den otomatik yükler.
    /// NASIL: Her IEntityTypeConfiguration<T> sınıfı Data/Configurations/** altındadır.
    ///        ApplyConfigurationsFromAssembly bunları tek seferde bulur ve uygular.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tüm konfigürasyon sınıflarını bu assembly'den otomatik yükle
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WordLearnerDbContext).Assembly);
    }

    /// <summary>
    /// AMAÇ: Kaydetme işleminden önce BaseEntity türevlerinin UpdatedAt alanını otomatik günceller.
    /// NEDEN: Her repository'de manuel güncelleme yapmak yerine merkezi kontrol tercih edildi.
    /// NASIL: ChangeTracker, Modified durumundaki BaseEntity kayıtlarını bulur ve zamanı atar.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Modified durumundaki BaseEntity türevlerinin UpdatedAt'ini güncelle
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
