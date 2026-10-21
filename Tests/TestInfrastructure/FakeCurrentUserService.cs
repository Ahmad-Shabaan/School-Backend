using Application.Interfaces;
using Domain.Entities;

namespace Tests.TestInfrastructure;

public sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; } = Guid.Empty;

    public string? Email { get; set; }

    public string? FullName { get; set; }

    public bool IsAuthenticated { get; set; }

    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    public void SignIn(
        ApplicationUser user,
        params string[] roles)
    {
        UserId = user.Id;
        Email = user.Email;
        FullName = $"{user.FirstName} {user.LastName}";
        IsAuthenticated = true;
        Roles = roles;
    }

    public void SignOut()
    {
        UserId = Guid.Empty;
        Email = null;
        FullName = null;
        IsAuthenticated = false;
        Roles = Array.Empty<string>();
    }
}
