using System;
using System.Collections.Generic;

namespace AppWeb.Models;

public partial class Estado
{
    public int EstadoId { get; set; }

    public string? NombreEstado { get; set; }

    public virtual ICollection<Event> Events { get; } = new List<Event>();
}
