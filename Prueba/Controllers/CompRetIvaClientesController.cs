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

    public class CompRetIvaClientesController : Controller
    {
        private readonly NuevaAppContext _context;

        public CompRetIvaClientesController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: CompRetIvaClientes
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.CompRetIvaClientes.Include(c => c.IdClienteNavigation).Include(c => c.IdFacturaNavigation).Include(c => c.IdNotaCreditoNavigation).Include(c => c.IdNotaDebitoNavigation);
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CompRetIvaClientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compRetIvaCliente = await _context.CompRetIvaClientes
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdNotaCreditoNavigation)
                .Include(c => c.IdNotaDebitoNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobanteIva == id);
            if (compRetIvaCliente == null)
            {
                return NotFound();
            }

            return View(compRetIvaCliente);
        }

        // GET: CompRetIvaClientes/Create
        public IActionResult Create()
        {
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente");
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida");
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito");
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito");
            return View();
        }

        // POST: CompRetIvaClientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdComprobanteIva,IdFactura,IdCliente,FechaEmision,IdNotaCredito,IdNotaDebito,TipoTransaccion,NumFacturaAfectada,TotalCompraIva,CompraSinCreditoIva,BaseImponible,Alicuota,ImpIva,IvaRetenido,TotalCompraRetIva")] CompRetIvaCliente compRetIvaCliente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(compRetIvaCliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", compRetIvaCliente.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", compRetIvaCliente.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIvaCliente.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIvaCliente.IdNotaDebito);
            return View(compRetIvaCliente);
        }

        // GET: CompRetIvaClientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compRetIvaCliente = await _context.CompRetIvaClientes.FindAsync(id);
            if (compRetIvaCliente == null)
            {
                return NotFound();
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", compRetIvaCliente.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", compRetIvaCliente.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIvaCliente.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIvaCliente.IdNotaDebito);
            return View(compRetIvaCliente);
        }

        // POST: CompRetIvaClientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdComprobanteIva,IdFactura,IdCliente,FechaEmision,IdNotaCredito,IdNotaDebito,TipoTransaccion,NumFacturaAfectada,TotalCompraIva,CompraSinCreditoIva,BaseImponible,Alicuota,ImpIva,IvaRetenido,TotalCompraRetIva")] CompRetIvaCliente compRetIvaCliente)
        {
            if (id != compRetIvaCliente.IdComprobanteIva)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(compRetIvaCliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompRetIvaClienteExists(compRetIvaCliente.IdComprobanteIva))
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
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", compRetIvaCliente.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "IdFacturaEmitida", compRetIvaCliente.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIvaCliente.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIvaCliente.IdNotaDebito);
            return View(compRetIvaCliente);
        }

        // GET: CompRetIvaClientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var compRetIvaCliente = await _context.CompRetIvaClientes
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdNotaCreditoNavigation)
                .Include(c => c.IdNotaDebitoNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobanteIva == id);
            if (compRetIvaCliente == null)
            {
                return NotFound();
            }

            return View(compRetIvaCliente);
        }

        // POST: CompRetIvaClientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var compRetIvaCliente = await _context.CompRetIvaClientes.FindAsync(id);
            if (compRetIvaCliente != null)
            {
                _context.CompRetIvaClientes.Remove(compRetIvaCliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompRetIvaClienteExists(int id)
        {
            return _context.CompRetIvaClientes.Any(e => e.IdComprobanteIva == id);
        }
    }
}
