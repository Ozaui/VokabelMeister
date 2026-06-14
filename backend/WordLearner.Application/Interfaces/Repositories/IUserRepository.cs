/// <summary>
/// IUserRepository.cs
///
/// AMAÇ: Kullanıcı sorgularına özgü repository sözleşmesi.
/// NEDEN: Generic CRUD yetmez — auth akışı için e-posta, Google ID, Apple ID ile arama gerekir.
/// BAĞIMLILIKLAR: IRepository (generic), User entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Kullanıcı repository arayüzü.
///
/// AMAÇ: Kimlik doğrulama ve profil sorgularını tanımlamak.
/// NEDEN: Login, kayıt ve sosyal giriş akışları özel sorgular gerektirir.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// AMAÇ: E-posta adresiyle kullanıcı bulur — login ve kayıt kontrolü için.
    /// NEDEN: Login akışında şifre karşılaştırması için kullanıcı önce e-posta ile bulunur.
    /// NASIL: Büyük/küçük harf duyarsız karşılaştırma yapılmalı.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Google ID ile kullanıcı bulur — Google OAuth akışı için.
    /// NEDEN: Google ile giriş yapan kullanıcı sistemde varsa mevcut hesaba bağlanır.
    /// </summary>
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Apple ID ile kullanıcı bulur — Apple Sign In akışı için.
    /// NEDEN: Apple ile giriş yapan kullanıcı sistemde varsa mevcut hesaba bağlanır.
    /// </summary>
    Task<User?> GetByAppleIdAsync(string appleId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: E-postanın sistemde kayıtlı olup olmadığını kontrol eder.
    /// NEDEN: Kayıt sırasında hızlı duplikasyon kontrolü için Count yerine Any kullanılır.
    /// NASIL: Sadece boolean döner — User nesnesi yüklenmez, performanslıdır.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
