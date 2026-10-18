using Application.Common;
using Application.Features.Identity.DTOs;
using Application.Features.Identity.Errors;
using Application.Features.Identity.Specifications;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Identity.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandHandler
    : IRequestHandler<ToggleUserStatusCommand, Result<UserDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public ToggleUserStatusCommandHandler(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<Result<UserDetailDto>> Handle(
        ToggleUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        var spec = new UserByIdSpecification(request.Id);

        var user = await _unitOfWork.Repository<ApplicationUser>()
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (user is null)
            return Result<UserDetailDto>.Failed("User not found.", UserErrors.UserNotFound);

        user.IsActive = request.IsActive;
        user.UpdateAt = DateTime.UtcNow;

        _unitOfWork.Repository<ApplicationUser>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var projectionSpec = new UserDetailProjectionSpecification(user.Id);

        var userDto = await _unitOfWork.Repository<ApplicationUser>()
            .FirstOrDefaultAsync(projectionSpec, cancellationToken);

        if (userDto is null)
            return Result<UserDetailDto>.Failed("User status updated but could not be retrieved.", UserErrors.UserNotFound);

        userDto.Roles = await _userManager.GetRolesAsync(user);

        return Result<UserDetailDto>.Succeeded(
            userDto,
            $"User {(request.IsActive ? "activated" : "deactivated")} successfully.");
    }
}
