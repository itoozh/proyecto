using AppWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AppWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize(Policy = "AdminPolicy")]
        public IActionResult IndexMaster()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            string nombreUsuario = "";

            if (claimUser.Identity.IsAuthenticated)
            {
                // Intenta obtener el nombre de usuario de las reclamaciones
                var claim = claimUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

                if (claim != null)
                {
                    nombreUsuario = claim.Value;
                }
                else
                {
                    // Si no se encuentra la reclamación del nombre de usuario, maneja la situación de acuerdo a tus necesidades
                    // Puede ser útil registrar o notificar sobre este problema
                    // También podrías redirigir al usuario a una página de error o a la página de inicio de sesión
                    // dependiendo de tus requisitos
                }
            }

            ViewData["nombreUsuario"] = nombreUsuario;

            // Lógica específica para la acción Index
            return View("Views/Master/IndexMaster.cshtml");
        }


        [Authorize(Policy = "CombinePolicy")]
        public IActionResult IndexGerencia()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            string nombreUsuario = "";

            if (claimUser.Identity.IsAuthenticated)
            {
                // Intenta obtener el nombre de usuario de las reclamaciones
                var claim = claimUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

                if (claim != null)
                {
                    nombreUsuario = claim.Value;
                }
                else
                {
                    // Si no se encuentra la reclamación del nombre de usuario, maneja la situación de acuerdo a tus necesidades
                    // Puede ser útil registrar o notificar sobre este problema
                    // También podrías redirigir al usuario a una página de error o a la página de inicio de sesión
                    // dependiendo de tus requisitos
                }
            }

            ViewData["nombreUsuario"] = nombreUsuario;

            // Lógica específica para la acción Index
            return View("Views/Master/IndexGerencia.cshtml");
        }


        public IActionResult Index()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            string nombreUsuario = "";

            if (claimUser.Identity.IsAuthenticated)
            {
                // Intenta obtener el nombre de usuario de las reclamaciones
                var claim = claimUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

                if (claim != null)
                {
                    nombreUsuario = claim.Value;
                }
                else
                {
                    // Si no se encuentra la reclamación del nombre de usuario, maneja la situación de acuerdo a tus necesidades
                    // Puede ser útil registrar o notificar sobre este problema
                    // También podrías redirigir al usuario a una página de error o a la página de inicio de sesión
                    // dependiendo de tus requisitos
                }
            }

            ViewData["nombreUsuario"] = nombreUsuario;

            // Lógica específica para la acción Index
            return View();
        }



        public IActionResult Privacy()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index","Home");
        }
    }
}