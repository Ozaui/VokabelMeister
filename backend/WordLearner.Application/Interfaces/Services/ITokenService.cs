// ─────────────────────────────────────────────────────────────────────────────
// ITokenService.cs
//
// AMAÇ: JWT access token + refresh token üretimi ve süresi dolmuş token'dan
//       kimlik bilgisi (claims) çıkarma sözleşmesi.
// NEDEN: AuthService (A-03) somut JwtTokenService implementasyonuna değil bu
//        arayüze bağımlı olmalı — testlerde mock enjekte edilebilir.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.User, System.Security.Claims.
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Claims;
using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Services;

// AMAÇ: GenerateRefreshToken'ın ürettiği ham token + geçerlilik süresini bir arada taşır.
// NEDEN: AuthService bu ikisini ayrı ayrı hesaplamak yerine tek çağrıda alır;
//        Token, RefreshTokens.TokenHash'e yazılmadan önce IPasswordService.HashToken'dan geçer.
public record RefreshTokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    // AMAÇ: Kullanıcı bilgilerinden (Id/Email/Role/FirstName) imzalı bir JWT access token üretir.
    // NEDEN: Access token kısa ömürlüdür (Jwt:ExpirationMinutes) — her istekte [Authorize]
    //        middleware'i bu token'ı doğrular, DB'ye gitmeden kullanıcı kimliğini claim'lerden okur.
    string GenerateAccessToken(User user);

    // AMAÇ: Rastgele, yüksek entropili bir refresh token üretir (JWT DEĞİL — düz rastgele veri).
    // NEDEN: Refresh token'ın kendisi bir kimlik taşımaz, yalnızca DB'deki RefreshTokens
    //        kaydıyla eşleştirilen bir sırdır (bkz. Token Family Pattern, SECURITY.md §1).
    RefreshTokenResult GenerateRefreshToken();

    // AMAÇ: Süresi dolmuş (expired) bir access token'dan, imzası hâlâ geçerliyse claims çıkarır.
    // NEDEN: /auth/refresh akışında yeni token üretmeden önce "bu access token gerçekten bu
    //        kullanıcıya mı aitti" doğrulanır — ValidateLifetime=false ile süre kontrolü
    //        bilerek atlanır (zaten dolmuş olması beklenir), ama imza + algoritma kontrolü
    //        (Algorithm Confusion önlemi) atlanmaz.
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
