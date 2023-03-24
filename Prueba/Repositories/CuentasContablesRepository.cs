using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;

namespace Prueba.Repositories
{
    public interface ICuentasContablesRepository
    {
        Task<int> CrearFondo(Fondo fondo);
        Task<bool> CrearProvision(Provision provision, int idCondominio);
        Task<int> EditarProvision(Provision provision, int idCondominio);
        Task<int> EliminarSubCuenta(int id);
        Task<ICollection<SubCuenta>> ObtenerBancos(int id);
        Task<ICollection<SubCuenta>> ObtenerCaja(int id);
        Task<ICollection<CodigoCuentasGlobal>> ObtenerCuentasCond(int id);
        Task<ICollection<SubCuenta>> ObtenerFondos(int id);
        Task<ICollection<SubCuenta>> ObtenerGastos(int id);
        Task<ICollection<SubCuenta>> ObtenerProvisiones(int id);
        Task<ICollection<SubCuenta>> ObtenerSubcuentas(int id);
    }
    public class CuentasContablesRepository : ICuentasContablesRepository
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly PruebaContext _context;

        public CuentasContablesRepository(IMonedaRepository repoMoneda,
            PruebaContext context)
        {
            _repoMoneda = repoMoneda;
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ICollection<CodigoCuentasGlobal>> ObtenerCuentasCond(int id)
        {
            var cuentasContables = await _context.CodigoCuentasGlobals
                .Where(c => c.IdCondominio == id)
                .Include(c => c.IdCodigoNavigation)
                .Include(c => c.IdCondominioNavigation)
                .ToListAsync();

            return cuentasContables;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ICollection<SubCuenta>> ObtenerSubcuentas(int id)
        {
            var subcuentas = from subcuenta in _context.SubCuenta
                             join codigo in _context.CodigoCuentasGlobals
                             on subcuenta.Id equals codigo.IdCodigo
                             where codigo.IdCondominio == id
                             select subcuenta;

            return await subcuentas.ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ICollection<SubCuenta>> ObtenerGastos(int id)
        {
            var cuentasCond = await ObtenerCuentasCond(id);

            var cuentasGastos = from g in _context.Grupos
                                join c in _context.Cuenta
                                on g.Id equals c.IdGrupo
                                where g.IdClase == 5
                                select c;

            var subcuentasGastos = from c in cuentasGastos
                                   join s in _context.SubCuenta
                                   on c.Id equals s.IdCuenta
                                   select s;

            var model = from s in subcuentasGastos.ToList()
                        join c in cuentasCond.ToList()
                        on s.Id equals c.IdCodigo
                        where s.Id == c.IdCodigo
                        select s;

            return model.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ICollection<SubCuenta>> ObtenerProvisiones(int id)
        {
            var cuentasCond = await ObtenerCuentasCond(id);

            var cuentaProvision = from c in _context.Cuenta
                                  where c.Descripcion.Trim().ToUpper() == "PROVISIONES"
                                  select c;

            var subcuentasProvision = from c in cuentaProvision.ToList()
                                      join s in _context.SubCuenta
                                      on c.Id equals s.IdCuenta
                                      select s;

            var model = from s in subcuentasProvision.ToList()
                        join c in cuentasCond
                        on s.Id equals c.IdCodigo
                        select s;

            return model.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provision"></param>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<bool> CrearProvision(Provision provision, int idCondominio)
        {
            var resultado = false;

            try
            {
                // provision 
                var subcuentaProvision = await _context.SubCuenta.FindAsync(provision.IdCodCuenta);
                var idProvision = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == subcuentaProvision.Id).FirstAsync();
                var subcuentaGasto = await _context.SubCuenta.FindAsync(provision.IdCodGasto);
                var idGasto = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == subcuentaGasto.Id).FirstAsync();

                // buscar moneda
                var moneda = from c in _context.MonedaConds
                             where c.Simbolo == provision.SimboloMoneda
                             select c;

                // buscar moneda principal
                var monedaPrincipal = await _repoMoneda.MonedaPrincipal(idCondominio);

                // cambiar los montos referencia
                decimal montoReferencia = 0;

                if (moneda == null || monedaPrincipal == null)
                {
                    return resultado;
                }
                else if (moneda.First().Equals(monedaPrincipal.First()))
                {
                    montoReferencia = provision.Monto;
                }
                else if (!moneda.Equals(monedaPrincipal))
                {
                    var montoDolares = provision.Monto * moneda.First().ValorDolar;

                    montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                }

                provision.SimboloRef = monedaPrincipal.First().Simbolo;
                provision.MontoRef = montoReferencia;
                provision.ValorDolar = monedaPrincipal.First().ValorDolar;

                int numAsiento = 1;

                var diarioCondominio = from a in _context.LdiarioGlobals
                                       join c in _context.CodigoCuentasGlobals
                                       on a.IdCodCuenta equals c.IdCodCuenta
                                       where c.IdCondominio == idCondominio
                                       select a;

                if (diarioCondominio.Count() > 0)
                {
                    numAsiento = diarioCondominio.ToList().Last().NumAsiento + 1;
                }

                LdiarioGlobal asientoProvision = new LdiarioGlobal
                {
                    IdCodCuenta = idProvision.IdCodCuenta,
                    Fecha = DateTime.Today,
                    Concepto = subcuentaProvision.Descricion + " - " + subcuentaGasto.Descricion,
                    Monto = provision.Monto,
                    TipoOperacion = false,
                    NumAsiento = numAsiento,
                    MontoRef = montoReferencia,
                    SimboloRef = monedaPrincipal.First().Simbolo,
                    SimboloMoneda = moneda.First().Simbolo,
                    ValorDolar = monedaPrincipal.First().ValorDolar
                };
                LdiarioGlobal asientoGastoProvisionado = new LdiarioGlobal
                {
                    IdCodCuenta = idGasto.IdCodCuenta,
                    Fecha = DateTime.Today,
                    Concepto = subcuentaProvision.Descricion + " - " + subcuentaGasto.Descricion,
                    Monto = provision.Monto,
                    TipoOperacion = true,
                    NumAsiento = numAsiento,
                    MontoRef = montoReferencia,
                    SimboloRef = monedaPrincipal.First().Simbolo,
                    SimboloMoneda = moneda.First().Simbolo,
                    ValorDolar = monedaPrincipal.First().ValorDolar

                };
                using (var db_context = new PruebaContext())
                {
                    await db_context.AddAsync(provision);
                    await db_context.AddAsync(asientoGastoProvisionado);
                    await db_context.AddAsync(asientoProvision);
                    await db_context.SaveChangesAsync();
                }

                resultado = true;

                return resultado;
            }
            catch (Exception)
            {
                return resultado;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provision"></param>
        /// <param name="idCondominio"></param>
        /// <returns></returns>
        public async Task<int> EditarProvision(Provision provision, int idCondominio)
        {
            var idProvision = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == provision.IdCodCuenta).FirstAsync();
            var idGasto = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == provision.IdCodGasto).FirstAsync();

            // buscar moneda
            var moneda = from c in _context.MonedaConds
                         where c.Simbolo == provision.SimboloMoneda
                         select c;

            // buscar moneda principal
            var monedaPrincipal = await _repoMoneda.MonedaPrincipal(idCondominio);

            // cambiar los montos referencia
            decimal montoReferencia = 0;

            if (moneda.Equals(monedaPrincipal))
            {
                montoReferencia = provision.Monto;
            }
            else if (!moneda.Equals(monedaPrincipal))
            {
                var montoDolares = provision.Monto * moneda.First().ValorDolar;

                montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
            }

            if (idProvision != null && idGasto != null)
            {
                provision.IdCodCuenta = idProvision.IdCodCuenta;
                provision.IdCodGasto = idGasto.IdCodCuenta;
                provision.SimboloRef = monedaPrincipal.First().Simbolo;
                provision.MontoRef = montoReferencia;
                provision.ValorDolar = moneda.First().ValorDolar;
            }

            _context.Update(provision);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ICollection<SubCuenta>> ObtenerFondos(int id)
        {
            var cuentasCond = await ObtenerCuentasCond(id);

            var gruposPatrimonio = from c in _context.Grupos
                                   where c.IdClase == 3
                                   select c;

            var cuentas = from g in gruposPatrimonio.ToList()
                          join c in _context.Cuenta
                          on g.Id equals c.IdGrupo
                          select c;

            var subcuentas = from c in cuentas.ToList()
                             join s in _context.SubCuenta
                             on c.Id equals s.IdCuenta
                             select s;

            var model = from s in subcuentas.ToList()
                        join c in cuentasCond.ToList()
                        on s.Id equals c.IdCodigo
                        select s;

            return model.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fondo"></param>
        /// <returns></returns>
        public async Task<int> CrearFondo(Fondo fondo)
        {
            var idFondo = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == fondo.IdCodCuenta).FirstAsync();

            fondo.IdCodCuenta = idFondo.IdCodCuenta;

            _context.Add(fondo);

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> EliminarSubCuenta(int id)
        {
            var subCuenta = await _context.SubCuenta.FindAsync(id);

            if (subCuenta != null)
            {
                // buscar codigo cuentas -> codigo
                var codigosCuentas = await _context.CodigoCuentasGlobals.Where(c => c.IdCodigo == subCuenta.Id).ToListAsync();
                // eliminar del condominio
                if (codigosCuentas != null && codigosCuentas.Any())
                {
                    foreach (var cuenta in codigosCuentas)
                    {
                        var asientos = await _context.LdiarioGlobals.Where(c => c.IdCodCuenta == cuenta.IdCodCuenta).ToListAsync();

                        if (asientos.Any())
                        {
                            return 0;
                        }
                        //foreach (var asiento in asientos)
                        //{
                        //    _context.LdiarioGlobals.Remove(asiento);
                        //}
                        _context.CodigoCuentasGlobals.Remove(cuenta);
                    }
                }
                // eliminar subcuenta
                _context.SubCuenta.Remove(subCuenta);
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public async Task<ICollection<SubCuenta>> ObtenerBancos(int id)
        {
            var bancos = from c in _context.Cuenta
                         where c.Descripcion.ToUpper().Trim() == "BANCO"
                         select c;

            var subcuentasBancos = from c in _context.SubCuenta
                                   join d in _context.CodigoCuentasGlobals
                                   on c.Id equals d.IdCodigo
                                   where d.IdCondominio == id
                                   where c.IdCuenta == bancos.First().Id
                                   select c;

            return await subcuentasBancos.ToListAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public async Task<ICollection<SubCuenta>> ObtenerCaja(int id)
        {
            var caja = from c in _context.Cuenta
                       where c.Descripcion.ToUpper().Trim() == "CAJA"
                       select c;

            var subcuentasCaja = from c in _context.SubCuenta
                                 join d in _context.CodigoCuentasGlobals
                                 on c.Id equals d.IdCodigo
                                 where d.IdCondominio == id

                                 where c.IdCuenta == caja.First().Id
                                 select c;

            return await subcuentasCaja.ToListAsync();
        }
    }
}
