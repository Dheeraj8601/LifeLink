using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace LifeLink.Infrastructure.Data.Entities;

public partial class DonorProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CityId { get; set; }

    public string BloodGroup { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsVerified { get; set; }

    public DateOnly? LastDonationDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public Geometry? Location { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<RequestResponse> RequestResponses { get; set; } = new List<RequestResponse>();

    public virtual User User { get; set; } = null!;
}
