using ProductApi.Domain.Entities;

namespace ProductApi.Application.Interfaces;

public sealed record AccessTokenResult(string Token, DateTime ExpiresOn);

public sealed record RefreshTokenResult(string Token, string TokenHash, DateTime ExpiresOn);

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(AppUser user);
    RefreshTokenResult CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
