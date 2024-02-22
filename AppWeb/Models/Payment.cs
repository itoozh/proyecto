using System;
using System.Collections.Generic;

namespace AppWeb.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public decimal Amount { get; set; }

    public int UserId { get; set; }

    public virtual Usuario User { get; set; } = null!;
}
