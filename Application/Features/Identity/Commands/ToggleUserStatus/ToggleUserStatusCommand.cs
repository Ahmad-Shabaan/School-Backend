using Application.Common;
using Application.Features.Identity.DTOs;
using MediatR;

namespace Application.Features.Identity.Commands.ToggleUserStatus;

public class ToggleUserStatusCommand : IRequest<Result<UserDetailDto>>
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }
}
