namespace Prueba.Models
{
    public class Estacionamiento
    {
        public Estacionamiento()
        {
            Puesto_Es = new HashSet<Puesto_Est>();
        }

        public int IdEstacionamiento { get; set; }
        public int IdInmueble { get; set; }
        public string Nombre { get; set; } = null!;
        public int NumPuestos { get; set; }

        public virtual Inmueble IdInmuebleNavigation { get; set; } = null!;
        public virtual ICollection<Puesto_Est> Puesto_Es { get; set; }
    }
}
