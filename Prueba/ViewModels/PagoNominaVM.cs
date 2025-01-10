using Prueba.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Prueba.ViewModels
{
    public class PagoNominaVM
    {
        public int IdCondominio { get; set; }
        public int IdEmpleado { get; set; }
        public int IdSubcuenta { get; set; }
        public int NumReferencia { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        public string? Descripcion { get; set; }
        public string? Concepto { get; set; }
        public FormaPago Pagoforma { get; set; }
        public bool deducciones { get; set; }
        public bool percepciones { get; set; }
        public bool Bonos { get; set; }
        public bool Anticipos { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public decimal ValorDolar { get; set; }
        public decimal MontoRef { get; set; }
        public string SimboloMoneda { get; set; } = null!;
        public string SimboloRef { get; set; } = null!;
        public IList<SelectListItem>? Empleados { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem>? SubCuentasGastos { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem>? SubCuentasBancos { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem>? SubCuentasCaja { get; set; } = new List<SelectListItem>();
        //public IList<SelectListItem>? ReferenciasDolar { get; set; }
        public IList<SelectListItem>? ListDeducciones { get; set; } = new List<SelectListItem>();
        public IList<int>? ListDeduccionesIDs { get; set; } = new List<int>();
        public IList<SelectListItem>? ListPercepciones { get; set; } = new List<SelectListItem>();
        public IList<int>? ListPercepcionesIDs { get; set; } = new List<int>();
        public IList<SelectListItem>? ListBonos { get; set; } = new List<SelectListItem>(); 
        public IList<int>? ListBonosIDs { get; set; } = new List<int>();

        public IList<SelectListItem>? ListAnticipos { get; set; } = new List<SelectListItem>();
        public IList<int>? ListAnticiposIDs { get; set; } = new List<int>();
    }
}
