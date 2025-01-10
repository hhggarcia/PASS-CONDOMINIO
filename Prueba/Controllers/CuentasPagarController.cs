using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClosedXML.Excel;
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
                //.Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                    .ThenInclude(p => p.IdProveedorNavigation)
                //.Include(c => c.IdFacturaNavigation.IdProveedorNavigation)
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

                var dataPdf = new List<CuentasPagarVM>();

                foreach (var item in modelo)
                {
                    if (item.Status.Equals("En Proceso"))
                    {
                        var itemLibro = _context.LibroCompras.FirstOrDefault(x => x.IdFactura == item.IdFactura);
                        var proveedor = _context.Proveedors.FirstOrDefault(x => x.IdProveedor == item.IdFacturaNavigation.IdProveedor);
                        dataPdf.Add(new CuentasPagarVM()
                        {
                            Condominio = condominio != null ? condominio.Nombre : "",
                            Proveedor = proveedor != null ? proveedor.Nombre : "",
                            NumFactura = item.IdFacturaNavigation.NumFactura.ToString(),
                            BaseImponible = item.IdFacturaNavigation.Subtotal,
                            MontoTotal = item.IdFacturaNavigation.MontoTotal,
                            Iva = item.IdFacturaNavigation.Iva,
                            RetIva = itemLibro != null ? itemLibro.RetIva : 0,
                            RetIslr = itemLibro != null ? itemLibro.RetIslr : 0,
                            TotalPagar = item.IdFacturaNavigation.MontoTotal - (itemLibro != null ? itemLibro.RetIva : 0) - (itemLibro != null ? itemLibro.RetIslr : 0)
                        });
                    }
                }
                
                TempData.Keep();

                var data = _servicesPdf.CuentasPagarPDF(dataPdf);
                var base64 = Convert.ToBase64String(data);
                return Content(base64, "application/pdf");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generando PDF: {e.Message}");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content($"{{ \"error\": \"Error generando el PDF\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
            }
        }

        [HttpPost]
        public ContentResult CuentasPagarExcel([FromBody] IEnumerable<CuentasPagar> modelo)
        {
            try
            {
                var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var condominio = _context.Condominios.Find(IdCondominio);

                var dataExcel = new List<CuentasPagarVM>();

                foreach (var item in modelo)
                {
                    if (item.Status.Equals("En Proceso"))
                    {
                        var itemLibro = _context.LibroCompras.FirstOrDefault(x => x.IdFactura == item.IdFactura);
                        var proveedor = _context.Proveedors.FirstOrDefault(x => x.IdProveedor == item.IdFacturaNavigation.IdProveedor);

                        dataExcel.Add(new CuentasPagarVM()
                        {
                            Condominio = condominio != null ? condominio.Nombre : "",
                            Proveedor = proveedor != null ? proveedor.Nombre : "",
                            NumFactura = item.IdFacturaNavigation.NumFactura.ToString(),
                            BaseImponible = item.IdFacturaNavigation.Subtotal,
                            Iva = item.IdFacturaNavigation.Iva,
                            MontoTotal = item.IdFacturaNavigation.MontoTotal,
                            RetIva = itemLibro != null ? itemLibro.RetIva : 0,
                            RetIslr = itemLibro != null ? itemLibro.RetIslr : 0,
                            TotalPagar = item.IdFacturaNavigation.MontoTotal - (itemLibro != null ? itemLibro.RetIva : 0) - (itemLibro != null ? itemLibro.RetIslr : 0)
                        });
                    }
                }

                TempData.Keep();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("CuentasPagar");
                    var currentRow = 1;

                    // Encabezados
                    //worksheet.Cell(currentRow, 1).Value = "Condominio";
                    worksheet.Cell(currentRow, 1).Value = "Proveedor";
                    worksheet.Cell(currentRow, 2).Value = "NumFactura";
                    worksheet.Cell(currentRow, 3).Value = "BaseImponible";
                    worksheet.Cell(currentRow, 4).Value = "Iva";
                    worksheet.Cell(currentRow, 5).Value = "MontoTotal";
                    worksheet.Cell(currentRow, 6).Value = "RetIva";
                    worksheet.Cell(currentRow, 7).Value = "RetIslr";
                    worksheet.Cell(currentRow, 8).Value = "TotalPagar";

                    // Datos
                    foreach (var item in dataExcel)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = item.Proveedor;
                        worksheet.Cell(currentRow, 2).Value = item.NumFactura;
                        worksheet.Cell(currentRow, 3).Value = item.BaseImponible;
                        worksheet.Cell(currentRow, 4).Value = item.Iva;
                        worksheet.Cell(currentRow, 5).Value = item.MontoTotal;
                        worksheet.Cell(currentRow, 6).Value = item.RetIva;
                        worksheet.Cell(currentRow, 7).Value = item.RetIslr;
                        worksheet.Cell(currentRow, 8).Value = item.TotalPagar;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        var base64 = Convert.ToBase64String(content);
                        return Content(base64, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error generando Excel: {e.Message}");
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content($"{{ \"error\": \"Error generando el Excel\", \"message\": \"{e.Message}\", \"innerException\": \"{e.InnerException?.Message}\" }}");
            }
        }
    }
}
