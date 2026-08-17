namespace Zausel.Domain.Exceptions;

// AppException'dan bilinçli olarak türemez (Application katmanında yaşar, Domain ona bağımlı olamaz)
// ama aynı ilkeyi taşır: Code sabit/dilden bağımsız, istemciye ErrorMessages sözlüğünden çözülerek gider.
public class EntityNotFoundException : Exception
{
    public string Code { get; }

    public EntityNotFoundException(string message) : base(message)
    {
        Code = "ENTITY_NOT_FOUND";
    }
}
