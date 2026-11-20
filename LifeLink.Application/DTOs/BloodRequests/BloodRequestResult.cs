
namespace LifeLink.Application.DTOs.BloodRequests;

public class BloodRequestResult
{
    public int Id { get; set; }

    public int RequesterId { get; set; }

    public int CityId { get; set; }

    public string BloodGroup { get; set; } = string.Empty;

    public string HospitalName { get; set; } = string.Empty;

    public string HospitalAddress { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public int UnitsRequired { get; set; }

    public string Urgency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }
}
