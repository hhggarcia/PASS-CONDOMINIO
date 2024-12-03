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

    public class CuentasCobrarController : Controller
    {
        private readonly IPdfReportesServices _servicesPdf;
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;

        public CuentasCobrarController(IPdfReportesServices pdfReportesServices,
            IFiltroFechaRepository filtroFechaRepository,
            NuevaAppContext context)
        {
            _servicesPdf = pdfReportesServices;
            _reposFiltroFecha=filtroFechaRepository;
            _context = context;
        }

        // GET: CuentasCobrar
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.CuentasCobrars.OrderByDescending(c => c.Status)
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdFacturaNavigation.IdClienteNavigation)                
                .Where(c => c.IdCondominio == IdCondominio);

            TempData.Keep();

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: CuentasCobrar/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasCobrar = await _context.CuentasCobrars
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentasCobrar == null)
            {
                return NotFound();
            }

            return View(cuentasCobrar);
        }

        // GET: CuentasCobrar/Create
        public IActionResult Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura");

            TempData.Keep();
            return View();
        }

        // POST: CuentasCobrar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCondominio,IdFactura,Monto,Status")] CuentasCobrar cuentasCobrar)
        {
            ModelState.Remove(nameof(cuentasCobrar.IdCondominioNavigation));
            ModelState.Remove(nameof(cuentasCobrar.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(cuentasCobrar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasCobrar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", cuentasCobrar.IdFactura);
            return View(cuentasCobrar);
        }

        // GET: CuentasCobrar/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasCobrar = await _context.CuentasCobrars.FindAsync(id);
            if (cuentasCobrar == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasCobrar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", cuentasCobrar.IdFactura);
            return View(cuentasCobrar);
        }

        // POST: CuentasCobrar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCondominio,IdFactura,Monto,Status")] CuentasCobrar cuentasCobrar)
        {
            if (id != cuentasCobrar.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(cuentasCobrar.IdCondominioNavigation));
            ModelState.Remove(nameof(cuentasCobrar.IdFacturaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuentasCobrar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuentasCobrarExists(cuentasCobrar.Id))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cuentasCobrar.IdCondominio);
            ViewData["IdFactura"] = new SelectList(_context.FacturaEmitida, "IdFacturaEmitida", "NumFactura", cuentasCobrar.IdFactura);
            return View(cuentasCobrar);
        }

        // GET: CuentasCobrar/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuentasCobrar = await _context.CuentasCobrars
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cuentasCobrar == null)
            {
                return NotFound();
            }

            return View(cuentasCobrar);
        }

        // POST: CuentasCobrar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuentasCobrar = await _context.CuentasCobrars.FindAsync(id);
            if (cuentasCobrar != null)
            {
                _context.CuentasCobrars.Remove(cuentasCobrar);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CuentasCobrarExists(int id)
        {
            return _context.CuentasCobrars.Any(e => e.Id == id);
        }
        [HttpPost]
        public async Task<IActionResult> FiltrarFecha(FiltrarFechaVM filtrarFechaVM)
        {
            var cuotas = await _reposFiltroFecha.ObtenerCuentaCobrar(filtrarFechaVM);
            return View("Index", cuotas);
        }

        [HttpPost]
        public ContentResult CuentasCobrarPDF([FromBody] IEnumerable<CuentasCobrar> modelo)
        {
            try
            {
                var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var condominio = _context.Condominios.Find(IdCondominio);

                var ede = (from item in modelo
                           where item.Status.Equals("En Proceso")
                           let itemLibro = _context.LibroVentas.FirstOrDefault(x => x.IdFactura == item.IdFactura)
                           select new CuentasCobrarVM()
                           {
                               Condominio = condominio != null ? condominio.Nombre : "",
                               Cliente = item.IdFacturaNavigation.IdClienteNavigation.Nombre,
                               NumFactura = item.IdFacturaNavigation.NumFactura.ToString(),
                               BaseImponible = item.IdFacturaNavigation.SubTotal,
                               MontoTotal = item.IdFacturaNavigation.MontoTotal,
                               Iva = item.IdFacturaNavigation.Iva,
                               RetIva = itemLibro != null ? itemLibro.RetIva : 0,
                               RetIslr = itemLibro != null ? itemLibro.RetIslr : 0,
                               TotalPagar = item.IdFacturaNavigation.MontoTotal - (itemLibro != null ? itemLibro.RetIva : 0) - (itemLibro != null ? itemLibro.RetIslr : 0)
                           }).ToList();

                var data = _servicesPdf.CuentasCobrarPDF(ede);
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
