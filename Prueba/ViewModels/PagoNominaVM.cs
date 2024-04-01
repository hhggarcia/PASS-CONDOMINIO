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
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public decimal ValorDolar { get; set; }
        public decimal MontoRef { get; set; }
        public string SimboloMoneda { get; set; } = null!;
        public string SimboloRef { get; set; } = null!;
        public IList<SelectListItem>? Empleados { get; set; }
        public IList<SelectListItem>? SubCuentasGastos { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public IList<SelectListItem>? ReferenciasDolar { get; set; }
    }
}
