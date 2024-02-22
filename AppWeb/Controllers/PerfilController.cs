// PerfilController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
using System.Threading.Tasks;
using AppWeb.Recursos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;

[Authorize]
public class PerfilController : Controller
{
    private readonly ProyectoContext _context;

    public PerfilController(ProyectoContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
        return View();
    }
    

    [HttpGet] // Cambiado de [Route("Perfil/MostrarPerfil")]
    public async Task<IActionResult> MostrarPerfil()
    {
        ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
        var nombreUsuarioActual = User.Identity.Name;
        var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuarioActual);
        return View(usuarioActual);
    }
    [HttpPost]
    public async Task<IActionResult> VerificarContrasena(string contrasenaIngresada)
    {
        ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
        var nombreUsuarioActual = User.Identity.Name;

        // Llamada al procedimiento almacenado para verificar la contraseña
        var contrasenaCorrecta = false;

        var parametros = new[]
        {
        new SqlParameter("@nombreUsuario", nombreUsuarioActual),
        new SqlParameter("@contrasena", Utilidades.EncriptarClave(contrasenaIngresada)),
        new SqlParameter("@contrasenaCorrecta", SqlDbType.Bit) { Direction = ParameterDirection.Output }
    };

        _context.Database.ExecuteSqlRaw("EXEC VerificarContrasenaUsuario @nombreUsuario, @contrasena, @contrasenaCorrecta OUTPUT", parametros);

        contrasenaCorrecta = (bool)parametros[2].Value;

        if (!contrasenaCorrecta)
        {
            // La contraseña no es correcta, puedes manejar esto de acuerdo a tus necesidades
            ViewBag.ContrasenaVerificada = false;
            TempData["ErrorContraseñaIncorrecta"] = "La clave no coincide.";
            return RedirectToAction(nameof(EditarPerfil));
        }


        // La contraseña es correcta, redirige al método MostrarPerfil y establece la variable ViewBag
        ViewBag.ContrasenaVerificada = true;

        // Recuperar el usuario y mostrar el perfil
        var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuarioActual);
        return View("EditarPerfil", usuarioActual);
    }



    [HttpGet] // Cambiado de [Route("Perfil/EditarPerfil")]
    public async Task<IActionResult> EditarPerfil()
    {
        ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
        var nombreUsuarioActual = User.Identity.Name;
        var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuarioActual);
        return View(usuarioActual);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPerfil([Bind("Id,NombreUsuario,Correo,Contrasena,IdRol,Cedula,Celular")] Usuario usuario)
    {
        ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
        var nombreUsuarioActual = User.Identity.Name;
        var usuarioActual = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuarioActual);

        // Actualiza solo los campos necesarios
        usuarioActual.NombreUsuario = usuario.NombreUsuario;
        usuarioActual.Correo = usuario.Correo;
        usuarioActual.Celular = usuario.Celular;

        // Verifica si la contraseña ha cambiado
        if (!string.IsNullOrEmpty(usuario.Contrasena) && usuarioActual.Contrasena != usuario.Contrasena)
        {
            // La contraseña ha cambiado, actualiza la contraseña encriptándola
            usuarioActual.Contrasena = Utilidades.EncriptarClave(usuario.Contrasena);
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(usuarioActual);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Maneja las excepciones según sea necesario
            }

            // Actualiza la identidad del usuario con la nueva información
            await ActualizarIdentidadUsuario(usuarioActual);
            return RedirectToAction(nameof(MostrarPerfil));
        }

        return View(usuarioActual);
    }

    private async Task ActualizarIdentidadUsuario(Usuario usuario)
    {
        // Obtiene las reclamaciones actuales del usuario
        var existingClaims = HttpContext.User.Claims.ToList();

        // Actualiza la identidad del usuario con la nueva información
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.NombreUsuario)
        // Puedes agregar más reclamaciones según sea necesario
    };

        claims.AddRange(existingClaims); // Agrega las reclamaciones existentes

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // Actualiza la cookie de autenticación con la nueva identidad
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true }
        );
    }

}
