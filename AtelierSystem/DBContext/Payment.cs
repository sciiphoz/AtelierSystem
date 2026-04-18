using System;
using System.Collections.Generic;

namespace AtelierSystem.DBContext;

public partial class Payment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? TransactionDate { get; set; }

    public virtual User User { get; set; } = null!;
}
