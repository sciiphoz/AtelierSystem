using System;
using System.Collections.Generic;

namespace AtelierSystem.DBContext;

public partial class Master
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? QualificationLevel { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<MasterService> MasterServices { get; set; } = new List<MasterService>();

    public virtual ICollection<QualificationRequest> QualificationRequests { get; set; } = new List<QualificationRequest>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User User { get; set; } = null!;
}
