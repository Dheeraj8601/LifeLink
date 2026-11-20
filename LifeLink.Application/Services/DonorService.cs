using LifeLink.Application.DTOs.Donors;
using LifeLink.Application.Exceptions;
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Application.Interfaces.Security;
using LifeLink.Application.Interfaces.Services;
using LifeLink.Domain.Entities;
using LifeLink.Application.Models.Donors;

namespace LifeLink.Application.Services;

public class DonorService : IDonorService
{
    private readonly IDonorRepository _donorRepository;
    private readonly IPasswordHasher _passwordHasher;

    public DonorService(
        IDonorRepository donorRepository,
        IPasswordHasher passwordHasher)
    {
        _donorRepository = donorRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Donor?> GetDonorByIdAsync(
        int donorId,
        CancellationToken cancellationToken = default)
    {
        if (donorId <= 0)
        {
            throw new ArgumentException(
                "Donor ID must be greater than zero.",
                nameof(donorId));
        }

        return await _donorRepository.GetByIdAsync(
            donorId,
            cancellationToken);
    }

    public async Task<Donor?> GetDonorByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentException(
                "User ID must be greater than zero.",
                nameof(userId));
        }

        return await _donorRepository.GetByUserIdAsync(
            userId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Donor>> GetAvailableDonorsAsync(
        int cityId,
        string bloodGroup,
        CancellationToken cancellationToken = default)
    {
        if (cityId <= 0)
        {
            throw new ArgumentException(
                "City ID must be greater than zero.",
                nameof(cityId));
        }

        if (string.IsNullOrWhiteSpace(bloodGroup))
        {
            throw new ArgumentException(
                "Blood group is required.",
                nameof(bloodGroup));
        }

        return await _donorRepository.GetAvailableDonorsAsync(
            cityId,
            bloodGroup,
            cancellationToken);
    }

    public async Task<Donor> RegisterDonorAsync(
        RegisterDonorCommand command,
        CancellationToken cancellationToken = default)
    {
        var cityExists = await _donorRepository.CityExistsAsync(command.CityId,cancellationToken);

        if (!cityExists)
        {
            throw new CityNotFoundException(command.CityId);
        }

        // 1. Normalize email
        var email = command.Email.Trim().ToLowerInvariant();

        // 2. Check duplicate email
        var emailExists = await _donorRepository.EmailExistsAsync(
            email,
            cancellationToken);

        if (emailExists)
        {
            throw new DuplicateEmailException(email);
        }

        // 3. Hash password
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        // 4. Create User domain entity
        var user = new User
        {
            FullName = command.FullName.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            PhoneNumber = command.PhoneNumber.Trim(),
            Role = "Donor",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 5. Create Donor domain entity
        var donor = new Donor
        {
            CityId = command.CityId,
            BloodGroup = command.BloodGroup,
            Latitude = command.Latitude,
            Longitude = command.Longitude,
            IsAvailable = true,
            IsVerified = false,
            LastDonationDate = null,
            CreatedAt = DateTime.UtcNow
        };

        // 6. Persist both records transactionally
        await _donorRepository.RegisterAsync(
            user,
            donor,
            cancellationToken
         );

        return donor;
    }

    public async Task<(IReadOnlyList<DonorMatchResult> Items, int TotalCount)> GetNearbyDonorsAsync(
        string bloodGroup,
        double hospitalLatitude,
        double hospitalLongitude,
        double radiusKm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bloodGroup))
        {
            throw new ArgumentException(
                "Blood group is required.",
                nameof(bloodGroup));
        }

        if (!double.IsFinite(hospitalLatitude) ||
            hospitalLatitude < -90 || hospitalLatitude > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hospitalLatitude),
                "Latitude must be between -90 and 90 degrees.");
        }

        if (!double.IsFinite(hospitalLongitude) ||
            hospitalLongitude < -180 || hospitalLongitude > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hospitalLongitude),
                "Longitude must be between -180 and 180 degrees.");
        }

        if (!double.IsFinite(radiusKm) || radiusKm <= 0 || radiusKm > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radiusKm),
                "Search radius must be greater than 0 and cannot exceed 100 km.");
        }

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 100.");
        }

        var skip = (long)(pageNumber - 1) * pageSize;

        if (skip > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Requested page number is too large.");
        }

        return await _donorRepository.GetNearbyDonorsAsync(
            bloodGroup.Trim().ToUpperInvariant(),
            hospitalLatitude,
            hospitalLongitude,
            radiusKm,
            pageNumber,
            pageSize,
            cancellationToken);
    }

    public async Task UpdateAvailabilityAsync(
        int userId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentException(
                "User ID must be greater than zero.",
                nameof(userId));
        }

        await _donorRepository.UpdateAvailabilityAsync(
            userId,
            isAvailable,
            cancellationToken);
    }
}