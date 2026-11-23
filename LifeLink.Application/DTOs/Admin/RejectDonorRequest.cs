using System.ComponentModel.DataAnnotations;

namespace LifeLink.Application.DTOs.Admin;

public class RejectDonorRequest
{
    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string RejectionReason { get; set; } = string.Empty;
}