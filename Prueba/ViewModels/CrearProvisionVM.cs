using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.ViewModels
{
    public class CrearProvisionVM
    {
        public IList<SelectListItem>? Provisiones { get; set; }
        public int IdcodCuenta { get; set; }
        public IList<SelectListItem>? Gastos { get; set; }
        public int IdGasto { get; set; }
        public int Monto { get; set; }
        public string? Concepto { get; set; }

    }
}
