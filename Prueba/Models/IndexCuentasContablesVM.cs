using Microsoft.AspNetCore.Mvc.Rendering;

namespace Prueba.Models
{
    public class IndexCuentasContablesVM
    {

        public IList<Clase>? Clases { get; set; }
        public IList<Grupo>? Grupos { get; set; }
        public IList<Cuenta>? Cuentas { get; set; }
        public IList<SubCuenta>? SubCuentas { get; set; }

    }
}
