using Application.Common;
using Application.Features.Authentication.DTOs;
using MediatR;

namespace Application.Features.Authentication.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
