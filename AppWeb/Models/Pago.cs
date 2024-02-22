using System;
using System.Collections.Generic;

namespace AppWeb.Models;

public partial class Pago
{
    public int PagoId { get; set; }

    public decimal? Monto { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Event> Events { get; } = new List<Event>();
}
