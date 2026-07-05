// ─────────────────────────────────────────────────────────────────────────────
// SocialLoginDtos.cs
//
// AMAÇ: POST /auth/google ve POST /auth/apple girdi şekilleri.
// NEDEN: İkisi de tek bir alan taşır (sağlayıcının verdiği kimlik token'ı) —
//        backend bu token'ı ilgili doğrulayıcıya (IGoogleTokenValidator/
//        IAppleTokenValidator) iletir, kendisi asla token içeriğini parse etmez.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: Google Sign-In SDK'sının istemcide ürettiği ID token'ı taşır.
public record GoogleLoginRequest(string IdToken);

// AMAÇ: Apple Sign-In'in istemcide ürettiği identity token'ı taşır (yalnızca iOS).
public record AppleLoginRequest(string IdentityToken);
