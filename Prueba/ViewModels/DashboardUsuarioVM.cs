using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class DashboardUsuarioVM
    {
        public IList<Propiedad> Propiedades { get; set; } = new List<Propiedad>();

        public AspNetUser? Usuario { get; set; }
        public IList<SelectListItem>? SubCuentasBancos { get; set; }
        public IList<SelectListItem>? SubCuentasCaja { get; set; }
        public int IdCodigoCuentaBanco { get; set; }
        public int IdCodigoCuentaCaja { get; set; }
        public int NumReferencia { get; set; }
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public byte[]? Imagen { get; set; }
        public FormaPago Pagoforma { get; set; }

    }
}
