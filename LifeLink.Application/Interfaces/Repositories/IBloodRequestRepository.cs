
using LifeLink.Domain.Entities;

namespace LifeLink.Application.Interfaces.Repositories;

public interface IBloodRequestRepository
{
    Task AddAsync(
        BloodRequest bloodRequest,
        CancellationToken cancellationToken = default);

    Task<BloodRequest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> CityExistsAsync(
        int cityId,
        CancellationToken cancellationToken = default);

    Task<bool> IsActiveRequesterAsync(
        int requesterId,
        CancellationToken cancellationToken = default);

    Task<List<BloodRequest>> GetByRequesterIdAsync(
        int requesterId,
        CancellationToken cancellationToken = default);

    Task<(List<BloodRequest> Items, int TotalCount)> GetOpenRequestsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BloodRequest bloodRequest,
        CancellationToken cancellationToken = default);

    Task<BloodRequest?> GetByIdForUpdateAsync(
    int id,
    CancellationToken cancellationToken = default);
}
