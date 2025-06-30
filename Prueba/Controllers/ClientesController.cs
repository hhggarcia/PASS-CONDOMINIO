using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Prueba.Context;
using Prueba.Models;
using Prueba.Services;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IPdfReportesServices _servicesPDF;
        private readonly NuevaAppContext _context;

        public ClientesController(IPdfReportesServices servicePdf,
            NuevaAppContext context)
        {
            _servicesPDF = servicePdf;
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var nuevaAppContext = _context.Clientes
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdRetencionIslrNavigation)
                .Include(c => c.IdRetencionIvaNavigation)
                .Where(c => c.IdCondominio == IdCondominio);

            return View(await nuevaAppContext.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdRetencionIslrNavigation)
                .Include(c => c.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public async Task<IActionResult> Create()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            ViewData["IdCondominio"] = new SelectList(_context.Condominios.Where(c => c.IdCondominio == IdCondominio), "IdCondominio", "Nombre");
            
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion");

            var selectIslrs = await(from c in _context.Islrs
                                    where c.Tarifa > 0
                                    select new
                                    {
                                        DataValue = c.Id,
                                        DataText = c.Concepto
                                        + ((c.Pjuridica) ? " PJ" : "")
                                        + ((c.Pnatural) ? " PN" : "")
                                        + ((c.Domiciliada) ? " Domiciliado" : "")
                                        + ((c.NoDomiciliada) ? " No Domiciliado" : "")
                                        + ((c.Residenciada) ? " Residenciada" : "")
                                        + ((c.NoResidenciada) ? " No Residenciada" : "")
                                        + " " + c.Tarifa + "%"

                                    }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText");

            TempData.Keep();

            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,IdCondominio,Nombre,Direccion,Telefono,Rif,Email,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Cliente cliente, bool checkIslr, bool checkIva)
        {
            ModelState.Remove(nameof(cliente.IdCondominioNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIvaNavigation));
            if (ModelState.IsValid)
            {
                if (checkIslr)
                {

                    cliente.IdRetencionIslr = null;                  

                }

                if (checkIva)
                {

                    cliente.IdRetencionIva = null;

                }

                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cliente.IdCondominio);
            
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", cliente.IdRetencionIva);

            var selectIslrs = await (from c in _context.Islrs
                                     where c.Tarifa > 0
                                     select new
                                     {
                                         DataValue = c.Id,
                                         DataText = c.Concepto
                                         + ((c.Pjuridica) ? " PJ" : "")
                                         + ((c.Pnatural) ? " PN" : "")
                                         + ((c.Domiciliada) ? " Domiciliado" : "")
                                         + ((c.NoDomiciliada) ? " No Domiciliado" : "")
                                         + ((c.Residenciada) ? " Residenciada" : "")
                                         + ((c.NoResidenciada) ? " No Residenciada" : "")
                                         + " " + c.Tarifa + "%"

                                     }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText");

            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cliente.IdCondominio);
            //ViewData["IdRetencionIslr"] = new SelectList(_context.Islrs, "Id", "Concepto", cliente.IdRetencionIslr);
            var selectIslrs = await (from c in _context.Islrs
                                     where c.Tarifa > 0
                                     select new
                                     {
                                         DataValue = c.Id,
                                         DataText = c.Concepto
                                         + ((c.Pjuridica) ? " PJ" : "")
                                         + ((c.Pnatural) ? " PN" : "")
                                         + ((c.Domiciliada) ? " Domiciliado" : "")
                                         + ((c.NoDomiciliada) ? " No Domiciliado" : "")
                                         + ((c.Residenciada) ? " Residenciada" : "")
                                         + ((c.NoResidenciada) ? " No Residenciada" : "")
                                         + " " + c.Tarifa + "%"

                                     }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText");
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", cliente.IdRetencionIva);

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,IdCondominio,Nombre,Direccion,Telefono,Rif,Email,IdRetencionIslr,IdRetencionIva,Saldo,Representante,ContribuyenteEspecial")] Cliente cliente, bool checkIslr, bool checkIva)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(cliente.IdCondominioNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIslrNavigation));
            ModelState.Remove(nameof(cliente.IdRetencionIvaNavigation));

            if (ModelState.IsValid)
            {
                try
                {
                    if (checkIslr)
                    {

                        cliente.IdRetencionIslr = null;

                    }

                    if (checkIva)
                    {

                        cliente.IdRetencionIva = null;

                    }
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.IdCliente))
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
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "Nombre", cliente.IdCondominio);
            ViewData["IdRetencionIva"] = new SelectList(_context.Ivas, "Id", "Descripcion", cliente.IdRetencionIva);

            var selectIslrs = await (from c in _context.Islrs
                                     where c.Tarifa > 0
                                     select new
                                     {
                                         DataValue = c.Id,
                                         DataText = c.Concepto
                                         + ((c.Pjuridica) ? " PJ" : "")
                                         + ((c.Pnatural) ? " PN" : "")
                                         + ((c.Domiciliada) ? " Domiciliado" : "")
                                         + ((c.NoDomiciliada) ? " No Domiciliado" : "")
                                         + ((c.Residenciada) ? " Residenciada" : "")
                                         + ((c.NoResidenciada) ? " No Residenciada" : "")
                                         + " " + c.Tarifa + "%"

                                     }).ToListAsync();

            ViewData["IdRetencionIslr"] = new SelectList(selectIslrs, "DataValue", "DataText");

            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdRetencionIslrNavigation)
                .Include(c => c.IdRetencionIvaNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ClientesFacturasPendientes()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var model = _context.Clientes.Where(c => c.IdCondominio == IdCondominio).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DetalleFacturasPendientes(int id)
        {
            var model = new ClienteFacturasPendientesVM();
            var pagoFacturas = new Dictionary<string, PagoRecibido>();
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente != null)
            {
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

                model.Cliente = cliente;
                model.FacturasPendientes = facturas;
                model.PagosFacturas = pagoFacturas;
            }

            return View(model);
        }
        public async Task<IActionResult> PdfFacturasPendientes(int id)
        {
            var model = new ClienteFacturasPendientesVM();
            var pagoFacturas = new Dictionary<string, PagoRecibido>();
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente != null)
            {
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

                model.Cliente = cliente;
                model.FacturasPendientes = facturas;
                model.PagosFacturas = pagoFacturas;

                var data = _servicesPDF.DetalleFacturasPendientes(model);
                Stream stream = new MemoryStream(data);
                return File(stream, "application/pdf", "FacturasPendientes_" + cliente.Nombre + "_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }

            return View();
        }

        public async Task<IActionResult> ExcelFacturasPendientes(int id)
        {
            try
            {
                var model = new ClienteFacturasPendientesVM();
                var pagoFacturas = new Dictionary<string, PagoRecibido>();
                var cliente = await _context.Clientes.FindAsync(id);

                if (cliente != null)
                {
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

                    model.Cliente = cliente;
                    model.FacturasPendientes = facturas;
                    model.PagosFacturas = pagoFacturas;
                }

                var data = new List<FacturasPendientesExcelVM>();

                foreach (var factura in model.FacturasPendientes)
                {
                    var auxMontoTotalFactura = factura.MontoTotal;
                    var pagoRecibido = new PagoRecibido();
                    var totalCobrar = factura.MontoTotal -
                                (factura.LibroVenta.Any() ? factura.LibroVenta.First().RetIva : 0) -
                                (factura.LibroVenta.Any() ? factura.LibroVenta.First().RetIslr : 0);

                    if (model.Cliente.IdRetencionIva != null)
                    {
                        auxMontoTotalFactura -= factura.CompRetIvaClientes.Any() ? factura.CompRetIvaClientes.First().IvaRetenido : 0;
                    }

                    if (model.Cliente.IdRetencionIslr != null)
                    {
                        auxMontoTotalFactura -= factura.ComprobanteRetencionClientes.Any() ? factura.ComprobanteRetencionClientes.First().ValorRetencion : 0;
                    }

                    if (model.PagosFacturas.ContainsKey(factura.NumFactura.ToString()))
                    {
                        pagoRecibido = model.PagosFacturas[factura.NumFactura.ToString()];
                    }

                    data.Add(new FacturasPendientesExcelVM()
                    {
                        Factura = factura.NumFactura.ToString(),
                        Base = Math.Round(factura.SubTotal, 2),
                        Iva = Math.Round(factura.Iva, 2),
                        MontoTotal = Math.Round(factura.MontoTotal, 2),
                        RetIva = (factura.LibroVenta.Any() ? factura.LibroVenta.First().RetIva : 0),
                        RetIslr = (factura.LibroVenta.Any() ? factura.LibroVenta.First().RetIslr : 0),
                        TotalPagar = totalCobrar,
                        PagoRecibido = pagoRecibido != null ? pagoRecibido.Monto : 0,
                        PendientePago = auxMontoTotalFactura - (pagoRecibido != null ? pagoRecibido.Monto : 0)
                    });
                }

                DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data), (typeof(DataTable)));

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
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FacturasPendientes_" + model.Cliente.Nombre + DateTime.Today.ToString("dd/MM/yyyy") + ".xls");
                }
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
        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}
