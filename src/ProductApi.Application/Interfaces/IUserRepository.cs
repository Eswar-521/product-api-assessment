using ProductApi.Domain.Entities;

namespace ProductApi.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email, CancellationToken cancellationToken);
    Task<AppUser?> GetByUserNameOrEmailAsync(string userNameOrEmail, bool trackChanges, CancellationToken cancellationToken);
    Task<AppUser?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken);
    void Add(AppUser user);
}
