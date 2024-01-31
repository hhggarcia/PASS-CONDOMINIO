
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.ViewModels;

namespace Prueba.Repositories
{
    public interface IFiltroFechaRepository
    {
        Task<ICollection<CuotasEspeciale>> ObtenerCuoetasEspeciales(string id, FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<Anticipo>> ObtenerAnticipos(FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<Factura>> ObtenerFacturas(FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<LdiarioGlobal>> ObtenerLdiarioGlobals(int id, FiltrarFechaVM filtrarFechaVM);
    }
    public class FiltroFechaRepository : IFiltroFechaRepository
    {
        private readonly NuevaAppContext _context;

        public FiltroFechaRepository(NuevaAppContext context)
        {
            _context = context;
        }
        public async Task<ICollection<CuotasEspeciale>> ObtenerCuoetasEspeciales(string idAdministrador, FiltrarFechaVM filtrarFechaVM)
        {
            var idCondominio = await _context.Condominios.Where(c => c.IdAdministrador == idAdministrador).Select(c => c.IdCondominio).FirstAsync();
            var cuotasCondominio = _context.CuotasEspeciales
          .Where(c => c.IdCondominio == idCondominio && c.FechaInicio >= filtrarFechaVM.Desde && c.FechaInicio <= filtrarFechaVM.Hasta);
            return cuotasCondominio.ToList();
        }
        public async Task<ICollection<Anticipo>> ObtenerAnticipos(FiltrarFechaVM filtrarFechaVM)
        {
            var nuevaAppContext = _context.Anticipos.Where(c=>c.Fecha >= filtrarFechaVM.Desde && c.Fecha <= filtrarFechaVM.Hasta).Include(a => a.IdProveedorNavigation);
            return await nuevaAppContext.ToListAsync();
        }
        public async Task<ICollection<Factura>> ObtenerFacturas(FiltrarFechaVM filtrarFechaVM)
        {
            var nuevaAppContext = _context.Facturas.Where(c=>c.FechaEmision >= filtrarFechaVM.Desde && c.FechaEmision <= filtrarFechaVM.Hasta).Include(f => f.IdProveedorNavigation); 
            return await nuevaAppContext.ToListAsync();
        } 
        public async Task<LibroDiarioVM> ObtenerLdiarioGlobals(int id, FiltrarFechaVM filtrarFechaVM)
        {
            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == id
                                       select c;

            var subcuentas = from c in _context.SubCuenta
                             select c;
            var clases = from c in _context.Clases
                         select c;
            var grupos = from c in _context.Grupos
                         select c;
            var cuentas = from c in _context.Cuenta
                          select c;
            // CARGAR DIARIO COMPLETO
            var diario = from d in _context.LdiarioGlobals
                         where d.Fecha >= filtrarFechaVM.Desde && d.Fecha <= filtrarFechaVM.Hasta
                         select d;

            // BUSCAR ASIENTOS CORRESPONDIENTES A LAS SUBCUENTAS DEL CONDOMINIO
            // CALCULAR EL TOTAL DEL DEBE Y HABER Y SU DIFERENCIA

            IList<LdiarioGlobal> asientosCondominio = new List<LdiarioGlobal>();
            IList<SubCuenta> subCuentasModel = new List<SubCuenta>();
            decimal totalDebe = 0;
            decimal totalHaber = 0;

            foreach (var asiento in diario)
            {
                foreach (var ccCondominio in cuentasContablesCond)
                {
                    if (asiento.IdCodCuenta == ccCondominio.IdCodCuenta)
                    {
                        asientosCondominio.Add(asiento);
                        var aux = subcuentas.Where(c => c.Id == ccCondominio.IdCodCuenta).ToList();
                        subCuentasModel.Add(aux.First());
                        if (asiento.TipoOperacion)
                        {
                            totalDebe += asiento.MontoRef;
                        }
                        else
                        {
                            totalHaber += asiento.MontoRef;
                        }
                    }
                    continue;
                }

            }

            decimal diferencia = totalDebe - totalHaber;

            // LLENAR MODELO

            var modelo = new LibroDiarioVM
            {
                AsientosCondominio = asientosCondominio,
                CuentasDiarioCondominio = subCuentasModel,
                CuentasCondominio = cuentasContablesCond.ToList(),
                Clases = clases.ToList(),
                Grupos = grupos.ToList(),
                Cuentas = cuentas.ToList(),
                TotalDebe = totalDebe,
                TotalHaber = totalHaber,
                Diferencia = diferencia
            };
            return modelo;
        }

        Task<ICollection<LdiarioGlobal>> IFiltroFechaRepository.ObtenerLdiarioGlobals(int id, FiltrarFechaVM filtrarFechaVM)
        {
            throw new NotImplementedException();
        }
    }
}
