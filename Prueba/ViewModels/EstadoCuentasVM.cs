using Prueba.Models;

namespace Prueba.ViewModels
{
    public class EstadoCuentasVM
    {
        public IList<ReciboCobro> ReciboCobro { get; set; } = new List<ReciboCobro>();
        public Condominio Condominio { get; set; } = null!;
        public Propiedad Propiedad { get; set; } = null!;
        public AspNetUser User { get; set; } = new AspNetUser();

    }
}
