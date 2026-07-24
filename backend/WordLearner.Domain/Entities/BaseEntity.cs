namespace WordLearner.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Null = hiç güncellenmedi; WordLearnerDbContext.SaveChangesAsync yalnızca EntityState.Modified'ta set eder.
    public DateTime? UpdatedAt { get; set; }

    // Soft delete bayrağı — WordLearnerDbContext'teki global query filter buna göre filtreler.
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? UpdatedByUserId { get; set; }
    public int? DeletedByUserId { get; set; }
}
