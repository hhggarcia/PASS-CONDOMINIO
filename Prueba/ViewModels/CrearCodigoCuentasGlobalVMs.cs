using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CrearCodigoCuentasGlobalVMs : CodigoCuentasGlobal
    {
        public CrearCodigoCuentasGlobalVMs()
        {
            Clases = new List<SelectListItem>();
            Grupos = new List<SelectListItem>();
            Cuentas = new List<SelectListItem>();
            SubCuenta = new List<SelectListItem>();
        }
        public List<SelectListItem>? Clases { get; set; }
        public List<SelectListItem>? Grupos { get; set; }
        public List<SelectListItem>? Cuentas { get; set; }
        public List<SelectListItem>? SubCuenta { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public int Codigo { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal IdCondominio { get; set; }
        public short IdClase { get; set; }
        public short IdGrupo { get; set; }
        public short IdCuenta { get; set; }
    }
}
