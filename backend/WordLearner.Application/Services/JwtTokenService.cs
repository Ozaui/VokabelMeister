// ─────────────────────────────────────────────────────────────────────────────
// JwtTokenService.cs
//
// AMAÇ: ITokenService'in JWT (HMAC-SHA256) tabanlı implementasyonu.
// NEDEN: ASP.NET Identity kullanılmadığı için (SECURITY.md §1) JWT üretimi/doğrulaması
//        manuel yazılır; Algorithm Confusion saldırısına karşı imzalanan algoritmanın
//        gerçekten HS256 olduğu her seferinde elle doğrulanır.
// BAĞIMLILIKLAR: System.IdentityModel.Tokens.Jwt, Microsoft.Extensions.Configuration,
//                WordLearner.Domain.Entities.User.
// ─────────────────────────────────────────────────────────────────────────────

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

    // AMAÇ: Kullanıcı bilgilerinden imzalı bir JWT access token üretir.
    // NEDEN: [Authorize] middleware'i her istekte bu claim'leri okuyup DB'ye gitmeden
    //        kimlik/rol doğrular; süre appsettings.json Jwt:ExpirationMinutes'ten okunur
    //        (Program.cs'teki JwtBearer doğrulama ayarlarıyla aynı anahtarı paylaşır).
    // NASIL: 1) SecretKey'den simetrik imza anahtarı oluştur  2) NameIdentifier/Email/Role/
    //        firstName claim'lerini ekle  3) HMAC-SHA256 ile imzala  4) string'e serileştir.
    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!)
        );
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

    // AMAÇ: Rastgele, yüksek entropili (64 byte) bir refresh token üretir.
    // NEDEN: 64 byte, brute-force ile tahmin edilemeyecek kadar büyük bir arama uzayı
    //        sağlar; JWT değildir çünkü kendi başına hiçbir kimlik bilgisi taşımaz,
    //        yalnızca DB'deki RefreshTokens kaydıyla (hash'i üzerinden) eşleştirilir.
    public RefreshTokenResult GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        var days = _configuration.GetValue("Jwt:RefreshTokenExpirationDays", 7);
        return new RefreshTokenResult(Convert.ToBase64String(bytes), DateTime.UtcNow.AddDays(days));
    }

    // AMAÇ: Süresi dolmuş bir access token'dan, imzası hâlâ geçerliyse ClaimsPrincipal döner.
    // NEDEN: /auth/refresh akışında yeni token üretmeden önce eski access token'ın gerçekten
    //        bu sunucu tarafından üretildiği doğrulanır — ValidateLifetime=false bilerek
    //        atlanır (zaten süresi dolmuş olması beklenir).
    // NASIL: 1) Aynı SecretKey ile ValidateToken çağır (süre kontrolü kapalı)  2) Doğrulanan
    //        token'ın header'ındaki algoritmanın gerçekten HS256 olduğunu kontrol et
    //        (Algorithm Confusion önlemi — TECHNICAL_SPECIFICATIONS.md §5)  3) Geçersizse null dön.
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!)
        );
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

            // NEDEN: Saldırgan "alg: none" veya asimetrik bir algoritmaya geçirilmiş bir token
            //        sunarsa, ValidateToken bunu SecretKey ile doğrulamadan geçirebilir —
            //        bu yüzden doğrulanan token'ın header'ında GERÇEKTEN HS256 kullanıldığı
            //        elle teyit edilir (Algorithm Confusion Attack önlemi).
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
}
