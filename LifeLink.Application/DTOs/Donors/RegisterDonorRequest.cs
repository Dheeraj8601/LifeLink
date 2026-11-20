using System.ComponentModel.DataAnnotations;

namespace LifeLink.Api.DTOs.Donors;

public class RegisterDonorRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CityId { get; set; }

    [Required]
    [RegularExpression(
        "^(A\\+|A-|B\\+|B-|AB\\+|AB-|O\\+|O-)$")]
    public string BloodGroup { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}