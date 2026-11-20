
using LifeLink.Domain.Enums;

namespace LifeLink.Domain.Entities;

public class BloodRequest
{
    public int Id { get; set; }

    public int RequesterId { get; set; }

    public int CityId { get; set; }

    public string BloodGroup { get; set; } = string.Empty;

    public string HospitalName { get; set; } = string.Empty;
    public string HospitalAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int UnitsRequired { get; set; }

    public RequestUrgency Urgency { get; set; } = RequestUrgency.Normal;

    public BloodRequestStatus Status { get; set; } = BloodRequestStatus.Open;

    public DateTime CreatedAt { get; set; }

    public void UpdateStatus(BloodRequestStatus newStatus)
    {
        bool isValidTransition = (Status, newStatus) switch
        {
            (BloodRequestStatus.Open, BloodRequestStatus.PartiallyFulfilled) => true,
            (BloodRequestStatus.Open, BloodRequestStatus.Fulfilled) => true,
            (BloodRequestStatus.Open, BloodRequestStatus.Cancelled) => true,

            (BloodRequestStatus.PartiallyFulfilled, BloodRequestStatus.Fulfilled) => true,
            (BloodRequestStatus.PartiallyFulfilled, BloodRequestStatus.Cancelled) => true,

            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidOperationException(
                $"Cannot change request status from {Status} to {newStatus}.");
        }

        Status = newStatus;
    }
}
