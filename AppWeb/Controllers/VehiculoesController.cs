using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppWeb.Models;

namespace AppWeb.Controllers
{
    public class VehiculoesController : Controller
    {
        private readonly ProyectoContext _context;

        public VehiculoesController(ProyectoContext context)
        {
            _context = context;
        }

        public IActionResult Calender(int idAuto)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            // Lógica de la acción Calendar
            return View();
        }

        // GET: Vehiculoes
        public async Task<IActionResult> Index()
        {
            if (_context.Vehiculos == null)
            {
                return Problem("Entity set 'ProyectoContext.Vehiculos' is null.");
            }

            var vehiculos = await _context.Vehiculos
                .Include(v => v.Imagen) // Include para cargar la información de la imagen
                .ToListAsync();
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View(vehiculos);
        }

        // GET: Vehiculoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vehiculos == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var vehiculo = await _context.Vehiculos
                .Include(v => v.Imagen)
                .FirstOrDefaultAsync(m => m.IdAuto == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAuto,Nombre,Marca,Año,Color,ImagenId")] Vehiculo vehiculo, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    // Si se ha enviado un archivo, guarda la imagen
                    vehiculo.Imagen = new Imagen
                    {
                        ImagenData = await ConvertToByteArray(file),
                        ImagenMimeType = file.ContentType
                    };
                }

                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAuto,Nombre,Marca,Año,Color,ImagenId")] Vehiculo vehiculo, IFormFile file)
        {
            if (id != vehiculo.IdAuto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null && file.Length > 0)
                    {
                        // Si se ha enviado un archivo, actualiza la imagen
                        vehiculo.Imagen = new Imagen
                        {
                            ImagenData = await ConvertToByteArray(file),
                            ImagenMimeType = file.ContentType
                        };
                    }

                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.IdAuto))
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
            return View(vehiculo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Vehiculos == null)
            {
                return Problem("Entity set 'ProyectoContext.Vehiculos' is null.");
            }

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                // Elimina la imagen asociada si existe
                if (vehiculo.Imagen != null)
                {
                    _context.Imagens.Remove(vehiculo.Imagen);
                }

                _context.Vehiculos.Remove(vehiculo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VehiculoExists(int id)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return (_context.Vehiculos?.Any(e => e.IdAuto == id)).GetValueOrDefault();
        }

        private async Task<byte[]> ConvertToByteArray(IFormFile file)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}