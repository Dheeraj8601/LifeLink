
using System.ComponentModel.DataAnnotations;

namespace LifeLink.Api.DTOs.BloodRequests;

public class CreateBloodRequestRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CityId { get; set; }

    [Required]
    [RegularExpression(
        "^(A\\+|A-|B\\+|B-|AB\\+|AB-|O\\+|O-)$",
        ErrorMessage = "Invalid blood group.")]
    public string BloodGroup { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string HospitalName { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string HospitalAddress { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    [Range(1, 100)]
    public int UnitsRequired { get; set; }

    [Required]
    [RegularExpression(
        "^(Normal|Urgent|Critical)$",
        ErrorMessage = "Urgency must be Normal, Urgent, or Critical.")]
    public string Urgency { get; set; } = "Normal";

    [StringLength(1000)]
    public string? Description { get; set; }
}
