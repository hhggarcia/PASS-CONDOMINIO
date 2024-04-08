using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Prueba.Context;
using Prueba.Models;
using Prueba.ViewModels;
using SQLitePCL;
using System.Runtime.InteropServices;

namespace Prueba.Repositories
{
    public interface IRelacionGastoRepository
    {
        Task<List<CodigoCuentasGlobal>> BuscarCuentasGlobalGrupo(int id, int idGrupo);
        Task<List<CuentasGrupo>> BuscarCuentasGrupo(int id, int idGrupo);
        Task<List<CodigoCuentasGlobal>> BuscarCuentasGrupoPropiedad(int id, int idPropiedad);
        Task<List<GrupoGasto>> BuscarGruposGastos(int id);
        Task<bool> DeleteRecibosCobroRG(int idRG);
        Task<DetalleReciboVM> DetalleRecibo(int id);
        Task<RelacionDeGastosVM> LoadDataRelacionGastos(int id);
        Task<RelacionDeGastosVM> LoadDataRelacionGastosMes(int? idRG);
        Task<TransaccionVM> LoadTransacciones(int id);
        Task<TransaccionVM> LoadTransaccionesMes(int? idRg);
        bool RelacionGastoExists(int id);
    }
    public class RelacionGastoRepository : IRelacionGastoRepository
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public RelacionGastoRepository(ICuentasContablesRepository repoCuentas,
            IMonedaRepository repoMoneda,
            NuevaAppContext context)
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
            var modelo = new RelacionDeGastosVM();

            var condominio = await _context.Condominios.FindAsync(id);

            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == condominio.IdCondominio && c.IdClase == 5
                                       select c;

            var gastos = from c in _context.Gastos
                         select c;

            //var gruposGastos = from c in _context.Grupos
            //                   where c.IdClase == 5
            //                   select c;

            //var cuentas = from c in _context.Cuenta
            //              select c;

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
            var subcuentasGastos = await _repoCuentas.ObtenerGastos(condominio.IdCondominio);

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
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(aux.First().IdCodCuenta);

                    foreach (var item in subcuentasGastos)
                    {
                        if (idcc.IdSubCuenta == item.Id)
                        {
                            subtotal += aux.First().MontoRef;
                            total += aux.First().MontoRef;
                            asientosGastosCondominio.Add(aux.First());
                            subcuentasModel.Add(item);
                            modelo.CCGastos.Add(idcc);
                        }
                        continue;
                    }
                }

            }

            // CREAR MODELO PARA LOS TOTALES DE LA RELACION DE GASTOS Y CARGAR VISTA

            modelo.GastosDiario = asientosGastosCondominio;
            modelo.SubcuentasGastos = subcuentasModel;
            modelo.Total = total;
            modelo.SubTotal = subtotal;
            modelo.Fecha = DateTime.Today;
            modelo.Condominio = condominio;


            if (proviciones.Any() && fondos.Any())
            {
                modelo.Provisiones = await proviciones.ToListAsync();
                modelo.Fondos = await fondos.ToListAsync();


                IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                foreach (var provision in modelo.Provisiones)
                {
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);

                    var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                    modelo.CCProvisiones.Add(idcc);

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
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);

                    var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                    modelo.CCFondos.Add(idcc);

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
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);
                    var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                    modelo.CCProvisiones.Add(idcc);

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
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);
                    var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                    modelo.CCFondos.Add(idcc);

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


        public async Task<TransaccionVM> LoadTransacciones(int id)
        {
            var modelo = new TransaccionVM();

            var condominio = await _context.Condominios.FindAsync(id);

            var gruposGastos = await (from c in _context.GrupoGastos
                                      join e in _context.CuentasGrupos
                                      on c.IdGrupoGasto equals e.IdGrupoGasto
                                      join cc in _context.CodigoCuentasGlobals
                                      on e.IdCodCuenta equals cc.IdCodCuenta
                                      where cc.IdCondominio == id
                                      select c).ToListAsync();

            var transacciones = await (from t in _context.Transaccions
                                       join cc in _context.CodigoCuentasGlobals
                                       on t.IdCodCuenta equals cc.IdCodCuenta
                                       where cc.IdCondominio == id
                                       where t.Fecha.Month == DateTime.Today.Month
                                       select t).ToListAsync();

            var transaccionesInd = await (from t in _context.Transaccions
                                          join cc in _context.CodigoCuentasGlobals
                                          on t.IdCodCuenta equals cc.IdCodCuenta
                                          where t.Fecha.Month == DateTime.Today.Month
                                          where cc.IdCondominio == id
                                          where t.IdPropiedad != null
                                          select t).ToListAsync();

            var subcuentas = await _repoCuentas.ObtenerSubcuentas(id);
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

            decimal totalIngresos = 0;
            decimal totalEgresos = 0;
            decimal totalEgresosInd = 0;
            decimal totalIngresoInd = 0;

            foreach (var item in transacciones)
            {
                if (item.TipoTransaccion && item.IdPropiedad == null)
                {
                    totalIngresos += item.MontoTotal;
                }
                else if (!item.TipoTransaccion && item.IdPropiedad == null)
                {
                    totalEgresos += item.MontoTotal;

                }

                if (item.TipoTransaccion && item.IdPropiedad != null)
                {
                    totalIngresoInd += item.MontoTotal;
                }
                else if (!item.TipoTransaccion && item.IdPropiedad != null)
                {
                    totalEgresosInd += item.MontoTotal;
                }
            }

            modelo.Condominio = condominio;
            modelo.Fecha = DateTime.Today;
            modelo.GruposGastos = gruposGastos;
            modelo.Transaccions = transacciones;
            modelo.TransaccionesIndividuales = transaccionesInd;
            modelo.TotalIngresos = totalIngresos;
            modelo.TotalGastos = totalEgresos;
            modelo.TotalIngresoIndividual = totalIngresoInd;
            modelo.TotalEgresoIndividual = totalEgresosInd;
            modelo.Total = totalEgresos - totalIngresos;
            modelo.TotalIndividual = totalEgresosInd - totalIngresoInd;
            modelo.TotalGeneral = modelo.Total + modelo.TotalIndividual;

            if (proviciones.Any() && fondos.Any())
            {
                modelo.Provisiones = await proviciones.ToListAsync();
                modelo.Fondos = await fondos.ToListAsync();


                IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                foreach (var provision in modelo.Provisiones)
                {
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);

                    var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                    modelo.CCProvisiones.Add(idcc);

                    if (subcuentaProvision.Any())
                    {
                        SubCuenta aux = subcuentaProvision.First();
                        if (aux != null)
                        {
                            //modelo.SubTotal += provision.Monto;
                            modelo.Total += provision.Monto;
                            subcuentasProvisionesModel.Add(aux);
                        }
                    }
                }

                IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();
                foreach (var fondo in modelo.Fondos)
                {
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);

                    var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                    modelo.CCFondos.Add(idcc);

                    if (subcuentaFondo.Any())
                    {
                        SubCuenta aux = subcuentaFondo.First();
                        if (aux != null)
                        {
                            modelo.Total += modelo.Total * fondo.Porcentaje / 100;
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
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);
                    var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                    modelo.CCProvisiones.Add(idcc);

                    if (subcuentaProvision.Any())
                    {
                        SubCuenta aux = subcuentaProvision.First();
                        if (aux != null)
                        {
                            //modelo.SubTotal += provision.MontoRef;
                            modelo.Total += provision.Monto;
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
                    var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);
                    var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                    modelo.CCFondos.Add(idcc);

                    if (subcuentaFondo.Any())
                    {
                        SubCuenta aux = subcuentaFondo.First();
                        if (aux != null)
                        {
                            modelo.Total += modelo.Total * fondo.Porcentaje / 100;
                            subcuentasFondosModel.Add(aux);
                        }
                    }
                }
                modelo.SubCuentasFondos = subcuentasFondosModel;
            }

            return modelo;
        }

        public async Task<TransaccionVM> LoadTransaccionesMes(int? idRg)
        {
            var modelo = new TransaccionVM();

            var rg = await _context.RelacionGastos.FindAsync(idRg);

            if (rg != null)
            {
                var condominio = await _context.Condominios.FindAsync(rg.IdCondominio);

                var gruposGastos = await (from c in _context.GrupoGastos
                                          join e in _context.CuentasGrupos
                                          on c.IdGrupoGasto equals e.IdGrupoGasto
                                          join cc in _context.CodigoCuentasGlobals
                                          on e.IdCodCuenta equals cc.IdCodCuenta
                                          where cc.IdCondominio == condominio.IdCondominio
                                          select c).ToListAsync();

                var transacciones = await (from t in _context.Transaccions
                                           join cc in _context.CodigoCuentasGlobals
                                           on t.IdCodCuenta equals cc.IdCodCuenta
                                           join tt in _context.RelacionGastoTransaccions
                                           on t.IdTransaccion equals tt.IdTransaccion
                                           where cc.IdCondominio == condominio.IdCondominio
                                           where t.Fecha.Month == rg.Fecha.Month
                                           where tt.IdRelacionGasto == rg.IdRgastos
                                           select t).ToListAsync();

                var transaccionesInd = await (from t in _context.Transaccions
                                              join cc in _context.CodigoCuentasGlobals
                                              on t.IdCodCuenta equals cc.IdCodCuenta
                                              join tt in _context.RelacionGastoTransaccions
                                              on t.IdTransaccion equals tt.IdTransaccion
                                              where t.Fecha.Month == rg.Fecha.Month
                                              where cc.IdCondominio == condominio.IdCondominio
                                              where t.IdPropiedad != null
                                              where tt.IdRelacionGasto == rg.IdRgastos
                                              select t).ToListAsync();

                var subcuentas = await _repoCuentas.ObtenerSubcuentas(condominio.IdCondominio);
                // BUSCAR PROVISIONES en los asientos del diario
                DateTime fechaActual = DateTime.Now;

                var proviciones = from p in _context.Provisiones
                                  join c in _context.CodigoCuentasGlobals
                                  on p.IdCodCuenta equals c.IdCodCuenta
                                  where c.IdCondominio == condominio.IdCondominio
                                  where DateTime.Compare(fechaActual, p.FechaFin) < 0 && DateTime.Compare(fechaActual, p.FechaInicio) >= 0
                                  select p;
                // BUSCAR FONDOS
                var fondos = from f in _context.Fondos
                             join c in _context.CodigoCuentasGlobals
                             on f.IdCodCuenta equals c.IdCodCuenta
                             where c.IdCondominio == condominio.IdCondominio
                             where DateTime.Compare(fechaActual, f.FechaFin) < 0 && DateTime.Compare(fechaActual, f.FechaInicio) >= 0
                             select f;

                decimal totalIngresos = 0;
                decimal totalEgresos = 0;
                decimal totalEgresosInd = 0;
                decimal totalIngresoInd = 0;

                foreach (var item in transacciones)
                {
                    if (item.TipoTransaccion && item.IdPropiedad == null)
                    {
                        totalIngresos += item.MontoTotal;
                    }
                    else if (!item.TipoTransaccion && item.IdPropiedad == null)
                    {
                        totalEgresos += item.MontoTotal;

                    }

                    if (item.TipoTransaccion && item.IdPropiedad != null)
                    {
                        totalIngresoInd += item.MontoTotal;
                    }
                    else if (!item.TipoTransaccion && item.IdPropiedad != null)
                    {
                        totalEgresosInd += item.MontoTotal;
                    }
                }

                modelo.Condominio = condominio;
                modelo.Fecha = DateTime.Today;
                modelo.GruposGastos = gruposGastos;
                modelo.Transaccions = transacciones;
                modelo.TransaccionesIndividuales = transaccionesInd;
                modelo.TotalIngresos = totalIngresos;
                modelo.TotalGastos = totalEgresos;
                modelo.TotalIngresoIndividual = totalIngresoInd;
                modelo.TotalEgresoIndividual = totalEgresosInd;
                modelo.Total = totalEgresos - totalIngresos;
                modelo.TotalIndividual = totalEgresosInd - totalIngresoInd;
                modelo.TotalGeneral = modelo.Total + modelo.TotalIndividual;

                if (proviciones.Any() && fondos.Any())
                {
                    modelo.Provisiones = await proviciones.ToListAsync();
                    modelo.Fondos = await fondos.ToListAsync();


                    IList<SubCuenta> subcuentasProvisionesModel = new List<SubCuenta>();
                    foreach (var provision in modelo.Provisiones)
                    {
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);

                        var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                        modelo.CCProvisiones.Add(idcc);

                        if (subcuentaProvision.Any())
                        {
                            SubCuenta aux = subcuentaProvision.First();
                            if (aux != null)
                            {
                                //modelo.SubTotal += provision.Monto;
                                modelo.Total += provision.Monto;
                                subcuentasProvisionesModel.Add(aux);
                            }
                        }
                    }

                    IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();
                    foreach (var fondo in modelo.Fondos)
                    {
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);

                        var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                        modelo.CCFondos.Add(idcc);

                        if (subcuentaFondo.Any())
                        {
                            SubCuenta aux = subcuentaFondo.First();
                            if (aux != null)
                            {
                                modelo.Total += modelo.Total * fondo.Porcentaje / 100;
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
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);
                        var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                        modelo.CCProvisiones.Add(idcc);

                        if (subcuentaProvision.Any())
                        {
                            SubCuenta aux = subcuentaProvision.First();
                            if (aux != null)
                            {
                                //modelo.SubTotal += provision.MontoRef;
                                modelo.Total += provision.Monto;
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
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);
                        var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                        modelo.CCFondos.Add(idcc);

                        if (subcuentaFondo.Any())
                        {
                            SubCuenta aux = subcuentaFondo.First();
                            if (aux != null)
                            {
                                modelo.Total += modelo.Total * fondo.Porcentaje / 100;
                                subcuentasFondosModel.Add(aux);
                            }
                        }
                    }
                    modelo.SubCuentasFondos = subcuentasFondosModel;
                }
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
                                           where c.IdClase == 5
                                           where c.IdCondominio == rg.IdCondominio
                                           select c;

                var gastos = from c in _context.Gastos
                             select c;

                //var gruposGastos = from c in _context.Grupos
                //                   where c.IdClase == 5
                //                   select c;

                var cuentas = from c in _context.Cuenta
                              select c;

                var subcuentas = from c in _context.SubCuenta
                                 select c;

                // BUSCAR PROVISIONES en los asientos del diario
                DateTime fechaActual = rg.Fecha;

                var proviciones = from p in _context.Provisiones
                                  join c in _context.CodigoCuentasGlobals
                                  on p.IdCodCuenta equals c.IdCodCuenta
                                  where c.IdCondominio == condominio.IdCondominio
                                  where DateTime.Compare(fechaActual, p.FechaFin) < 0 && DateTime.Compare(fechaActual, p.FechaInicio) >= 0
                                  select p;
                // BUSCAR FONDOS
                var fondos = from f in _context.Fondos
                             join c in _context.CodigoCuentasGlobals
                             on f.IdCodCuenta equals c.IdCodCuenta
                             where c.IdCondominio == condominio.IdCondominio
                             where DateTime.Compare(fechaActual, f.FechaFin) < 0 && DateTime.Compare(fechaActual, f.FechaInicio) >= 0
                             select f;

                // CARGAR DIARIO COMPLETO
                var diario = from d in _context.LdiarioGlobals
                             join c in cuentasContablesCond
                             on d.IdCodCuenta equals c.IdCodCuenta
                             where d.Fecha.Month == fechaActual.Month && c.IdCondominio == condominio.IdCondominio
                             select d;

                // CARGAR CUENTAS GASTOS DEL CONDOMINIO
                var subcuentasGastos = await _repoCuentas.ObtenerGastos(condominio.IdCondominio);

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
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(aux.First().IdCodCuenta);

                        foreach (var item in subcuentasGastos)
                        {
                            if (idcc.IdSubCuenta == item.Id)
                            {
                                subtotal += aux.First().MontoRef;
                                total += aux.First().MontoRef;
                                asientosGastosCondominio.Add(aux.First());
                                subcuentasModel.Add(item);
                                modelo.CCGastos.Add(idcc);
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
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);

                        var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                        modelo.CCProvisiones.Add(idcc);

                        if (subcuentaProvision.Any())
                        {
                            SubCuenta aux = subcuentaProvision.First();
                            if (aux != null)
                            {
                                modelo.SubTotal += provision.MontoRef;
                                modelo.Total = modelo.SubTotal;
                                subcuentasProvisionesModel.Add(aux);
                            }
                        }
                    }
                    IList<SubCuenta> subcuentasFondosModel = new List<SubCuenta>();
                    foreach (var fondo in modelo.Fondos)
                    {
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);

                        var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);

                        modelo.CCFondos.Add(idcc);

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
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(provision.IdCodCuenta);
                        var subcuentaProvision = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                        modelo.CCProvisiones.Add(idcc);

                        if (subcuentaProvision.Any())
                        {
                            SubCuenta aux = subcuentaProvision.First();
                            if (aux != null)
                            {
                                modelo.SubTotal += provision.MontoRef;
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
                        var idcc = await _context.CodigoCuentasGlobals.FindAsync(fondo.IdCodCuenta);

                        var subcuentaFondo = subcuentas.Where(c => idcc.IdSubCuenta == c.Id);
                        modelo.CCFondos.Add(idcc);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id del Recibo</param>
        /// <returns></returns>
        public async Task<DetalleReciboVM> DetalleRecibo(int id)
        {
            var modelo = new DetalleReciboVM();
            var recibo = await _context.ReciboCobros.FindAsync(id);
            if (recibo != null)
            {
                var propiedad = await _context.Propiedads.FindAsync(recibo.IdPropiedad);
                if (propiedad != null)
                {
                    var usuario = await _context.AspNetUsers.FindAsync(propiedad.IdUsuario);
                    if (usuario != null)
                    {
                        modelo.Recibo = recibo;
                        modelo.Propiedad = propiedad;
                        modelo.Propietario = usuario;
                        modelo.RelacionGastos = await LoadDataRelacionGastosMes(recibo.IdRgastos);
                    }

                }

            }

            return modelo;
        }

        public async Task<List<GrupoGasto>> BuscarGruposGastos(int id)
        {
            var gruposGastos = await (from c in _context.GrupoGastos
                                      join e in _context.CuentasGrupos
                                      on c.IdGrupoGasto equals e.IdGrupoGasto
                                      join cc in _context.CodigoCuentasGlobals
                                      on e.IdCodCuenta equals cc.IdCodCuenta
                                      where cc.IdCondominio == id
                                      select c).ToListAsync();

            return gruposGastos;
        }

        public async Task<List<CuentasGrupo>> BuscarCuentasGrupo(int id, int idGrupo)
        {
            var cuentasGrupo = await (from c in _context.GrupoGastos
                                      where c.IdGrupoGasto == idGrupo
                                      join e in _context.CuentasGrupos
                                      on c.IdGrupoGasto equals e.IdGrupoGasto
                                      join cc in _context.CodigoCuentasGlobals
                                      on e.IdCodCuenta equals cc.IdCodCuenta
                                      where cc.IdCondominio == id
                                      select e).ToListAsync();

            return cuentasGrupo;
        }

        public async Task<List<CodigoCuentasGlobal>> BuscarCuentasGlobalGrupo(int id, int idGrupo)
        {
            var cuentasGrupo = await (from c in _context.GrupoGastos
                                      where c.IdGrupoGasto == idGrupo
                                      join e in _context.CuentasGrupos
                                      on c.IdGrupoGasto equals e.IdGrupoGasto
                                      join cc in _context.CodigoCuentasGlobals
                                      on e.IdCodCuenta equals cc.IdCodCuenta
                                      where cc.IdCondominio == id
                                      select cc).ToListAsync();

            return cuentasGrupo;
        }

        /// <summary>
        /// Buscar todas las cuentas de todos los grupos 
        /// a los que pertenece una propiedad
        /// </summary>
        /// <param name="id">Id del condominio</param>
        /// <param name="idPropiedad">id de la propiedad</param>
        /// <returns>Lista de Cuentas Globales</returns>
        public async Task<List<CodigoCuentasGlobal>> BuscarCuentasGrupoPropiedad(int id, int idPropiedad)
        {
            List<CodigoCuentasGlobal> cuentas = new List<CodigoCuentasGlobal>();

            var cuentasGruposIds = from gp in _context.GrupoGastos
                         join cc in _context.PropiedadesGrupos.Where(c => c.IdPropiedad == idPropiedad)
                         on gp.IdGrupoGasto equals cc.IdGrupoGasto
                         join cuentaGrupo in _context.CuentasGrupos
                         on cc.IdGrupoGasto equals cuentaGrupo.IdGrupoGasto
                         select cuentaGrupo;

            if (cuentasGruposIds != null)
            {
                cuentas = await (from cuentaGrupo in cuentasGruposIds
                          join cc in _context.CodigoCuentasGlobals.Where(c => c.IdCondominio == id)
                          on cuentaGrupo.IdCodCuenta equals cc.IdCodCuenta
                          select cc).ToListAsync();

                return cuentas;
            }

            return cuentas;
        }
    }
}
