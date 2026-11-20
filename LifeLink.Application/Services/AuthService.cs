using LifeLink.Application.DTOs.Auth;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Interfaces.Services;

namespace LifeLink.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IAuthRepository authRepository,IPasswordHasher passwordHasher)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResult?> LoginAsync(LoginCommand command,CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var user = await _authRepository.GetUserByEmailAsync(
            email,
            cancellationToken);

        if (user is null || !user.IsActive)
            return null;

        var isPasswordValid = _passwordHasher.VerifyPassword(
            command.Password,
            user.PasswordHash);

        if (!isPasswordValid)
            return null;

        return new LoginResult
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}