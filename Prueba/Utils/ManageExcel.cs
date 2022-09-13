using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Prueba.Models;
using EFCore.BulkExtensions;
using Prueba.Areas.Identity.Data;

namespace Prueba.Utils
{
    public interface IManageExcel
    {
        List<PuestoE> ExcelPuestos_Est(IFormFile archivoExcel);
        List<Usuario> ExcelUsuarios(IFormFile archivoExcel);
    }
    public class ManageExcel: IManageExcel
    {

        /*Crear metodo para leer excel y retornar lista de MODELO de Usuarios a los cuales asignar rol de usuarios*/
        public List<Usuario> ExcelUsuarios(IFormFile archivoExcel)
        {
            //falta validar si es null
            Stream stream = archivoExcel.OpenReadStream();

            IWorkbook MiExcel = null;

            if (Path.GetExtension(archivoExcel.FileName) == ".xlsx")
            {
                MiExcel = new XSSFWorkbook(stream);
            }
            else
            {
                MiExcel = new HSSFWorkbook(stream);
            }

            ISheet HojaExcel = MiExcel.GetSheetAt(0);

            int cantidadFilas = HojaExcel.LastRowNum;

            //Lista de usuarios
            List<Usuario> listaUsuarios = new List<Usuario>();

            //FOR - para recorrer la hoja y extraer los usuarios
            for (int i = 1; i <= cantidadFilas; i++)
            {

                IRow fila = HojaExcel.GetRow(i);

                listaUsuarios.Add(new Usuario
                {
                    FirstName = fila.GetCell(0).ToString(),
                    LastName = fila.GetCell(1).ToString(),
                    Email = fila.GetCell(2).ToString()
                    //generar claves alaetorias
                });
            }

            return listaUsuarios;
        }

        public List<PuestoE> ExcelPuestos_Est(IFormFile archivoExcel)
        {
            //falta validar si es null
            Stream stream = archivoExcel.OpenReadStream();

            IWorkbook MiExcel = null;

            if (Path.GetExtension(archivoExcel.FileName) == ".xlsx")
            {
                MiExcel = new XSSFWorkbook(stream);
            }
            else
            {
                MiExcel = new HSSFWorkbook(stream);
            }

            ISheet HojaExcel = MiExcel.GetSheetAt(0);

            int cantidadFilas = HojaExcel.LastRowNum;

            //Lista de usuarios
            List<PuestoE> listaPuestos_Est = new List<PuestoE>();

            //FOR - para recorrer la hoja y extraer los usuarios
            for (int i = 1; i <= cantidadFilas; i++)
            {

                IRow fila = HojaExcel.GetRow(i);

                listaPuestos_Est.Add(new PuestoE
                {
                    Codigo = fila.GetCell(0).ToString(),
                    Alicuota = fila.GetCell(1).ToString()
                    //generar claves alaetorias
                });
            }

            return listaPuestos_Est;
        }

        public void RandomPasswords()
        {

        }

        /*Metodo para generar excel */
    }
}
