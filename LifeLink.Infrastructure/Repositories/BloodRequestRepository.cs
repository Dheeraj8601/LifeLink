
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Domain.Entities;
using LifeLink.Infrastructure.Data;
using LifeLink.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using DomainBloodRequest = LifeLink.Domain.Entities.BloodRequest;

namespace LifeLink.Infrastructure.Repositories;

public class BloodRequestRepository : IBloodRequestRepository
{
    private readonly LifeLinkDbContext _context;

    public BloodRequestRepository(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        BloodRequest bloodRequest,
        CancellationToken cancellationToken = default)
    {
        var entity = BloodRequestMapper.ToEntity(bloodRequest);

        entity.Location = CreateLocation(
            bloodRequest.Latitude,
            bloodRequest.Longitude);

        await _context.BloodRequests.AddAsync(
            entity,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        bloodRequest.Id = entity.Id;
        bloodRequest.CreatedAt = entity.CreatedAt;
    }

    public async Task<BloodRequest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.BloodRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken);

        return entity is null
            ? null
            : BloodRequestMapper.ToDomain(entity);
    }

    public async Task<BloodRequest?> GetByIdForUpdateAsync(int id,CancellationToken cancellationToken = default)
    {
        var entity = await _context.BloodRequests
            .FromSqlInterpolated($"""
            SELECT *
            FROM dbo.BloodRequests WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = {id}
            """)
            .AsTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? null
            : BloodRequestMapper.ToDomain(entity);
    }

    public async Task<bool> CityExistsAsync(
        int cityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .AnyAsync(
                city => city.Id == cityId,
                cancellationToken);
    }

    public async Task<bool> IsActiveRequesterAsync(
        int requesterId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(
                user => user.Id == requesterId
                        && user.IsActive
                        && user.Role == "Requester",
                cancellationToken);
    }

    public async Task<List<DomainBloodRequest>> GetByRequesterIdAsync(
        int requesterId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.BloodRequests
            .AsNoTracking()
            .Where(request => request.RequesterId == requesterId)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities
            .Select(BloodRequestMapper.ToDomain)
            .ToList();
    }

    public async Task<(List<DomainBloodRequest> Items, int TotalCount)> GetOpenRequestsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BloodRequests
            .AsNoTracking()
            .Where(request => request.Status == "Open");

        // Count all matching requests before pagination.
        var totalCount = await query.CountAsync(cancellationToken);

        // Retrieve only the requested page.
        var entities = await query
            .OrderByDescending(request => request.CreatedAt)
            .ThenByDescending(request => request.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities
            .Select(BloodRequestMapper.ToDomain)
            .ToList();

        return (items, totalCount);
    }

    public async Task UpdateAsync(
        BloodRequest bloodRequest,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.BloodRequests
            .FirstOrDefaultAsync(
                request => request.Id == bloodRequest.Id,
                cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"Blood request with ID {bloodRequest.Id} was not found.");
        }

        entity.Status = bloodRequest.Status.ToString();

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static Point? CreateLocation(
        decimal? latitude,
        decimal? longitude)
    {
        if (latitude is null && longitude is null)
            return null;

        if (latitude is null || longitude is null)
            throw new ArgumentException(
                "Both latitude and longitude must be provided.");

        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        return new Point(
            (double)longitude.Value,
            (double)latitude.Value)
        {
            SRID = 4326
        };
    }
}
