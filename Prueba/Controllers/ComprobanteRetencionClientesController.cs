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

    public class ComprobanteRetencionClientesController : Controller
    {
        private readonly NuevaAppContext _context;

        public ComprobanteRetencionClientesController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: ComprobanteRetencionClientes
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.ComprobanteRetencionClientes
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdFacturaNavigation)
                .Where(c => c.IdClienteNavigation.IdCondominio == IdCondominio);

            TempData.Keep();
            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: ComprobanteRetencionClientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comprobanteRetencionCliente = await _context.ComprobanteRetencionClientes
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobanteCliente == id);
            if (comprobanteRetencionCliente == null)
            {
                return NotFound();
            }

            return View(comprobanteRetencionCliente);
        }

        // GET: ComprobanteRetencionClientes/Create
        public IActionResult Create()
        {
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura");
            return View();
        }

        // POST: ComprobanteRetencionClientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdComprobanteCliente,IdFactura,IdCliente,FechaEmision,Description,Retencion,Sustraendo,ValorRetencion,TotalImpuesto,NumCompRet,NumComprobante,TotalFactura,BaseImponible")] ComprobanteRetencionCliente comprobanteRetencionCliente)
        {
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdFacturaNavigation");
            if (ModelState.IsValid)
            {
                _context.Add(comprobanteRetencionCliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre", comprobanteRetencionCliente.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", comprobanteRetencionCliente.IdFactura);
            return View(comprobanteRetencionCliente);
        }

        // GET: ComprobanteRetencionClientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comprobanteRetencionCliente = await _context.ComprobanteRetencionClientes.FindAsync(id);
            if (comprobanteRetencionCliente == null)
            {
                return NotFound();
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre", comprobanteRetencionCliente.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", comprobanteRetencionCliente.IdFactura);
            return View(comprobanteRetencionCliente);
        }

        // POST: ComprobanteRetencionClientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdComprobanteCliente,IdFactura,IdCliente,FechaEmision,Description,Retencion,Sustraendo,ValorRetencion,TotalImpuesto,NumCompRet,NumComprobante,TotalFactura,BaseImponible")] ComprobanteRetencionCliente comprobanteRetencionCliente)
        {
            if (id != comprobanteRetencionCliente.IdComprobanteCliente)
            {
                return NotFound();
            }

            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdFacturaNavigation");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comprobanteRetencionCliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComprobanteRetencionClienteExists(comprobanteRetencionCliente.IdComprobanteCliente))
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
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombre", comprobanteRetencionCliente.IdCliente);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", comprobanteRetencionCliente.IdFactura);
            return View(comprobanteRetencionCliente);
        }

        // GET: ComprobanteRetencionClientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comprobanteRetencionCliente = await _context.ComprobanteRetencionClientes
                .Include(c => c.IdClienteNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobanteCliente == id);
            if (comprobanteRetencionCliente == null)
            {
                return NotFound();
            }

            return View(comprobanteRetencionCliente);
        }

        // POST: ComprobanteRetencionClientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comprobanteRetencionCliente = await _context.ComprobanteRetencionClientes.FindAsync(id);
            if (comprobanteRetencionCliente != null)
            {
                _context.ComprobanteRetencionClientes.Remove(comprobanteRetencionCliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComprobanteRetencionClienteExists(int id)
        {
            return _context.ComprobanteRetencionClientes.Any(e => e.IdComprobanteCliente == id);
        }
    }
}
