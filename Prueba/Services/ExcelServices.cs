using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Prueba.Context;
using Prueba.ViewModels;
using SQLitePCL;
using System.Data;

namespace Prueba.Services
{
    public interface IExcelServices
    {
        string ExcelDeudoresDia(RecibosCreadosVM modelo);
    }
    public class ExcelServices : IExcelServices
    {
        private readonly NuevaAppContext _context;

        public ExcelServices(NuevaAppContext context)
        {
            _context = context;
        }

        public string ExcelDeudoresDia(RecibosCreadosVM modelo)
        {
            try
            {
                var data = new List<DeudoresDiarioVM>();
                var dataRef = new List<DeudoresRefVM>();

                foreach (var propiedad in modelo.Propiedades)
                {
                    decimal totalMontoRef = 0;
                    decimal acumMoraRef = 0;
                    decimal acumIndexRef = 0;
                    decimal totalPagarRef = 0;

                    var propietario = modelo.Propietarios.First(c => c.Id == propiedad.IdUsuario);
                    var recibos = modelo.Recibos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();

                    foreach (var item in recibos)
                    {
                        totalMontoRef += item.MontoRef;
                        acumMoraRef += item.MontoMora / item.ValorDolar;
                        acumIndexRef += item.MontoIndexacion / item.ValorDolar;
                        totalPagarRef += totalMontoRef + acumMoraRef + acumIndexRef;
                    }

                    dataRef.Add(new DeudoresRefVM()
                    {
                        Oficina = propiedad.Codigo,
                        Propietario = propietario.FirstName,
                        CantRecibos = recibos.Count,
                        TotalRef = totalPagarRef,
                        Total = 0
                    });
                    //data.Add(new DeudoresDiarioVM()
                    //{
                    //    Codigo = propiedad.Codigo,
                    //    Propietario = propietario.FirstName,
                    //    CantRecibos = recibos.Count,
                    //    AcumDeuda = propiedad.Deuda,
                    //    AcumMora = propiedad.MontoIntereses,
                    //    AcumIndexacion = propiedad.MontoMulta != null ? (decimal)propiedad.MontoMulta : 0,
                    //    Credito = propiedad.Creditos != null ? (decimal)propiedad.Creditos : 0,
                    //    Saldo = propiedad.Saldo,
                    //    Total = propiedad.Deuda + propiedad.MontoIntereses + (decimal)propiedad.MontoMulta + propiedad.Saldo - (decimal)propiedad.Creditos,
                    //});
                }

                DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataRef), (typeof(DataTable)));
                var memoryStream = new MemoryStream();

                using (var fs = new FileStream("DeudoresDia_" + DateTime.Today.ToString("dd/MM/yyyy") + ".xlsx", FileMode.Create, FileAccess.Write))
                {
                    IWorkbook workbook = new XSSFWorkbook();
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
                    workbook.Write(fs);
                }

                return "exito";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }            
        }

        
    }
}
