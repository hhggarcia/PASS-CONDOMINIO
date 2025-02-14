using Prueba.Models;

namespace Prueba.ViewModels
{
    public class HistoricoPropiedadPagosVM
    {
        public Propiedad? Propiedad { get; set; }
        public IList<PagoPropiedad> Pagos { get; set; } = new List<PagoPropiedad>();
    }
}