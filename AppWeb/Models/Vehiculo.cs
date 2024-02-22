using System;
using System.Collections.Generic;

namespace AppWeb.Models;

public partial class Vehiculo
{
    public int IdAuto { get; set; }

    public string? Nombre { get; set; }

    public string? Marca { get; set; }

    public int? Año { get; set; }

    public string? Color { get; set; }

    public int? ImagenId { get; set; }

    public virtual ICollection<Event> Events { get; } = new List<Event>();

    public virtual Imagen? Imagen { get; set; }

    public virtual ICollection<Imagen> Imagens { get; } = new List<Imagen>();
}