using Prueba.Models;

namespace Prueba.ViewModels
{
    public class TransaccionVM
    {
        public Condominio? Condominio { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal TotalIngresoIndividual { get; set; }
        public decimal TotalEgresoIndividual { get; set; }
        public decimal TotalIndividual { get; set; }
        public decimal TotalGeneral { get; set; }
        public IList<Transaccion>? Transaccions { get; set; } = new List<Transaccion>();
        public IList<Transaccion>? TransaccionesIndividuales { get; set; } = new List<Transaccion>();
        public IList<GrupoGasto> GruposGastos { get; set; } = new List<GrupoGasto>();

        // LISTA DE PROVISIONES
        public IList<Provision>? Provisiones { get; set; } = new List<Provision>();
        public IList<CodigoCuentasGlobal> CCProvisiones { get; set; } = new List<CodigoCuentasGlobal>();
        public IList<SubCuenta>? SubCuentasProvisiones { get; set; } = new List<SubCuenta>();

        // LISTA DE FONDOS
        public IList<Fondo>? Fondos { get; set; } = new List<Fondo>();
        public IList<SubCuenta>? SubCuentasFondos { get; set; } = new List<SubCuenta>();
        public IList<CodigoCuentasGlobal> CCFondos { get; set; } = new List<CodigoCuentasGlobal>();
    }
}
