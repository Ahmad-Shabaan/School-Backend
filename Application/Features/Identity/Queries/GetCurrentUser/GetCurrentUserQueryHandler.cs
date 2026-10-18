using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Features.Identity.Errors;
using Application.Features.Identity.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, Result<UserDetailDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Result<UserDetailDto>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            return Result<UserDetailDto>.Failed("User is not authenticated.");

        var spec = new UserDetailProjectionSpecification(_currentUserService.UserId);

        var user = await _unitOfWork.Repository<ApplicationUser>()
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (user is null)
            return Result<UserDetailDto>.Failed("User not found.",UserErrors.UserNotFound);

        user.Roles = await _userManager.GetRolesAsync(
            new ApplicationUser { Id = _currentUserService.UserId });

        return Result<UserDetailDto>.Succeeded(user, "Current user retrieved successfully.");
    }
}
