namespace Prueba.Models
{
    public class Inmueble
    {
        public Inmueble()
        {      
            Condominios = new HashSet<Condominio>();
            Estacionamientos = new HashSet<Estacionamiento>();
            Propiedades = new HashSet<Propiedad>();
        }

        public int IdInmueble { get; set; }
        public int IdZona { get; set; }
        public string Nombre { get; set; } = null!;
        public int TotalPropiedad { get; set; }

        public virtual ICollection<Condominio> Condominios { get; set; }
        public virtual ICollection<Estacionamiento> Estacionamientos { get; set; }
        public virtual ICollection<Propiedad> Propiedades { get; set; }
    }
}
