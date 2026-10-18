using Application.Common;
using Application.Features.Authentication.DTOs;
using Application.Features.Authentication.Errors;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;
namespace Application.Features.Authentication.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler(ICurrentUserService currentUserService, ISettings settings, UserManager<ApplicationUser> userManager, RoleManager<AppRoles> roleManager) : IRequestHandler<ChangePasswordCommand, Result<UserDto>>
    {

        public async Task<Result<UserDto>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId;
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return Result<UserDto>.Failed("No user with id", AuthErrors.UserNotFound);
            }
            user.ForceChangedPassword = false;
            var changePasswordResult = await userManager.ChangePasswordAsync(user, settings.DefaultPassword, request.Password);
            if (!changePasswordResult.Succeeded)
            {
                var errors = changePasswordResult.Errors.Select(e => new Error(e.Code, e.Description, HttpStatusCode.BadRequest));
                return Result<UserDto>.Failed($"Failed to create user", null, errors);
            }

            var roles = await userManager.GetRolesAsync(user);
            return Result<UserDto>.Succeeded(new UserDto()
            {
                Roles = roles
            });
        }
    }
}
