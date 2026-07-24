// ─────────────────────────────────────────────────────────────────────────────
// UpdateUserRoleCommandValidator.cs
//
// AMAÇ: UpdateUserRoleCommand.Role alanının yalnızca "User" veya "Admin" olmasını
//       zorunlu kılar — `Users.Role` CHECK constraint'iyle aynı küme, DB hatasına
//       düşmeden önce uygulama katmanında yakalanır.
// BAĞIMLILIKLAR: FluentValidation.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using WordLearner.Application.Features.Admin;

namespace WordLearner.Application.Validators.Admin;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.Role)
            .Must(role => role is "User" or "Admin")
            .WithMessage("Role must be 'User' or 'Admin'.")
            .WithErrorCode("INVALID_USER_ROLE");
    }
}
