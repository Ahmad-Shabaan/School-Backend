using Application.Common;
using MediatR;

namespace Application.Features.Identity.Commands.AssignRole;

public class AssignRoleCommand : IRequest<Result>
{
    public Guid UserId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}
