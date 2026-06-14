/// <summary>
/// RefreshTokenResult.cs
///
/// AMAÇ: GenerateRefreshToken metodunun döndürdüğü sonuç nesnesi.
/// NEDEN: Refresh token üretilince son kullanma tarihi de aynı anda hesaplanmalı.
///        AuthService'in süreyi ayrıca hesaplaması yerine bu nesne üretici servisin
///        kendi politikasını (kaç gün?) taşımasını sağlar — tek sorumluluk.
/// BAĞIMLILIKLAR: ITokenService
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Üretilen refresh token ile son kullanma tarihini birlikte taşır.
///
/// AMAÇ: Token üretim anında sürenin de belirlenmesini zorlamak.
/// NEDEN: AuthService ExpiresAt'ı bağımsız hesaplarsa ITokenService politikasıyla
///        çakışabilir (örn: config'de 7 gün ama servis 14 gün yazarsa tutarsızlık).
/// </summary>
public record RefreshTokenResult(
    /// <summary>
    /// Ham (raw) refresh token değeri — istemciye döndürülür.
    /// NEDEN: Veritabanına SHA-256 hash'i kaydedilir, bu değer asla saklanmaz.
    /// </summary>
    string Token,

    /// <summary>
    /// Token'ın geçerlilik bitiş tarihi (UTC).
    /// NEDEN: Üretici (ITokenService implementasyonu) politikayı belirler;
    ///        tüketici (AuthService) bu değeri doğrudan veritabanına yazar.
    /// </summary>
    DateTime ExpiresAt
);
