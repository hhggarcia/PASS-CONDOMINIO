using Prueba.Models;

namespace Prueba.ViewModels
{
    public class IndexUserPropVM
    {
        public AspNetUser? Propietario { get; set; }
        public IList<Propiedad> Propiedades { get; set; } = new List<Propiedad>();
    }
}
