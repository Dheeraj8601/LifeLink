
namespace LifeLink.Application.Models.Donors;

public class EligibleDonorNotification
{
    public int DonorId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string BloodGroup { get; set; } = string.Empty;

    public double DistanceKm { get; set; }
}
