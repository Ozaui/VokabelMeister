// ─────────────────────────────────────────────────────────────────────────────
// GenerateQrLoginCommandHandlerTests.cs
//
// AMAÇ: GenerateQrLoginCommandHandler'ın oturumu Pending olarak oluşturduğunu,
//       ham token'ın URL-safe olduğunu ve 4 haneli PairingCode ürettiğini doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using System.Text.RegularExpressions;
using FluentAssertions;
using Moq;
using WordLearner.Application.Features.QrLogin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.QrLogin;

public class GenerateQrLoginCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    private GenerateQrLoginCommandHandler CreateHandler() => new(_qrRepo.Object, _passwordService.Object);

    /// <summary>
    /// Generate_HappyPath_CreatesPendingSessionAndReturnsTokenPlusPairingCode
    ///
    /// AMAÇ: Yeni oturumun Pending durumunda, isteği atan (web) tarafın IP/cihaz
    ///       bilgisiyle DB'ye eklendiğini ve yanıtta ham token+4 haneli PairingCode+120sn döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task Generate_HappyPath_CreatesPendingSessionAndReturnsTokenPlusPairingCode()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        QrLoginSession? savedSession = null;
        _qrRepo
            .Setup(r => r.AddAsync(It.IsAny<QrLoginSession>(), null, default))
            .Callback<QrLoginSession, int?, CancellationToken>((s, _, _) => savedSession = s)
            .ReturnsAsync((QrLoginSession s, int? _, CancellationToken _) => s);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(
            new GenerateQrLoginCommand { ClientIp = "1.2.3.4", DeviceInfo = "Chrome/Windows" },
            default
        );

        // ASSERT
        result.ExpiresIn.Should().Be(120);
        result.QrToken.Should().NotBeNullOrWhiteSpace();
        result.PairingCode.Should().MatchRegex("^[0-9]{4}$");
        savedSession.Should().NotBeNull();
        savedSession!.Status.Should().Be(QrLoginStatus.Pending);
        savedSession.RequesterIp.Should().Be("1.2.3.4");
        savedSession.RequesterDeviceInfo.Should().Be("Chrome/Windows");
        savedSession.QrTokenHash.Should().Be("hash");
    }

    /// <summary>
    /// Generate_GeneratedToken_IsUrlSafeBase64
    ///
    /// AMAÇ: Ham token'ın route parametresi olarak kullanılacağı için standart
    ///       Base64'teki '+'/'/' karakterlerini İÇERMEDİĞİNİ doğrulamak.
    /// NEDEN: '+' ve '/' path segment'inde routing hatasına yol açar.
    /// </summary>
    [Fact]
    public async Task Generate_GeneratedToken_IsUrlSafeBase64()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo
            .Setup(r => r.AddAsync(It.IsAny<QrLoginSession>(), null, default))
            .ReturnsAsync((QrLoginSession s, int? _, CancellationToken _) => s);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GenerateQrLoginCommand(), default);

        // ASSERT
        Regex.IsMatch(result.QrToken, "^[A-Za-z0-9_-]+$").Should().BeTrue();
    }
}
