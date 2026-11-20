using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Domain.Entities;

namespace LifeLink.Application.Services;

public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;

    public CityService(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;
    }

    public async Task<IReadOnlyList<City>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _cityRepository.GetAllAsync(cancellationToken);
    }
}