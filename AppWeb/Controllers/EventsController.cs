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
    public class EventsController : Controller
    {
        private readonly ProyectoContext _context;

        public EventsController(ProyectoContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var proyectoContext = _context.Events.Include(e => e.Estado).Include(e => e.Pago).Include(e => e.User).Include(e => e.Vehiculo);

            return View(await proyectoContext.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var @event = await _context.Events
                .Include(e => e.Estado)
                .Include(e => e.Pago)
                .Include(e => e.User)
                .Include(e => e.Vehiculo)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "NombreEstado");
            ViewData["PagoId"] = new SelectList(_context.Pagos, "PagoId", "Monto");
            ViewData["UserId"] = new SelectList(_context.Usuarios, "Id", "Cedula");
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "IdAuto", "Nombre");
            return View();
        }


        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,Title,Start,End,UserId,PagoId,VehiculoId,EstadoId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "NombreEstado", @event.EstadoId);
            ViewData["PagoId"] = new SelectList(_context.Pagos, "PagoId", "Monto", @event.PagoId);
            ViewData["UserId"] = new SelectList(_context.Usuarios, "Id", "Cedula", @event.UserId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "IdAuto", "Nombre", @event.VehiculoId);

            return View(@event);
        }


        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "NombreEstado", @event.EstadoId);
            ViewData["PagoId"] = new SelectList(_context.Pagos, "PagoId", "Monto", @event.PagoId);
            ViewData["UserId"] = new SelectList(_context.Usuarios, "Id", "Cedula", @event.UserId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "IdAuto", "Nombre", @event.VehiculoId);
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Start,End,UserId,PagoId,VehiculoId,EstadoId")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
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
            ViewData["EstadoId"] = new SelectList(_context.Estados, "EstadoId", "NombreEstado", @event.EstadoId);
            ViewData["PagoId"] = new SelectList(_context.Pagos, "PagoId", "Monto", @event.PagoId);
            ViewData["UserId"] = new SelectList(_context.Usuarios, "Id", "Cedula", @event.UserId);
            ViewData["VehiculoId"] = new SelectList(_context.Vehiculos, "IdAuto", "Nombre", @event.VehiculoId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }
            ViewData["nombreUsuario"] = HttpContext.User?.Identity?.Name;
            var @event = await _context.Events
                .Include(e => e.Estado)
                .Include(e => e.Pago)
                .Include(e => e.User)
                .Include(e => e.Vehiculo)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Events == null)
            {
                return Problem("Entity set 'ProyectoContext.Events'  is null.");
            }
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return (_context.Events?.Any(e => e.EventId == id)).GetValueOrDefault();
        }

    }
}
