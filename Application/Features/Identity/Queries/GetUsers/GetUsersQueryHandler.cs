using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Features.Identity.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Collections;

namespace Application.Features.Identity.Queries.GetUsers;

public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<UserListItemDto>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new UserListItemProjectionSpecification(
            request.SearchTerm ?? string.Empty,
            request.Page,
            request.PageSize);

        var users = await _unitOfWork.Repository<ApplicationUser>()
            .ListAsync(spec, cancellationToken);
        var countSpec = new UsersCountSpecification(request.SearchTerm);
        var usersCount = await _unitOfWork.Repository<ApplicationUser>().CountAsync(countSpec,cancellationToken);

        var result =  Result<IReadOnlyList<UserListItemDto>>.Succeeded(
            users,
            "Users retrieved successfully.");

        result.Pagination = new Pagination()
        {
            PageIndex = request.Page,
            PageSize = request.PageSize,
            TotalCount = usersCount,
            TotalPages = (int)Math.Ceiling((double)usersCount / request.PageSize),

        };

        return result;
    }
}
