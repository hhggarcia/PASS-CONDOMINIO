using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class CuentasPagarController : Controller
    {
        private readonly IPdfReportesServices _servicesPdf;
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public CuentasPagarController(IPdfReportesServices pdfReportesServices,
            IFiltroFechaRepository filtroFechaRepository, 
            NuevaAppContext context)
        {
            _servicesPdf = pdfReportesServices;
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
        }

        // GET: CuentasPagar
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.CuentasPagars.OrderByDescending(c => c.Status)
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdFacturaNavigation.IdProveedorNavigation)
                .Where(c => c.IdCondominio == IdCondominio);

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CuentasPagar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasPagar = await _context.CuentasPagars
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentasPagar == null)
            {
                return NotFound();
            }

            return View(cuentasPagar);
        }

        // GET: CuentasPagar/Create
        public IActionResult Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura");

            TempData.Keep();
            return View();
        }

        // POST: CuentasPagar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCondominio,IdFactura,Monto,Status")] CuentasPagar cuentasPagar)
        {
            ModelState.Remove(nameof(cuentasPagar.IdCondominioNavigation));
            ModelState.Remove(nameof(cuentasPagar.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(cuentasPagar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasPagar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", cuentasPagar.IdFactura);
            return View(cuentasPagar);
        }

        // GET: CuentasPagar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasPagar = await _context.CuentasPagars.FindAsync(id);
            if (cuentasPagar == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasPagar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", cuentasPagar.IdFactura);
            return View(cuentasPagar);
        }

        // POST: CuentasPagar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCondominio,IdFactura,Monto,Status")] CuentasPagar cuentasPagar)
        {
            if (id != cuentasPagar.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(cuentasPagar.IdCondominioNavigation));
            ModelState.Remove(nameof(cuentasPagar.IdFacturaNavigation));


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentasPagar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentasPagarExists(cuentasPagar.Id))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasPagar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.Facturas, "IdFactura", "NumFactura", cuentasPagar.IdFactura);
            return View(cuentasPagar);
        }

        // GET: CuentasPagar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasPagar = await _context.CuentasPagars
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentasPagar == null)
            {
                return NotFound();
            }

            return View(cuentasPagar);
        }

        // POST: CuentasPagar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuentasPagar = await _context.CuentasPagars.FindAsync(id);
            if (cuentasPagar != null)
            {
                _context.CuentasPagars.Remove(cuentasPagar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentasPagarExists(int id)
        {
            return _context.CuentasPagars.Any(e => e.Id == id);
        }
        [HttpPost]
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var cuotas = await _reposFiltroFecha.ObtenerCuentasPagar(filtrarFechaVM);
            return View("Index", cuotas);
        }

        [HttpPost]
        public ContentResult CuentasPagarPDF([FromBody] IEnumerable<CuentasPagar> modelo)
        {
            try
            {
                var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var condominio = _context.Condominios.Find(IdCondominio);

                var ede = (from CuentasPagar item in modelo
                           where item.Status.Equals("En Proceso")
                           let itemLibro = _context.LibroCompras.FirstOrDefault(x => x.IdFactura == item.IdFactura)
                           select new CuentasPagarVM()
                           {
                               Condominio = condominio != null ? condominio.Nombre : "",
                               Proveedor = item.IdFacturaNavigation.IdProveedorNavigation.Nombre,
                               NumFactura = item.IdFacturaNavigation.NumFactura.ToString(),
                               BaseImponible = item.IdFacturaNavigation.Subtotal,
                               MontoTotal = item.IdFacturaNavigation.MontoTotal,
                               Iva = item.IdFacturaNavigation.Iva,
                               RetIva = itemLibro != null ? itemLibro.RetIva : 0,
                               RetIslr = itemLibro != null ? itemLibro.RetIslr : 0,
                               TotalPagar = item.IdFacturaNavigation.MontoTotal - (itemLibro != null ? itemLibro.RetIva : 0) - (itemLibro != null ? itemLibro.RetIslr : 0)
                           }).ToList();
                var data = _servicesPdf.CuentasPagarPDF(ede);
                var base64 = Convert.ToBase64String(data);

                TempData.Keep();
                return Content(base64, "application/pdf");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generando PDF: {e.Message}");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
            }
        }
    }
}
