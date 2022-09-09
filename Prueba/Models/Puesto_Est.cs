namespace Prueba.Models
{
    public class Puesto_Est
    {
        public int IdPuesto_Est { get; set; }
        public int IdEstacionamiento { get; set; }
        public int IdPropiedad { get; set; }
        public string Codigo { get; set; } = null!;

        public virtual Estacionamiento IdEstacionamientoNavigation { get; set; } = null!;
        public virtual Propiedad IdPropiedadNavigation { get; set; } = null!;
    }
}
