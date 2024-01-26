using Prueba.Models;

namespace Prueba.ViewModels
{
    public class CuentaGlobalSubCuentasVM
    {
        public SubCuenta? SubCuentas { get; set; } = new SubCuenta();
        public decimal Saldo { get; set; }
        public decimal SaldoInicial { get; set; }
    }
}
