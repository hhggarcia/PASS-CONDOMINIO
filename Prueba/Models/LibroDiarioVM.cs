namespace Prueba.Models
{
    public class LibroDiarioVM
    {
        public IList<LdiarioGlobal>? AsientosCondominio { get; set; }
        public IList<SubCuenta>? CuentasDiarioCondominio { get; set; }
        public IList<CodigoCuentasGlobal>? CuentasCondominio { get; set; }
        public IList<Clase>? Clases { get; set; }
        public IList<Grupo>? Grupos { get; set; }
        public IList<Cuenta>? Cuentas { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public decimal Diferencia { get; set; }

    }
}
