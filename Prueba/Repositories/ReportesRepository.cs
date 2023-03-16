using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.ViewModels;
using System.Linq;

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
        Task<RecibosCreadosVM> LoadDataDeudores(int idCondominio);
    }
    public class ReportesRepository : IReportesRepository
    {
        private readonly PruebaContext _context;

        public ReportesRepository(PruebaContext context)
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
                var inmuebles = from a in _context.Inmuebles
                                where a.IdCondominio == condominio.IdCondominio
                                select a;

                IList<Propiedad> propiedades = new List<Propiedad>();

                foreach (var inmueble in inmuebles)
                {
                    IList<Propiedad> propiedad = await _context.Propiedads.Where(c => c.IdInmueble == inmueble.IdInmueble).ToListAsync();

                    var aux = propiedades.Concat(propiedad).ToList();
                    propiedades = aux.ToList();
                }

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
                var inmuebles = from a in _context.Inmuebles
                                where a.IdCondominio == condominio.IdCondominio
                                select a;

                IList<Propiedad> propiedades = new List<Propiedad>();

                foreach (var inmueble in inmuebles)
                {
                    var propiedad = await _context.Propiedads.Where(c => c.IdInmueble == inmueble.IdInmueble).ToListAsync();

                    var aux = propiedades.Concat(propiedad).ToList();
                    propiedades = aux.ToList();
                }

                foreach (var propiedad in propiedades)
                {
                    var recibos = _context.ReciboCobros
                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad
                        && c.EnProceso == true);

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
                var inmuebles = from a in _context.Inmuebles
                                where a.IdCondominio == condominio.IdCondominio
                                select a;

                IList<Propiedad> propiedades = new List<Propiedad>();

                foreach (var inmueble in inmuebles)
                {
                    var propiedad = await _context.Propiedads.Where(c => c.IdInmueble == inmueble.IdInmueble).ToListAsync();

                    var aux = propiedades.Concat(propiedad).ToList();
                    propiedades = aux.ToList();
                }

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
                var inmuebles = from a in _context.Inmuebles
                                where a.IdCondominio == condominio.IdCondominio
                                select a;

                IList<Propiedad> propiedades = new List<Propiedad>();

                foreach (var inmueble in inmuebles)
                {
                    var propiedad = await _context.Propiedads.Where(c => c.IdInmueble == inmueble.IdInmueble).ToListAsync();

                    var aux = propiedades.Concat(propiedad).ToList();
                    propiedades = aux.ToList();
                }

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
                var inmuebles = from a in _context.Inmuebles
                                where a.IdCondominio == condominio.IdCondominio
                                select a;

                IList<Propiedad> propiedades = new List<Propiedad>();

                foreach (var inmueble in inmuebles)
                {
                    var propiedad = await _context.Propiedads.Where(c => c.IdInmueble == inmueble.IdInmueble).ToListAsync();

                    var aux = propiedades.Concat(propiedad).ToList();
                    propiedades = aux.ToList();
                }

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
                var inmuebles = from a in _context.Inmuebles
                                where a.IdCondominio == condominio.IdCondominio
                                select a;

                IList<Propiedad> propiedades = new List<Propiedad>();

                foreach (var inmueble in inmuebles)
                {
                    var propiedad = await _context.Propiedads.Where(c => c.IdInmueble == inmueble.IdInmueble).ToListAsync();

                    var aux = propiedades.Concat(propiedad).ToList();
                    propiedades = aux.ToList();
                }

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
                var inmuebles = from a in _context.Inmuebles
                                where a.IdCondominio == condominio.IdCondominio
                                select a;

                IList<Propiedad> propiedades = new List<Propiedad>();

                foreach (var inmueble in inmuebles)
                {
                    var propiedad = await _context.Propiedads.Where(c => c.IdInmueble == inmueble.IdInmueble).ToListAsync();

                    var aux = propiedades.Concat(propiedad).ToList();
                    propiedades = aux.ToList();
                }

                foreach (var propiedad in propiedades)
                {
                    var pagosMes = _context.PagoRecibidos
                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad
                        && c.Fecha.Month == mes
                        && c.Confirmado == true)
                        .Sum(c => c.MontoRef);

                    ingresoMes += pagosMes;
                }
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
                    .Sum(c => c.MontoRef);

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
            var inmueblesCondominio = from c in _context.Inmuebles
                                      where c.IdCondominio == idCondominio
                                      select c;
            var propiedades = from c in _context.Propiedads
                              where c.Solvencia == false
                              select c;
            var propietarios = from c in _context.AspNetUsers
                               select c;
            var relacionesGastos = from c in _context.RelacionGastos
                                   where c.IdCondominio == idCondominio
                                   select c;
            var recibosCobro = from c in _context.ReciboCobros
                               select c;

            if (inmueblesCondominio != null && inmueblesCondominio.Any()
                && propiedades != null && propiedades.Any()
                && propietarios != null && propietarios.Any()
                && relacionesGastos != null && relacionesGastos.Any())
            {

                IList<Propiedad> listaPropiedadesCondominio = new List<Propiedad>();
                IList<ReciboCobro> recibosCobroCond = new List<ReciboCobro>();
                // BUSCAR PROPIEADES DE LOS INMUEBLES

                foreach (var item in inmueblesCondominio)
                {
                    var propiedadsCond = await propiedades.Where(c => c.IdInmueble == item.IdInmueble).ToListAsync();
                    var aux2 = listaPropiedadesCondominio.Concat(propiedadsCond).ToList();
                    listaPropiedadesCondominio = aux2;
                }

                // BUSCAR SUS RECIBOS DE COBRO
                // BUSCAR PROPIEDADES CON DEUDA
                foreach (var propiedad in listaPropiedadesCondominio)
                {
                    var recibo = await recibosCobro.Where(c => c.IdPropiedad == propiedad.IdPropiedad
                                                            && c.EnProceso != true
                                                            && c.Pagado != true)
                                                    .ToListAsync();

                    var aux = recibosCobroCond.Concat(recibo).ToList();
                    recibosCobroCond = aux;
                }

                var modelo = new RecibosCreadosVM
                {
                    Propiedades = listaPropiedadesCondominio,
                    Propietarios = await propietarios.ToListAsync(),
                    Recibos = recibosCobroCond,
                    Inmuebles = await inmueblesCondominio.ToListAsync()
                };

                return modelo;
            }

            return new RecibosCreadosVM();
        }
    }
}
