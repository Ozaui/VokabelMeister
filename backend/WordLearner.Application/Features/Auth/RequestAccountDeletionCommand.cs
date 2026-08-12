using MediatR;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Exceptions;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

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
