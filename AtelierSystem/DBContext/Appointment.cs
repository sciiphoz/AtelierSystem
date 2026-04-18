using System;
using System.Collections.Generic;

namespace AtelierSystem.DBContext;

public partial class Appointment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MasterId { get; set; }

    public int ServiceId { get; set; }

    public DateTime AppointmentTime { get; set; }

    public int? QueueNumber { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Master Master { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
