using Application.Interfaces;
using Domain.Entities;

namespace Application.Common;

public static class CurrentUserExtensions
{
    public static bool IsStaff(this ICurrentUserService currentUserService)
    {
        return currentUserService.Roles.Any(role =>
            role.Equals(AppRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
            role.Equals(AppRoles.Teacher, StringComparison.OrdinalIgnoreCase));
    }
}
