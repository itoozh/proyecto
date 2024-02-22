using System;
using System.Collections.Generic;

namespace AppWeb.Models;

public partial class Imagen
{
    public int IdImagen { get; set; }

    public byte[]? ImagenData { get; set; }

    public string? ImagenMimeType { get; set; }

    public int? VehiculoId { get; set; }

    public virtual Vehiculo? Vehiculo { get; set; }

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();

}