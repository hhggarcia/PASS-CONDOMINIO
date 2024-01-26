using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.Models;

namespace Prueba.ViewModels
{
    public class IndexCuentasContablesVM
    {

        public IList<Clase>? Clases { get; set; }
        public IList<Grupo>? Grupos { get; set; }
        public IList<Cuenta>? Cuentas { get; set; }
        public IList<SubCuenta>? SubCuentas { get; set; }
        public IList<CuentaGlobalSubCuentasVM>? SubCuentasSaldo { get; set; }
    }
}
