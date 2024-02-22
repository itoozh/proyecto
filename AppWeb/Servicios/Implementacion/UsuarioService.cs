using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
using AppWeb.Servicios.Contrato;
using Microsoft.AspNetCore.Rewrite;

namespace AppWeb.Servicios.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ProyectoContext _dbContext;

        public UsuarioService(ProyectoContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Usuario> GetUsuarios(string correo, string contrasena)
        {
            Usuario usuario_encontrado = await _dbContext.Usuarios
                .Where(u => u.Correo == correo && u.Contrasena == contrasena)
                .FirstOrDefaultAsync();

            return usuario_encontrado;
        }

        public async Task<Usuario> SaveUsuarios(Usuario modelo, int idRol = 2)
        {
            modelo.IdRol = idRol;
            _dbContext.Add(modelo);
            await _dbContext.SaveChangesAsync();
            return modelo;
        }
    }
}

