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
    public class FacturaEmitidasController : Controller
    {
        private readonly NuevaAppContext _context;

        public FacturaEmitidasController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: FacturaEmitidas
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.FacturaEmitida.Include(f => f.IdProductoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: FacturaEmitidas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaEmitida = await _context.FacturaEmitida
                .Include(f => f.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdFacturaEmitida == id);
            if (facturaEmitida == null)
            {
                return NotFound();
            }

            return View(facturaEmitida);
        }

        // GET: FacturaEmitidas/Create
        public IActionResult Create()
        {
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto");
            return View();
        }

        // POST: FacturaEmitidas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFacturaEmitida,IdProducto,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,SubTotal,Iva,MontoTotal,Abonado,Pagada,EnProceso")] FacturaEmitida facturaEmitida)
        {
            if (ModelState.IsValid)
            {
                _context.Add(facturaEmitida);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", facturaEmitida.IdProducto);
            return View(facturaEmitida);
        }

        // GET: FacturaEmitidas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaEmitida = await _context.FacturaEmitida.FindAsync(id);
            if (facturaEmitida == null)
            {
                return NotFound();
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", facturaEmitida.IdProducto);
            return View(facturaEmitida);
        }

        // POST: FacturaEmitidas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFacturaEmitida,IdProducto,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,SubTotal,Iva,MontoTotal,Abonado,Pagada,EnProceso")] FacturaEmitida facturaEmitida)
        {
            if (id != facturaEmitida.IdFacturaEmitida)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(facturaEmitida);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaEmitidaExists(facturaEmitida.IdFacturaEmitida))
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
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "IdProducto", facturaEmitida.IdProducto);
            return View(facturaEmitida);
        }

        // GET: FacturaEmitidas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facturaEmitida = await _context.FacturaEmitida
                .Include(f => f.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdFacturaEmitida == id);
            if (facturaEmitida == null)
            {
                return NotFound();
            }

            return View(facturaEmitida);
        }

        // POST: FacturaEmitidas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facturaEmitida = await _context.FacturaEmitida.FindAsync(id);
            if (facturaEmitida != null)
            {
                _context.FacturaEmitida.Remove(facturaEmitida);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaEmitidaExists(int id)
        {
            return _context.FacturaEmitida.Any(e => e.IdFacturaEmitida == id);
        }
    }
}
