using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

public record ConfirmQrLoginCommand(string QrToken, int UserId) : IRequest<Unit>;

public class ConfirmQrLoginCommandHandler : IRequestHandler<ConfirmQrLoginCommand, Unit>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;

    public ConfirmQrLoginCommandHandler(IQrLoginSessionRepository qrLoginSessionRepository, IPasswordService passwordService)
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
    }

    public async Task<Unit> Handle(ConfirmQrLoginCommand request, CancellationToken cancellationToken)
    {
        var session = await _qrLoginSessionRepository.GetByTokenHashAsync(_passwordService.HashToken(request.QrToken), cancellationToken)
            ?? throw new QrSessionGoneException();

        if (session.ExpiresAt < DateTime.UtcNow || session.Status != QrLoginStatus.Scanned)
            throw new QrSessionGoneException();

        // Yalnızca session'ı tarayan (Scanned'de UserId'yi dolduran) kullanıcı onaylayabilir.
        if (session.UserId != request.UserId)
            throw new QrSessionForbiddenException();

        session.Status = QrLoginStatus.Confirmed;
        session.ConfirmedAt = DateTime.UtcNow;
        await _qrLoginSessionRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
