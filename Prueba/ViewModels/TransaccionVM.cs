using Prueba.Models;

namespace Prueba.ViewModels
{
    public class TransaccionVM
    {
        public IList<Transaccion>?  Transaccions {  get; set; }
        public Condominio Condominio { get; set; }
    }
}
