using Prueba.Models;

namespace Prueba.ViewModels
{
    public class ItemConciliacionVM
    {
        public CodigoCuentasGlobal CodigoCuenta { get; set; } = null!;
        public SubCuenta SubCuenta { get; set; } = null!;
        public List<LdiarioGlobal> Asientos { get; set; } = new List<LdiarioGlobal>();
        public Conciliacion ConciliacionAnterior { get; set; } = null!;
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-1);
        public DateTime FechaFin { get; set; } = DateTime.Today;
        public decimal TotalIngreso { get; set; }
        public decimal TotalEgreso { get; set; }
        public decimal SaldoBanco { get; set; }
    }
}
