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
using Prueba.Repositories;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class FacturasController : Controller
    {
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public FacturasController(IFiltroFechaRepository filtroFechaRepository, NuevaAppContext context)
        {
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: Facturas
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = await _context.Facturas.Include(f => f.IdProveedorNavigation).ToListAsync();
            return View(nuevaAppContext);
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdFactura == id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // GET: Facturas/Create
        public async Task<IActionResult> Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var listFacturas = await (from f in _context.Facturas
                                      join c in _context.CodigoCuentasGlobals on f.IdCodCuenta equals c.IdCodCuenta
                                      where c.IdCondominio == IdCondominio
                                      select f).ToListAsync();
            ViewData["NumFactura"] = listFacturas[listFacturas.Count -1].NumFactura;
            ViewData["NumControl"] = listFacturas[listFacturas.Count -1].NumControl;
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre");
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");
            return View();
        }

        // POST: Facturas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFactura,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,Subtotal,Iva,MontoTotal,IdProveedor,IdCodCuenta,Abonado,Pagada,EnProceso")] Factura factura)
        {
            var idCuenta = _context.SubCuenta.Where(c => c.Id == factura.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c=>c.IdCodCuenta).FirstOrDefault();
            factura.IdCodCuenta = (short)idCodCuenta;
            factura.MontoTotal = factura.Subtotal + factura.Iva;
            ModelState.Remove(nameof(factura.IdProveedorNavigation));
            ModelState.Remove(nameof(factura.IdCodCuentaGlobalNavigation));
            if (ModelState.IsValid)
            {
                _context.Add(factura);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", factura.IdProveedor);
            return View(factura);
        }

        // GET: Facturas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound();
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", factura.IdProveedor);
            return View(factura);
        }

        // POST: Facturas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFactura,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,Subtotal,Iva,MontoTotal,IdProveedor,Abonado,Pagada,EnProceso")] Factura factura)
        {
            if (id != factura.IdFactura)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(factura.IdProveedorNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(factura);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaExists(factura.IdFactura))
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
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", factura.IdProveedor);
            return View(factura);
        }

        // GET: Facturas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdFactura == id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura != null)
            {
                _context.Facturas.Remove(factura);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaExists(int id)
        {
            return _context.Facturas.Any(e => e.IdFactura == id);
        }
        [HttpPost]
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var filtrarFecha = await _reposFiltroFecha.ObtenerFacturas(filtrarFechaVM);
            return View("Index", filtrarFecha);
        }
    }
}
