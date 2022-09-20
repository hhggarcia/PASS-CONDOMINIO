namespace Prueba.Models
{
    public partial class Grupo
    {
        public Grupo()
        {
            Cuentas = new HashSet<Cuenta>();
        }
        public int Id { get; set; }
        public int IdClase { get; set; }
        public string Descripcion { get; set; } = null!;
        public string codigo { get; set; } = null!;


        public virtual Clase IdClaseNavigation { get; set; } = null!;
        public virtual ICollection<Cuenta> Cuentas { get; set; }

    }
}
