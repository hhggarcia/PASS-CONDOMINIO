using Prueba.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.ViewModels
{
    public class ItemConciliacionVM
    {
        public CodigoCuentasGlobal CodigoCuenta { get; set; } = null!;
        public int IdCodigoCuenta { get; set; }
        public SubCuenta SubCuenta { get; set; } = null!;
        public IList<LdiarioGlobal> Asientos { get; set; } = new List<LdiarioGlobal>();
        public Conciliacion ConciliacionAnterior { get; set; } = null!;
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-1);
        public DateTime FechaFin { get; set; } = DateTime.Today;
        public decimal TotalIngreso { get; set; }
        public decimal TotalEgreso { get; set; }
        public decimal SaldoBanco { get; set; }
        public IList<PagosConciliacionVM> Pagos { get; set; } = new List<PagosConciliacionVM>();
        public IList<SelectListItem> PagosIds { get; set; } = new List<SelectListItem>();
    }
}
