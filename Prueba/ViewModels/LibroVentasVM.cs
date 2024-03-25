using Prueba.Models;

namespace Prueba.ViewModels
{
    public class LibroVentasVM
    {
        public LibroVenta? libroVenta { get; set; }
        public FacturaEmitida? FacturaEmitida { get; set; }
        public Producto? Producto { get; set; }
        public Cliente? cliente { get; set; }
    }
}
