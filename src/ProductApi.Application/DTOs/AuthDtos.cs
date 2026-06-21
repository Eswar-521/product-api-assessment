namespace ProductApi.Application.DTOs;

public sealed class RegisterRequest
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
}

public sealed class LoginRequest
{
    public string UserNameOrEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresOn { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresOn { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
