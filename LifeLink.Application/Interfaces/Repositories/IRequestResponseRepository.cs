
using LifeLink.Application.DTOs.RequestResponses;
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

    Task<List<DonorResponseResult>> GetResponsesByDonorIdAsync(
        int donorId,
        CancellationToken cancellationToken = default);

    // Locks the donor row for the current transaction.
    Task LockDonorForCommitmentAsync(
        int donorId,
        CancellationToken cancellationToken = default);

    // Checks whether the donor has another accepted commitment.
    Task<bool> HasOtherActiveCommitmentAsync(
        int donorId,
        int excludedResponseId,
        CancellationToken cancellationToken = default);

    Task<string?> GetEligibleDonorBloodGroupAsync(
    int userId,
    CancellationToken cancellationToken = default);
}
