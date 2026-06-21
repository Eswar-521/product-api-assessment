using Microsoft.EntityFrameworkCore;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Infrastructure.Data;

namespace ProductApi.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedUserName = userName.Trim();

        return _dbContext.Users.AnyAsync(
            user => user.UserName == normalizedUserName || user.Email == normalizedEmail,
            cancellationToken);
    }

    public Task<AppUser?> GetByUserNameOrEmailAsync(string userNameOrEmail, bool trackChanges, CancellationToken cancellationToken)
    {
        var value = userNameOrEmail.Trim();
        var normalizedEmail = value.ToLowerInvariant();

        var query = _dbContext.Users
            .Include(user => user.RefreshTokens)
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(
            user => user.UserName == value || user.Email == normalizedEmail,
            cancellationToken);
    }

    public Task<AppUser?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(
                user => user.RefreshTokens.Any(token => token.TokenHash == refreshTokenHash),
                cancellationToken);
    }

    public void Add(AppUser user)
    {
        _dbContext.Users.Add(user);
    }
}
