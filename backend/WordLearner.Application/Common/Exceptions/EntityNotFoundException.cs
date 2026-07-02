// ─────────────────────────────────────────────────────────────────────────────
// EntityNotFoundException.cs
//
// AMAÇ: İstenilen entity DB'de bulunamadığında fırlatılan özel exception.
// NEDEN: Genel KeyNotFoundException veya Exception yerine özel tip kullanmak,
//        global exception middleware'inin bu durumu yakalayıp 404 döndürmesini sağlar.
// BAĞIMLILIKLAR: Yok — saf C# sınıfı.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }

    // AMAÇ: Entity tipini ve aranan anahtarı alıp standart formatta mesaj üreten kısayol.
    // NEDEN: Repository<T> gibi çağıranların "{Entity} bulunamadı: Id={key}" string'ini
    //        elle interpolate etmesini önler; format tek yerden yönetilir.
    public EntityNotFoundException(Type entityType, object key)
        : base($"{entityType.Name} bulunamadı: Id={key}") { }
}
