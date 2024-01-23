using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CodigoCuentasGlobalVm
    {
        public CodigoCuentasGlobal CodigoCuentasGlobal { get; set; }=new CodigoCuentasGlobal();
        public string Grupo { get; set; }
        public string Clase { get; set; }
        public string Cuenta { get; set; }
        public string SubCuenta { get; set; }
    }
}
