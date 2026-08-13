using System.Security.Cryptography;
using MediatR;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.QrLogin;

public record GenerateQrLoginCommand(string? IpAddress, string? DeviceInfo) : IRequest<QrLoginGenerateResponse>;

public class GenerateQrLoginCommandHandler : IRequestHandler<GenerateQrLoginCommand, QrLoginGenerateResponse>
{
    private const int TokenByteLength = 64;
    private const int PairingCodeLength = 4;
    private const int ExpirationMinutes = 2;

    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;

    public GenerateQrLoginCommandHandler(IQrLoginSessionRepository qrLoginSessionRepository, IPasswordService passwordService)
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
    }

    public async Task<QrLoginGenerateResponse> Handle(GenerateQrLoginCommand request, CancellationToken cancellationToken)
    {
        var tokenBytes = new byte[TokenByteLength];
        RandomNumberGenerator.Fill(tokenBytes);
        // Standart Base64 DEĞİL — bu token URL path segmenti olarak kullanılıyor (/auth/qr/{token}/...),
        // '+'/'/' route eşleşmesini bozar; '-'/'_' URL-güvenli, '=' dolgusu path'te gereksiz.
        var token = Convert.ToBase64String(tokenBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var pairingCode = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, PairingCodeLength)).ToString($"D{PairingCodeLength}");

        var session = new QrLoginSession
        {
            QrTokenHash = _passwordService.HashToken(token),
            PairingCode = pairingCode,
            RequesterIp = request.IpAddress,
            RequesterDeviceInfo = request.DeviceInfo,
            ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes)
        };

        await _qrLoginSessionRepository.AddAsync(session, cancellationToken);
        await _qrLoginSessionRepository.SaveChangesAsync(cancellationToken);

        return new QrLoginGenerateResponse(token, pairingCode, ExpirationMinutes * 60);
    }
}
