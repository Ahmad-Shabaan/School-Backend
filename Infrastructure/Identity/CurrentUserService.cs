using Application.Interfaces;
using Infrastructure.Identity.Extensions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        _httpContextAccessor.HttpContext?.User.GetUserId() ?? Guid.Empty;

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.GetEmail();

    public string? FullName =>
        _httpContextAccessor.HttpContext?.User.GetFullName();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User.GetRoles()
        ?? Enumerable.Empty<string>();
}
