// ─────────────────────────────────────────────────────────────────────────────
// RequestAccountDeletionCommand.cs
//
// AMAÇ: POST /auth/delete-account/request — hesap silme OTP'si gönderir (15dk geçerli).
// NEDEN: Gövdesi yoktur — JWT'deki kullanıcı kimliği kullanılır; bu yüzden eskiden
//        de ayrı bir Request DTO'su yoktu, controller doğrudan bu Command'ı üretir.
// BAĞIMLILIKLAR: IUserRepository, IOtpService, IEmailService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

// NEDEN Language init-property: bkz. LoginCommand.
public record RequestAccountDeletionCommand(int UserId) : IRequest<MessageResponse>
{
    public string? Language { get; init; }
}

public class RequestAccountDeletionCommandHandler
    : IRequestHandler<RequestAccountDeletionCommand, MessageResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public RequestAccountDeletionCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        IEmailService emailService
    )
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<MessageResponse> Handle(RequestAccountDeletionCommand request, CancellationToken ct)
    {
        var user =
            await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new EntityNotFoundException(typeof(User), request.UserId);

        var (otpCode, otpHash) = _otpService.Generate();
        user.PendingOtpCodeHash = otpHash;
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(IOtpService.DeleteAccountOtpExpiryMinutes);
        user.PendingOtpCodePurpose = OtpPurpose.AccountDeletion;
        await _userRepository.UpdateAsync(user, user.Id, ct);

        await _emailService.SendAccountDeletionOtpAsync(user.Email, otpCode, ct);
        return new MessageResponse(
            "ACCOUNT_DELETION_OTP_SENT",
            SuccessMessages.Resolve("ACCOUNT_DELETION_OTP_SENT", request.Language)
        );
    }
}
