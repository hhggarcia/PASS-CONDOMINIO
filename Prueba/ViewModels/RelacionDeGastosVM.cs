using Prueba.Models;

namespace Prueba.ViewModels
{
    public class RelacionDeGastosVM
    {
        // LISTA DE LOS GASTOS REGISTRADOS EN DIARIO DEL MES
        public IList<LdiarioGlobal>? GastosDiario { get; set; } = new List<LdiarioGlobal>();

        public IList<CodigoCuentasGlobal> CCGastos { get; set; } = new List<CodigoCuentasGlobal>();
        public IList<SubCuenta>? SubcuentasGastos { get; set; } = new List<SubCuenta>();
        // LISTA DE PROVISIONES
        public IList<Provision>? Provisiones { get; set; } = new List<Provision>();

        public IList<CodigoCuentasGlobal> CCProvisiones { get; set; } = new List<CodigoCuentasGlobal>();
        public IList<SubCuenta>? SubCuentasProvisiones { get; set; } = new List<SubCuenta>();
        
        // SUBCUENTAS FONDOS
        // LISTA DE FONDOS
        public IList<Fondo>? Fondos { get; set; } = new List<Fondo>();
        public IList<SubCuenta>? SubCuentasFondos { get; set; } = new List<SubCuenta>();
        public IList<CodigoCuentasGlobal> CCFondos { get; set; } = new List<CodigoCuentasGlobal>();

        
        // SUB TOTAL
        public decimal SubTotal { get; set; }
        // TOTAL
        public decimal Total { get; set; }
        // MES
        public DateTime Fecha { get; set; }
        // INFO DE CONDOMINIO
        public Condominio? Condominio { get; set; }
        public int? IdDetail { get; set; }
        //public IList<ReferenciaDolar>? ReferenciasDolar { get; set; }

    }
}
