using Prueba.Models;
using System.Web.Mvc;

namespace Prueba.ViewModels
{
    public class ItemConciliacionVM
    {
        public CodigoCuentasGlobal CodigoCuenta { get; set; } = null!;
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

        public IList<PagoRecibido> PagosRecibidos { get; set; } = new List<PagoRecibido>();
        public IList<PagoEmitido> PagosEmitidos { get; set; } = new List<PagoEmitido>();

        public IList<SelectListItem> PagosRecibidosIds { get; set; } = new List<SelectListItem>();
        public IList<SelectListItem> PagosEmitidosIds { get; set; } = new List<SelectListItem>();
    }
}
