using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
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
        private readonly IMonedaRepository _repoMoneda;
        private readonly IPdfReportesServices _servicesPdf;
        private readonly IFiltroFechaRepository _reposFiltroFecha;
        private readonly NuevaAppContext _context;
        private readonly decimal _tasaActual;

        public CuentasCobrarController(IMonedaRepository repoMoneda,
            IPdfReportesServices pdfReportesServices,
            IFiltroFechaRepository filtroFechaRepository,
            NuevaAppContext context)
        {
            _repoMoneda = repoMoneda;
            _servicesPdf = pdfReportesServices;
            _reposFiltroFecha = filtroFechaRepository;
            _context = context;
            _tasaActual = _repoMoneda.TasaActualMonedaPrincipal();
        }

        // GET: CuentasCobrar
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.CuentasCobrars.OrderByDescending(c => c.Status)
                //.Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdFacturaNavigation)
                    .ThenInclude(c => c.IdClienteNavigation)                
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
                var dataPdf = new List<CuentasCobrarVM>();
                foreach (var item in modelo)
                {
                    if (item.Status.Equals("En Proceso") 
                        && !item.IdFacturaNavigation.Anulada
                        && item.IdFacturaNavigation.EnProceso)
                    {
                        var itemLibro = _context.LibroVentas.FirstOrDefault(x => x.IdFactura == item.IdFactura);
                        var cliente = _context.Clientes.FirstOrDefault(x => x.IdCliente == item.IdFacturaNavigation.IdCliente);
                        dataPdf.Add(new CuentasCobrarVM()
                        {
                            Condominio = condominio != null ? condominio.Nombre : "",
                            Cliente = cliente != null ? cliente.Nombre : "",
                            NumFactura = item.IdFacturaNavigation.NumFactura.ToString(),
                            BaseImponible = item.IdFacturaNavigation.SubTotal,
                            MontoTotal = item.IdFacturaNavigation.MontoTotal,
                            Iva = item.IdFacturaNavigation.Iva,
                            RetIva = itemLibro != null ? itemLibro.RetIva : 0,
                            RetIslr = itemLibro != null ? itemLibro.RetIslr : 0,
                            TotalPagar = item.IdFacturaNavigation.MontoTotal - (itemLibro != null ? itemLibro.RetIva : 0) - (itemLibro != null ? itemLibro.RetIslr : 0)
                        });
                    }
                }
               

                TempData.Keep();

                var data = _servicesPdf.CuentasCobrarPDF(dataPdf);
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
        public ContentResult CuentasCobrarExcel([FromBody] IEnumerable<CuentasCobrar> modelo)
        {
            try
            {
                var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var condominio = _context.Condominios.Find(IdCondominio);
                var dataExcel = new List<CuentasCobrarVM>();
                foreach (var item in modelo)
                {
                    if (item.Status.Equals("En Proceso")
                        && !item.IdFacturaNavigation.Anulada
                        && item.IdFacturaNavigation.EnProceso)
                    {
                        var itemLibro = _context.LibroVentas.FirstOrDefault(x => x.IdFactura == item.IdFactura);
                        var cliente = _context.Clientes.FirstOrDefault(x => x.IdCliente == item.IdFacturaNavigation.IdCliente);

                        dataExcel.Add(new CuentasCobrarVM()
                        {
                            Condominio = condominio != null ? condominio.Nombre : "",
                            Cliente = cliente != null ? cliente.Nombre : "",
                            NumFactura = item.IdFacturaNavigation.NumFactura.ToString(),
                            BaseImponible = item.IdFacturaNavigation.SubTotal,
                            MontoTotal = item.IdFacturaNavigation.MontoTotal,
                            Iva = item.IdFacturaNavigation.Iva,
                            RetIva = itemLibro != null ? itemLibro.RetIva : 0,
                            RetIslr = itemLibro != null ? itemLibro.RetIslr : 0,
                            TotalPagar = item.IdFacturaNavigation.MontoTotal - (itemLibro != null ? itemLibro.RetIva : 0) - (itemLibro != null ? itemLibro.RetIslr : 0)
                        });
                    }
                }

                TempData.Keep();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("CuentasCobrar");
                    var currentRow = 1;

                    // Encabezados
                    //worksheet.Cell(currentRow, 1).Value = "Condominio";
                    worksheet.Cell(currentRow, 1).Value = "Cliente";
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
                        worksheet.Cell(currentRow, 1).Value = item.Cliente;
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

        public async Task<IActionResult> FacturasPendientes()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var model = new List<ClienteFacturasPendientesVM>();
            var clientes = await _context.Clientes.Where(c => c.IdCondominio == IdCondominio).ToListAsync();

            if (clientes.Any())
            {
                foreach (var cliente in clientes)
                {
                    var modelCliente = new ClienteFacturasPendientesVM();
                    var pagoFacturas = new Dictionary<string, PagoRecibido>();

                    var facturas = await _context.FacturaEmitida
                        .Include(c => c.LibroVenta)
                        .Include(c => c.CompRetIvaClientes)
                        .Include(c => c.ComprobanteRetencionClientes)
                        .Include(c => c.PagoFacturaEmitida)
                        .Where(c => c.EnProceso && c.IdCliente == cliente.IdCliente && !c.Anulada)
                        .ToListAsync();

                    foreach (var factura in facturas)
                    {
                        if (factura.PagoFacturaEmitida.Any())
                        {
                            var pagoRecibido = await _context.PagoRecibidos
                            .FirstOrDefaultAsync(c => c.IdPagoRecibido == factura.PagoFacturaEmitida.First().IdPagoRecibido);

                            if (pagoRecibido != null)
                            {
                                pagoFacturas.Add(factura.NumFactura.ToString(), pagoRecibido);
                            }
                        }

                    }

                    modelCliente.Cliente = cliente;
                    modelCliente.FacturasPendientes = facturas;
                    modelCliente.PagosFacturas = pagoFacturas;

                    model.Add(modelCliente);
                }

                var data = _servicesPdf.FacturasPendientes(model);
                Stream stream = new MemoryStream(data);
                TempData.Keep();
                return File(stream, "application/pdf", "FacturasPendientes_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> FacturasPendientesExcel()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            var clientes = await _context.Clientes.Where(c => c.IdCondominio == IdCondominio).ToListAsync();
            var dataRef = new List<CuentasCobrarExcelVM>();

            if (clientes.Any())
            {
                foreach (var cliente in clientes)
                {
                    
                    var retencionesPendientes = "";
                    var retencionesPendientesIva = "";
                    decimal totalPagar = 0;

                    var facturas = await _context.FacturaEmitida
                        .Include(c => c.LibroVenta)
                        .Include(c => c.CompRetIvaClientes)
                        .Include(c => c.ComprobanteRetencionClientes)
                        .Include(c => c.PagoFacturaEmitida)
                        .Where(c => c.EnProceso && c.IdCliente == cliente.IdCliente && !c.Anulada)
                        .ToListAsync();

                    foreach (var factura in facturas)
                    {
                        var totalCobrar = factura.MontoTotal -
                                                (factura.LibroVenta.Any() ? factura.LibroVenta.First().RetIva : 0) -
                                                (factura.LibroVenta.Any() ? factura.LibroVenta.First().RetIslr : 0);

                        if (cliente.IdRetencionIslr != null && !factura.ComprobanteRetencionClientes.Any())
                        {
                            retencionesPendientes = $"{retencionesPendientes} - {factura.NumFactura}";                            
                        }

                        if (cliente.IdRetencionIva != null && !factura.CompRetIvaClientes.Any())
                        {
                            retencionesPendientesIva = $"{retencionesPendientesIva} - {factura.NumFactura}";
                        }

                        totalPagar += (totalCobrar - factura.Abonado);
                    }

                    if (facturas.Count > 0)
                    {
                        dataRef.Add(new CuentasCobrarExcelVM()
                        {
                            Empresa = cliente.Nombre,
                            FacturasPendientes = facturas.Count,
                            RetencionesPendientesIslr = retencionesPendientes,
                            RetencionesPendientesIva = retencionesPendientesIva,
                            TotalPagar = Math.Round(totalPagar, 2),
                            TotalPagarRef = Math.Round(totalPagar / _tasaActual, 2)
                        });
                    }
                    
                }

                DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataRef), (typeof(DataTable)));

                using (var workbook = new HSSFWorkbook())
                {
                    //IWorkbook workbook = new XSSFWorkbook();
                    ISheet excelSheet = workbook.CreateSheet("Sheet1");

                    List<String> columns = new List<string>();
                    IRow row = excelSheet.CreateRow(0);
                    int columnIndex = 0;

                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);
                        row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
                        columnIndex++;
                    }

                    int rowIndex = 1;
                    foreach (DataRow dsrow in table.Rows)
                    {
                        row = excelSheet.CreateRow(rowIndex);
                        int cellIndex = 0;
                        foreach (String col in columns)
                        {
                            row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
                            cellIndex++;
                        }

                        rowIndex++;
                    }

                    var memoryStream = new MemoryStream();
                    workbook.Write(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    TempData.Keep();
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FacturasPendientes_" + DateTime.Today.ToString("dd/MM/yyyy") + ".xls");
                }
            }
            TempData.Keep();
            return RedirectToAction("Index");
        }
    }
}
