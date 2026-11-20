namespace LifeLink.Api.DTOs.Donors;

public class NearbyDonorResponse
{
    public int DonorId { get; set; }

    public int CityId { get; set; }

    public string BloodGroup { get; set; } = string.Empty;

    public double DistanceKm { get; set; }
}