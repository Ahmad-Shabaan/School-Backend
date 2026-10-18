using Application.Features.Identity.Commands.ToggleUserStatus;
using FluentValidation;

namespace Application.Features.Identity.Validators;

public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
