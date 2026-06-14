/// <summary>
/// AppleTokenPayload.cs
///
/// AMAÇ: Apple identity token doğrulamasından çıkarılan kullanıcı bilgileri.
/// NEDEN: IAppleTokenValidator → AuthService arasındaki veri taşıyıcısı.
///        Apple JWT claims'ini doğrudan AuthService'e açmak sıkı bağımlılık yaratır.
/// BAĞIMLILIKLAR: IAppleTokenValidator
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Apple token doğrulama sonucu.
///
/// AMAÇ: Apple identity token'dan elde edilen kullanıcı tanımlayıcılarını taşımak.
/// NEDEN: Apple, Google'dan farklı olarak e-postayı gizleyebilir (private relay).
/// </summary>
public class AppleTokenPayload
{
    /// <summary>
    /// Apple kullanıcı ID'si — "sub" claim. Değişmez ve benzersiz.
    /// NEDEN: Apple e-postayı gizlese bile bu ID her zaman gelir.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının e-posta adresi — NULL veya "@privaterelay.appleid.com" olabilir.
    /// NEDEN: Kullanıcı "E-postamı gizle" seçeneğini kullandıysa Apple relay adresi verir.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>E-posta adresi Apple tarafından doğrulandı mı?</summary>
    public bool IsEmailVerified { get; init; }
}
