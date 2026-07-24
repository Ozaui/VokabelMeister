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
