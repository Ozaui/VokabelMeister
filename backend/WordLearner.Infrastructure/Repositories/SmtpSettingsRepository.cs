// ─────────────────────────────────────────────────────────────────────────────
// SmtpSettingsRepository.cs
//
// AMAÇ: ISmtpSettingsRepository'nin EF Core implementasyonu.
// NEDEN: Repository<SmtpSettings>'i miras alarak genel CRUD'u yeniden yazmadan
//        yalnızca "tekil satırı getir" sorgusunu ekler (CategoryRepository ile
//        aynı desen, A-06).
// BAĞIMLILIKLAR: EF Core, Repository<SmtpSettings>, WordLearnerDbContext.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.System;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class SmtpSettingsRepository : Repository<SmtpSettings>, ISmtpSettingsRepository
{
    public SmtpSettingsRepository(WordLearnerDbContext db)
        : base(db) { }

    // NEDEN FirstOrDefaultAsync (WHERE'sız): tabloda hiçbir zaman birden fazla
    //       satır oluşmaz — UpdateSmtpSettingsCommandHandler yalnızca ilk kayıt
    //       yoksa AddAsync, varsa UpdateAsync çağırır (aşağıdaki NEDEN notuna bkz.
    //       ISmtpSettingsRepository.cs).
    // NEDEN OrderBy(Id) (kod denetiminde bulundu): tabloda "en fazla bir satır"
    //       yalnızca UYGULAMA seviyesinde garanti edilir (DB'de UNIQUE/CHECK
    //       constraint YOK) — iki eşzamanlı PUT (iki ayrı DbContext scope'u)
    //       ikisi de "kayıt yok" görüp ikisi de AddAsync çağırabilir, tabloda
    //       BİRDEN FAZLA satır oluşabilir. Bu senaryo admin-only, düşük trafikli
    //       bir ayar ekranı için kabul edilebilir bir risk olarak bilinçli
    //       BIRAKILDI (DB seviyesinde bir engelleme eklenmedi) — ama en azından
    //       OrderBy ile "birden fazla satır oluşursa HANGİSİ döner" sorusunun
    //       cevabı DETERMİNİSTİK kalır (her zaman en küçük Id).
    public Task<SmtpSettings?> GetCurrentAsync(CancellationToken ct = default) =>
        _set.OrderBy(s => s.Id).FirstOrDefaultAsync(ct);
}
