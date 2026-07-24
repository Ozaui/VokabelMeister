// ─────────────────────────────────────────────────────────────────────────────
// BulkImportWordsCommandValidator.cs
//
// AMAÇ: BulkImportWordsCommand.Rows'un boş olmamasını zorunlu kılar.
// NEDEN yalnızca BU kontrol (satır bazlı alan doğrulaması BURADA YOK):
//        CreateWordCommandValidator'ın AKSİNE, bu Command'ın satır bazlı
//        doğrulaması BulkImportWordsCommandHandler.TryImportRowAsync içinde
//        yapılır — FluentValidation TÜM isteği reddetmek için tasarlanmış
//        (ValidationFilter, tek bir hata bulunca 400 döner), ama bu endpoint'in
//        amacı TAM TERSİ: 795 satırdan biri hatalıysa diğer 794'ünü YİNE DE
//        işlemek. Bu yüzden yalnızca "hiç satır YOK" gibi TÜM isteği anlamsız
//        kılan bir durum burada, en dış seviyede reddedilir.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.Features.Admin;

namespace WordLearner.Application.Validators.Admin;

public class BulkImportWordsCommandValidator : AbstractValidator<BulkImportWordsCommand>
{
    public BulkImportWordsCommandValidator()
    {
        RuleFor(x => x.Rows)
            .NotEmpty()
            .WithMessage("At least one row is required.")
            .WithErrorCode("BULK_IMPORT_ROWS_REQUIRED");
    }
}
