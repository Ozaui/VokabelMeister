using MediatR;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Application.Features.Auth;

public record RequestAccountDeletionCommand(int UserId, string? Language) : IRequest<Unit>;

public class RequestAccountDeletionCommandHandler : IRequestHandler<RequestAccountDeletionCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public RequestAccountDeletionCommandHandler(IUserRepository userRepository, IOtpService otpService, IEmailService emailService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(RequestAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"User not found: Id={request.UserId}");

        var otpCode = _otpService.Generate(user, OtpPurpose.AccountDeletion);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendAccountDeletionConfirmationAsync(user.Email, user.FirstName, otpCode, request.Language, cancellationToken);
        return Unit.Value;
    }
}
