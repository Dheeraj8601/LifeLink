using LifeLink.Domain.Enums;

namespace LifeLink.Domain.Entities;

public class RequestResponse
{
    public int Id { get; set; }

    public int BloodRequestId { get; set; }

    public int DonorId { get; set; }

    public RequestResponseStatus Status { get; private set; }
        = RequestResponseStatus.Pending;

    public DateTime RespondedAt { get; private set; }

    public RequestResponse()
    {
        RespondedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(RequestResponseStatus newStatus)
    {
        bool isValidTransition = Status switch
        {
            RequestResponseStatus.Pending =>
                newStatus is RequestResponseStatus.Accepted
                    or RequestResponseStatus.Rejected
                    or RequestResponseStatus.Withdrawn,

            RequestResponseStatus.Accepted =>
                newStatus == RequestResponseStatus.Withdrawn,

            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException(
                $"Cannot change response status from {Status} to {newStatus}.");
        }

        Status = newStatus;
        RespondedAt = DateTime.UtcNow;
    }

    public static RequestResponse Reconstitute(
        int id,
        int bloodRequestId,
        int donorId,
        RequestResponseStatus status,
        DateTime respondedAt)
    {

        if (id <= 0)
            throw new ArgumentException("Invalid response ID.");

        if (bloodRequestId <= 0)
            throw new ArgumentException("Invalid blood request ID.");

        if (donorId <= 0)
            throw new ArgumentException("Invalid donor ID.");

        if (!Enum.IsDefined(status))
            throw new ArgumentException("Invalid response status.");

        return new RequestResponse
        {
            Id = id,
            BloodRequestId = bloodRequestId,
            DonorId = donorId,
            Status = status,
            RespondedAt = respondedAt
        };
    }
}