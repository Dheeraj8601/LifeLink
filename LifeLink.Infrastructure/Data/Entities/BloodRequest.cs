using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace LifeLink.Infrastructure.Data.Entities;

public partial class BloodRequest
{
    public int Id { get; set; }

    public int RequesterId { get; set; }

    public int CityId { get; set; }

    public string BloodGroup { get; set; } = null!;

    public string HospitalName { get; set; } = null!;

    public string HospitalAddress { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int UnitsRequired { get; set; }

    public string Urgency { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Geometry? Location { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<RequestResponse> RequestResponses { get; set; } = new List<RequestResponse>();

    public virtual User Requester { get; set; } = null!;
}
