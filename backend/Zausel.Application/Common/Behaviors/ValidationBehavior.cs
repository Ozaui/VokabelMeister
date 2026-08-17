using FluentValidation;
using MediatR;

namespace Zausel.Application.Common.Behaviors;

// MediatR pipeline behavior — her Handler'dan ÖNCE çalışır, o Command/Query için kayıtlı TÜM
// IValidator<T>'leri çalıştırır. Hiç validator yoksa (ör. GenerateQrLoginCommand) no-op, Handler
// doğrudan çağrılır. Başarısızlık FluentValidation.ValidationException fırlatır — ExceptionHandlingMiddleware
// bunu yakalayıp ilk kuralın WithErrorCode'unu ErrorMessages'tan çözer (400).
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
