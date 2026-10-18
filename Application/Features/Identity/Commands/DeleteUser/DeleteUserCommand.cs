using Application.Common;
using MediatR;

namespace Application.Features.Identity.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
