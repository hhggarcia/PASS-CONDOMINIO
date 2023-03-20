using Prueba.Models;

namespace Prueba.ViewModels
{
    public class RelacionDeGastosVM
    {
        // LISTA DE LOS GASTOS REGISTRADOS EN DIARIO DEL MES
        public IList<LdiarioGlobal>? GastosDiario { get; set; }

        public IList<CodigoCuentasGlobal> CCGastos { get; set; } = new List<CodigoCuentasGlobal>();
        public IList<SubCuenta>? SubcuentasGastos { get; set; }
        // LISTA DE PROVISIONES
        public IList<Provision>? Provisiones { get; set; }

        public IList<CodigoCuentasGlobal> CCProvisiones { get; set; } = new List<CodigoCuentasGlobal>();
        public IList<SubCuenta>? SubCuentasProvisiones { get; set; }
        // SUBCUENTAS FONDOS
        public IList<SubCuenta>? SubCuentasFondos { get; set; }
        public IList<CodigoCuentasGlobal> CCFondos { get; set; } = new List<CodigoCuentasGlobal>();

        // LISTA DE FONDOS
        public IList<Fondo>? Fondos { get; set; }
        // SUB TOTAL
        public decimal SubTotal { get; set; }
        // TOTAL
        public decimal Total { get; set; }
        // MES
        public DateTime Fecha { get; set; }
        // INFO DE CONDOMINIO
        public Condominio? Condominio { get; set; }
        //public IList<ReferenciaDolar>? ReferenciasDolar { get; set; }

    }
}
