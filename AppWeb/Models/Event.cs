using System;
using System.Collections.Generic;

namespace AppWeb.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string? Title { get; set; }

    public DateTime? Start { get; set; }

    public DateTime? End { get; set; }

    public int? UserId { get; set; }

    public int? PagoId { get; set; }

    public int? VehiculoId { get; set; }

    public int? EstadoId { get; set; }

    public virtual Estado? Estado { get; set; }

    public virtual Pago? Pago { get; set; }

    public virtual Usuario? User { get; set; }

    public virtual Vehiculo? Vehiculo { get; set; }
}
