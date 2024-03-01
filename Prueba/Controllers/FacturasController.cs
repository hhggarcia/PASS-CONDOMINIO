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
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            //var listFacturas = await (from f in _context.Facturas.Include(f => f.IdProveedorNavigation)
            //                          join c in _context.CodigoCuentasGlobals on f.IdCodCuenta equals c.IdCodCuenta
            //                          where c.IdCondominio == IdCondominio
            //                          select f).ToListAsync();

            var nuevaAppContext = await _context.Facturas.Include(f => f.IdProveedorNavigation).ToListAsync();

            TempData.Keep();

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

            if (listFacturas.Count != 0)
            {
                ViewData["NumFactura"] = listFacturas[listFacturas.Count - 1].NumFactura;
                ViewData["NumControl"] = listFacturas[listFacturas.Count - 1].NumControl;
            }

            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre");
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");

            TempData.Keep();
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
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();

            factura.IdCodCuenta = idCodCuenta;
            factura.MontoTotal = factura.Subtotal + factura.Iva;

            ModelState.Remove(nameof(factura.IdProveedorNavigation));
            ModelState.Remove(nameof(factura.IdCodCuentaNavigation));

            if (ModelState.IsValid)
            {
                factura.EnProceso = true;
                _context.Add(factura);
                await _context.SaveChangesAsync();

                // calcular retenciones al proveedor

                var proveedor = await _context.Proveedors.FindAsync(factura.IdProveedor);

                if (proveedor == null)
                {
                    return NotFound();
                }

                var rtiva = await _context.Ivas.FindAsync(proveedor.IdRetencionIva);

                var rtislr = await _context.Islrs.FindAsync(proveedor.IdRetencionIslr);

                decimal montoRTIVA = 0;
                decimal montoRTISLR = 0;

                if (rtiva != null)
                {
                    montoRTIVA = factura.Iva * (rtiva.Porcentaje / 100);
                }

                if (rtislr != null)
                {
                    montoRTISLR = factura.Subtotal * (rtislr.Tarifa / 100);
                }


                // registrar libro de compras

                var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var itemLibroCompra = new LibroCompra
                {
                    IdCondominio = IdCondominio,
                    IdFactura = factura.IdFactura,
                    BaseImponible = factura.Subtotal,
                    ExentoIva = 0,
                    Iva = factura.Iva,
                    Igtf = 0,
                    RetIva = montoRTIVA,
                    RetIslr = montoRTISLR,
                    Monto = factura.MontoTotal,
                    NumComprobanteRet = 0,
                    FechaComprobanteRet = DateTime.Now
                };

                // registrar cuentas por pagar

                var itemCuentaPorPagar = new CuentasPagar
                {
                    IdCondominio = IdCondominio,
                    IdFactura = factura.IdFactura,
                    Monto = factura.MontoTotal,
                    Status = factura.EnProceso ? "En Proceso" : "Pagada"
                };
                _context.Add(itemLibroCompra);
                _context.Add(itemCuentaPorPagar);
              

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", factura.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");

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
            var cc = await _context.CodigoCuentasGlobals.FindAsync(factura.IdCodCuenta);
            if (cc == null)
            {
                return NotFound();
            }
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", factura.IdProveedor);
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion", cc.IdSubCuenta);

            return View(factura);
        }

        // POST: Facturas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFactura,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,Subtotal,Iva,MontoTotal,IdProveedor,IdCodCuenta,Abonado,Pagada,EnProceso")] Factura factura)
        {
            if (id != factura.IdFactura)
            {
                return NotFound();
            }

            var idCuenta = _context.SubCuenta.Where(c => c.Id == factura.IdCodCuenta).Select(c => c.Id).FirstOrDefault();
            var idCodCuenta = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == idCuenta).Select(c => c.IdCodCuenta).FirstOrDefault();
            factura.IdCodCuenta = idCodCuenta;

            ModelState.Remove(nameof(factura.IdProveedorNavigation));
            ModelState.Remove(nameof(factura.IdCodCuentaNavigation));

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
            ViewData["IdCodCuenta"] = new SelectList(_context.SubCuenta, "Id", "Descricion");

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
                var pagosFactura = await _context.PagoFacturas.Where(c => c.IdFactura.Equals(id)).ToListAsync();

                if (pagosFactura != null)
                {
                    _context.PagoFacturas.RemoveRange(pagosFactura);
                }

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
