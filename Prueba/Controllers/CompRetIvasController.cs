using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ceTe.DynamicPDF.Printing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class CompRetIvasController : Controller
    {
        private readonly IPrintServices _printServices;
        private readonly IPDFServices _servicesPDF;
        private readonly NuevaAppContext _context;

        public CompRetIvasController(IPrintServices printServices,
            IPDFServices servicesPDF,
            NuevaAppContext context)
        {
            _printServices = printServices;
            _servicesPDF = servicesPDF;
            _context = context;
        }

        // GET: CompRetIvas
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.CompRetIvas
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdNotaCreditoNavigation)
                .Include(c => c.IdNotaDebitoNavigation)
                .Include(c => c.IdProveedorNavigation)
                .Where(c => c.IdProveedorNavigation.IdCondominio == IdCondominio);

            TempData.Keep();
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
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura");
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito");
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre");
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
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", compRetIva.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIva.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIva.IdNotaDebito);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", compRetIva.IdProveedor);
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
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", compRetIva.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIva.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIva.IdNotaDebito);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", compRetIva.IdProveedor);
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
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", compRetIva.IdFactura);
            ViewData["IdNotaCredito"] = new SelectList(_context.NotaCreditos, "IdNotaCredito", "IdNotaCredito", compRetIva.IdNotaCredito);
            ViewData["IdNotaDebito"] = new SelectList(_context.NotaDebitos, "IdNotaDebito", "IdNotaDebito", compRetIva.IdNotaDebito);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "Nombre", compRetIva.IdProveedor);
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

        public async Task<IActionResult> PrintComprobante(int id)
        {
            try
            {
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var comprobanteRetencion = await _context.CompRetIvas.FindAsync(id);
                if (comprobanteRetencion != null)
                {
                    var proveedor = await _context.Proveedors.Where(c => c.IdProveedor == comprobanteRetencion.IdProveedor).FirstOrDefaultAsync();
                    var condominio = await _context.Condominios.Where(c => c.IdCondominio == proveedor.IdCondominio).FirstOrDefaultAsync();
                    //_context.ComprobanteRetencions.Remove(comprobanteRetencion);
                    var modelo = new ComprobanteRetencionesIVAVM
                    {
                        Condominio = condominio,
                        Proveedor = proveedor,
                        compRetIva = comprobanteRetencion
                    };

                    var data = _servicesPDF.ComprobanteRetencionesIVA(modelo);

                    var resultado = _printServices.PrintCompRetencionIva(data, idCondominio);

                    TempData.Keep();

                    return RedirectToAction("Index");
                }

                TempData.Keep();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }

        public async Task<IActionResult> ComprobantePDF(int id)
        {

            var comprobanteRetencion = await _context.CompRetIvas.FindAsync(id);
            if (comprobanteRetencion != null)
            {
                var proveedor = await _context.Proveedors.Where(c => c.IdProveedor == comprobanteRetencion.IdProveedor).FirstOrDefaultAsync();
                var condominio = await _context.Condominios.Where(c => c.IdCondominio == proveedor.IdCondominio).FirstOrDefaultAsync();
                //_context.ComprobanteRetencions.Remove(comprobanteRetencion);
                var modelo = new ComprobanteRetencionesIVAVM
                {
                    Condominio = condominio,
                    Proveedor = proveedor,
                    compRetIva = comprobanteRetencion
                };
                var data = _servicesPDF.ComprobanteRetencionesIVA(modelo);

                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "ComprobanteRetencion.pdf");
            }

            return View("Index");

        }
    }
}
