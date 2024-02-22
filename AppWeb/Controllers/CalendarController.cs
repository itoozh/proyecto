using AppWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AppWeb.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ProyectoContext _context;

        public CalendarController(ProyectoContext context)
        {
            _context = context;
        }
        private decimal CalculatePrice(DateTime start, DateTime end)
        {
            decimal startDecimal = (decimal)(start.TimeOfDay.TotalHours);
            decimal endDecimal = (decimal)(end.TimeOfDay.TotalHours);

            decimal duration = endDecimal - startDecimal;

            int pagoId = (duration > 2) ? 3 : (duration > 1) ? 2 : 1;

            // Supongamos que aquí tienes alguna lógica adicional para calcular el precio basado en pagoId
            decimal precioCalculado = 0; // Asigna el valor inicial que corresponda a tu lógica de cálculo

            // Lógica adicional para calcular el precio basado en el pagoId si es necesario

            // Retornar el precio calculado
            return precioCalculado;
        }


        [HttpPost]
        public IActionResult RegisterEvent([FromBody] Event eventViewModel, int IdAuto)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            Vehiculo vehiculo = null;

            try
            {
                // Consultar la entidad Vehiculo usando el idAuto
                vehiculo = _context.Vehiculos.FirstOrDefault(v => v.IdAuto == IdAuto);

                if (vehiculo == null)
                {
                    Console.WriteLine($"No se encontró ningún vehículo con IdAuto: {IdAuto}");
                    return Json(new { success = false, message = "No se encontró ningún vehículo con el IdAuto proporcionado." });
                }

                // Ahora puedes utilizar la entidad Vehiculo según sea necesario
                Console.WriteLine($"IdAuto recibido: {IdAuto} ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar el IdAuto: {IdAuto}, Error: {ex.Message}");
                return Json(new { success = false, message = "Error al procesar el IdAuto.", error = ex.Message });
            }

            if (ModelState.IsValid)
            {
                // Verificar si la fecha y hora seleccionadas son posteriores al momento actual
                if (eventViewModel.Start < DateTime.Now)
                {
                    // Devolver un mensaje de error indicando que la selección no es permitida
                    return Json(new { success = false, message = "No puedes seleccionar fechas y horas pasadas." });
                }

                // Contar las reservas pendientes del usuario
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserIdClaim");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    Console.WriteLine("Error al obtener el UserId");
                    return Json(new { success = false, message = "Error al obtener el UserId del usuario en sesión." });
                }

                int reservasPendientes = _context.Events.Count(e => e.UserId == userId && e.EstadoId == 2);

                // Aplicar la lógica de restricción
                if (reservasPendientes >= 3)
                {
                    return Json(new { success = false, message = "Ya tienes 3 reservas pendientes. No puedes hacer más reservas en este momento." });
                }


                // Comprobar si hay algún evento existente que se superponga parcialmente con el nuevo evento
                bool eventoExiste = _context.Events.Any(e =>
                    e.VehiculoId == vehiculo.IdAuto &&
                    ((e.Start <= eventViewModel.Start && e.End > eventViewModel.Start) ||   // Comprueba si el nuevo evento comienza durante un evento existente
                     (e.Start < eventViewModel.End && e.End >= eventViewModel.End) ||       // Comprueba si el nuevo evento termina durante un evento existente
                     (e.Start >= eventViewModel.Start && e.End <= eventViewModel.End)));   // Comprueba si el nuevo evento abarca completamente un evento existente

                if (eventoExiste)
                {
                    return Json(new { success = false, message = "El evento ya existe." });
                }

                decimal startDecimal = (decimal)(eventViewModel.Start?.TimeOfDay.TotalHours ?? 0.0);
                decimal endDecimal = (decimal)(eventViewModel.End?.TimeOfDay.TotalHours ?? 0.0);

                decimal duration = endDecimal - startDecimal;

                int pagoId = (duration > 2) ? 3 : (duration > 1) ? 2 : 1;

                // Establecer el EstadoId por defecto como "Activo" (ajusta según tu modelo)
                int estadoPorDefecto = 2;

                Event newEvent = new Event
                {
                    Title = eventViewModel.Title,
                    Start = eventViewModel.Start,
                    End = eventViewModel.End,
                    UserId = userId,
                    PagoId = pagoId,
                    VehiculoId = vehiculo.IdAuto,
                    EstadoId = estadoPorDefecto  // Establecer el EstadoId por defecto
                };

                _context.Events.Add(newEvent);

                try
                {
                    _context.SaveChanges();
                    int newEventId = newEvent.EventId;
                    return Json(new
                    {
                        success = true,
                        message = new
                        {
                            title = "Registro exitoso",
                            text = "Presione el botón para continuar con el pago.",
                            eventId = newEventId
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error al registrar el evento", error = ex.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = "Modelo no válido" });
            }
        }

        public IActionResult Pay(int eventId)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View("~/Views/Payment/ConfirmacionReserva.cshtml");
        }

        [HttpGet]
        public IActionResult GetAllEvents(int idAuto)
        {
            var userIdClaim = User.FindFirst("UserIdClaim")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json("El usuario no tiene un UserIdClaim válido.");
            }

            var events = _context.Events
                .Include(e => e.User)
                .Where(e => e.VehiculoId == idAuto && e.UserId != userId)
                .Select(e => new
                 {
                     e.EventId,
                     e.Title,
                     e.Start,
                     e.End,
                     e.UserId,
                     UserName = e.User.NombreUsuario,
                 })
                .ToList();

            return Json(events);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserEventsForAuto(int idAuto)
        {
            try
            {
                Console.WriteLine($"IdAuto recibido: {idAuto}");
                var userIdClaim = User.FindFirst("UserIdClaim")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Json("El usuario no tiene un UserIdClaim válido.");
                }

                // Consulta la base de datos para obtener eventos del usuario con el userId y el idAuto
                var events = await _context.Events
                    .Where(e => e.UserId == userId && e.VehiculoId == idAuto)
                    .ToListAsync();

                return Json(events);
            }
            catch (Exception ex)
            {
                // Manejar errores según tus requisitos
                Console.WriteLine($"Error al obtener eventos: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpPost]
        public IActionResult CancelarReserva(int eventId)
        {
            try
            {
                var reserva = _context.Events.FirstOrDefault(e => e.EventId == eventId);
                if (reserva == null)
                {
                    return Json(new { success = false, message = "La reserva no existe." });
                }

                _context.Events.Remove(reserva);
                _context.SaveChanges();

                return Json(new { success = true, message = "La reserva se ha cancelado correctamente." });
            }
            catch (DbUpdateException ex)
            {
                // Manejar excepciones específicas de la base de datos, como violaciones de clave externa, etc.
                return Json(new { success = false, message = "Error al cancelar la reserva.", error = ex.InnerException?.Message });
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones generales
                return Json(new { success = false, message = "Error al cancelar la reserva.", error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetEventPrice(DateTime start, DateTime end)
        {
            try
            {
                // Calcula el precio usando el método CalculatePrice que ya definiste
                decimal price = CalculatePrice(start, end);

                // Devuelve el precio como respuesta en formato JSON
                return Json(new { price });
            }
            catch (Exception ex)
            {
                // Maneja cualquier error que pueda ocurrir durante el cálculo del precio
                Console.WriteLine($"Error al calcular el precio del evento: {ex.Message}");
                return Json(new { error = "Error al calcular el precio del evento." });
            }
        }

        /*

           MIS RESERVAS

        */
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserIdClaim")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Json("El usuario no tiene un UserIdClaim válido.");
                }

                // Consulta la base de datos para obtener eventos del usuario con el userId
                var events = await _context.Events.Where(e => e.UserId == userId).ToListAsync();
                return Json(events);
            }
            catch (Exception ex)
            {
                // Manejar errores según tus requisitos
                Console.WriteLine($"Error al obtener eventos: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
		[HttpGet]
		public IActionResult GetImagenForAuto(int idAuto)
		{
			try
			{
				// Buscar el vehículo en la base de datos que corresponde al idAuto
				var vehiculo = _context.Vehiculos.Include(v => v.Imagen).FirstOrDefault(v => v.IdAuto == idAuto);

				if (vehiculo != null && vehiculo.Imagen != null)
				{
					// Convertir los datos de la imagen a una URL
					var imageDataUrl = $"data:{vehiculo.Imagen.ImagenMimeType};base64,{Convert.ToBase64String(vehiculo.Imagen.ImagenData)}";
					return Json(new { imageUrl = imageDataUrl });
				}
				else
				{
					// Si no se encuentra ninguna imagen para el vehículo, devuelve una URL de imagen predeterminada o vacía
					return Json(new { imageUrl = "" }); // O puedes proporcionar una URL predeterminada si deseas
				}
			}
			catch (Exception ex)
			{
				// Maneja cualquier excepción que pueda ocurrir durante la obtención de la imagen
				Console.WriteLine($"Error al obtener la imagen del vehículo: {ex.Message}");
				return StatusCode(500, "Error interno del servidor");
			}
		}
    
	}
}