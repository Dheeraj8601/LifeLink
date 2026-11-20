using LifeLink.Domain.Entities;

namespace LifeLink.Application.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email,CancellationToken cancellationToken = default);
}