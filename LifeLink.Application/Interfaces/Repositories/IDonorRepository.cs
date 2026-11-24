using LifeLink.Domain.Entities;
using LifeLink.Application.Models.Donors;


namespace LifeLink.Application.Interfaces.Repositories;

public interface IDonorRepository
{
    Task<Donor?> GetByIdAsync(int donorId,CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Donor>> GetAvailableDonorsAsync(
        int cityId,
        string bloodGroup,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Donor donor,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Donor donor,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task RegisterAsync(
        User user,
        Donor donor,
        CancellationToken cancellationToken = default);

    Task<bool> CityExistsAsync(
        int cityId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<DonorMatchResult> Items, int TotalCount)> GetNearbyDonorsAsync(
        string bloodGroup,
        double hospitalLatitude,
        double hospitalLongitude,
        double radiusKm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Donor?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task UpdateAvailabilityAsync(
        int userId,
        bool isAvailable,
        CancellationToken cancellationToken = default);


    Task<IReadOnlyList<EligibleDonorNotification>> GetEligibleDonorsForBloodRequestAsync(
            string recipientBloodGroup,
            double latitude,
            double longitude,
            double radiusKm,
            CancellationToken cancellationToken = default);

}