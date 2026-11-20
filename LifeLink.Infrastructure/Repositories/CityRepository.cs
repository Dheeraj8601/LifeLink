using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Domain.Entities;
using LifeLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeLink.Infrastructure.Repositories;

public class CityRepository : ICityRepository
{
    private readonly LifeLinkDbContext _context;

    public CityRepository(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<City>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var cities = await _context.Cities
            .AsNoTracking()
            .OrderBy(city => city.Name)
            .ToListAsync(cancellationToken);

        return cities.Select(city => new City
        {
            Id = city.Id,
            Name = city.Name,
            State = city.State
        }).ToList();
    }
}