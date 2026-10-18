using Application.Features.Identity.Commands.RemoveRole;
using FluentValidation;

namespace Application.Features.Identity.Validators;

public class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
