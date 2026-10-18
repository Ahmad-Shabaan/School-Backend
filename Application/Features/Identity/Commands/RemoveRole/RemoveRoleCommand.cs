using Application.Common;
using MediatR;

namespace Application.Features.Identity.Commands.RemoveRole;

public class RemoveRoleCommand : IRequest<Result>
{
    public Guid UserId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}
