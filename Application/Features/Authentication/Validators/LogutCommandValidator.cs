using Application.Features.Authentication.Commands.logout;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Authentication.Validators
{
    public class LogutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogutCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
