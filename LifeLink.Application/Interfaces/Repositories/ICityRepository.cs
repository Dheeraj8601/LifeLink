using LifeLink.Domain.Entities;

namespace LifeLink.Application.Interfaces.Repositories;

public interface ICityRepository
{
    Task<IReadOnlyList<City>> GetAllAsync(CancellationToken cancellationToken);
}