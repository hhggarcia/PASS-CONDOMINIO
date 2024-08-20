using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace Prueba.Repositories
{
    public interface IReportesRepository
    {
        Task<decimal> CalculoCuentaPorCobrar(int idCondominio);
        Task<int> CantidadDeudores(int idCondominio);
        Task<int> CantidadNoDeudores(int idCondominio);
        Task<decimal> CantidadRecibosNoPagados(int idCondominio);
        Task<decimal> CantidadRecibosPagados(int idCondominio);
        Task<decimal> CantidadRecibosPendientes(int idCondominio);
        Task<decimal> EgresosPorMes(int mes, int idCondominio);
        Task<InformacionDashboardVM> InformacionGeneral(int id);
        Task<decimal> IngresosPorMes(int mes, int idCondominio);
        Task<ItemConciliacionVM> LoadConciliacionCuenta(FiltroBancoVM filtro);
        Task<ItemConciliacionVM> LoadConciliacionPagos(FiltroBancoVM filtro);
        Task<RecibosCreadosVM> LoadDataDeudores(int idCondominio);
    }
    public class ReportesRepository : IReportesRepository
    {
        private readonly NuevaAppContext _context;

        public ReportesRepository(NuevaAppContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<decimal> CalculoCuentaPorCobrar(int idCondominio)
        {
            decimal totalPorCobrar = 0;

            var condominio = await _context.Condominios.FindAsync(idCondominio);
            if (condominio != null)
            {
                //var inmuebles = from a in _context.Inmuebles
                //                where a.IdCondominio == condominio.IdCondominio
                //                select a;

                //IList<Propiedad> propiedades = new List<Propiedad>();

                IList<Propiedad> propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                //var aux = propiedades.Concat(propiedad).ToList();
                //propiedades = aux.ToList();

                foreach (var propiedad in propiedades)
                {
                    var recibos = _context.ReciboCobros
                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad
                        && c.Pagado == false)
                        .Sum(c => c.Monto);

                    totalPorCobrar += recibos;
                }
            }

            return totalPorCobrar;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<decimal> CantidadRecibosPendientes(int idCondominio)
        {
            decimal cantidadRecibosPendientes = 0;

            var condominio = await _context.Condominios.FindAsync(idCondominio);
            if (condominio != null)
            {
                //var inmuebles = from a in _context.Inmuebles
                //                where a.IdCondominio == condominio.IdCondominio
                //                select a;

                //IList<Propiedad> propiedades = new List<Propiedad>();

                IList<Propiedad> propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                //var aux = propiedades.Concat(propiedad).ToList();
                //propiedades = aux.ToList();

                foreach (var propiedad in propiedades)
                {
                    var recibos = _context.ReciboCobros
                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad
                        && c.EnProceso);

                    cantidadRecibosPendientes += recibos.Count();
                }
            }

            return cantidadRecibosPendientes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<decimal> CantidadRecibosPagados(int idCondominio)
        {
            decimal cantidadRecibosPagados = 0;

            var condominio = await _context.Condominios.FindAsync(idCondominio);
            if (condominio != null)
            {
                //var inmuebles = from a in _context.Inmuebles
                //                where a.IdCondominio == condominio.IdCondominio
                //                select a;

                //IList<Propiedad> propiedades = new List<Propiedad>();

                IList<Propiedad> propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                //var aux = propiedades.Concat(propiedad).ToList();
                //propiedades = aux.ToList();

                foreach (var propiedad in propiedades)
                {
                    var recibos = _context.ReciboCobros
                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad
                        && c.Pagado == true);

                    cantidadRecibosPagados += recibos.Count();
                }
            }

            return cantidadRecibosPagados;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<decimal> CantidadRecibosNoPagados(int idCondominio)
        {
            decimal cantidadRecibosNoPagados = 0;

            var condominio = await _context.Condominios.FindAsync(idCondominio);
            if (condominio != null)
            {
                //var inmuebles = from a in _context.Inmuebles
                //                where a.IdCondominio == condominio.IdCondominio
                //                select a;

                //IList<Propiedad> propiedades = new List<Propiedad>();

                IList<Propiedad> propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                //var aux = propiedades.Concat(propiedad).ToList();
                //propiedades = aux.ToList();

                foreach (var propiedad in propiedades)
                {
                    var recibos = _context.ReciboCobros
                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad
                        && c.Pagado == false);

                    cantidadRecibosNoPagados += recibos.Count();
                }
            }

            return cantidadRecibosNoPagados;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<int> CantidadDeudores(int idCondominio)
        {
            int totalDeudores = 0;

            var condominio = await _context.Condominios.FindAsync(idCondominio);
            if (condominio != null)
            {
                //var inmuebles = from a in _context.Inmuebles
                //                where a.IdCondominio == condominio.IdCondominio
                //                select a;

                //IList<Propiedad> propiedades = new List<Propiedad>();

                IList<Propiedad> propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                //var aux = propiedades.Concat(propiedad).ToList();
                //propiedades = aux.ToList();

                foreach (var propiedad in propiedades)
                {
                    if (propiedad.Solvencia == false)
                    {
                        totalDeudores += 1;
                    }
                }
            }

            return totalDeudores;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<int> CantidadNoDeudores(int idCondominio)
        {
            int totalNoDeudores = 0;

            var condominio = await _context.Condominios.FindAsync(idCondominio);
            if (condominio != null)
            {
                //var inmuebles = from a in _context.Inmuebles
                //                where a.IdCondominio == condominio.IdCondominio
                //                select a;

                //IList<Propiedad> propiedades = new List<Propiedad>();

                IList<Propiedad> propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                //var aux = propiedades.Concat(propiedad).ToList();
                //propiedades = aux.ToList();

                foreach (var propiedad in propiedades)
                {
                    if (propiedad.Solvencia == true)
                    {
                        totalNoDeudores += 1;
                    }
                }
            }

            return totalNoDeudores;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mes"></param>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<decimal> IngresosPorMes(int mes, int idCondominio)
        {
            var condominio = await _context.Condominios.FindAsync(idCondominio);
            decimal ingresoMes = 0;
            if (condominio != null)
            {
                //var inmuebles = from a in _context.Inmuebles
                //                where a.IdCondominio == condominio.IdCondominio
                //                select a;

                //IList<Propiedad> propiedades = new List<Propiedad>();

                // IList<Propiedad> propiedades = await _context.Propiedads.Where(c => c.IdCondominio == idCondominio).ToListAsync();

                //var aux = propiedades.Concat(propiedad).ToList();
                //propiedades = aux.ToList();

                var pagosMes = _context.PagoRecibidos
                        .Where(c => c.IdCondominio == idCondominio
                        && c.Fecha.Month == mes
                        && c.Confirmado)
                        .Sum(c => c.Monto);

                ingresoMes += pagosMes;
            }
            return ingresoMes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mes"></param>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<decimal> EgresosPorMes(int mes, int idCondominio)
        {
            var condominio = await _context.Condominios.FindAsync(idCondominio);
            decimal egresoMes = 0;
            if (condominio != null)
            {
                var pagosEmitidosMes = _context.PagoEmitidos
                    .Where(c => c.IdCondominio == condominio.IdCondominio
                    && c.Fecha.Month == mes)
                    .Sum(c => c.Monto);

                egresoMes += pagosEmitidosMes;
            }
            return egresoMes;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<InformacionDashboardVM> InformacionGeneral(int id)
        {
            var modelo = new InformacionDashboardVM();
            modelo.Ingresos = new Dictionary<int, decimal>();
            modelo.Egresos = new Dictionary<int, decimal>();

            if (id > 0)
            {
                modelo.CuentasPorCobrar = await CalculoCuentaPorCobrar(id);
                modelo.RecibosPendientes = await CantidadRecibosPendientes(id);
                modelo.RecibosPagados = await CantidadRecibosPagados(id);
                modelo.RecibosNoPagados = await CantidadRecibosNoPagados(id);
                modelo.Deudores = await CantidadDeudores(id);
                modelo.NoDeudores = await CantidadNoDeudores(id);


                for (int i = 1; i < 13; i++)
                {
                    var ingreso = await IngresosPorMes(i, id);
                    var egreso = await EgresosPorMes(i, id);

                    modelo.Ingresos.Add(i, ingreso);
                    modelo.Egresos.Add(i, egreso);
                }
            }

            return modelo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<RecibosCreadosVM> LoadDataDeudores(int idCondominio)
        {
            // CARGAR PROPIEDADES DE CADA INMUEBLE DEL CONDOMINIO
            //var inmueblesCondominio = from c in _context.Inmuebles
            //                          where c.IdCondominio == idCondominio
            //                          select c;
            var propiedades = from c in _context.Propiedads
                              where c.IdCondominio == idCondominio
                              where !c.Solvencia
                              select c;

            var propietarios = from c in _context.AspNetUsers
                               join p in propiedades
                               on c.Id equals p.IdUsuario
                               select c;

            var relacionesGastos = from c in _context.RelacionGastos
                                   where c.IdCondominio == idCondominio
                                   select c;
            var recibosCobro = from c in _context.ReciboCobros
                               join p in propiedades
                               on c.IdPropiedad equals p.IdPropiedad
                               select c;

            if (propiedades != null && propiedades.Any()
                && propietarios != null && propietarios.Any()
                && relacionesGastos != null && relacionesGastos.Any())
            {

                IList<Propiedad> listaPropiedadesCondominio = new List<Propiedad>();
                IList<ReciboCobro> recibosCobroCond = new List<ReciboCobro>();
                // BUSCAR PROPIEADES DE LOS INMUEBLES

                var propiedadsCond = await propiedades.Where(c => c.IdCondominio == idCondominio).ToListAsync();
                var aux2 = listaPropiedadesCondominio.Concat(propiedadsCond).ToList();
                listaPropiedadesCondominio = aux2;

                // BUSCAR SUS RECIBOS DE COBRO
                // BUSCAR PROPIEDADES CON DEUDA
                foreach (var propiedad in listaPropiedadesCondominio)
                {
                    var recibo = await recibosCobro.Where(c => c.IdPropiedad == propiedad.IdPropiedad
                                                            && !c.EnProceso
                                                            && !c.Pagado)
                                                    .ToListAsync();

                    var aux = recibosCobroCond.Concat(recibo).ToList();
                    recibosCobroCond = aux;
                }

                var modelo = new RecibosCreadosVM
                {
                    Propiedades = listaPropiedadesCondominio,
                    Propietarios = await propietarios.ToListAsync(),
                    Recibos = recibosCobroCond,
                    //Inmuebles = await inmueblesCondominio.ToListAsync()
                };

                return modelo;
            }

            return new RecibosCreadosVM();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public async Task<ItemConciliacionVM> LoadConciliacionCuenta(FiltroBancoVM filtro)
        {
            var subCuenta = await _context.SubCuenta.FindAsync(filtro.IdCodCuenta);
            if (subCuenta != null)
            {
                var cc = await _context.CodigoCuentasGlobals.FirstOrDefaultAsync(c => c.IdSubCuenta == subCuenta.Id);

                var conciliacionAnterior = await _context.Conciliacions.FirstOrDefaultAsync(c => c.Actual && c.Activo);

                var asientosIngresos = await _context.LdiarioGlobals
                    .Where(c => c.Fecha >= filtro.FechaInicio
                    && c.Fecha <= filtro.FechaFin
                    && c.IdCodCuenta == cc.IdCodCuenta
                    && c.TipoOperacion == cc.Aumenta)
                    .ToListAsync();

                var asientosEgresos = await _context.LdiarioGlobals
                    .Where(c => c.Fecha >= filtro.FechaInicio
                    && c.Fecha <= filtro.FechaFin
                    && c.IdCodCuenta == cc.IdCodCuenta
                    && c.TipoOperacion == cc.Disminuye)
                    .ToListAsync();

                var asientos = await _context.LdiarioGlobals
                    .Where(c => c.Fecha >= filtro.FechaInicio
                    && c.Fecha <= filtro.FechaFin
                    && c.IdCodCuenta == cc.IdCodCuenta)
                    .ToListAsync();

                return new ItemConciliacionVM()
                {
                    CodigoCuenta = cc,
                    SubCuenta = subCuenta,
                    Asientos = asientos,
                    ConciliacionAnterior = conciliacionAnterior,
                    SaldoInicial = conciliacionAnterior != null ? conciliacionAnterior.SaldoFinal : 0,
                    FechaInicio = filtro.FechaInicio,
                    FechaFin = filtro.FechaFin,
                    TotalEgreso = asientosEgresos.Where(c => !c.TipoOperacion).Sum(c => c.Monto),
                    TotalIngreso = asientosIngresos.Where(c => c.TipoOperacion).Sum(c => c.Monto),
                    SaldoFinal = asientosIngresos.Sum(c => c.Monto) - asientosEgresos.Sum(c => c.Monto)
                };

            }

            return new ItemConciliacionVM();
        }

        public async Task<ItemConciliacionVM> LoadConciliacionPagos(FiltroBancoVM filtro)
        {
            var subCuenta = await _context.SubCuenta.FindAsync(filtro.IdCodCuenta);
            if (subCuenta != null)
            {
                var cc = await _context.CodigoCuentasGlobals.FirstOrDefaultAsync(c => c.IdSubCuenta == subCuenta.Id);

                var conciliacionAnterior = await _context.Conciliacions.FirstOrDefaultAsync(c => c.Actual && c.Activo);

                var asientos = await _context.LdiarioGlobals
                    .Where(c => c.Fecha >= filtro.FechaInicio
                    && c.Fecha <= filtro.FechaFin
                    && c.IdCodCuenta == cc.IdCodCuenta)
                    .ToListAsync();

                var pagosRecibidos = await (from pr in _context.PagoRecibidos
                                            join referencia in _context.ReferenciasPrs
                                            on pr.IdPagoRecibido equals referencia.IdPagoRecibido
                                            where pr.Fecha >= filtro.FechaInicio && pr.Fecha <= filtro.FechaFin
                                            where cc.IdSubCuenta.ToString() == referencia.Banco
                                            select pr).OrderBy(c => c.Fecha).ToListAsync();

                var pagosEmitidos = await (from pr in _context.PagoEmitidos
                                           join referencia in _context.ReferenciasPes
                                           on pr.IdPagoEmitido equals referencia.IdPagoEmitido
                                           where pr.Fecha >= filtro.FechaInicio && pr.Fecha <= filtro.FechaFin
                                           where cc.IdSubCuenta.ToString() == referencia.Banco
                                           select pr).OrderBy(c => c.Fecha).ToListAsync();

                IList<PagosConciliacionVM> pagos = new List<PagosConciliacionVM>();

                foreach (var item in pagosRecibidos)
                {
                    pagos.Add(new PagosConciliacionVM()
                    {
                        Id = item.IdPagoRecibido,
                        IdCondominio = item.IdCondominio,
                        Fecha = item.Fecha,
                        Monto = item.Monto,
                        FormaPago = item.FormaPago,
                        Concepto = item.Concepto,
                        TipoOperacion = true,
                        Activo = item.Activo
                    });
                }
                foreach (var item in pagosEmitidos)
                {
                    pagos.Add(new PagosConciliacionVM()
                    {
                        Id = item.IdPagoEmitido,
                        IdCondominio = item.IdCondominio,
                        Fecha = item.Fecha,
                        Monto = item.Monto,
                        FormaPago = item.FormaPago,
                        Concepto = item.Concepto,
                        TipoOperacion = false,
                        Activo = item.Activo
                    });
                }

                return new ItemConciliacionVM()
                {
                    CodigoCuenta = cc,
                    IdCodigoCuenta = cc != null ? cc.IdCodCuenta : 0,
                    SubCuenta = subCuenta,
                    Asientos = asientos,
                    ConciliacionAnterior = conciliacionAnterior,
                    SaldoInicial = conciliacionAnterior != null ? conciliacionAnterior.SaldoFinal : 0,
                    FechaInicio = filtro.FechaInicio,
                    FechaFin = filtro.FechaFin,
                    TotalEgreso = pagosEmitidos.Sum(c => c.Monto),
                    TotalIngreso = pagosRecibidos.Sum(c => c.Monto),
                    SaldoFinal = pagosRecibidos.Sum(c => c.Monto) - pagosEmitidos.Sum(c => c.Monto),
                    Pagos = pagos,
                    PagosIds = pagos.Select(c => new SelectListItem
                    {
                        Text = c.TipoOperacion ? "Ingreso" : "Egreso",
                        Value = c.Id.ToString(),
                        Selected = false
                    }).ToList()
                };
            }

            return new ItemConciliacionVM();
        }
    }
}
