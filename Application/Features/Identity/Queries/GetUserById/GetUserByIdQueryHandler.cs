using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Features.Identity.Errors;
using Application.Features.Identity.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.Queries.GetUserById;

public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Result<UserDetailDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new UserDetailProjectionSpecification(request.Id);

        var user = await _unitOfWork.Repository<ApplicationUser>()
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (user is null)
            return Result<UserDetailDto>.Failed("User not found.",UserErrors.UserNotFound);

        user.Roles = await _userManager.GetRolesAsync(
            new ApplicationUser { Id = request.Id });

        return Result<UserDetailDto>.Succeeded(user, "User retrieved successfully.");
    }
}
