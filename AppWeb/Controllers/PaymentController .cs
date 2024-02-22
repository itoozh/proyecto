using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
using System.Threading.Tasks;

namespace AppWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ProyectoContext _context;

        public PaymentController(ProyectoContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View();
        }
        // En tu controlador PaymentController, agrega el siguiente método
        [HttpGet]
        public IActionResult GetEventId()
        {
            try
            {
                // Aquí debes implementar la lógica para obtener el ID del evento según tus necesidades
                // Puedes obtener el ID desde la base de datos, la sesión, u otras fuentes

                // Por ejemplo, si tu ID del evento está en la query string de la URL
                var eventId = HttpContext.Request.Query["eventId"].ToString();

                // Puedes devolver el ID del evento como JSON
                return Json(new { eventId });
            }
            catch (Exception ex)
            {
                // Manejo de errores, por ejemplo, devolver un error 500
                return StatusCode(500, $"Error al obtener el ID del evento: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] Event request)
        {
            try
            {
                int eventId = request.EventId;
                ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;

                // Obtener la reserva con información de pago incluida
                var reserva = await _context.Events
                    .Include(e => e.Estado)
                    .Include(e => e.Pago)  // Incluir la relación con el pago
                    .FirstOrDefaultAsync(e => e.EventId == eventId);

                Console.WriteLine($"ID del evento: {eventId}");

                if (reserva == null)
                {
                    return Json(new { success = false, message = "Evento no encontrado." });
                }

                // Acceder al monto del pago
                decimal amount = reserva.Pago?.Monto ?? 0;

                // Aquí puedes realizar las operaciones de procesamiento de pago con PayPal, tarjeta de crédito, etc.

                // Una vez que el pago se ha completado con éxito, actualiza el estado de la reserva a "Activo" (EstadoId = 1)
                reserva.EstadoId = 1;

                // Guardar cambios en la base de datos
                await _context.SaveChangesAsync();

                return Json(new { success = true, amount = amount });
            }
            catch (Exception ex)
            {
                // Manejar cualquier error y devolver una respuesta de error
                return Json(new { success = false, message = "Error en el procesamiento del pago.", error = ex.Message });
            }
        }




        public IActionResult PagoExitoso()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View();
        }
    }
}
