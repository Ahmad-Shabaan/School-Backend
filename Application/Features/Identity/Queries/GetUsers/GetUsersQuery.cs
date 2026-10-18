using Application.Common;
using Application.Features.Identity.DTOs;
using MediatR;

namespace Application.Features.Identity.Queries.GetUsers;

public class GetUsersQuery : IRequest<Result<IReadOnlyList<UserListItemDto>>>
{
    public string? SearchTerm { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
