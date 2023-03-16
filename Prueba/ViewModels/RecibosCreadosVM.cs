using Prueba.Models;

namespace Prueba.ViewModels
{
    public class RecibosCreadosVM
    {
        public IList<Propiedad>? Propiedades { get; set; }
        public IList<AspNetUser>? Propietarios { get; set; }
        public IList<ReciboCobro>? Recibos { get; set; }
        public IList<Inmueble>? Inmuebles { get; set; }
        public RelacionDeGastosVM? RelacionGastos { get; set; }
    }
}
