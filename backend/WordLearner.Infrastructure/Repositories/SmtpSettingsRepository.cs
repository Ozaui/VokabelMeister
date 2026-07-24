using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.System;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class SmtpSettingsRepository : Repository<SmtpSettings>, ISmtpSettingsRepository
{
    public SmtpSettingsRepository(WordLearnerDbContext db)
        : base(db) { }

    // "En fazla bir satır" yalnızca uygulama seviyesinde garanti — DB'de UNIQUE/CHECK yok, iki
    // eşzamanlı PUT teorik olarak iki satır oluşturabilir (bilinçli kabul edilen düşük risk).
    // OrderBy(Id) bu durumda hangi satırın döneceğini en azından deterministik tutar.
    public Task<SmtpSettings?> GetCurrentAsync(CancellationToken ct = default) =>
        _set.OrderBy(s => s.Id).FirstOrDefaultAsync(ct);
}
