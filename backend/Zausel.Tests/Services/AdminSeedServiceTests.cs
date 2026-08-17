using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Tests.Services;

public class AdminSeedServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();

    private AdminSeedService CreateService(string? email = "admin@zausel.com", string? password = "Admin123!") =>
        new(_userRepository.Object, new PasswordService(),
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["INITIAL_ADMIN_EMAIL"] = email,
                ["INITIAL_ADMIN_PASSWORD"] = password
            }).Build(),
            Mock.Of<ILogger<AdminSeedService>>());

    [Fact]
    public async Task SeedInitialAdminAsync_EnvVarsNotConfigured_DoesNothing()
    {
        // ARRANGE
        var service = CreateService(email: null, password: null);

        // ACT
        await service.SeedInitialAdminAsync();

        // ASSERT
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SeedInitialAdminAsync_EmailIsMalformed_DoesNothing()
    {
        // ARRANGE
        var service = CreateService(email: "not-an-email");

        // ACT
        await service.SeedInitialAdminAsync();

        // ASSERT
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SeedInitialAdminAsync_AdminAlreadyExists_DoesNotCreateDuplicate()
    {
        // ARRANGE — idempotent: aynı e-posta zaten varsa dokunma
        _userRepository.Setup(r => r.GetByEmailAsync("admin@zausel.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "admin@zausel.com", Role = "Admin" });
        var service = CreateService();

        // ACT
        await service.SeedInitialAdminAsync();

        // ASSERT
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SeedInitialAdminAsync_NoExistingAdmin_CreatesAdminUser()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByEmailAsync("admin@zausel.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var service = CreateService();

        // ACT
        await service.SeedInitialAdminAsync();

        // ASSERT — Role=Admin, hesap aktif+doğrulanmış, şifre bcrypt ile hash'lenmiş (ham metin DEĞİL)
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "admin@zausel.com" &&
            u.Role == "Admin" &&
            u.IsActive &&
            u.IsEmailVerified &&
            u.PasswordHash != "Admin123!" &&
            !string.IsNullOrEmpty(u.PasswordHash)),
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
