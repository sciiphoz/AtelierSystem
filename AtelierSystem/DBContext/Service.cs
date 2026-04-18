using System;
using System.Collections.Generic;

namespace AtelierSystem.DBContext;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? CollectionId { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ServiceCategory? Category { get; set; }

    public virtual Collection? Collection { get; set; }

    public virtual ICollection<MasterService> MasterServices { get; set; } = new List<MasterService>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
