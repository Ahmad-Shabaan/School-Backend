namespace Application.Features.Authentication.DTOs;

public class AuthResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool ForceChangePassword { get; set; }

    public UserDto? User { get; set; }
}
