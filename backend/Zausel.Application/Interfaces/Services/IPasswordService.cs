namespace Zausel.Application.Interfaces.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
    string HashToken(string token); // SHA-256 — refresh/OTP token hash, bcrypt DEĞİL
}
