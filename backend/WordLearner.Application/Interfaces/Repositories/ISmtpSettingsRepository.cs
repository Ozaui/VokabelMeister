using WordLearner.Domain.Entities.System;

namespace WordLearner.Application.Interfaces.Repositories;

public interface ISmtpSettingsRepository : IRepository<SmtpSettings>
{
    // Kayıtlı tek SMTP ayarı satırını döner — hiç kaydedilmemişse null.
    Task<SmtpSettings?> GetCurrentAsync(CancellationToken ct = default);
}
