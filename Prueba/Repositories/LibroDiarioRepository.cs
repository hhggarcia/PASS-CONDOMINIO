using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Repositories
{
    public interface ILibroDiarioRepository
    {
        Task<int> Crear(LdiarioGlobal asiento);
        Task<int> Editar(LdiarioGlobal asiento);
        Task<int> Eliminar(int id);
        bool LdiarioGlobalExists(int id);
        LibroDiarioVM LibroDiario(int id);
    }
    public class LibroDiarioRepository : ILibroDiarioRepository
    {
        private readonly PruebaContext _context;

        public LibroDiarioRepository(PruebaContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LibroDiarioVM LibroDiario(int id)
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
                        var aux = subcuentas.Where(c => c.Id == asiento.IdCodCuenta).ToList();
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
                Clases = clases.ToList(),
                Grupos = grupos.ToList(),
                Cuentas = cuentas.ToList(),
                TotalDebe = totalDebe,
                TotalHaber = totalHaber,
                Diferencia = diferencia
            };

            return modelo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asiento"></param>
        /// <returns></returns>
        public async Task<int> Crear(LdiarioGlobal asiento)
        {
            _context.Add(asiento);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asiento"></param>
        /// <returns></returns>
        public async Task<int> Editar(LdiarioGlobal asiento)
        {
            _context.Update(asiento);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> Eliminar(int id)
        {
            var asiento = await _context.LdiarioGlobals.FindAsync(id);
            if (asiento != null)
            {
                // buscar relaciones FK con el asiento
                var activos = await _context.Activos.Where(c => c.IdAsiento == asiento.IdAsiento).ToListAsync();
                var pasivos = await _context.Pasivos.Where(c => c.IdAsiento == asiento.IdAsiento).ToListAsync();
                var patrimonios = await _context.Patrimonios.Where(c => c.IdAsiento == asiento.IdAsiento).ToListAsync();
                var ingresos = await _context.Ingresos.Where(c => c.IdAsiento == asiento.IdAsiento).ToListAsync();
                var egresos = await _context.Gastos.Where(c => c.IdAsiento == asiento.IdAsiento).ToListAsync();

                // eliminar si existen activos relacionados 
                if (activos != null && activos.Any())
                {
                    foreach (var activo in activos)
                    {
                        _context.Activos.Remove(activo);
                    }
                }

                // eliminar si existen pasivos relacionados
                if (pasivos != null && pasivos.Any())
                {
                    foreach (var pasivo in pasivos)
                    {
                        _context.Pasivos.Remove(pasivo);
                    }
                }

                // eliminar si existen patrimonios relacionados 

                if (patrimonios != null && patrimonios.Any())
                {
                    foreach (var patrimonio in patrimonios)
                    {
                        _context.Patrimonios.Remove(patrimonio);
                    }
                }

                // eliminar si existen ingresos relacionados 

                if (ingresos != null && ingresos.Any())
                {
                    foreach (var ingreso in ingresos)
                    {
                        _context.Ingresos.Remove(ingreso);
                    }
                }

                // eliminar si existen egresos relacionados 

                if (egresos != null && egresos.Any())
                {
                    foreach (var gasto in egresos)
                    {
                        _context.Gastos.Remove(gasto);
                    }
                }

                // eliminar asiento
                _context.LdiarioGlobals.Remove(asiento);
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool LdiarioGlobalExists(int id)
        {
            return (_context.LdiarioGlobals?.Any(e => e.IdAsiento == id)).GetValueOrDefault();
        }
    }
}
