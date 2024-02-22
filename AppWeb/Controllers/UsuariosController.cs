using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
using AppWeb.Recursos;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Kernel.Exceptions;
using iText.Kernel.Colors;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using iText.IO.Image;


namespace AppWeb.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class UsuariosController : Controller
    {
        private readonly ProyectoContext _context;
        private readonly ServicioUtilidades _servicioUtilidades;

        public UsuariosController(ProyectoContext context, ServicioUtilidades servicioUtilidades)
        {
            _context = context;
            _servicioUtilidades = servicioUtilidades;
        }
        public async Task<IActionResult> Index()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;

            // Obtener el mensaje exitoso de TempData
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
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var proyectoContext = _context.Usuarios.Include(u => u.IdRolNavigation);
            return View(await proyectoContext.ToListAsync());
        }


        [HttpGet]
        [Route("Usuarios/GeneratePdf")]
        public IActionResult GeneratePdf()
        {
            try
            {
                var usuarios = _context.Usuarios.Include(u => u.IdRolNavigation);

                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(memoryStream))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf);

                            // Encabezado con imagen y texto
                            var header = new Paragraph();
                            header.SetTextAlignment(TextAlignment.CENTER);

                            // Agregar la imagen desde la URL
                            header.Add(new Image(ImageDataFactory.Create("https://static.vecteezy.com/system/resources/previews/015/605/709/original/palm-tree-icon-outline-style-vector.jpg")).SetWidth(100));

                            // Agregar el texto "PALMS CARS"
                            header.Add(new Paragraph("PALMS CARS").SetFontColor(DeviceRgb.BLACK).SetFontSize(55).SetTextAlignment(TextAlignment.LEFT));

                            document.Add(header);

                            document.Add(new Paragraph("REPORTE DE USUARIOS").SetFontColor(DeviceRgb.BLACK).SetFontSize(24).SetTextAlignment(TextAlignment.CENTER)
                                .SetBackgroundColor(DeviceRgb.WHITE).SetMarginBottom(20));

                            // Establecer márgenes
                            document.SetMargins(30, 30, 30, 30);

                            var table = new Table(new float[] { 2, 2, 2, 2 }).UseAllAvailableWidth(); // 4 columnas con ancho relativo
                            table.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                            // Encabezados de la tabla
                            table.AddCell(new Cell().Add(new Paragraph("NOMBRE").SetFontColor(DeviceRgb.BLACK).SetFontSize(14).SetBackgroundColor(new DeviceRgb(200, 200, 200))));
                            table.AddCell(new Cell().Add(new Paragraph("CORREO").SetFontColor(DeviceRgb.BLACK).SetFontSize(14).SetBackgroundColor(new DeviceRgb(200, 200, 200))));
                            table.AddCell(new Cell().Add(new Paragraph("CÉDULA").SetFontColor(DeviceRgb.BLACK).SetFontSize(14).SetBackgroundColor(new DeviceRgb(200, 200, 200))));
                            table.AddCell(new Cell().Add(new Paragraph("CELULAR").SetFontColor(DeviceRgb.BLACK).SetFontSize(14).SetBackgroundColor(new DeviceRgb(200, 200, 200))));

                            // Agregar contenido dinámico al PDF
                            foreach (var usuario in usuarios)
                            {
                                table.AddCell(usuario.NombreUsuario);
                                table.AddCell(usuario.Correo);
                                table.AddCell(usuario.Cedula);
                                table.AddCell(usuario.Celular); // Asumiendo que la propiedad del celular es "Celular"
                            }

                            document.Add(table); // Cerrar el documento


                            document.Close();

                            // Establecer el nombre del archivo y el tipo de contenido
                            var fileName = "ReporteUsuarios.pdf";
                            var contentType = "application/pdf";

                            // Devolver el archivo como resultado
                            return File(memoryStream.ToArray(), contentType, fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el PDF: {ex.Message}");
                return BadRequest("Error al generar el PDF");
            }
        }







        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        public IActionResult Create()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreUsuario,Correo,Contrasena,IdRol,Cedula,Celular")] Usuario usuario)
        {
            // Validar existencia de correo y cédula antes de crear
            if (_servicioUtilidades.ValidarRegistroUsuario(usuario.Correo, usuario.Cedula))
            {
                if (_servicioUtilidades.ExisteCorreo(usuario.Correo))
                {
                    ViewData["Mensaje"] = "Correo ya registrado";
                }

                if (_servicioUtilidades.ExisteCedula(usuario.Cedula))
                {
                    ViewData["Mensaje"] = "Cedula ya registrada";
                }
                ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
                ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
                return View(usuario);
            }

            if (ModelState.IsValid)
            {
                ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
                usuario.Contrasena = Utilidades.EncriptarClave(usuario.Contrasena);
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["MensajeExitoso"] = "Usuario registrado exitosamente";
                return RedirectToAction(nameof(Index));

            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreUsuario,Correo,Contrasena,IdRol,Cedula,Celular")] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                usuario.Contrasena = Utilidades.EncriptarClave(usuario.Contrasena);

                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Usuarios == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Usuarios == null)
            {
                return Problem("Entity set 'ProyectoContext.Usuarios' is null.");
            }
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return (_context.Usuarios?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}