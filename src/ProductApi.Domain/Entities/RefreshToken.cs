namespace ProductApi.Domain.Entities;

public sealed class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public AppUser? User { get; set; }

    public bool IsActive => RevokedOn is null && DateTime.UtcNow <= ExpiresOn;
}
