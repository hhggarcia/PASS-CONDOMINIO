using Prueba.Models;

namespace Prueba.ViewModels
{
    public class MonedaCuentasVM
    {
        public IEnumerable<MonedaCuenta>? MonedaCuentas { get; set; }
        public IEnumerable<SubCuenta>? SubCuentas { get; set; }
        public IEnumerable<CodigoCuentasGlobal>? Codigos { get; set; }
    }
}
