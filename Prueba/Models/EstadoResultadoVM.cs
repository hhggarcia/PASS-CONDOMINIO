namespace Prueba.Models
{
    public class EstadoResultadoVM
    {
        public IList<Gasto>? Egresos { get; set; }
        public IList<Ingreso>? Ingresos { get; set; }
        public IList<LdiarioGlobal>? AsientosIngresos { get; set; }
        public IList<LdiarioGlobal>? AsientosEgresos { get; set; }
        public IList<LdiarioGlobal>? AsientosCondominio { get; set; }
        public IList<SubCuenta>? CuentasDiarioCondominio { get; set; }
        public IList<Clase>? Clases { get; set; }
        public IList<Grupo>? Grupos { get; set; }
        public IList<Cuenta>? Cuentas { get; set; }
        public IList<SubCuenta>? SubCuentas { get; set; }
        public DateTime Fecha { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal Difenrencia { get; set; }
    }
}
