using LifeLink.Domain.Enums;

namespace LifeLink.Application.DTOs.RequestResponses;

public class DonorResponseResult
{
    public int ResponseId { get; set; }

    public int BloodRequestId { get; set; }

    public string BloodGroup { get; set; } = string.Empty;

    public string HospitalName { get; set; } = string.Empty;

    public string HospitalAddress { get; set; } = string.Empty;

    public int UnitsRequired { get; set; }

    public RequestUrgency Urgency { get; set; }

    public BloodRequestStatus RequestStatus { get; set; }

    public RequestResponseStatus ResponseStatus { get; set; }

    public DateTime RespondedAt { get; set; }
}