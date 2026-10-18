using Application.Common;
using Application.Features.Identity.Errors;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Application.Features.Identity.Commands.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppRoles> _roleManager;

    public AssignRoleCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<AppRoles> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result> Handle(
        AssignRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _roleManager.RoleExistsAsync(request.RoleName))
            return Result.Failed($"Role '{request.RoleName}' does not exist.", UserErrors.RoleNotFount);

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
            return Result.Failed("User not found.", UserErrors.RoleNotFount);

        if (await _userManager.IsInRoleAsync(user, request.RoleName))
            return Result.Failed("User already has the specified role.", UserErrors.RoleAlreadyExists);

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error(e.Code, e.Description, HttpStatusCode.BadRequest));
            return Result.Failed($"Failed to assign role",null ,errors);
        }

        return Result.Succeeded("Role assigned successfully.");
    }
}
