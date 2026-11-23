using LifeLink.Application.DTOs.Admin;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Services;

namespace LifeLink.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<IReadOnlyList<PendingDonorResponse>> GetPendingDonorsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _adminRepository.GetPendingDonorsAsync(cancellationToken);
    }

    public async Task<bool> VerifyDonorAsync(
        int donorId,
        CancellationToken cancellationToken = default)
    {
        return await _adminRepository.VerifyDonorAsync(
            donorId,
            cancellationToken);
    }

    public async Task<bool> RejectDonorAsync(
        int donorId,
        string rejectionReason,
        CancellationToken cancellationToken = default)
    {
        return await _adminRepository.RejectDonorAsync(
            donorId,
            rejectionReason,
            cancellationToken);
    }
}