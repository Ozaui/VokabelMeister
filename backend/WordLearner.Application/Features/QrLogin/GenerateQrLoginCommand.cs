// ─────────────────────────────────────────────────────────────────────────────
// GenerateQrLoginCommand.cs
//
// AMAÇ: POST /auth/qr/generate — web'in görüntüleyeceği QR kodu için yeni bir
//       oturum başlatır (token+hash+pairingCode+2dk süre).
// NEDEN: REFERENCE/SECURITY.md §1.3 ADIM 1. RequesterIp/RequesterDeviceInfo bu
//        adımda (istekte bulunan WEB tarayıcısından) kaydedilir — mobil tarafta
//        "confirm login request from Chrome, IP 1.2.3.4" gibi gösterilip relay/
//        phishing saldırısına karşı kullanıcı tarafından gözle doğrulanır
//        (bkz. DATABASE_SCHEMA/Auth.md "QR'ı isteyen tarafın User-Agent'ı").
// NASIL: 1) 64 byte kriptografik rastgele token üret, URL-safe Base64'e çevir
//        (route parametresi olarak kullanılacağı için '+'/'/' kaçışsız güvenli
//        olmalı)  2) SHA-256 hash'ini DB'ye yaz, ham token yalnızca yanıtta döner
//        3) 4 haneli PairingCode üret  4) Session'ı Pending olarak kaydet.
// BAĞIMLILIKLAR: IQrLoginSessionRepository, IPasswordService (HashToken).
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Cryptography;
using MediatR;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.QrLogin;

// AMAÇ: Oturumu isteyen (web) tarafın IP/cihaz bilgisini taşır — controller
//       action parametresinden değil `with` ile eklenir (bkz. RefreshCommand deseni).
public record GenerateQrLoginCommand : IRequest<QrGenerateResponse>
{
    public string? ClientIp { get; init; }
    public string? DeviceInfo { get; init; }
}

public class GenerateQrLoginCommandHandler : IRequestHandler<GenerateQrLoginCommand, QrGenerateResponse>
{
    // NEDEN 2dk: REFERENCE/SECURITY.md §1.3 — QR oturumunun geçerlilik süresi.
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
        // NEDEN URL-safe Base64: token doğrudan route'a (/auth/qr/{token}/...) gömülür;
        //       standart Base64'teki '+'/'/' karakterleri path segment'inde sorun çıkarır.
        var qrToken = Convert
            .ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        // NEDEN RandomNumberGenerator (Random değil): PairingCode kriptografik rastgele
        //       olmasa da (yalnızca gözle karşılaştırma amaçlı) proje genelinde OTP/token
        //       üretiminde tutarlı olarak güvenli rastgelelik kullanılır (bkz. OtpService).
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
