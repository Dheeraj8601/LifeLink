namespace LifeLink.Application.Models.Donors;

public class DonorMatchResult
{
    public int DonorId { get; set; }

    public int UserId { get; set; }

    public int CityId { get; set; }

    public string BloodGroup { get; set; } = string.Empty;

    public double DistanceKm { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsVerified { get; set; }
}