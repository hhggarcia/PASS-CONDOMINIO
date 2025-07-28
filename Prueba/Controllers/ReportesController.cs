using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Prueba.Context;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.ViewModels;
using SQLitePCL;
using System.Data;

namespace Prueba.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly IExcelServices _excelServices;
        private readonly IReportesRepository _repoReportes;
        private readonly IPdfReportesServices _servicesPDF;
        private readonly NuevaAppContext _context;
        private readonly decimal _tasaActual;

        public ReportesController(IMonedaRepository repoMoneda,
            IExcelServices excelServices,
            IReportesRepository repoReportes,
            IPdfReportesServices servicesPDF,
            NuevaAppContext context)
        {
            _repoMoneda = repoMoneda;
            _excelServices = excelServices;
            _repoReportes = repoReportes;
            _servicesPDF = servicesPDF;
            _context = context;
            _tasaActual = _repoMoneda.TasaActualMonedaPrincipal();
        }
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard", "Adminsitrador");
        }

        public async Task<IActionResult> EstadoCuentas()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var condominio = await _context.Condominios.FindAsync(IdCondominio);

            if (condominio != null)
            {
                var propiedades = await _context.Propiedads.Where(c => c.IdCondominio == IdCondominio).ToListAsync();
                var modelo = new List<EstadoCuentasVM>();

                if (propiedades != null && propiedades.Any())
                {
                    foreach (var propiedad in propiedades)
                    {
                        var usuario = await _context.AspNetUsers.FirstAsync(c => c.Id == propiedad.IdUsuario);
                        var recibos = await _context.ReciboCobros.
                            Where(c => c.IdPropiedad == propiedad.IdPropiedad && !c.Pagado)
                            .OrderBy(c => c.Fecha)
                            .ToListAsync();

                        modelo.Add(new EstadoCuentasVM()
                        {
                            Condominio = condominio,
                            Propiedad = propiedad,
                            User = usuario,
                            ReciboCobro = recibos
                        });
                    }

                    TempData.Keep();
                    var data = _servicesPDF.EstadoCuentas(modelo);
                    Stream stream = new MemoryStream(data);
                    return File(stream, "application/pdf", "EstadoCuentasOficina_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
                }

            }

            return RedirectToAction("Dashboard", "Administrator");
        }

        [HttpGet]
        public async Task<IActionResult> DeudoresPDF()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = await _repoReportes.LoadDataDeudores(idCondominio);

            if (modelo.Propiedades != null && modelo.Propiedades.Any()
                && modelo.Propietarios != null && modelo.Propietarios.Any()
                && modelo.Recibos != null && modelo.Recibos.Any())
            {
                var data = await _servicesPDF.Deudores(modelo, idCondominio);
                Stream stream = new MemoryStream(data);
                TempData.Keep();
                return File(stream, "application/pdf", "Deudores_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }

            TempData.Keep();

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> DeudoresResumenPDF()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var modelo = await _repoReportes.LoadDataDeudores(idCondominio);

            if (modelo.Propiedades != null && modelo.Propiedades.Any()
                && modelo.Propietarios != null && modelo.Propietarios.Any()
                && modelo.Recibos != null && modelo.Recibos.Any())
            {
                var data = await _servicesPDF.DeudoresResumen(modelo, idCondominio);
                Stream stream = new MemoryStream(data);
                TempData.Keep();
                return File(stream, "application/pdf", "Deudores_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
            }

            TempData.Keep();

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> DeudoresExcel()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await _repoReportes.LoadDataDeudores(idCondominio);

                if (modelo.Propiedades != null && modelo.Propiedades.Any()
                    && modelo.Propietarios != null && modelo.Propietarios.Any()
                    && modelo.Recibos != null && modelo.Recibos.Any())
                {
                    var dataRef = new List<DeudoresRefVM>();

                    foreach (var propiedad in modelo.Propiedades)
                    {                        
                        decimal totalPagarRef = 0;

                        var propietario = modelo.Propietarios.First(c => c.Id == propiedad.IdUsuario);
                        var recibos = modelo.Recibos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();

                        foreach (var item in recibos)
                        {
                            decimal acumMoraRef = 0;
                            decimal acumIndexRef = 0;
                            decimal totalAbono = 0;
                            decimal totalMontoRef = 0;

                            acumMoraRef += item.MontoMora / item.ValorDolar;
                            acumIndexRef += item.MontoIndexacion / item.ValorDolar;
                            totalAbono += item.Abonado / item.ValorDolar;
                            totalMontoRef += item.ReciboActual ? item.MontoRef : (item.MontoRef + acumMoraRef + acumIndexRef - totalAbono);
                            totalPagarRef += totalMontoRef;
                        }

                        dataRef.Add(new DeudoresRefVM()
                        {
                            Oficina = propiedad.Codigo,
                            Propietario = propietario.FirstName,
                            CantRecibos = recibos.Count,
                            TotalRef = totalPagarRef,
                            Total = totalPagarRef * _tasaActual,
                        });
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
                        return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DeudoresDia_" + DateTime.Today.ToString("dd/MM/yyyy") + ".xls");
                    }

                }

                var modeloError = new ErrorViewModel()
                {
                    RequestId = "No hay datos!"
                };

                return View("Error", modeloError);

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

        public IActionResult ExcelTest()
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet excelSheet = workbook.CreateSheet("Demo");
            IRow row = excelSheet.CreateRow(0);

            row.CreateCell(0).SetCellValue("ID");
            row.CreateCell(1).SetCellValue("Name");
            row.CreateCell(2).SetCellValue("Age");

            row = excelSheet.CreateRow(1);
            row.CreateCell(0).SetCellValue(1);
            row.CreateCell(1).SetCellValue("Kane Williamson");
            row.CreateCell(2).SetCellValue(29);

            row = excelSheet.CreateRow(2);
            row.CreateCell(0).SetCellValue(2);
            row.CreateCell(1).SetCellValue("Martin Guptil");
            row.CreateCell(2).SetCellValue(33);

            row = excelSheet.CreateRow(3);
            row.CreateCell(0).SetCellValue(3);
            row.CreateCell(1).SetCellValue("Colin Munro");
            row.CreateCell(2).SetCellValue(23);

            var memory = new MemoryStream();
            workbook.Write(memory);
            memory.Seek(0, SeekOrigin.Begin);

            // Devolver el archivo Excel como una descarga al cliente
            return File(memory.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "demo.xls");
        }

        public async Task<IActionResult> HistoricoRecibos()
        {
            var IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var condominio = await _context.Condominios.FindAsync(IdCondominio);

            if (condominio != null)
            {
                var propiedades = await _context.Propiedads.Where(c => c.IdCondominio == IdCondominio).ToListAsync();
                var modelo = new List<EstadoCuentasVM>();

                if (propiedades != null && propiedades.Any())
                {
                    foreach (var propiedad in propiedades)
                    {
                        var usuario = await _context.AspNetUsers.FirstAsync(c => c.Id == propiedad.IdUsuario);
                        var recibos = await _context.ReciboCobros.
                            Where(c => c.IdPropiedad == propiedad.IdPropiedad)
                            .OrderBy(c => c.Fecha)
                            .ToListAsync();

                        modelo.Add(new EstadoCuentasVM()
                        {
                            Condominio = condominio,
                            Propiedad = propiedad,
                            User = usuario,
                            ReciboCobro = recibos
                        });
                    }

                    TempData.Keep();
                    var data = _servicesPDF.ReporteHistorico(modelo);
                    Stream stream = new MemoryStream(data);
                    return File(stream, "application/pdf", "HistoricoOficinas_" + DateTime.Today.ToString("dd/MM/yyyy") + ".pdf");
                }

            }

            return RedirectToAction("Dashboard", "Administrator");
        }

    }
}
