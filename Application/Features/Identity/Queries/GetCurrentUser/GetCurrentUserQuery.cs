using Application.Common;
using Application.Features.Identity.DTOs;
using MediatR;

namespace Application.Features.Identity.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<Result<UserDetailDto>>
{
}
