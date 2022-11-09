namespace Prueba.Models
{
    public class ComprobanteVM
    {
        public Condominio? Condominio { get; set; }
        public Inmueble? Inmueble { get; set; }
        public Propiedad? Propiedad { get; set; }
        public PagoRecibido? PagoRecibido { get; set; }
        public ReferenciasPr? Referencias { get; set; }
        public DateTime FechaComprobante { get; set; }
    }
}
