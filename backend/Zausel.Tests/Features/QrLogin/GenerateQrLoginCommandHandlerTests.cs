using Moq;
using FluentAssertions;
using Zausel.Application.Features.QrLogin;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Tests.Features.QrLogin;

public class GenerateQrLoginCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrLoginSessionRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    public GenerateQrLoginCommandHandlerTests()
    {
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash-of-{t}");
    }

    private GenerateQrLoginCommandHandler CreateHandler() =>
        new(_qrLoginSessionRepository.Object, _passwordService.Object);

    [Fact]
    public async Task Handle_ValidRequest_CreatesSessionWithUrlSafeTokenAndFourDigitPairingCode()
    {
        // ARRANGE
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GenerateQrLoginCommand("1.2.3.4", "Chrome/Mac"), default);

        // ASSERT — standart Base64 DEĞİL: '+'/'/' route eşleşmesini bozar (bkz. GenerateQrLoginCommand notu)
        result.QrToken.Should().NotContain("+").And.NotContain("/").And.NotContain("=");
        result.PairingCode.Should().MatchRegex("^[0-9]{4}$");
        result.ExpiresIn.Should().Be(120);
        _qrLoginSessionRepository.Verify(r => r.AddAsync(It.Is<QrLoginSession>(s =>
            s.QrTokenHash == $"hash-of-{result.QrToken}" && s.RequesterIp == "1.2.3.4" && s.RequesterDeviceInfo == "Chrome/Mac"),
            It.IsAny<CancellationToken>()), Times.Once);
        _qrLoginSessionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
