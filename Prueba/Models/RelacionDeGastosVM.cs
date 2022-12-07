namespace Prueba.Models
{
    public class RelacionDeGastosVM
    {
        // LISTA DE LOS GASTOS REGISTRADOS EN DIARIO DEL MES
        public IList<LdiarioGlobal>? GastosDiario { get; set; }
        public IList<SubCuenta>? SubcuentasGastos { get; set; }
        // LISTA DE PROVISIONES
        public IList<Provision>? Provisiones { get; set; }
        public IList<SubCuenta>? SubCuentasProvisiones { get; set; }
        // SUBCUENTAS FONDOS
        public IList<SubCuenta>? SubCuentasFondos { get; set; }
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
    }
}
