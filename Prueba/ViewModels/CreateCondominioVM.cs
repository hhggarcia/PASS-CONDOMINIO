using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CreateCondominioVM
    {
        public AspNetUser Owner { get; set; }
        public AspNetUser Administrator { get; set; }
        public Condominio CondminioNuevo { get; set; }
    }
}
