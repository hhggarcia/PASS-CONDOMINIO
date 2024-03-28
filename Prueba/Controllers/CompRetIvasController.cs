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

    public class CompRetIvasController : Controller
    {
        private readonly NuevaAppContext _context;

        public CompRetIvasController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: CompRetIvas
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.CompRetIvas.Include(c => c.IdFacturaNavigation).Include(c => c.IdNotaCreditoNavigation).Include(c => c.IdNotaDebitoNavigation).Include(c => c.IdProveedorNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CompRetIvas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compRetIva = await _context.CompRetIvas
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdNotaCreditoNavigation)
                .Include(c => c.IdNotaDebitoNavigation)
                .Include(c => c.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobanteIva == id);
            if (compRetIva == null)
            {
                return NotFound();
            }

            return View(compRetIva);
        }

        // GET: CompRetIvas/Create
        public IActionResult Create()
        {
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura");
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito");
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor");
            return View();
        }

        // POST: CompRetIvas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdComprobanteIva,IdFactura,IdProveedor,FechaEmision,IdNotaCredito,IdNotaDebito,TipoTransaccion,NumFacturaAfectada,TotalCompraIva,CompraSinCreditoIva,BaseImponible,Alicuota,ImpIva,IvaRetenido,TotalCompraRetIva")] CompRetIva compRetIva)
        {
            if (ModelState.IsValid)
            {
                _context.Add(compRetIva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura", compRetIva.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIva.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIva.IdNotaDebito);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", compRetIva.IdProveedor);
            return View(compRetIva);
        }

        // GET: CompRetIvas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compRetIva = await _context.CompRetIvas.FindAsync(id);
            if (compRetIva == null)
            {
                return NotFound();
            }
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura", compRetIva.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIva.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIva.IdNotaDebito);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", compRetIva.IdProveedor);
            return View(compRetIva);
        }

        // POST: CompRetIvas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdComprobanteIva,IdFactura,IdProveedor,FechaEmision,IdNotaCredito,IdNotaDebito,TipoTransaccion,NumFacturaAfectada,TotalCompraIva,CompraSinCreditoIva,BaseImponible,Alicuota,ImpIva,IvaRetenido,TotalCompraRetIva")] CompRetIva compRetIva)
        {
            if (id != compRetIva.IdComprobanteIva)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(compRetIva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompRetIvaExists(compRetIva.IdComprobanteIva))
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
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura", compRetIva.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIva.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIva.IdNotaDebito);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", compRetIva.IdProveedor);
            return View(compRetIva);
        }

        // GET: CompRetIvas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compRetIva = await _context.CompRetIvas
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdNotaCreditoNavigation)
                .Include(c => c.IdNotaDebitoNavigation)
                .Include(c => c.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobanteIva == id);
            if (compRetIva == null)
            {
                return NotFound();
            }

            return View(compRetIva);
        }

        // POST: CompRetIvas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var compRetIva = await _context.CompRetIvas.FindAsync(id);
            if (compRetIva != null)
            {
                _context.CompRetIvas.Remove(compRetIva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompRetIvaExists(int id)
        {
            return _context.CompRetIvas.Any(e => e.IdComprobanteIva == id);
        }
    }
}
