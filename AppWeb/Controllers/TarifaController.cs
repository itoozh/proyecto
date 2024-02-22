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

    [Authorize(Policy = "CombinePolicy")]
    public class TarifaController : Controller
    {

        private readonly ProyectoContext _context;

        public TarifaController(ProyectoContext context)
        {
            _context = context;
        }

        // GET: Tarifa
        public async Task<IActionResult> Index()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return _context.Pagos != null ? 
                          View(await _context.Pagos.ToListAsync()) :
                          Problem("Entity set 'ProyectoContext.Pagos'  is null.");
        }

        // GET: Tarifa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            if (id == null || _context.Pagos == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .FirstOrDefaultAsync(m => m.PagoId == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // GET: Tarifa/Create
        public IActionResult Create()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            return View();
        }

        // POST: Tarifa/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PagoId,Monto,Nombre")] Pago pago)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            if (ModelState.IsValid)
            {
                _context.Add(pago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pago);
        }

        // GET: Tarifa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            if (id == null || _context.Pagos == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            return View(pago);
        }

        // POST: Tarifa/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PagoId,Monto,Nombre")] Pago pago)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            if (id != pago.PagoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoExists(pago.PagoId))
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
            return View(pago);
        }

        // GET: Tarifa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            if (id == null || _context.Pagos == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .FirstOrDefaultAsync(m => m.PagoId == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // POST: Tarifa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            if (_context.Pagos == null)
            {
                return Problem("Entity set 'ProyectoContext.Pagos'  is null.");
            }
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PagoExists(int id)
        {
          return (_context.Pagos?.Any(e => e.PagoId == id)).GetValueOrDefault();
        }
    }
}
