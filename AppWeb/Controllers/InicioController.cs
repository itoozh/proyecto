using Microsoft.AspNetCore.Mvc;
using AppWeb.Models;
using AppWeb.Recursos;
using AppWeb.Servicios.Contrato;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Text.Json;

namespace AppWeb.Controllers
{
    public class InicioController : Controller
    {
        private readonly IUsuarioService _usuarioServicio;
        private readonly IConfiguration _configuration;

        public InicioController(IUsuarioService usuarioServicio, IConfiguration configuration)
        {
            _usuarioServicio = usuarioServicio;
            _configuration = configuration;
        }

        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(Usuario modelo)
        {

            // Encriptar la contraseña
            modelo.Contrasena = Utilidades.EncriptarClave(modelo.Contrasena);

            // Validar existencia de correo y cédula
            bool existeCorreo = false;
            bool existeCedula = false;


            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                using (SqlCommand cmd = new SqlCommand("ValidarRegistroUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@correo", modelo.Correo));
                    cmd.Parameters.Add(new SqlParameter("@cedula", modelo.Cedula));
                    cmd.Parameters.Add(new SqlParameter("@existeCorreo", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@existeCedula", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                    connection.Open();
                    cmd.ExecuteNonQuery();

                    existeCorreo = (bool)cmd.Parameters["@existeCorreo"].Value;
                    existeCedula = (bool)cmd.Parameters["@existeCedula"].Value;
                }
            }

            // Validar formato de correo
            if (!modelo.Correo.Contains("@"))
            {
                ViewData["Mensaje"] = "Formato de Correo no valido";
                return View(modelo);
            }

            // Validar Cédula ecuatoriana
            if (!ValidarCedulaEcuatoriana(modelo.Cedula))
            {
                ViewData["Mensaje"] = "La Cedula ingresada no es valida";
                return View(modelo);
            }

            // Validar formato de número de celular
            if (!ValidarNumeroCelular(modelo.Celular))
            {
                ViewData["Mensaje"] = "Numero de Celular no valido";
                return View(modelo);
            }


            // Validar requisitos de contraseña

            if (existeCorreo)
            {
                ViewData["Mensaje"] = "Este Correo ya existe";
            }

            if (existeCedula)
            {
                ViewData["Mensaje"] = "Esta Cedula ya esta registrada";
            }

            if (existeCorreo || existeCedula)
            {
                // Devolver la vista con los mensajes de error
                return View(modelo);
            }


            // Guardar el usuario si no existen problemas
            Usuario usuario_creado = await _usuarioServicio.SaveUsuarios(modelo);

            if (usuario_creado.Id > 0)
            {
                TempData["MensajeExitoso"] = "Usuario creado exitosamente";
                return RedirectToAction("IniciarSesion", "Inicio");
            }

            ModelState.AddModelError("RegistroError", "No se pudo crear el usuario");
            return View();


        }


        // Función para validar Cédula ecuatoriana
        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            const int longitudCedula = 10;

            // Verificar que la cédula tenga la longitud correcta
            if (cedula.Length != longitudCedula)
            {
                return false;
            }

            // Verificar que la cédula sea numérica
            if (!long.TryParse(cedula, out _))
            {
                return false;
            }

            // Obtener el dígito verificador
            int digitoVerificador = int.Parse(cedula.Substring(9, 1));

            // Calcular el dígito verificador esperado
            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString());
                suma += (i % 2 == 0) ? ((digito * 2) > 9 ? (digito * 2) - 9 : digito * 2) : digito;
            }

            int digitoVerificadorCalculado = (10 - (suma % 10)) % 10;

            // Comparar el dígito verificador calculado con el proporcionado
            return digitoVerificador == digitoVerificadorCalculado;
        }


        // Función para validar requisitos de contraseña
        private bool ValidarNumeroCelular(string celular)
        {
            const int longitudCelular = 10;

            // Verificar que el número de celular tenga la longitud correcta
            if (celular.Length != longitudCelular)
            {
                return false;
            }

            // Verificar que el número de celular sea numérico
            if (!long.TryParse(celular, out _))
            {
                return false;
            }

            // Otros criterios de validación específicos del formato de número de celular de Ecuador
            // Puedes agregar aquí tus propias reglas de validación si es necesario

            return true;
        }
        public IActionResult Reservar()
        {
            // Verifica si el usuario ha iniciado sesión
            if (!User.Identity.IsAuthenticated)
            {
                // Si no ha iniciado sesión, redirige a la vista de registro
                return RedirectToAction("Registrarse", "Inicio");
            }

            // Si ha iniciado sesión, redirige a la vista de vehículos
            return RedirectToAction("Index", "Vehiculoes");
        }

        public IActionResult IniciarSesion()
        {
            if (TempData.ContainsKey("MensajeExitoso"))
            {
                ViewData["MensajeExitoso"] = TempData["MensajeExitoso"];
                TempData.Remove("MensajeExitoso"); // Limpiar TempData después de usarlo
            }

            if (ViewData.ContainsKey("Mensaje"))
            {
                // Puedes agregar aquí cualquier lógica adicional necesaria para mensajes de error
                ViewData["Mensaje"] = TempData["Mensaje"];
                TempData.Remove("Mensaje"); // Limpiar TempData después de usarlo
            }
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> IniciarSesion(string correo, string contrasena)
        {
            Usuario usuario_encontrado = await _usuarioServicio.GetUsuarios(correo, Utilidades.EncriptarClave(contrasena));
            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encontro el usuario";
                return View();
            }


            List<Claim> claims = new List<Claim>(){
                    new Claim(ClaimTypes.Name, usuario_encontrado.NombreUsuario),
                    new Claim("UserIdClaim", usuario_encontrado.Id.ToString()),
                    new Claim("IdRolClaim", usuario_encontrado.IdRol.ToString())
                };


            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties() { AllowRefresh = true };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );



            if (usuario_encontrado.IdRol == 1)
            {
                return RedirectToAction("IndexMaster", "Home");
            }
            else if (usuario_encontrado.IdRol == 2)
            {

                return RedirectToAction("Index", "Home");
            }
            else if (usuario_encontrado.IdRol == 3)
            {
                return RedirectToAction("IndexGerencia", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }



        }

    }
}
