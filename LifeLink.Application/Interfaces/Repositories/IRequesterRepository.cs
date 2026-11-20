using LifeLink.Domain.Entities;

namespace LifeLink.Application.Interfaces.Repositories;

public interface IRequesterRepository
{
    Task<int> RegisterAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> CityExistsAsync(
        int cityId,
        CancellationToken cancellationToken = default);
}