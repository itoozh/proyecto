using System;
using System.Collections.Generic;

namespace AppWeb.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public string? Cedula { get; set; }

    public int? IdRol { get; set; }

	public string Celular { get; set; } = null!;

    public virtual ICollection<Event> Events { get; } = new List<Event>();

    public virtual Rol? IdRolNavigation { get; set; }

    public virtual ICollection<Payment> Payments { get; } = new List<Payment>();
}
