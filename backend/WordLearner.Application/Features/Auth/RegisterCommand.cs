using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName, string? Language) : IRequest<RegisterResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IOtpService otpService, IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
            throw new EmailAlreadyRegisteredException();

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordService.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var otpCode = _otpService.Generate(user, OtpPurpose.EmailVerification);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName, otpCode, request.Language, cancellationToken);

        return new RegisterResponse(user.Id, user.Email, user.FirstName, user.CurrentLevel, user.ThemePreference, user.LanguagePreference);
    }
}
