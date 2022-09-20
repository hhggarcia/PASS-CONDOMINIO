namespace Prueba.Models
{
    public partial class SubCuenta
    {
        public int Id { get; set; }
        public int IdCuenta { get; set; }
        public string Descripcion { get; set; } = null!;
        public string codigo { get; set; } = null!;


        public virtual Cuenta IdGrupoNavigation { get; set; } = null!;
        public virtual CodigoCuentasGlobal codigoCuenta { get; set; } = null!;

    }
}
