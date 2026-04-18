using System;
using System.Collections.Generic;

namespace AtelierSystem.DBContext;

public partial class MasterService
{
    public int MasterId { get; set; }

    public int ServiceId { get; set; }

    public int Id { get; set; }

    public virtual Master Master { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
