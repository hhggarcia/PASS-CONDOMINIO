using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Controllers
{
    public class PagoFacturaEmitidasController : Controller
    {
        private readonly NuevaAppContext _context;

        public PagoFacturaEmitidasController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: PagoFacturaEmitidas
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.PagoFacturaEmitida.Include(p => p.IdFacturaNavigation).Include(p => p.IdPagoRecibidoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: PagoFacturaEmitidas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoFacturaEmitida = await _context.PagoFacturaEmitida
                .Include(p => p.IdFacturaNavigation)
                .Include(p => p.IdPagoRecibidoNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoFacturaEmitida == id);
            if (pagoFacturaEmitida == null)
            {
                return NotFound();
            }

            return View(pagoFacturaEmitida);
        }

        // GET: PagoFacturaEmitidas/Create
        public IActionResult Create()
        {
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida");
            ViewData["IdPagoRecibido"] = new SelectList(_context.PagoRecibidos, "IdPagoRecibido", "IdPagoRecibido");
            return View();
        }

        // POST: PagoFacturaEmitidas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPagoFacturaEmitida,IdFactura,IdPagoRecibido")] PagoFacturaEmitida pagoFacturaEmitida)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pagoFacturaEmitida);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", pagoFacturaEmitida.IdFactura);
            ViewData["IdPagoRecibido"] = new SelectList(_context.PagoRecibidos, "IdPagoRecibido", "IdPagoRecibido", pagoFacturaEmitida.IdPagoRecibido);
            return View(pagoFacturaEmitida);
        }

        // GET: PagoFacturaEmitidas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoFacturaEmitida = await _context.PagoFacturaEmitida.FindAsync(id);
            if (pagoFacturaEmitida == null)
            {
                return NotFound();
            }
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", pagoFacturaEmitida.IdFactura);
            ViewData["IdPagoRecibido"] = new SelectList(_context.PagoRecibidos, "IdPagoRecibido", "IdPagoRecibido", pagoFacturaEmitida.IdPagoRecibido);
            return View(pagoFacturaEmitida);
        }

        // POST: PagoFacturaEmitidas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPagoFacturaEmitida,IdFactura,IdPagoRecibido")] PagoFacturaEmitida pagoFacturaEmitida)
        {
            if (id != pagoFacturaEmitida.IdPagoFacturaEmitida)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pagoFacturaEmitida);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoFacturaEmitidaExists(pagoFacturaEmitida.IdPagoFacturaEmitida))
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
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", pagoFacturaEmitida.IdFactura);
            ViewData["IdPagoRecibido"] = new SelectList(_context.PagoRecibidos, "IdPagoRecibido", "IdPagoRecibido", pagoFacturaEmitida.IdPagoRecibido);
            return View(pagoFacturaEmitida);
        }

        // GET: PagoFacturaEmitidas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagoFacturaEmitida = await _context.PagoFacturaEmitida
                .Include(p => p.IdFacturaNavigation)
                .Include(p => p.IdPagoRecibidoNavigation)
                .FirstOrDefaultAsync(m => m.IdPagoFacturaEmitida == id);
            if (pagoFacturaEmitida == null)
            {
                return NotFound();
            }

            return View(pagoFacturaEmitida);
        }

        // POST: PagoFacturaEmitidas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pagoFacturaEmitida = await _context.PagoFacturaEmitida.FindAsync(id);
            if (pagoFacturaEmitida != null)
            {
                _context.PagoFacturaEmitida.Remove(pagoFacturaEmitida);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PagoFacturaEmitidaExists(int id)
        {
            return _context.PagoFacturaEmitida.Any(e => e.IdPagoFacturaEmitida == id);
        }
    }
}
