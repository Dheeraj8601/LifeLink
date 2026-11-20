using System;
using System.Collections.Generic;

namespace LifeLink.Infrastructure.Data.Entities;

public partial class City
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string State { get; set; } = null!;

    public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    public virtual ICollection<DonorProfile> DonorProfiles { get; set; } = new List<DonorProfile>();
}
