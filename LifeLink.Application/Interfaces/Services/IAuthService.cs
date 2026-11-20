using LifeLink.Application.DTOs.Auth;

namespace LifeLink.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(LoginCommand command,CancellationToken cancellationToken = default);
}