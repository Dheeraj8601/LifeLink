using System.ComponentModel.DataAnnotations;

namespace LifeLink.Api.DTOs.Requesters;

public class RegisterRequesterRequest
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
}