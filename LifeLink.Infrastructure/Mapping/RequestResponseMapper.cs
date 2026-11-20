
using DomainRequestResponse = LifeLink.Domain.Entities.RequestResponse;
using EfRequestResponse = LifeLink.Infrastructure.Data.Entities.RequestResponse;
using LifeLink.Domain.Enums;

namespace LifeLink.Infrastructure.Mapping;

public static class RequestResponseMapper
{
    public static EfRequestResponse ToEntity(
        DomainRequestResponse domain)
    {
        return new EfRequestResponse
        {
            Id = domain.Id,
            BloodRequestId = domain.BloodRequestId,
            DonorId = domain.DonorId,
            Status = domain.Status.ToString(),
            RespondedAt = domain.RespondedAt
        };
    }

    public static DomainRequestResponse ToDomain(EfRequestResponse entity)
    {
        if (!Enum.TryParse<RequestResponseStatus>(
                entity.Status,
                ignoreCase: false,
                out var status) ||
            !Enum.IsDefined(status))
        {
            throw new InvalidOperationException(
                $"Invalid RequestResponseStatus value '{entity.Status}' in database.");
        }

        return DomainRequestResponse.Reconstitute(
            entity.Id,
            entity.BloodRequestId,
            entity.DonorId,
            status,
            entity.RespondedAt);
    }
}
