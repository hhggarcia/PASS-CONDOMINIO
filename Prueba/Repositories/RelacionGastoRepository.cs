using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prueba.Context;
using Prueba.Models;
using SQLitePCL;
using System.Runtime.InteropServices;

namespace Prueba.Repositories
{
    public interface IRelacionGastoRepository
    {
        Task<bool> DeleteRecibosCobroRG(int idRG);
        Task<RelacionDeGastosVM> LoadDataRelacionGastos(int id);
        Task<RelacionDeGastosVM> LoadDataRelacionGastosMes(int? idRG);
        bool RelacionGastoExists(int id);
    }
    public class RelacionGastoRepository: IRelacionGastoRepository
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IMonedaRepository _repoMoneda;
        private readonly PruebaContext _context;

        public RelacionGastoRepository(ICuentasContablesRepository repoCuentas,
            IMonedaRepository repoMoneda,
            PruebaContext context)
        {
            _repoCuentas = repoCuentas;
            _repoMoneda = repoMoneda;
            _context = context;
        }

        /// <summary>
        /// CARGAR DATA DE LA RELACION DE GASTOS DEL MES ACTUAL
        /// cargar informacion de la relacion de gastos dependiendo del condominio
        /// </summary>
        /// <param name="id">Id del condominio</param>
        /// <returns>modelo RelacionDeGastosVM</returns>
        public async Task<RelacionDeGastosVM> LoadDataRelacionGastos(int id)
        {
            var condominio = await _context.Condominios.FindAsync(id);

            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == condominio.IdCondominio
                                       select c;

            var gastos = from c in _context.Gastos
                         select c;

            var gruposGastos = from c in _context.Grupos
                               where c.IdClase == 5
                               select c;

            var cuentas = from c in _context.Cuenta
                          select c;

            var subcuentas = from c in _context.SubCuenta
                             select c;

            // BUSCAR PROVISIONES en los asientos del diario
            DateTime fechaActual = DateTime.Now;

            var proviciones = from p in _context.Provisiones
                              join c in _context.CodigoCuentasGlobals
                              on p.IdCodCuenta equals c.IdCodCuenta
                              where c.IdCondominio == id
                              where DateTime.Compare(fechaActual, p.FechaFin) < 0 && DateTime.Compare(fechaActual, p.FechaInicio) >= 0
                              select p;
            // BUSCAR FONDOS
            var fondos = from f in _context.Fondos
                         join c in _context.CodigoCuentasGlobals
                         on f.IdCodCuenta equals c.IdCodCuenta
                         where c.IdCondominio == id
                         where DateTime.Compare(fechaActual, f.FechaFin) < 0 && DateTime.Compare(fechaActual, f.FechaInicio) >= 0
                         select f;

            // CARGAR DIARIO COMPLETO
            var diario = from d in _context.LdiarioGlobals
                         join c in cuentasContablesCond
                         on d.IdCodCuenta equals c.IdCodCuenta
                         where d.Fecha.Month == DateTime.Today.Month
                         where c.IdCondominio == id
                         select d;

            // CARGAR CUENTAS GASTOS DEL CONDOMINIO
            IList<Cuenta> cuentasGastos = new List<Cuenta>();
            foreach (var grupo in gruposGastos)
            {
                foreach (var cuenta in cuentas)
                {
                    if (cuenta.IdGrupo == grupo.Id)
                    {
                        cuentasGastos.Add(cuenta);
                    }
                    continue;
                }
            }

            IList<SubCuenta> subcuentasGastos = new List<SubCuenta>();
            foreach (var cuenta in cuentasGastos)
            {
                foreach (var subcuenta in subcuentas)
                {
                    if (subcuenta.IdCuenta == cuenta.Id)
                    {
                        subcuentasGastos.Add(subcuenta);
                    }
                    continue;
                }
            }

            // BUSCAR ASIENTOS EN EL DIARIO CORRESPONDIENTES A LAS CUENTAS GASTOS DEL CONDOMINIO
            IList<LdiarioGlobal> asientosGastosCondominio = new List<LdiarioGlobal>();
            IList<SubCuenta> subcuentasModel = new List<SubCuenta>();
            decimal subtotal = 0;
            decimal total = 0;
            foreach (var gasto in gastos)
            {
                var aux = diario.Where(c => c.IdAsiento == gasto.IdAsiento).ToList();
                if (aux.Any())
                {
                    foreach (var item in subcuentasGastos)
                    {
                        if (aux.FirstOrDefault().IdCodCuenta == item.Id)
                        {
                            subtotal += aux.First().MontoRef;
                            total += aux.First().MontoRef;
                            asientosGastosCondominio.Add(aux.First());
                            subcuentasModel.Add(item);
                        }
                        continue;
                    }
                }

            }

            // CREAR MODELO PARA LOS TOTALES DE LA RELACION DE GASTOS Y CARGAR VISTA
            var modelo = new RelacionDeGastosVM
            {
                GastosDiario = asientosGastosCondominio,
                SubcuentasGastos = subcuentasModel,
                Total = total,
                SubTotal = subtotal,
                Fecha = DateTime.Today,
                Condominio = condominio
            };

            if (proviciones.Any() && fondos.Any())
            {
                modelo.Provisiones = await proviciones.ToListAsync();
                modelo.Fondos = await fondos.ToListAsync();


                IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                foreach (var provision in modelo.Provisiones)
                {
                    var subcuentaProvision = subcuentas.Where(c => provision.IdCodCuenta == c.Id);

                    if (subcuentaProvision.Any())
                    {
                        SubCuenta aux = subcuentaProvision.First();
                        if (aux != null)
                        {
                            modelo.SubTotal += provision.MontoRef;
                            modelo.Total += provision.MontoRef;
                            subcuentasProvisionesModel.Add(aux);
                        }
                    }
                }

                IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();
                foreach (var fondo in modelo.Fondos)
                {
                    var subcuentaFondo = subcuentas.Where(c => fondo.IdCodCuenta == c.Id);

                    if (subcuentaFondo.Any())
                    {
                        SubCuenta aux = subcuentaFondo.First();
                        if (aux != null)
                        {
                            modelo.Total += modelo.SubTotal * fondo.Porcentaje / 100;
                            subcuentasFondosModel.Add(aux);
                        }
                    }
                }

                modelo.SubCuentasFondos = subcuentasFondosModel;
                modelo.SubCuentasProvisiones = subcuentasProvisionesModel;
            }
            else if (proviciones.Any() && !fondos.Any())
            {
                modelo.Provisiones = await proviciones.ToListAsync();

                IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                foreach (var provision in modelo.Provisiones)
                {
                    var subcuentaProvision = subcuentas.Where(c => provision.IdCodCuenta == c.Id);

                    if (subcuentaProvision.Any())
                    {
                        SubCuenta aux = subcuentaProvision.First();
                        if (aux != null)
                        {
                            modelo.SubTotal += provision.MontoRef;
                            modelo.Total += provision.MontoRef;
                            subcuentasProvisionesModel.Add(aux);
                        }
                    }
                }

                modelo.SubCuentasProvisiones = subcuentasProvisionesModel;

            }
            else if (!proviciones.Any() && fondos.Any())
            {
                modelo.Fondos = await fondos.ToListAsync();
                IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();

                foreach (var fondo in modelo.Fondos)
                {
                    var subcuentaFondo = subcuentas.Where(c => fondo.IdCodCuenta == c.Id);

                    if (subcuentaFondo.Any())
                    {
                        SubCuenta aux = subcuentaFondo.First();
                        if (aux != null)
                        {
                            modelo.Total += modelo.SubTotal * fondo.Porcentaje / 100;
                            subcuentasFondosModel.Add(aux);
                        }
                    }
                }
                modelo.SubCuentasFondos = subcuentasFondosModel;
            }

            return modelo;
        }

        /// <summary>
        /// Carga información de la relación de gastos dependiendo de su id.
        /// Busca la información por su mes de creación
        /// </summary>
        /// <param name="idRG">Id de la relación de gastos</param>
        /// <returns>modelo RelacionDeGastosVM</returns>
        public async Task<RelacionDeGastosVM> LoadDataRelacionGastosMes(int? idRG)
        {
            // buscar relacion de gastos por id
            var rg = await _context.RelacionGastos.FindAsync(idRG);
            var modelo = new RelacionDeGastosVM();
            if (rg != null)
            {
                // buscar todos los gastos del mes en el que fue creada la RG


                // buscar fondos y provisiones de esas fechas

                // agregar atributos fecha a la tabla en BD Fondos y provisione
                var condominio = await _context.Condominios.FindAsync(rg.IdCondominio);

                var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                           where c.IdCondominio == rg.IdCondominio
                                           select c;

                var gastos = from c in _context.Gastos
                             select c;

                var gruposGastos = from c in _context.Grupos
                                   where c.IdClase == 5
                                   select c;

                var cuentas = from c in _context.Cuenta
                              select c;

                var subcuentas = from c in _context.SubCuenta
                                 select c;
                // BUSCAR PROVISIONES en los asientos del diario
                var proviciones = from p in _context.Provisiones
                                  select p;
                // BUSCAR FONDOS
                var fondos = from f in _context.Fondos
                             select f;

                // CARGAR DIARIO COMPLETO
                var diario = from d in _context.LdiarioGlobals
                             where d.Fecha.Month == rg.Fecha.Month
                             select d;

                // CARGAR CUENTAS GASTOS DEL CONDOMINIO
                IList<Cuenta> cuentasGastos = new List<Cuenta>();
                foreach (var grupo in gruposGastos)
                {
                    foreach (var cuenta in cuentas)
                    {
                        if (cuenta.IdGrupo == grupo.Id)
                        {
                            cuentasGastos.Add(cuenta);
                        }
                        continue;
                    }
                }

                IList<SubCuenta> subcuentasGastos = new List<SubCuenta>();
                foreach (var cuenta in cuentasGastos)
                {
                    foreach (var subcuenta in subcuentas)
                    {
                        if (subcuenta.IdCuenta == cuenta.Id)
                        {
                            subcuentasGastos.Add(subcuenta);
                        }
                        continue;
                    }
                }

                // BUSCAR ASIENTOS EN EL DIARIO CORRESPONDIENTES A LAS CUENTAS GASTOS DEL CONDOMINIO
                IList<LdiarioGlobal> asientosGastosCondominio = new List<LdiarioGlobal>();
                IList<SubCuenta> subcuentasModel = new List<SubCuenta>();
                decimal subtotal = 0;
                decimal total = 0;
                foreach (var gasto in gastos)
                {
                    var aux = diario.Where(c => c.IdAsiento == gasto.IdAsiento).ToList();
                    if (aux.Any())
                    {
                        foreach (var item in subcuentasGastos)
                        {
                            if (aux.First().IdCodCuenta == item.Id)
                            {
                                subtotal += aux.First().Monto;
                                total += aux.First().Monto;
                                asientosGastosCondominio.Add(aux.First());
                                subcuentasModel.Add(item);
                            }
                            continue;
                        }
                    }

                }

                modelo.GastosDiario = asientosGastosCondominio;
                modelo.SubcuentasGastos = subcuentasModel;
                modelo.Total = total;
                modelo.SubTotal = subtotal;
                modelo.Fecha = rg.Fecha;
                modelo.Condominio = condominio;

                if (proviciones.Any() && fondos.Any())
                {
                    modelo.Provisiones = await proviciones.ToListAsync();
                    modelo.Fondos = await fondos.ToListAsync();


                    IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                    foreach (var provision in modelo.Provisiones)
                    {
                        var subcuentaProvision = subcuentas.Where(c => provision.IdCodCuenta == c.Id);

                        if (subcuentaProvision.Any())
                        {
                            SubCuenta aux = subcuentaProvision.First();
                            if (aux != null)
                            {
                                modelo.SubTotal += provision.Monto;
                                modelo.Total = modelo.SubTotal;
                                subcuentasProvisionesModel.Add(aux);
                            }
                        }
                    }
                    IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();
                    foreach (var fondo in modelo.Fondos)
                    {
                        var subcuentaFondo = subcuentas.Where(c => fondo.IdCodCuenta == c.Id);

                        if (subcuentaFondo.Any())
                        {
                            SubCuenta aux = subcuentaFondo.First();
                            if (aux != null)
                            {
                                modelo.Total += modelo.SubTotal * fondo.Porcentaje / 100;
                                subcuentasFondosModel.Add(aux);
                            }
                        }
                    }
                    modelo.SubCuentasFondos = subcuentasFondosModel;
                    modelo.SubCuentasProvisiones = subcuentasProvisionesModel;
                }
                else if (proviciones.Any() && !fondos.Any())
                {
                    modelo.Provisiones = await proviciones.ToListAsync();

                    IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                    foreach (var provision in modelo.Provisiones)
                    {
                        var subcuentaProvision = subcuentas.Where(c => provision.IdCodCuenta == c.Id);

                        if (subcuentaProvision.Any())
                        {
                            SubCuenta aux = subcuentaProvision.First();
                            if (aux != null)
                            {
                                modelo.SubTotal += provision.Monto;
                                subcuentasProvisionesModel.Add(aux);
                            }
                        }
                    }

                    modelo.SubCuentasProvisiones = subcuentasProvisionesModel;

                }
                else if (!proviciones.Any() && fondos.Any())
                {
                    modelo.Fondos = await fondos.ToListAsync();
                    IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();

                    foreach (var fondo in modelo.Fondos)
                    {
                        var subcuentaFondo = subcuentas.Where(c => fondo.IdCodCuenta == c.Id);

                        if (subcuentaFondo.Any())
                        {
                            SubCuenta aux = subcuentaFondo.First();
                            if (aux != null)
                            {
                                modelo.Total += modelo.SubTotal * fondo.Porcentaje / 100;
                                subcuentasFondosModel.Add(aux);
                            }
                        }
                    }
                    modelo.SubCuentasFondos = subcuentasFondosModel;
                }

                return modelo;
            }
            return modelo;

        }

        /// <summary>
        /// Elimina todos los Recibos de Cobro relacionados a una Relacion de Gastos
        /// </summary>
        /// <param name="idRG">Id de la relacion de gastos a eliminar</param>
        /// <returns>Retorna True si Se eliminan todos los recibos, sino retorna false</returns>
        public async Task<bool> DeleteRecibosCobroRG(int idRG)
        {
            bool result = false;
            var relacionGasto = await _context.RelacionGastos.FindAsync(idRG);

            if (relacionGasto != null)
            {
                var recibos = await _context.ReciboCobros.Where(c => c.IdRgastos == relacionGasto.IdRgastos).ToListAsync();

                foreach (var recibo in recibos)
                {
                    // buscar la propiedad 
                    var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);

                    var pagosRecibos = await _context.PagosRecibos.Where(c => c.IdRecibo == recibo.IdReciboCobro).ToListAsync();

                    if (propiedad != null)
                    {
                        // verificar si es el recibo atual o uno viejo

                        if (recibo.Fecha.Month == DateTime.Today.Month)
                        {
                            // si es actual eliminar saldo = 0
                            propiedad.Saldo = 0;

                            // verificar solvencia de la propiedad
                            if (propiedad.Deuda == 0)
                            {
                                propiedad.Solvencia = true;
                            }

                            // actualizar propiedad
                            _context.Propiedads.Update(propiedad);
                        }
                        else if (recibo.Fecha.Month != DateTime.Today.Month)
                        {
                            // si es viejo restar de la deuda -= Monto
                            propiedad.Deuda -= recibo.Monto;

                            // verificar solvencia de la propiedad
                            if (propiedad.Deuda == 0 && propiedad.Saldo == 0)
                            {
                                propiedad.Solvencia = true;
                            }

                            // actualizar propiedad
                            _context.Propiedads.Update(propiedad);
                        }
                    }
                    _context.ReciboCobros.Remove(recibo);

                    if (pagosRecibos.Any())
                    {
                        foreach (var item in pagosRecibos)
                        {
                            _context.PagosRecibos.Remove(item);
                        }
                    }
                    
                }

                await _context.SaveChangesAsync();

                result = true;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RelacionGastoExists(int id)
        {
            return (_context.RelacionGastos?.Any(e => e.IdRgastos == id)).GetValueOrDefault();
        }
    }
}
