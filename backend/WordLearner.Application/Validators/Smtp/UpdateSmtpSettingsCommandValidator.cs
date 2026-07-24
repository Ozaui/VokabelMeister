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

        // NotEmpty ama içeriği serbest — "***" maskesi de geçerli (Handler "değiştirme" sinyali olarak okur).
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
