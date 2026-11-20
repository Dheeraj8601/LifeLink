
using LifeLink.Domain.Entities;

namespace LifeLink.Application.Interfaces.Repositories;

public interface IRequestResponseRepository
{
    Task AddAsync(
        RequestResponse response,
        CancellationToken cancellationToken = default);

    Task<RequestResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int bloodRequestId,
        int donorId,
        CancellationToken cancellationToken = default);

    Task<int?> GetEligibleDonorProfileIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<List<RequestResponse>> GetByBloodRequestIdAsync(
        int bloodRequestId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        RequestResponse response,
        CancellationToken cancellationToken = default);

    Task<int> GetAcceptedResponseCountAsync(
        int bloodRequestId,
        CancellationToken cancellationToken = default);
}
