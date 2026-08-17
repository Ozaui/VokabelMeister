using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Application.Services;

// Program.cs'te app.Run()'dan önce, tek seferlik çağrılır — AdminController (A-18) ve
// [Authorize(Roles="Admin")] uçları için döngüsel bağımlılığı kırar (ilk Admin'i kimse
// Register+UpdateUserRole ile oluşturamaz, çünkü UpdateUserRole zaten Admin ister).
public class AdminSeedService : IAdminSeedService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeedService> _logger;

    public AdminSeedService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IConfiguration configuration,
        ILogger<AdminSeedService> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedInitialAdminAsync(CancellationToken cancellationToken = default)
    {
        var email = _configuration["INITIAL_ADMIN_EMAIL"];
        var password = _configuration["INITIAL_ADMIN_PASSWORD"];

        // İkisi de tanımlı değilse (ör. prod'da bilinçli atlanmışsa) sessizce çık — zorunlu değil.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        if (!MailAddress.TryCreate(email, out _))
        {
            _logger.LogWarning("INITIAL_ADMIN_EMAIL is not a valid email address, skipping admin seed");
            return;
        }

        if (await _userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            return;

        var now = DateTime.UtcNow;
        var admin = new User
        {
            Email = email,
            PasswordHash = _passwordService.Hash(password),
            FirstName = "Admin",
            LastName = "Admin",
            Role = "Admin",
            IsActive = true,
            IsEmailVerified = true,
            EmailVerifiedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _userRepository.AddAsync(admin, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Initial admin account seeded for {Email}", email);
    }
}
