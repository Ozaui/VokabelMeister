// ─────────────────────────────────────────────────────────────────────────────
// UpdateSmtpSettingsCommandValidator.cs
//
// AMAÇ: UpdateSmtpSettingsCommand'ın zorunlu alanlarını doğrular.
// NEDEN Password'e NotEmpty (ama içeriği serbest — "***" da geçerli bir değer):
//       PUT her zaman TAM bir form gönderir (kısmi güncelleme YOK) — admin panel
//       şifre alanını hiç değiştirmese bile GET'ten aldığı "***" maskesini AYNEN
//       geri gönderir; Handler bu sabit değeri "değiştirme" sinyali olarak okur
//       (UpdateSmtpSettingsCommand.cs "NEDEN MaskedPassword"). Boş string ise ne
//       "değiştirme" ne de geçerli bir yeni şifre anlamına gelir — reddedilir.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.Features.Smtp;

namespace WordLearner.Application.Validators.Smtp;

public class UpdateSmtpSettingsCommandValidator : AbstractValidator<UpdateSmtpSettingsCommand>
{
    public UpdateSmtpSettingsCommandValidator()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required").WithErrorCode("SMTP_HOST_REQUIRED");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage("Port must be between 1 and 65535")
            .WithErrorCode("SMTP_PORT_INVALID");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .WithErrorCode("SMTP_USERNAME_REQUIRED");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .WithErrorCode("SMTP_PASSWORD_REQUIRED");

        RuleFor(x => x.FromEmail)
            .NotEmpty()
            .WithMessage("From email is required")
            .WithErrorCode("SMTP_FROM_EMAIL_REQUIRED")
            .EmailAddress()
            .WithMessage("From email must be a valid email address")
            .WithErrorCode("SMTP_FROM_EMAIL_INVALID");

        RuleFor(x => x.FromName)
            .NotEmpty()
            .WithMessage("From name is required")
            .WithErrorCode("SMTP_FROM_NAME_REQUIRED");
    }
}
