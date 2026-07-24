using System.Security.Cryptography;
using MediatR;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.QrLogin;

// RequesterIp/DeviceInfo mobil ekranda gösterilip kullanıcı tarafından gözle doğrulanır (relay/phishing önlemi).
public record GenerateQrLoginCommand : IRequest<QrGenerateResponse>
{
    public string? ClientIp { get; init; }
    public string? DeviceInfo { get; init; }
}

public class GenerateQrLoginCommandHandler : IRequestHandler<GenerateQrLoginCommand, QrGenerateResponse>
{
    private const int ExpirySeconds = 120;

    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;

    public GenerateQrLoginCommandHandler(
        IQrLoginSessionRepository qrLoginSessionRepository,
        IPasswordService passwordService
    )
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
    }

    public async Task<QrGenerateResponse> Handle(GenerateQrLoginCommand request, CancellationToken ct)
    {
        var tokenBytes = new byte[64];
        RandomNumberGenerator.Fill(tokenBytes);
        // URL-safe Base64 — token doğrudan route'a gömülür, standart Base64'teki '+'/'/' path segment'inde sorun çıkarır.
        var qrToken = Convert
            .ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var pairingCode = RandomNumberGenerator.GetInt32(0, 10_000).ToString("D4");

        var session = new QrLoginSession
        {
            QrTokenHash = _passwordService.HashToken(qrToken),
            PairingCode = pairingCode,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ExpirySeconds),
            RequesterIp = request.ClientIp,
            RequesterDeviceInfo = request.DeviceInfo,
        };
        await _qrLoginSessionRepository.AddAsync(session, ct: ct);

        return new QrGenerateResponse(qrToken, pairingCode, ExpirySeconds);
    }
}
