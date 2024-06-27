
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
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
        Task<LibroDiarioVM> ObtenerLdiarioGlobals(int id, FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<FacturaEmitida>> ObteneFactirasEmitidas(FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<LibroCompra>> ObtenerLibroCompras(FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<LibroVenta>> ObtenerLibroVentas(FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<CuentasCobrar>> ObtenerCuentaCobrar(FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<CuentasPagar>> ObtenerCuentasPagar(FiltrarFechaVM filtrarFechaVM);
        Task<ICollection<CompRetIva>> ObtenerCompIva(FiltrarFechaVM fecha, int id);
        Task<ICollection<ComprobanteRetencion>> ObtenerCompIslr(FiltrarFechaVM fecha, int id);
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
        // DateTime.Compare(DateTime.Today, f.FechaFin) < 0 && DateTime.Compare(DateTime.Today, f.FechaInicio) >= 0
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
        public async Task<ICollection<FacturaEmitida>> ObteneFactirasEmitidas(FiltrarFechaVM filtrarFechaVM)
        {
            var nuevaAppContext = _context.FacturaEmitida.Where(c=>c.FechaEmision >= filtrarFechaVM.Desde && c.FechaEmision <= filtrarFechaVM.Hasta).Include(f => f.IdProductoNavigation);
            return await nuevaAppContext.ToListAsync();
        }
        public async Task<ICollection<LibroCompra>> ObtenerLibroCompras(FiltrarFechaVM filtrarFechaVM)
        {
            //var nuevaAppContext = _context.LibroCompras.Include(l => l.IdCondominioNavigation).Include(l => l.IdFacturaNavigation);
            var consulta = _context.LibroCompras
                .Join(_context.Facturas,
                    libro => libro.IdFactura,
                    factura => factura.IdFactura,
                    (libro, factura) => new { Libro = libro, Factura = factura })
                .Where(c => c.Factura.FechaEmision >= filtrarFechaVM.Desde &&
                                      c.Factura.FechaEmision <= filtrarFechaVM.Hasta)
                .Select(c => c.Libro)
                .Include(libro => libro.IdCondominioNavigation)
                .Include(libro => libro.IdFacturaNavigation);

            return await consulta.ToListAsync();
            //return await nuevaAppContext.ToListAsync();
        }
          public async Task<ICollection<LibroVenta>> ObtenerLibroVentas(FiltrarFechaVM filtrarFechaVM)
        {
            //var nuevaAppContext = _context.LibroCompras.Include(l => l.IdCondominioNavigation).Include(l => l.IdFacturaNavigation);
            var consulta = _context.LibroVentas
                .Join(_context.FacturaEmitida,
                    libro => libro.IdFactura,
                    factura => factura.IdFacturaEmitida,
                    (libro, factura) => new { Libro = libro, Factura = factura })
                .Where(c => c.Factura.FechaEmision >= filtrarFechaVM.Desde &&
                                      c.Factura.FechaEmision <= filtrarFechaVM.Hasta)
                .Select(c => c.Libro)
                .Include(libro => libro.IdCondominioNavigation)
                .Include(libro => libro.IdFacturaNavigation);

            return await consulta.ToListAsync();
            //return await nuevaAppContext.ToListAsync();
        }
         public async Task<ICollection<CuentasCobrar>> ObtenerCuentaCobrar(FiltrarFechaVM filtrarFechaVM)
        {
            //var nuevaAppContext = _context.LibroCompras.Include(l => l.IdCondominioNavigation).Include(l => l.IdFacturaNavigation);
            var consulta = _context.CuentasCobrars
                .Join(_context.FacturaEmitida,
                    libro => libro.IdFactura,
                    factura => factura.IdFacturaEmitida,
                    (libro, factura) => new { Libro = libro, Factura = factura })
                .Where(c => c.Factura.FechaEmision >= filtrarFechaVM.Desde &&
                                      c.Factura.FechaEmision <= filtrarFechaVM.Hasta)
                .Select(c => c.Libro)
                .Include(libro => libro.IdCondominioNavigation)
                .Include(libro => libro.IdFacturaNavigation);

            return await consulta.ToListAsync();
            //return await nuevaAppContext.ToListAsync();
        }
         public async Task<ICollection<CuentasPagar>> ObtenerCuentasPagar(FiltrarFechaVM filtrarFechaVM)
        {
            //var nuevaAppContext = _context.LibroCompras.Include(l => l.IdCondominioNavigation).Include(l => l.IdFacturaNavigation);
            var consulta = _context.CuentasPagars
                .Join(_context.Facturas,
                    libro => libro.IdFactura,
                    factura => factura.IdFactura,
                    (libro, factura) => new { Libro = libro, Factura = factura })
                .Where(c => c.Factura.FechaEmision >= filtrarFechaVM.Desde &&
                                      c.Factura.FechaEmision <= filtrarFechaVM.Hasta)
                .Select(c => c.Libro)
                .Include(libro => libro.IdCondominioNavigation)
                .Include(libro => libro.IdFacturaNavigation);

            return await consulta.ToListAsync();
            //return await nuevaAppContext.ToListAsync();
        }

        public async Task<ICollection<CompRetIva>> ObtenerCompIva(FiltrarFechaVM fecha, int id)
        {
            var nuevaAppContext = _context.CompRetIvas.Where(c => c.FechaEmision >= fecha.Desde && c.FechaEmision <= fecha.Hasta)
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdNotaCreditoNavigation)
                .Include(c => c.IdNotaDebitoNavigation)
                .Include(c => c.IdProveedorNavigation)
                .Where(c => c.IdProveedorNavigation.IdCondominio == id);

            return await nuevaAppContext.ToListAsync();
        }


        public async Task<ICollection<ComprobanteRetencion>> ObtenerCompIslr(FiltrarFechaVM fecha, int id)
        {
            var nuevaAppContext = _context.ComprobanteRetencions.Where(c => c.FechaEmision >= fecha.Desde && c.FechaEmision <= fecha.Hasta)
                .Include(c => c.IdFacturaNavigation)
                .Include(c => c.IdProveedorNavigation)
                .Where(c => c.IdProveedorNavigation.IdCondominio == id);

            return await nuevaAppContext.ToListAsync();
        }


    }
}
