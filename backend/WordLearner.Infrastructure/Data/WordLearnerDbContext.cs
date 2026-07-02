// ─────────────────────────────────────────────────────────────────────────────
// WordLearnerDbContext.cs
//
// AMAÇ: EF Core'un veritabanıyla konuştuğu merkezi DbContext sınıfı.
// NEDEN: Tüm entity konfigürasyonlarını tek yerden yükler; soft delete global filtresi
//        ile silinmiş kayıtların sorgulara otomatik karışmasını önler; SaveChangesAsync
//        override'ı UpdatedAt alanını elle set etme zorunluluğunu ortadan kaldırır.
// BAĞIMLILIKLAR: Microsoft.EntityFrameworkCore, WordLearner.Domain.Entities.BaseEntity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data;

public class WordLearnerDbContext : DbContext
{
    // AMAÇ: DI tarafından enjekte edilen seçenekleri (connection string vb.) taban sınıfa iletir.
    // NEDEN: EF Core'un standart constructor kalıbı; farklı ortamlar (dev/prod/test) için
    //        farklı seçenek setleri enjekte edilebilir.
    public WordLearnerDbContext(DbContextOptions<WordLearnerDbContext> options) : base(options) { }

    // AMAÇ: DbSet'ler feature entity'ler eklendikçe buraya yazılır (A-03'ten itibaren).
    // NEDEN: Şu an A-02 aşamasındayız; DbContext'in temel yapısı hazır, entity'ler henüz yok.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AMAÇ: Infrastructure assembly'sindeki tüm IEntityTypeConfiguration<T> sınıflarını
        //       otomatik olarak tarar ve uygular.
        // NEDEN: Her entity için tek tek modelBuilder.ApplyConfiguration(...) çağırmak yerine
        //        assembly taraması yaparız; yeni entity eklendikçe bu satır değişmez.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WordLearnerDbContext).Assembly);

        // AMAÇ: BaseEntity'den türeyen tüm entity'lere global soft delete filtresi uygular.
        // NEDEN: IsDeleted == true olan kayıtlar hiçbir sorguda görünmez; her sorguda
        //        .Where(e => !e.IsDeleted) yazmak zorunda kalmayız.
        //        IgnoreQueryFilters() ile gerektiğinde (admin silinen kayıtları görme) bypass edilir.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var notDeleted = System.Linq.Expressions.Expression.Not(property);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    // AMAÇ: Her kayıt güncellendiğinde UpdatedAt alanını otomatik olarak UTC şimdiki zamana set eder.
    // NEDEN: Servis veya repository katmanında her entity.UpdatedAt = DateTime.UtcNow yazmak
    //        kod tekrarına yol açar; bu override ile merkezi olarak yönetilir.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;

            // NEDEN: Yeni eklenen entity'lerde CreatedAt varsayılan olarak DateTime.UtcNow ile
            //        başlatılıyor (BaseEntity'de); UpdatedAt ise burada dokunulmadığı için
            //        insert sonrası null kalır — "hiç güncellenmedi" bilgisini korur.
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
