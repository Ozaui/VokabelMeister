using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Auth;

public class UserRepository : IUserRepository
{
    private readonly WordLearnerDbContext _context;

    public UserRepository(WordLearnerDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);

    public async Task<User?> GetByAppleIdAsync(string appleId, CancellationToken cancellationToken = default) =>
        await _context.Users.FirstOrDefaultAsync(u => u.AppleId == appleId, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
