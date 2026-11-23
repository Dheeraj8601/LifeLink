using LifeLink.Application.DTOs.Admin;

namespace LifeLink.Application.Interfaces.Repositories;

public interface IAdminRepository
{
    Task<IReadOnlyList<PendingDonorResponse>> GetPendingDonorsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> VerifyDonorAsync(
        int donorId,
        CancellationToken cancellationToken = default);

    Task<bool> RejectDonorAsync(
        int donorId,
        string rejectionReason,
        CancellationToken cancellationToken = default);
}