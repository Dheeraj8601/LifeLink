
using LifeLink.Application.DTOs.RequestResponses;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Domain.Entities;
using LifeLink.Domain.Enums;
using LifeLink.Infrastructure.Data;
using LifeLink.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;

namespace LifeLink.Infrastructure.Repositories;

public class RequestResponseRepository : IRequestResponseRepository
{
    private readonly LifeLinkDbContext _context;

    public RequestResponseRepository(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        RequestResponse response,
        CancellationToken cancellationToken = default)
    {
        var entity = RequestResponseMapper.ToEntity(response);

        await _context.RequestResponses.AddAsync(
            entity,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        response.Id = entity.Id;
    }

    public async Task<RequestResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.RequestResponses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                response => response.Id == id,
                cancellationToken);

        return entity is null
            ? null
            : RequestResponseMapper.ToDomain(entity);
    }

    public async Task<bool> ExistsAsync(
        int bloodRequestId,
        int donorId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RequestResponses.AnyAsync(
            response =>
                response.BloodRequestId == bloodRequestId &&
                response.DonorId == donorId,
            cancellationToken);
    }

    public async Task<int?> GetEligibleDonorProfileIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DonorProfiles
            .Where(donor =>
                donor.UserId == userId &&
                donor.IsAvailable &&
                donor.IsVerified)
            .Select(donor => (int?)donor.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<RequestResponse>> GetByBloodRequestIdAsync(
        int bloodRequestId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.RequestResponses
            .AsNoTracking()
            .Where(response => response.BloodRequestId == bloodRequestId)
            .OrderByDescending(response => response.RespondedAt)
            .ToListAsync(cancellationToken);

        return entities
            .Select(RequestResponseMapper.ToDomain)
            .ToList();
    }

    public async Task UpdateAsync(
        RequestResponse response,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.RequestResponses
            .FirstOrDefaultAsync(
                item => item.Id == response.Id,
                cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException(
                $"Response with ID {response.Id} was not found.");
        }

        entity.Status = response.Status.ToString();
        entity.RespondedAt = response.RespondedAt;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetAcceptedResponseCountAsync(
        int bloodRequestId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RequestResponses
            .CountAsync(
                response =>
                    response.BloodRequestId == bloodRequestId &&
                    response.Status == "Accepted",
                cancellationToken);
    }

    public async Task<List<DonorResponseResult>> GetResponsesByDonorIdAsync(
    int userId,
    CancellationToken cancellationToken = default)
    {
        var results = await (
            from response in _context.RequestResponses
            join donor in _context.DonorProfiles
                on response.DonorId equals donor.Id
            join request in _context.BloodRequests
                on response.BloodRequestId equals request.Id
            where donor.UserId == userId
            orderby response.RespondedAt descending
            select new
            {
                ResponseId = response.Id,
                BloodRequestId = request.Id,
                BloodGroup = request.BloodGroup,
                HospitalName = request.HospitalName,
                HospitalAddress = request.HospitalAddress,
                UnitsRequired = request.UnitsRequired,
                Urgency = request.Urgency,
                RequestStatus = request.Status,
                ResponseStatus = response.Status,
                RespondedAt = response.RespondedAt
            })
            .ToListAsync(cancellationToken);

        return results.Select(item => new DonorResponseResult
        {
            ResponseId = item.ResponseId,
            BloodRequestId = item.BloodRequestId,
            BloodGroup = item.BloodGroup,
            HospitalName = item.HospitalName,
            HospitalAddress = item.HospitalAddress,
            UnitsRequired = item.UnitsRequired,
            Urgency = Enum.Parse<RequestUrgency>(item.Urgency),
            RequestStatus = Enum.Parse<BloodRequestStatus>(item.RequestStatus),
            ResponseStatus = Enum.Parse<RequestResponseStatus>(item.ResponseStatus),
            RespondedAt = item.RespondedAt
        }).ToList();
    }
}
