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
using Prueba.Services;
using Prueba.ViewModels;
using ceTe.DynamicPDF.Printing;
using System.Net;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class ComprobanteRetencionsController : Controller
    {
        private readonly IPrintServices _printServices;
        private readonly IPDFServices _servicesPDF;
        private readonly NuevaAppContext _context;

        public ComprobanteRetencionsController(IPrintServices printServices,
            IPDFServices servicesPDF,
            NuevaAppContext context)
        {
            _printServices = printServices;
            _servicesPDF = servicesPDF;
            _context = context;
        }

        // GET: ComprobanteRetencions
        public async Task<IActionResult> Index()
        {
            var nuevaAppContext = _context.ComprobanteRetencions
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdProveedorNavigation);

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: ComprobanteRetencions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comprobanteRetencion = await _context.ComprobanteRetencions
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobante == id);
            if (comprobanteRetencion == null)
            {
                return NotFound();
            }

            return View(comprobanteRetencion);
        }

        // GET: ComprobanteRetencions/Create
        public IActionResult Create()
        {
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor");
            return View();
        }

        // POST: ComprobanteRetencions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdComprobante,IdFactura,IdProveedor,FechaEmision,Descripcion,Retencion,Sustraendo,ValorRetencion,TotalImpuesto")] ComprobanteRetencion comprobanteRetencion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comprobanteRetencion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura", comprobanteRetencion.IdFactura);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", comprobanteRetencion.IdProveedor);
            return View(comprobanteRetencion);
        }

        // GET: ComprobanteRetencions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comprobanteRetencion = await _context.ComprobanteRetencions.FindAsync(id);
            if (comprobanteRetencion == null)
            {
                return NotFound();
            }
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura", comprobanteRetencion.IdFactura);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", comprobanteRetencion.IdProveedor);
            return View(comprobanteRetencion);
        }

        // POST: ComprobanteRetencions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdComprobante,IdFactura,IdProveedor,FechaEmision,Descripcion,Retencion,Sustraendo,ValorRetencion,TotalImpuesto")] ComprobanteRetencion comprobanteRetencion)
        {
            if (id != comprobanteRetencion.IdComprobante)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comprobanteRetencion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComprobanteRetencionExists(comprobanteRetencion.IdComprobante))
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
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "IdFactura", comprobanteRetencion.IdFactura);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedors, "IdProveedor", "IdProveedor", comprobanteRetencion.IdProveedor);
            return View(comprobanteRetencion);
        }

        // GET: ComprobanteRetencions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comprobanteRetencion = await _context.ComprobanteRetencions
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdComprobante == id);
            if (comprobanteRetencion == null)
            {
                return NotFound();
            }

            return View(comprobanteRetencion);
        }

        // POST: ComprobanteRetencions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comprobanteRetencion = await _context.ComprobanteRetencions.FindAsync(id);
            if (comprobanteRetencion != null)
            {
                _context.ComprobanteRetencions.Remove(comprobanteRetencion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComprobanteRetencionExists(int id)
        {
            return _context.ComprobanteRetencions.Any(e => e.IdComprobante == id);
        }

        public async Task<IActionResult> PrintComprobante(int id)
        {
            var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var comprobanteRetencion = await _context.ComprobanteRetencions.FindAsync(id);
            if (comprobanteRetencion != null)
            {
                var proveedor = await _context.Proveedors.Where(c => c.IdProveedor == comprobanteRetencion.IdProveedor).FirstOrDefaultAsync();
                var condominio = await _context.Condominios.Where(c => c.IdCondominio == proveedor.IdCondominio).FirstOrDefaultAsync();
                //_context.ComprobanteRetencions.Remove(comprobanteRetencion);
                var modelo = new ComprobanteRetencionesISLRVM
                {
                    Condominio = condominio,
                    Proveedor = proveedor,
                    ComprobanteRetencion = comprobanteRetencion
                };

                var data = _servicesPDF.ComprobanteRetencionesISLR(modelo);

                var resultado = _printServices.PrintCompRetencionIva(data, idCondominio);

                TempData.Keep();

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> ComprobantePDF(int id)
        {

            var comprobanteRetencion = await _context.ComprobanteRetencions.FindAsync(id);
            if (comprobanteRetencion != null)
            {
                var proveedor = await _context.Proveedors.Where(c => c.IdProveedor == comprobanteRetencion.IdProveedor).FirstOrDefaultAsync();
                var condominio = await _context.Condominios.Where(c => c.IdCondominio == proveedor.IdCondominio).FirstOrDefaultAsync();
                //_context.ComprobanteRetencions.Remove(comprobanteRetencion);
                var modelo = new ComprobanteRetencionesISLRVM
                {
                    Condominio = condominio,
                    Proveedor = proveedor,
                    ComprobanteRetencion = comprobanteRetencion
                };
                var data = _servicesPDF.ComprobanteRetencionesISLR(modelo);

                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "ComprobanteRetencion.pdf");
            }

            return View("Index");

        }


    }
}
