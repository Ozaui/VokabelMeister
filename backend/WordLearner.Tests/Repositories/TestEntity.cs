using WordLearner.Domain.Entities;

namespace WordLearner.Tests.Repositories;

// Repository<T> generic olduğu için gerçek bir domain entity'sine ihtiyaç duymaz — bu sınıf
// yalnızca bu test dosyasında kullanılan bir test çiftidir (double), üretim kodunun parçası DEĞİLDİR.
public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
