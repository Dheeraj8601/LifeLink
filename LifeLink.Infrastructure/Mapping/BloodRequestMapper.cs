
using DomainBloodRequest = LifeLink.Domain.Entities.BloodRequest;
using EfBloodRequest = LifeLink.Infrastructure.Data.Entities.BloodRequest;
using LifeLink.Domain.Enums;

namespace LifeLink.Infrastructure.Mapping;

public static class BloodRequestMapper
{
    public static EfBloodRequest ToEntity(DomainBloodRequest domain)
    {
        return new EfBloodRequest
        {
            Id = domain.Id,
            RequesterId = domain.RequesterId,
            CityId = domain.CityId,
            BloodGroup = domain.BloodGroup,
            HospitalName = domain.HospitalName,
            HospitalAddress = domain.HospitalAddress,
            Latitude = domain.Latitude,
            Longitude = domain.Longitude,
            UnitsRequired = domain.UnitsRequired,
            Urgency = domain.Urgency.ToString(),
            Description = domain.Description,
            Status = domain.Status.ToString(),
            CreatedAt = domain.CreatedAt
        };
    }

    public static DomainBloodRequest ToDomain(EfBloodRequest entity)
    {
        return new DomainBloodRequest
        {
            Id = entity.Id,
            RequesterId = entity.RequesterId,
            CityId = entity.CityId,
            BloodGroup = entity.BloodGroup,
            HospitalName = entity.HospitalName,
            HospitalAddress = entity.HospitalAddress,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            UnitsRequired = entity.UnitsRequired,
            Urgency = ParseEnum<RequestUrgency>(entity.Urgency),
            Description = entity.Description,
            Status = ParseEnum<BloodRequestStatus>(entity.Status),
            CreatedAt = entity.CreatedAt
        };
    }

    private static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct, Enum
    {
        if (!Enum.TryParse<TEnum>(value, ignoreCase: false, out var result) ||
            !Enum.IsDefined(result))
        {
            throw new InvalidOperationException(
                $"Invalid {typeof(TEnum).Name} value '{value}' in database.");
        }

        return result;
    }
}
