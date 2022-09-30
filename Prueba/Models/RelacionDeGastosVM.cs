namespace Prueba.Models
{
    public class RelacionDeGastosVM
    {
        //LISTA DE LOS GASTOS REGISTRADOS EN DIARIO DEL MES
        public IList<LdiarioGlobal>? GastosDiario { get; set; }
        public IList<SubCuenta>? SubcuentasGastos { get; set; }
        // LISTA DE PROVISIONES
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
