using Application.Features.Identity.Commands.AssignRole;
using FluentValidation;

namespace Application.Features.Identity.Validators;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
