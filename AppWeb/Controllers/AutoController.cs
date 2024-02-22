using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace AppWeb.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class AutoController : Controller
    {
        private readonly ProyectoContext _context;

        public AutoController(ProyectoContext context)
        {
            _context = context;
        }

        // GET: Auto
        public async Task<IActionResult> Index()
        {
            var proyectoContext = _context.Vehiculos.Include(v => v.Imagen);
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View(await proyectoContext.ToListAsync());
        }

        // GET: Auto/Details/5
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

        // GET: Auto/Create
        public IActionResult Create()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            ViewData["ImagenId"] = new SelectList(_context.Imagens, "IdImagen", "ImagenData");
            return View();
        }

        // POST: Auto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Marca,Año,Color")] Vehiculo vehiculo, IFormFile imagenData)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imagenData != null && imagenData.Length > 0)
                    {
                        // Procesar la carga de archivos
                        using (var memoryStream = new MemoryStream())
                        {
                            await imagenData.CopyToAsync(memoryStream);
                            Imagen imagen = new Imagen
                            {
                                ImagenData = memoryStream.ToArray(),
                                ImagenMimeType = "image/jpeg"  // Asignar el tipo de imagen directamente como JPEG
                            };

                            // Asociar la imagen al vehículo
                            vehiculo.Imagen = imagen;
                        }
                    }

                    // Agregar el vehículo al contexto y guardar en la base de datos
                    _context.Add(vehiculo);
                    await _context.SaveChangesAsync();

                    // Asignar el IdAuto del vehículo recién creado al campo VehiculoId de la imagen
                    vehiculo.Imagen.VehiculoId = vehiculo.IdAuto;
                    vehiculo.Imagen.ImagenMimeType = $"{vehiculo.Nombre.Replace(" ", "")}.jpg"; // Asignar el nombre de la imagen
                    await _context.SaveChangesAsync(); // Guardar la imagen con el VehiculoId y nombre asignados

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Registrar el error, puedes usar la consola o un sistema de registro
                    Console.WriteLine($"Error al guardar datos: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Error al guardar los datos. Por favor, inténtelo de nuevo.");
                }
            }

            return View(vehiculo);
        }


        // GET: Auto/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Vehiculos == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            ViewData["ImagenId"] = new SelectList(_context.Imagens, "IdImagen", "IdImagen", vehiculo.ImagenId);
            return View(vehiculo);
        }

        // POST: Auto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo, IFormFile imagenData)
        {
            if (id != vehiculo.IdAuto)
            {
                ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Si se proporciona una nueva imagen, procesarla y actualizar la imagen asociada al vehículo
                    if (imagenData != null && imagenData.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await imagenData.CopyToAsync(memoryStream);
                            Imagen nuevaImagen = new Imagen
                            {
                                ImagenData = memoryStream.ToArray(),
                                ImagenMimeType = "image/jpeg"  // Asignar el tipo de imagen directamente como JPEG
                            };

                            // Asociar la nueva imagen al vehículo
                            vehiculo.Imagen = nuevaImagen;
                        }
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

            ViewData["ImagenId"] = new SelectList(_context.Imagens, "IdImagen", "IdImagen", vehiculo.ImagenId);
            return View(vehiculo);
        }
        // GET: Auto/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;

            // Obtener el vehículo y sus imágenes asociadas
            var vehiculo = await _context.Vehiculos
                .Include(v => v.Imagens)
                .FirstOrDefaultAsync(v => v.IdAuto == id);

            if (vehiculo != null)
            {
                // Verificar si las imágenes aún existen antes de eliminarlas
                foreach (var imagen in vehiculo.Imagens.ToList())
                {
                    _context.Imagens.Remove(imagen);
                }

                // Guardar cambios después de eliminar las imágenes
                await _context.SaveChangesAsync();

                // Eliminar el vehículo después de eliminar las imágenes
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
    }
}