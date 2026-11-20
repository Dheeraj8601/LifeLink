using System;
using System.Collections.Generic;

namespace LifeLink.Infrastructure.Data.Entities;

public partial class RequestResponse
{
    public int Id { get; set; }

    public int BloodRequestId { get; set; }

    public int DonorId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RespondedAt { get; set; }

    public virtual BloodRequest BloodRequest { get; set; } = null!;

    public virtual DonorProfile Donor { get; set; } = null!;
}
