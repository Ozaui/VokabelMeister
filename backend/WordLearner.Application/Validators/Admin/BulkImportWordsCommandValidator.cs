using FluentValidation;
using WordLearner.Application.Features.Admin;

namespace WordLearner.Application.Validators.Admin;

// Satır bazlı doğrulama BURADA YOK — BulkImportWordsCommandHandler.TryImportRowAsync içinde yapılır,
// çünkü bu endpoint 795 satırdan biri hatalıysa diğerlerini yine de işler (ValidationFilter tüm isteği reddeder).
// Yalnızca "hiç satır yok" gibi tüm isteği anlamsız kılan durum burada reddedilir.
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
