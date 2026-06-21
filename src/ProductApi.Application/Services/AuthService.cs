using FluentValidation;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(_registerValidator, request, cancellationToken);

        if (await _userRepository.ExistsByUserNameOrEmailAsync(request.UserName, request.Email, cancellationToken))
        {
            throw new ConflictException("A user with the same username or email already exists.");
        }

        var user = new AppUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            CreatedOn = DateTime.UtcNow
        };

        _userRepository.Add(user);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(_loginValidator, request, cancellationToken);

        var user = await _userRepository.GetByUserNameOrEmailAsync(request.UserNameOrEmail, trackChanges: true, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid username/email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(_refreshTokenValidator, request, cancellationToken);

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        var refreshToken = user.RefreshTokens.Single(token => token.TokenHash == tokenHash);
        if (!refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        var replacement = _tokenService.CreateRefreshToken();
        refreshToken.RevokedOn = DateTime.UtcNow;
        refreshToken.ReplacedByTokenHash = replacement.TokenHash;

        user.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = replacement.TokenHash,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = replacement.ExpiresOn
        });

        var accessToken = _tokenService.CreateAccessToken(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(user, accessToken, replacement);
    }

    private async Task<AuthResponse> IssueTokensAsync(AppUser user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = refreshToken.TokenHash,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = refreshToken.ExpiresOn
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(user, accessToken, refreshToken);
    }

    private static AuthResponse ToResponse(AppUser user, AccessTokenResult accessToken, RefreshTokenResult refreshToken)
    {
        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresOn = accessToken.ExpiresOn,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresOn = refreshToken.ExpiresOn,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role
        };
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
