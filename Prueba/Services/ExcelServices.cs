using Newtonsoft.Json;
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

                foreach (var propiedad in modelo.Propiedades)
                {
                    var propietario = modelo.Propietarios.First(c => c.Id == propiedad.IdUsuario);
                    var recibos = modelo.Recibos.Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToList();

                    data.Add(new DeudoresDiarioVM()
                    {
                        Codigo = propiedad.Codigo,
                        Propietario = propietario.FirstName,
                        CantRecibos = recibos.Count,
                        AcumDeuda = propiedad.Deuda,
                        AcumMora = propiedad.MontoIntereses,
                        AcumIndexacion = propiedad.MontoMulta != null ? (decimal)propiedad.MontoMulta : 0,
                        Credito = propiedad.Creditos != null ? (decimal)propiedad.Creditos : 0,
                        Saldo = propiedad.Saldo,
                        Total = propiedad.Deuda + propiedad.MontoIntereses + (decimal)propiedad.MontoMulta + propiedad.Saldo - (decimal)propiedad.Creditos,
                    });
                }

                DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data), (typeof(DataTable)));
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
