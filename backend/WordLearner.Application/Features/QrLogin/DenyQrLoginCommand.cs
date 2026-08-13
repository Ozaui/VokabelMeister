using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

public record DenyQrLoginCommand(string QrToken, int UserId) : IRequest<Unit>;

public class DenyQrLoginCommandHandler : IRequestHandler<DenyQrLoginCommand, Unit>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;

    public DenyQrLoginCommandHandler(IQrLoginSessionRepository qrLoginSessionRepository, IPasswordService passwordService)
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
    }

    public async Task<Unit> Handle(DenyQrLoginCommand request, CancellationToken cancellationToken)
    {
        var session = await _qrLoginSessionRepository.GetByTokenHashAsync(_passwordService.HashToken(request.QrToken), cancellationToken)
            ?? throw new QrSessionGoneException();

        if (session.ExpiresAt < DateTime.UtcNow || session.Status != QrLoginStatus.Scanned)
            throw new QrSessionGoneException();

        if (session.UserId != request.UserId)
            throw new QrSessionForbiddenException();

        session.Status = QrLoginStatus.Denied;
        await _qrLoginSessionRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
