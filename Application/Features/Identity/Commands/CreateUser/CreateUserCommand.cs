using Application.Common;
using Application.Features.Identity.DTOs;
using MediatR;

namespace Application.Features.Identity.Commands.CreateUser;

public class CreateUserCommand : IRequest<Result<UserDetailDto>>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();

    public bool IsActive { get; set; } = true;

    public bool ForceChangePassword { get; set; } = true;
}
