using System;
using System.Collections.Generic;

namespace LifeLink.Infrastructure.Data.Entities;

public partial class AiinteractionLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FeatureName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? DurationMs { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
