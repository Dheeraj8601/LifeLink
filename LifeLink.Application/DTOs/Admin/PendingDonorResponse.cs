namespace LifeLink.Application.DTOs.Admin;

public class PendingDonorResponse
{
    public int DonorId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string BloodGroup { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsVerified { get; set; }

    public DateTime RegisteredAt { get; set; }
}