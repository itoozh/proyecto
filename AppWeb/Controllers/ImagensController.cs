using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppWeb.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace AppWeb.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class ImagensController : Controller
    {
        private readonly ProyectoContext _context;

        public ImagensController(ProyectoContext context)
        {
            _context = context;
        }

        // GET: Imagens
        public async Task<IActionResult> Index()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return _context.Imagens != null ? 
                          View(await _context.Imagens.ToListAsync()) :
                          Problem("Entity set 'ProyectoContext.Imagens'  is null.");
        }

        // GET: Imagens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Imagens == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var imagen = await _context.Imagens
                .FirstOrDefaultAsync(m => m.IdImagen == id);
            if (imagen == null)
            {
                return NotFound();
            }

            return View(imagen);
        }

        // GET: Imagens/Create
        public IActionResult Create()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View();
        }

        // POST: Imagens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdImagen,ImagenData,ImagenMimeType")] Imagen imagen)
        {
            if (ModelState.IsValid)
            {
                ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    // Procesar la carga de archivos
                    using (var memoryStream = new MemoryStream())
                    {
                        await files[0].CopyToAsync(memoryStream);
                        imagen.ImagenData = memoryStream.ToArray();
                    }
                }

                _context.Add(imagen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(imagen);
        }

        // GET: Imagens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Imagens == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var imagen = await _context.Imagens.FindAsync(id);
            if (imagen == null)
            {
                return NotFound();
            }
            return View(imagen);
        }

        // POST: Imagens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdImagen,ImagenData,ImagenMimeType")] Imagen imagen)
        {
            if (id != imagen.IdImagen)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(imagen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImagenExists(imagen.IdImagen))
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
            return View(imagen);
        }

        // GET: Imagens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Imagens == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var imagen = await _context.Imagens
                .FirstOrDefaultAsync(m => m.IdImagen == id);
            if (imagen == null)
            {
                return NotFound();
            }

            return View(imagen);
        }

        // POST: Imagens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Imagens == null)
            {
                return Problem("Entity set 'ProyectoContext.Imagens'  is null.");
            }
            var imagen = await _context.Imagens.FindAsync(id);
            if (imagen != null)
            {
                _context.Imagens.Remove(imagen);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImagenExists(int id)
        {
          return (_context.Imagens?.Any(e => e.IdImagen == id)).GetValueOrDefault();
        }
    }
}
