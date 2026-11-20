using System;
using System.Collections.Generic;

namespace LifeLink.Infrastructure.Data.Entities;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AiinteractionLog> AiinteractionLogs { get; set; } = new List<AiinteractionLog>();

    public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    public virtual DonorProfile? DonorProfile { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
