using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;

namespace ProductApi.Infrastructure.Identity;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
    }

    public AccessTokenResult CreateAccessToken(AppUser user)
    {
        var now = DateTime.UtcNow;
        var expiresOn = now.AddMinutes(_options.AccessTokenMinutes);

        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object>
        {
            ["sub"] = user.Id.ToString(),
            ["nameid"] = user.Id.ToString(),
            ["name"] = user.UserName,
            ["email"] = user.Email,
            ["role"] = user.Role,
            ["iss"] = _options.Issuer,
            ["aud"] = _options.Audience,
            ["iat"] = ToUnixTimeSeconds(now),
            ["nbf"] = ToUnixTimeSeconds(now),
            ["exp"] = ToUnixTimeSeconds(expiresOn)
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";
        var signature = Sign(unsignedToken);

        return new AccessTokenResult($"{unsignedToken}.{signature}", expiresOn);
    }

    public RefreshTokenResult CreateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = HashRefreshToken(token);
        var expiresOn = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);

        return new RefreshTokenResult(token, tokenHash, expiresOn);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }

    private string Sign(string unsignedToken)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.Secret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));
        return Base64UrlEncode(signatureBytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static long ToUnixTimeSeconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }
}
