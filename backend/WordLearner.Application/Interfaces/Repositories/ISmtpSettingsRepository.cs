// ─────────────────────────────────────────────────────────────────────────────
// ISmtpSettingsRepository.cs
//
// AMAÇ: SmtpSettings'e özel sorgu — tek satırlık (singleton) yapılandırma kaydını okuma.
// NEDEN IRepository<SmtpSettings>'i GENİŞLETİR (Language ile AYNI istisna DEĞİL):
//       SmtpSettings BaseEntity'den türüyor (Category/WordConcept ile aynı sınıf),
//       bu yüzden AddAsync/UpdateAsync gibi genel CRUD'u Repository<T>'den miras alır —
//       yalnızca "TEK satırı getir" sorgusu bu arayüze özeldir (Id parametresiz).
// BAĞIMLILIKLAR: IRepository<SmtpSettings>.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.System;

namespace WordLearner.Application.Interfaces.Repositories;

public interface ISmtpSettingsRepository : IRepository<SmtpSettings>
{
    // AMAÇ: Kayıtlı tek SMTP ayarı satırını döner — hiç kaydedilmemişse null.
    Task<SmtpSettings?> GetCurrentAsync(CancellationToken ct = default);
}
