using Application.Common;
using Application.Features.Authentication.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Authentication.Commands.Refresh
{
    public class RefreshTokenCommand : IRequest<Result<TokenResultDto>>
    {
        public string RefresherToken { get; set; } = default!;
    }
}
