namespace WordLearner.Application.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message)
        : base(message) { }

    public EntityNotFoundException(Type entityType, object key)
        : base($"{entityType.Name} not found: Id={key}") { }
}
