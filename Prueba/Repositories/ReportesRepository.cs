using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using System.Linq;

namespace Prueba.Repositories
{
    public interface IReportesRepository
    {
        Task<decimal> CalculoCuentaPorCobrar(int idCondominio);
        Task<int> CantidadDeudores(int idCondominio);
        Task<decimal> CantidadRecibosNoPagados(int idCondominio);
        Task<decimal> CantidadRecibosPagados(int idCondominio);
        Task<decimal> CantidadRecibosPendientes(int idCondominio);
        Task<decimal> EgresosPorMes(int mes, int idCondominio);
        Task<InformacionDashboardVM> InformacionGeneral(int id);
        Task<decimal> IngresosPorMes(int mes, int idCondominio);
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
                        .Sum(c => c.Monto);

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
                    .Sum(c => c.Monto);

                egresoMes += pagosEmitidosMes;
            }
            return egresoMes;

        }

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
    }
}
