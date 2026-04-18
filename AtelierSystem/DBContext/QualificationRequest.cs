using System;
using System.Collections.Generic;

namespace AtelierSystem.DBContext;

public partial class QualificationRequest
{
    public int Id { get; set; }

    public int MasterId { get; set; }

    public string RequestedLevel { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual Master Master { get; set; } = null!;
}
