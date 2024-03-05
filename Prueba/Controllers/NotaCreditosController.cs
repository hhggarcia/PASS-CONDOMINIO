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
    public class NotaCreditosController : Controller
    {
        private readonly NuevaAppContext _context;

        public NotaCreditosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: NotaCreditos
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.NotaCreditos.Include(n => n.IdClienteNavigation).Include(n => n.IdFacturaNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: NotaCreditos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaCredito = await _context.NotaCreditos
                .Include(n => n.IdClienteNavigation)
                .Include(n => n.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.IdNotaCredito == id);
            if (notaCredito == null)
            {
                return NotFound();
            }

            return View(notaCredito);
        }

        // GET: NotaCreditos/Create
        public IActionResult Create()
        {
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente");
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida");
            return View();
        }

        // POST: NotaCreditos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdNotaCredito,IdFactura,IdCliente,Concepto,Comprobante,Fecha,Monto")] NotaCredito notaCredito)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notaCredito);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", notaCredito.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", notaCredito.IdFactura);
            return View(notaCredito);
        }

        // GET: NotaCreditos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaCredito = await _context.NotaCreditos.FindAsync(id);
            if (notaCredito == null)
            {
                return NotFound();
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", notaCredito.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", notaCredito.IdFactura);
            return View(notaCredito);
        }

        // POST: NotaCreditos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdNotaCredito,IdFactura,IdCliente,Concepto,Comprobante,Fecha,Monto")] NotaCredito notaCredito)
        {
            if (id != notaCredito.IdNotaCredito)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notaCredito);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotaCreditoExists(notaCredito.IdNotaCredito))
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
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", notaCredito.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", notaCredito.IdFactura);
            return View(notaCredito);
        }

        // GET: NotaCreditos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notaCredito = await _context.NotaCreditos
                .Include(n => n.IdClienteNavigation)
                .Include(n => n.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.IdNotaCredito == id);
            if (notaCredito == null)
            {
                return NotFound();
            }

            return View(notaCredito);
        }

        // POST: NotaCreditos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notaCredito = await _context.NotaCreditos.FindAsync(id);
            if (notaCredito != null)
            {
                _context.NotaCreditos.Remove(notaCredito);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotaCreditoExists(int id)
        {
            return _context.NotaCreditos.Any(e => e.IdNotaCredito == id);
        }
    }
}
