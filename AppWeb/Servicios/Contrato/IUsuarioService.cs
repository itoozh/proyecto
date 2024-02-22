using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
namespace AppWeb.Servicios.Contrato
{
    public interface IUsuarioService
    {
        Task<Usuario> GetUsuarios(string correo, string contrasena);
        Task<Usuario> SaveUsuarios (Usuario modelo, int idRol = 2);
    }
}
