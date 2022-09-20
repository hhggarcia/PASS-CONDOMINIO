namespace Prueba.Models
{
    public partial class Cuenta
    {
        public Cuenta()
        {
            Subcuentas = new HashSet<SubCuenta>();
        }
        public int Id { get; set; }
        public int IdGrupo { get; set; }
        public string Description { get; set; } = null!;
        public string codigo { get; set; } = null!;


        public virtual Grupo IdGrupoNavigation { get; set; } = null!;
        public virtual ICollection<SubCuenta> Subcuentas { get; set; }

    }
}
