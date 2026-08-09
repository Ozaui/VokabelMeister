using System.Security.Cryptography;
using System.Text;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class PasswordService : IPasswordService
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public string HashToken(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
    }
}
