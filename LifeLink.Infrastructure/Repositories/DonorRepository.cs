
using LifeLink.Application.Interfaces.Repositories;
using LifeLink.Domain.Entities;
using LifeLink.Domain.Services;
using LifeLink.Infrastructure.Data;
using LifeLink.Application.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using LifeLink.Application.Models.Donors;
using NetTopologySuite.Geometries;

namespace LifeLink.Infrastructure.Repositories;

public class DonorRepository : IDonorRepository
{
    private readonly LifeLinkDbContext _context;

    public DonorRepository(LifeLinkDbContext context)
    {
        _context = context;
    }

    public async Task<Donor?> GetByIdAsync(
        int donorId,
        CancellationToken cancellationToken = default)
    {
        var donorProfile = await _context.DonorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                d => d.Id == donorId,
                cancellationToken);

        return donorProfile is null
            ? null
            : MapToDomain(donorProfile);
    }

    public async Task<IReadOnlyList<Donor>> GetAvailableDonorsAsync(
        int cityId,
        string bloodGroup,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bloodGroup))
        {
            throw new ArgumentException(
                "Blood group is required.",
                nameof(bloodGroup));
        }

        var compatibleBloodGroups =
            BloodGroupCompatibility.GetCompatibleDonorBloodGroups(
                bloodGroup);

        var donorProfiles = await _context.DonorProfiles
            .AsNoTracking()
            .Include(d => d.City)
            .Where(d =>
                d.CityId == cityId &&
                compatibleBloodGroups.Contains(d.BloodGroup) &&
                d.IsAvailable &&
                d.IsVerified)
            .ToListAsync(cancellationToken);

        return donorProfiles
            .Select(MapToDomain)
            .ToList();
    }

    public async Task AddAsync(
        Donor donor,
        CancellationToken cancellationToken = default)
    {
        var donorProfile = new Data.Entities.DonorProfile
        {
            UserId = donor.UserId,
            CityId = donor.CityId,
            BloodGroup = donor.BloodGroup,
            Latitude = donor.Latitude,
            Longitude = donor.Longitude,
            Location = CreateLocation(
                donor.Latitude,
                donor.Longitude),
            IsAvailable = donor.IsAvailable,
            IsVerified = donor.IsVerified,
            LastDonationDate = donor.LastDonationDate,
            CreatedAt = donor.CreatedAt
        };

        await _context.DonorProfiles.AddAsync(
            donorProfile,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        donor.Id = donorProfile.Id;
    }

    public async Task UpdateAsync(
        Donor donor,
        CancellationToken cancellationToken = default)
    {
        var donorProfile = await _context.DonorProfiles
            .FirstOrDefaultAsync(
                d => d.Id == donor.Id,
                cancellationToken);

        if (donorProfile is null)
        {
            throw new KeyNotFoundException(
                $"Donor with ID {donor.Id} was not found.");
        }

        donorProfile.CityId = donor.CityId;
        donorProfile.BloodGroup = donor.BloodGroup;
        donorProfile.Latitude = donor.Latitude;
        donorProfile.Longitude = donor.Longitude;

        donorProfile.Location = CreateLocation(
            donor.Latitude,
            donor.Longitude);

        donorProfile.IsAvailable = donor.IsAvailable;
        donorProfile.IsVerified = donor.IsVerified;
        donorProfile.LastDonationDate = donor.LastDonationDate;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static Donor MapToDomain(
        Data.Entities.DonorProfile profile)
    {
        return new Donor
        {
            Id = profile.Id,
            UserId = profile.UserId,
            CityId = profile.CityId,
            CityName = profile.City.Name,
            CityState = profile.City.State,
            BloodGroup = profile.BloodGroup,
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            IsAvailable = profile.IsAvailable,
            IsVerified = profile.IsVerified,
            LastDonationDate = profile.LastDonationDate,
            CreatedAt = profile.CreatedAt
        };
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(
                u => u.Email == email,
                cancellationToken);
    }

    public async Task<bool> CityExistsAsync(
        int cityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .AnyAsync(
                c => c.Id == cityId,
                cancellationToken);
    }

    public async Task RegisterAsync(
        Domain.Entities.User user,
        Donor donor,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var userEntity = new Data.Entities.User
            {
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            await _context.Users.AddAsync(
                userEntity,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var donorEntity = new Data.Entities.DonorProfile
            {
                UserId = userEntity.Id,
                CityId = donor.CityId,
                BloodGroup = donor.BloodGroup,
                Latitude = donor.Latitude,
                Longitude = donor.Longitude,
                Location = CreateLocation(
                    donor.Latitude,
                    donor.Longitude),
                IsAvailable = donor.IsAvailable,
                IsVerified = donor.IsVerified,
                LastDonationDate = donor.LastDonationDate,
                CreatedAt = donor.CreatedAt
            };

            await _context.DonorProfiles.AddAsync(
                donorEntity,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            user.Id = userEntity.Id;
            donor.Id = donorEntity.Id;
            donor.UserId = userEntity.Id;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException &&
                  (sqlException.Number == 2601 ||
                   sqlException.Number == 2627) &&
                  sqlException.Message.Contains(
                      "UQ_Users_Email",
                      StringComparison.OrdinalIgnoreCase))
        {
            await transaction.RollbackAsync(cancellationToken);

            throw new DuplicateEmailException(user.Email);
        }
    }

    public async Task<(IReadOnlyList<DonorMatchResult> Items, int TotalCount)>
        GetNearbyDonorsAsync(
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

        var compatibleBloodGroups =
            BloodGroupCompatibility.GetCompatibleDonorBloodGroups(
                bloodGroup);

        if (!double.IsFinite(hospitalLatitude) ||
            hospitalLatitude < -90 ||
            hospitalLatitude > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hospitalLatitude));
        }

        if (!double.IsFinite(hospitalLongitude) ||
            hospitalLongitude < -180 ||
            hospitalLongitude > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hospitalLongitude));
        }

        if (!double.IsFinite(radiusKm) || radiusKm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(radiusKm));
        }

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize));
        }

        // NetTopologySuite uses X = Longitude and Y = Latitude.
        var hospitalLocation = new Point(
            hospitalLongitude,
            hospitalLatitude)
        {
            SRID = 4326
        };

        // SQL Server geography STDistance returns metres.
        var radiusMeters = radiusKm * 1000;

        var query = _context.DonorProfiles
            .AsNoTracking()
            .Where(d =>
                compatibleBloodGroups.Contains(d.BloodGroup) &&
                d.IsAvailable &&
                d.IsVerified &&
                d.Location != null &&
                d.Location.Distance(hospitalLocation) <= radiusMeters);

        var totalCount = await query.CountAsync(cancellationToken);

        var matchingDonors = await query
            .OrderBy(d => d.Location!.Distance(hospitalLocation))
            .ThenBy(d => d.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DonorMatchResult
            {
                DonorId = d.Id,
                UserId = d.UserId,
                CityId = d.CityId,
                BloodGroup = d.BloodGroup,
                IsAvailable = d.IsAvailable,
                IsVerified = d.IsVerified,
                DistanceKm =
                    d.Location!.Distance(hospitalLocation) / 1000.0
            })
            .ToListAsync(cancellationToken);

        return (matchingDonors, totalCount);
    }

    private static Point? CreateLocation(
        decimal? latitude,
        decimal? longitude)
    {
        if (latitude is null && longitude is null)
        {
            return null;
        }

        if (latitude is null || longitude is null)
        {
            throw new ArgumentException(
                "Both latitude and longitude must be provided.");
        }

        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }

        return new Point(
            (double)longitude.Value,
            (double)latitude.Value)
        {
            SRID = 4326
        };
    }

    public async Task<Donor?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var donorProfile = await _context.DonorProfiles
            .AsNoTracking()
            .Include(d => d.City)
            .FirstOrDefaultAsync(
                d => d.UserId == userId,
                cancellationToken);

        return donorProfile is null
            ? null
            : MapToDomain(donorProfile);
    }

    public async Task UpdateAvailabilityAsync(
        int userId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        var donorProfile = await _context.DonorProfiles
            .FirstOrDefaultAsync(
                d => d.UserId == userId,
                cancellationToken);

        if (donorProfile is null)
        {
            throw new KeyNotFoundException(
                $"Donor profile for User ID {userId} was not found.");
        }

        donorProfile.IsAvailable = isAvailable;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
