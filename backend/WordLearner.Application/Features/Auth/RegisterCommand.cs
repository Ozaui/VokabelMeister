using AutoMapper;
using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<RegisterResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IOtpService otpService,
        IEmailService emailService,
        IMapper mapper
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            throw new DuplicateEmailException();

        // Anonimleştirilmiş bir hesabın orijinal e-postasıyla tekrar kayıt açılmasını engeller.
        var emailHash = _passwordService.HashToken(request.Email);
        if (await _userRepository.OriginalEmailHashExistsAsync(emailHash, ct))
            throw new DuplicateEmailException();

        var (otpCode, otpHash) = _otpService.Generate();

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordService.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            AuthProvider = "Local",
            PendingOtpCodeHash = otpHash,
            PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(IOtpService.OtpExpiryMinutes),
            PendingOtpCodePurpose = OtpPurpose.EmailVerification,
        };

        await _userRepository.AddAsync(user, ct: ct);
        await _emailService.SendEmailVerificationOtpAsync(user.Email, otpCode, ct);

        return _mapper.Map<RegisterResponse>(user);
    }
}
