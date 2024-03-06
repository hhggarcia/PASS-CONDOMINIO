using Prueba.Models;

namespace Prueba.ViewModels
{
    public class EstadoCuentasVM
    {
        public ReciboCobro ReciboCobro { get; set; }
        public Condominio Condominio { get; set; }
        public Propiedad Propiedad { get; set; }
        public AspNetUser AspNetUser { get; set; } = new AspNetUser();

    }
}
