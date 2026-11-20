namespace LifeLink.Domain.Entities;

public class Donor
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CityId { get; set; }

    public string BloodGroup { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsVerified { get; set; }

    public DateOnly? LastDonationDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CityName { get; set; } = string.Empty;

    public string CityState { get; set; } = string.Empty;
}