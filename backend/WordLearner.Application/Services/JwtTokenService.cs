using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Services;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration) => _configuration = configuration;

    public string GenerateAccessToken(User user)
    {
        var key = GetSigningKey();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("firstName", user.FirstName),
        };
        var expirationMinutes = _configuration.GetValue("Jwt:ExpirationMinutes", 15);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // JWT değildir — kendi başına kimlik taşımaz, yalnızca DB'deki RefreshTokens kaydıyla eşleştirilir.
    public RefreshTokenResult GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var days = _configuration.GetValue("Jwt:RefreshTokenExpirationDays", 7);
        return new RefreshTokenResult(Convert.ToBase64String(bytes), DateTime.UtcNow.AddDays(days));
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = GetSigningKey();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                token,
                validationParameters,
                out var validatedToken
            );

            // Algorithm Confusion Attack önlemi — saldırgan "alg: none" veya asimetrik bir
            // algoritmaya geçirilmiş bir token sunarsa ValidateToken bunu doğrulamadan geçirebilir.
            if (
                validatedToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase
                )
            )
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private SymmetricSecurityKey GetSigningKey() =>
        new(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
}
