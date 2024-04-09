using Prueba.Models;
using SkiaSharp;

namespace Prueba.ViewModels
{
    public class DetalleReciboTransaccionesVM
    {
        public IList<PropiedadesGrupo> GruposPropiedad { get; set; } = new List<PropiedadesGrupo>();
        public Propiedad? Propiedad { get; set; }
        public ReciboCobro? Recibo { get; set; }
        public RelacionGasto? RelacionGasto { get; set; }
        public TransaccionVM Transacciones { get; set; } = new TransaccionVM();
    }
}
