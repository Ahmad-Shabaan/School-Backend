using Application.Common;
using Application.Features.Identity.DTOs;
using MediatR;

namespace Application.Features.Identity.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<Result<UserDetailDto>>
{
    public Guid Id { get; set; }
}
