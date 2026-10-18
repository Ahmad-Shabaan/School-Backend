using Application.Common;
using MediatR;
namespace Application.Features.Authentication.Commands.logout
{
    public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>
    {
    }
}
