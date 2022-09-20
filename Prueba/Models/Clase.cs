namespace Prueba.Models
{
    public partial class Clase
    {
        public Clase()
        {
            Grupos = new HashSet<Grupo>();
        }
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
        public string codigo { get; set; } = null!;

        public virtual ICollection<Grupo> Grupos { get; set; }

    }
}
