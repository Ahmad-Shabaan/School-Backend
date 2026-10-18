using Application.Features.Authentication.Commands.Refresh;
using FluentValidation;
namespace Application.Features.Authentication.Validators
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefresherToken)
            .NotEmpty();
        }
    }
}
