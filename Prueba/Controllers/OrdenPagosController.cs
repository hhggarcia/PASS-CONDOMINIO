using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class OrdenPagosController : Controller
    {
        private readonly NuevaAppContext _context;

        public OrdenPagosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: OrdenPagos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.OrdenPagos.Include(o => o.IdPagoEmitidoNavigation).Include(o => o.IdProveedorNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: OrdenPagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenPago = await _context.OrdenPagos
                .Include(o => o.IdPagoEmitidoNavigation)
                .Include(o => o.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdOrdenPago == id);
            if (ordenPago == null)
            {
                return NotFound();
            }

            return View(ordenPago);
        }

        // GET: OrdenPagos/Create
        public IActionResult Create()
        {
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor");
            return View();
        }

        // POST: OrdenPagos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrdenPago,IdPagoEmitido,IdProveedor,Fecha")] OrdenPago ordenPago)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ordenPago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido", ordenPago.IdPagoEmitido);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", ordenPago.IdProveedor);
            return View(ordenPago);
        }

        // GET: OrdenPagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenPago = await _context.OrdenPagos.FindAsync(id);
            if (ordenPago == null)
            {
                return NotFound();
            }
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido", ordenPago.IdPagoEmitido);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", ordenPago.IdProveedor);
            return View(ordenPago);
        }

        // POST: OrdenPagos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdOrdenPago,IdPagoEmitido,IdProveedor,Fecha")] OrdenPago ordenPago)
        {
            if (id != ordenPago.IdOrdenPago)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordenPago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdenPagoExists(ordenPago.IdOrdenPago))
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
            ViewData["IdPagoEmitido"] = new SelectList(_context.PagoEmitidos, "IdPagoEmitido", "IdPagoEmitido", ordenPago.IdPagoEmitido);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", ordenPago.IdProveedor);
            return View(ordenPago);
        }

        // GET: OrdenPagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordenPago = await _context.OrdenPagos
                .Include(o => o.IdPagoEmitidoNavigation)
                .Include(o => o.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdOrdenPago == id);
            if (ordenPago == null)
            {
                return NotFound();
            }

            return View(ordenPago);
        }

        // POST: OrdenPagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordenPago = await _context.OrdenPagos.FindAsync(id);
            if (ordenPago != null)
            {
                _context.OrdenPagos.Remove(ordenPago);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdenPagoExists(int id)
        {
            return _context.OrdenPagos.Any(e => e.IdOrdenPago == id);
        }
    }
}
