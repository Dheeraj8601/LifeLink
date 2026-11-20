using LifeLink.Domain.Entities;

namespace LifeLink.Application.Interfaces.Services;

public interface ICityService
{
    Task<IReadOnlyList<City>> GetAllAsync(
        CancellationToken cancellationToken);
}