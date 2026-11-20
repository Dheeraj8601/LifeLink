using System.ComponentModel.DataAnnotations;

namespace LifeLink.Api.DTOs.RequestResponses;

public class UpdateResponseStatusRequest
{
    [Required]
    [RegularExpression(
        "^(Accepted|Rejected)$",
        ErrorMessage = "Status must be Accepted or Rejected.")]
    public string Status { get; set; } = string.Empty;
}