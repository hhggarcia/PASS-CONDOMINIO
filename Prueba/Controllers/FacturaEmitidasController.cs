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

    public class FacturaEmitidasController : Controller
    {
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public FacturaEmitidasController(IFiltroFechaRepository filtroFechaRepository, NuevaAppContext context)
        {
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: FacturaEmitidas
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var listFacturas = await (from f in _context.FacturaEmitida.Include(f => f.IdProductoNavigation)
                                      join c in _context.Productos on f.IdProducto equals c.IdProducto
                                      where c.IdCondominio == IdCondominio
                                      select f).ToListAsync();

            //var nuevaAppContext = _context.FacturaEmitida.Include(f => f.IdProductoNavigation);

            TempData.Keep();

            return View(listFacturas);
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
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre");
            return View();
        }

        // POST: FacturaEmitidas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFacturaEmitida,IdProducto,NumFactura,NumControl,Descripcion,FechaEmision,FechaVencimiento,SubTotal,Iva,MontoTotal,Abonado,Pagada,EnProceso")] FacturaEmitida facturaEmitida)
        {
            ModelState.Remove(nameof(facturaEmitida.IdProductoNavigation));

            if (ModelState.IsValid)
            {
                facturaEmitida.EnProceso = true;
                _context.Add(facturaEmitida);
                await _context.SaveChangesAsync();

                // calcular retenciones al producto/servicio

                var producto = await _context.Productos.FindAsync(facturaEmitida.IdProducto);

                if (producto == null)
                {
                    return NotFound();
                }

                var rtiva = await _context.Ivas.FindAsync(producto.IdRetencionIva);

                var rtislr = await _context.Islrs.FindAsync(producto.IdRetencionIslr);

                decimal montoRTIVA = 0;
                decimal montoRTISLR = 0;


                if (rtiva != null)
                {
                    montoRTIVA = facturaEmitida.Iva * (rtiva.Porcentaje / 100);
                }

                if (rtislr != null)
                {
                    montoRTISLR = facturaEmitida.SubTotal * (rtislr.Tarifa / 100) - rtislr.Sustraendo;
                }


                // registrar en libro de ventas

                var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var itemLibroVenta = new LibroVenta
                {
                    IdCondominio = IdCondominio,
                    IdFactura = facturaEmitida.IdFacturaEmitida,
                    BaseImponible = facturaEmitida.SubTotal,
                    Iva = facturaEmitida.Iva,
                    Total = facturaEmitida.MontoTotal,
                    RetIva = montoRTIVA,
                    RetIslr = montoRTISLR,
                    Monto = facturaEmitida.MontoTotal,
                    NumComprobanteRet = 0
                };

                // registrar en cuentas por cobrar

                var itemCuentaPorCobrar = new CuentasCobrar
                {
                    IdCondominio = IdCondominio,
                    IdFactura = facturaEmitida.IdFacturaEmitida,
                    Monto = facturaEmitida.MontoTotal,
                    Status = facturaEmitida.EnProceso ? "En Proceso" : "Pagada"
                };

                _context.Add(itemLibroVenta);
                _context.Add(itemCuentaPorCobrar);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre", facturaEmitida.IdProducto);
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

            ModelState.Remove(nameof(facturaEmitida.IdProductoNavigation));


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
                var pagosFactura = await _context.PagoFacturaEmitida.Where(c => c.IdFactura.Equals(id)).ToListAsync();
                var itemLibroVenta = await _context.LibroVentas.Where(c => c.IdFactura == facturaEmitida.IdFacturaEmitida).ToListAsync();
                var itemCuentasCobrar = await _context.CuentasCobrars.Where(c => c.IdFactura == facturaEmitida.IdFacturaEmitida).ToListAsync();

                if (pagosFactura != null)
                {
                    _context.PagoFacturaEmitida.RemoveRange(pagosFactura);
                }
                if (itemLibroVenta != null)
                {
                    _context.LibroVentas.RemoveRange(itemLibroVenta);
                }
                if (itemCuentasCobrar != null)
                {
                    _context.CuentasCobrars.RemoveRange(itemCuentasCobrar);
                }
                _context.FacturaEmitida.Remove(facturaEmitida);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaEmitidaExists(int id)
        {
            return _context.FacturaEmitida.Any(e => e.IdFacturaEmitida == id);
        }

        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var facturasEmitdas = await _reposFiltroFecha.ObteneFactirasEmitidas(filtrarFechaVM);
            return View("Index", facturasEmitdas);
        }
    }
}
