
using LifeLink.Application.DTOs.Donors;
using LifeLink.Domain.Entities;
using LifeLink.Application.Models.Donors;

namespace LifeLink.Application.Interfaces.Services;

public interface IDonorService
{
    Task<Donor?> GetDonorByIdAsync(
        int donorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Donor>> GetAvailableDonorsAsync(
        int cityId,
        string bloodGroup,
        CancellationToken cancellationToken = default);

    Task<Donor> RegisterDonorAsync(
        RegisterDonorCommand command,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<DonorMatchResult> Items, int TotalCount)> GetNearbyDonorsAsync(
        string bloodGroup,
        double hospitalLatitude,
        double hospitalLongitude,
        double radiusKm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Donor?> GetDonorByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task UpdateAvailabilityAsync(
        int userId,
        bool isAvailable,
        CancellationToken cancellationToken = default);
}
