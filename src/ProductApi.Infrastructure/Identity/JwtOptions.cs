namespace ProductApi.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "ProductApiAssessment";
    public string Audience { get; init; } = "ProductApiAssessment";
    public string Secret { get; init; } = "change-me-to-a-secure-secret-with-at-least-32-characters";
    public int AccessTokenMinutes { get; init; } = 30;
    public int RefreshTokenDays { get; init; } = 7;
}
