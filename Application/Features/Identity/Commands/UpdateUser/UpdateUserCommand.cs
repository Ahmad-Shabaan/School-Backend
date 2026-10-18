using Application.Common;
using Application.Features.Identity.DTOs;
using MediatR;

namespace Application.Features.Identity.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<Result<UserDetailDto>>
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}
