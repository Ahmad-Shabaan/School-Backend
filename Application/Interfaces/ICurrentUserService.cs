namespace Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    string? Email { get; }

    string? FullName { get; }

    bool IsAuthenticated { get; }

    IEnumerable<string> Roles { get; }
}
