using Application.Common;
using Application.Features.Identity.Errors;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Application.Features.Identity.Commands.RemoveRole;

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RemoveRoleCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(
        RemoveRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
            return Result.Failed("User not found.", UserErrors.UserNotFound );

        if (!await _userManager.IsInRoleAsync(user, request.RoleName))
            return Result.Failed("User does not have the specified role.", UserErrors.RoleNotFount);

        var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error(e.Code, e.Description, HttpStatusCode.BadRequest));
            return Result.Failed($"Failed to remove role",null, errors);
        }

        return Result.Succeeded("Role removed successfully.");
    }
}
