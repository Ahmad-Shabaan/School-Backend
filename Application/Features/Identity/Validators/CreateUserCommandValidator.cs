using Application.Features.Identity.Commands.CreateUser;
using FluentValidation;

namespace Application.Features.Identity.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            //.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not a valid email addres.");


        RuleForEach(x => x.Roles)
            .NotEmpty()
            .MaximumLength(100);
    }
}
