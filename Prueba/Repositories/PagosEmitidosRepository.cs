using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using Prueba.Context;
using Prueba.Models;
using SQLitePCL;
using Microsoft.AspNetCore.Mvc.Rendering;
using Prueba.ViewModels;
using NPOI.POIFS.Crypt.Dsig;

namespace Prueba.Repositories
{
    public interface IPagosEmitidosRepository
    {
        Task<int> Delete(int id);
        Task<OrdenPagoVM> FormOrdenPago(int id);
        Task<PagoAnticipoVM> FormPagoAnticicipo(int id);
        Task<RegistroPagoVM> FormRegistrarPago(int id);
        Task<PagoNominaVM> FormRegistrarPagoNomina(int id);
        Task<IndexPagosVM> GetPagosEmitidos(int id);
        bool PagoEmitidoExists(int id);
        Task<string> RegistrarAnticipo(PagoAnticipoVM modelo);
        Task<string> RegistrarOrdenPago(OrdenPagoVM modelo);
        Task<string> RegistrarPago(RegistroPagoVM modelo);
        Task<string> RegistrarPagoNomina(PagoNominaVM modelo);
    }
    public class PagosEmitidosRepository : IPagosEmitidosRepository
    {
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly IMonedaRepository _repoMoneda;
        private readonly NuevaAppContext _context;

        public PagosEmitidosRepository(ICuentasContablesRepository repoCuentas,
            IMonedaRepository repoMoneda,
            NuevaAppContext context)
        {
            _repoCuentas = repoCuentas;
            _repoMoneda = repoMoneda;
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IndexPagosVM> GetPagosEmitidos(int id)
        {
            var idCondominio = id;

            var modelo = new IndexPagosVM();

            var listaPagos = (from c in _context.PagoEmitidos
                              where c.IdCondominio == idCondominio
                              select c).Include(c => c.IdCondominioNavigation);

            var referencias = from p in _context.PagoEmitidos
                              where p.IdCondominio == idCondominio
                              join r in _context.ReferenciasPes
                              on p.IdPagoEmitido equals r.IdPagoEmitido
                              select r;

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(idCondominio);


            modelo.PagosEmitidos = await listaPagos.ToListAsync();
            modelo.Referencias = await referencias.ToListAsync();
            modelo.BancosCondominio = subcuentasBancos.ToList();

            return modelo;
        }

        public async Task<int> Delete(int id)
        {
            var pagoEmitido = await _context.PagoEmitidos.FindAsync(id);

            if (pagoEmitido != null)
            {
                if (pagoEmitido.FormaPago)
                {
                    var referencia = await _context.ReferenciasPes.Where(c => c.IdPagoEmitido == pagoEmitido.IdPagoEmitido).ToListAsync();

                    if (referencia != null && referencia.Any())
                    {
                        _context.ReferenciasPes.Remove(referencia.First());
                    }
                }
                _context.PagoEmitidos.Remove(pagoEmitido);

                var pagoAnticipo = await _context.PagoAnticipos.Where(c => c.IdPagoEmitido == pagoEmitido.IdPagoEmitido).ToListAsync();
                var pagoNomina = await _context.PagosNominas.Where(c => c.IdPagoEmitido == pagoEmitido.IdPagoEmitido).ToListAsync();
                var pagoNotaDebito = await _context.PagosNotaDebitos.Where(c => c.IdPagoEmitido == pagoEmitido.IdPagoEmitido).ToListAsync();
                var pagoFactura = await _context.PagoFacturas.Where(c => c.IdPagoEmitido == pagoEmitido.IdPagoEmitido).ToListAsync();
                var ordenPagos = await _context.OrdenPagos.Where(c => c.IdPagoEmitido == pagoEmitido.IdPagoEmitido).ToListAsync();

                if (pagoAnticipo != null)
                {
                    _context.PagoAnticipos.RemoveRange(pagoAnticipo);
                }
                if (pagoNomina != null)
                {
                    _context.PagosNominas.RemoveRange(pagoNomina);
                }
                if (pagoNotaDebito != null)
                {
                    _context.PagosNotaDebitos.RemoveRange(pagoNotaDebito);
                }
                if (pagoNotaDebito != null)
                {
                    _context.PagosNotaDebitos.RemoveRange(pagoNotaDebito);
                }
                if (ordenPagos != null)
                {
                    _context.OrdenPagos.RemoveRange(ordenPagos);
                }
                if (pagoFactura != null)
                {
                    foreach (var item in pagoFactura)
                    {
                        // buscar factura
                        var factura = await _context.Facturas.FindAsync(item.IdFactura);
                        // en proceso nuevamente
                        factura.EnProceso = true;
                        factura.Pagada = false;
                        factura.Abonado -= pagoEmitido.Monto;
                        // restar de abonado el monto del pago
                        // actualizar factura
                        _context.Facturas.Update(factura);
                    }
                    _context.PagoFacturas.RemoveRange(pagoFactura);
                }

            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id del condominio</param>
        /// <returns></returns>
        public async Task<RegistroPagoVM> FormRegistrarPago(int id)
        {
            var modelo = new RegistroPagoVM();

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(id);
            var subcuentasCaja = await _repoCuentas.ObtenerCaja(id);
            var subcuentasModel = await _repoCuentas.ObtenerGastos(id);

            var proveedores = await _repoCuentas.ObtenerProveedores(id);
            //var facturas = await _repoCuentas.ObtenerFacturas(proveedores);
            //var anticipos = await _repoCuentas.ObtenerAnticipos(proveedores); 
            //modelo.Facturas = new List<SelectListItem> { new SelectListItem("Seleccione una factura", "") };
            //modelo.Anticipos = new List<SelectListItem> { new SelectListItem("Seleccione un anticipo", "") };

            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

            modelo.Proveedor = proveedores.Select(c => new SelectListItem(c.Nombre, c.IdProveedor.ToString())).ToList();
            //modelo.Facturas = facturas.Select(c => new SelectListItem(c.Descripcion, c.IdFactura.ToString())).ToList();
            //modelo.Anticipos = anticipos.Select(c => new SelectListItem(c.Detalle, c.IdAnticipo.ToString())).ToList();
            // ENVIAR MODELO

            return modelo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        public async Task<string> RegistrarPago(RegistroPagoVM modelo)
        {
            string resultado = "";
            decimal montoReferencia = 0;

            //var idCodCuenta = await _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).ToListAsync();
            //var idCodCuenta = from c in _context.CodigoCuentasGlobals
            //                  where c.IdSubCuenta == modelo.IdSubcuenta
            //                  select c;


            // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
            // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
            PagoEmitido pago = new PagoEmitido
            {
                IdCondominio = modelo.IdCondominio,
                Fecha = modelo.Fecha,
                Monto = modelo.Monto
            };
            Anticipo anticipo1 = new Anticipo();

            var factura = await _context.Facturas.Where(c => c.IdFactura == modelo.IdFactura).FirstAsync();

            //var cc = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == factura.IdCodCuenta).First();
            //modelo.IdSubcuenta = cc.IdCodCuenta;

            var itemLibroCompra = await _context.LibroCompras.Where(c => c.IdFactura == factura.IdFactura).FirstOrDefaultAsync();
            var itemCuentasPagar = await _context.CuentasPagars.Where(c => c.IdFactura == factura.IdFactura).FirstOrDefaultAsync();
            var proveedor = await _context.Proveedors.Where(c => c.IdProveedor == factura.IdProveedor).FirstOrDefaultAsync();

            if (itemLibroCompra != null)
            {
                if (modelo.retencionesIva && !modelo.retencionesIslr)
                {
                    factura.MontoTotal -= itemLibroCompra.RetIva;
                    pago.Monto -= itemLibroCompra.RetIva;

                    // comprobante de retencion de iva
                }
                else if (!modelo.retencionesIva && modelo.retencionesIslr)
                {
                    factura.MontoTotal -= itemLibroCompra.RetIslr;
                    pago.Monto -= itemLibroCompra.RetIslr;

                    // comprobante de retencion de islr
                }
                else if (modelo.retencionesIva && modelo.retencionesIslr)
                {
                    factura.MontoTotal -= itemLibroCompra.RetIva + itemLibroCompra.RetIslr;
                    pago.Monto -= itemLibroCompra.RetIva + itemLibroCompra.RetIslr;

                    // comprobante de retencion de iva
                    // comprobante de retencion de islr
                }
            }

            var provisiones = from c in _context.Provisiones
                              where c.IdCodGasto == modelo.IdSubcuenta
                              select c;

            var diario = from l in _context.LdiarioGlobals
                         select l;

            int numAsiento = 0;

            var diarioCondominio = from a in _context.LdiarioGlobals
                                   join c in _context.CodigoCuentasGlobals
                                   on a.IdCodCuenta equals c.IdCodCuenta
                                   where c.IdCondominio == modelo.IdCondominio
                                   select a;

            if (diarioCondominio.Count() > 0)
            {
                numAsiento = diarioCondominio.ToList().Last().NumAsiento;
            }

            if (modelo.Pagoforma == FormaPago.Efectivo)
            {
                try
                {
                    var idCaja = (from c in _context.CodigoCuentasGlobals
                                  where c.IdSubCuenta == modelo.IdCodigoCuentaCaja
                                  select c).First();

                    // buscar moneda asigna a la subcuenta
                    var moneda = from m in _context.MonedaConds
                                 join mc in _context.MonedaCuenta
                                 on m.IdMonedaCond equals mc.IdMoneda
                                 where mc.IdCodCuenta == idCaja.IdCodCuenta
                                 select m;

                    // si no es principal hacer el cambio
                    var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.IdCondominio);

                    // calcular monto referencia
                    if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                    {
                        return "No hay monedas registradas en el sistema!";
                    }
                    else if (moneda.First().Equals(monedaPrincipal.First()))
                    {
                        montoReferencia = modelo.Monto / monedaPrincipal.First().ValorDolar;
                    }
                    else if (!moneda.First().Equals(monedaPrincipal.First()))
                    {
                        montoReferencia = modelo.Monto / moneda.First().ValorDolar;

                        //montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                        //montoReferencia = montoDolares;
                    }

                    // disminuir saldo de la cuenta de CAJA
                    var monedaCuenta = (from m in _context.MonedaCuenta
                                        where m.IdCodCuenta == idCaja.IdCodCuenta
                                        select m).First();

                    monedaCuenta.SaldoFinal -= modelo.Monto;
                    // añadir al pago

                    // CRISTHIAN
                    //if (modelo.IdAnticipo != 0)
                    //{
                    //    var anticipos = await _context.Anticipos.Where(a => a.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                    //    anticipo1 = anticipos;
                    //    pago.MontoRef = anticipos.Saldo;
                    //    anticipo1.Activo = false;
                    //}
                    //else
                    //{
                    //    pago.MontoRef = montoReferencia;
                    //}

                    pago.FormaPago = false;
                    pago.SimboloMoneda = moneda.First().Simbolo;
                    pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                    pago.SimboloRef = "$";
                    pago.MontoRef = montoReferencia;

                    // CRISTHIAN 

                    //if (factura.Abonado == 0)
                    //{
                    //    factura.Abonado = pago.MontoRef;
                    //}
                    //else
                    //{
                    //    var montototal = factura.Abonado + pago.MontoRef;
                    //    factura.Abonado = montototal;
                    //}
                    //factura.Pagada = true;
                    //factura.EnProceso = false;

                    // validar si existe anticipo
                    if (modelo.IdAnticipo == 0)
                    {
                        // valido si hay abonado en la factura
                        if (factura.Abonado == 0)
                        {
                            if (pago.Monto < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;

                            }
                            else if (pago.Monto == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";
                            }
                            else
                            {
                                return "El monto es mayor al total de la Factura!";
                            }
                        }
                        else
                        {
                            if ((pago.Monto + factura.Abonado) < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;

                            }
                            else if ((pago.Monto + factura.Abonado) == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";

                            }
                            else
                            {
                                return "El monto más lo abonado en la factura excede el total de la Factura!";
                            }
                        }
                    }
                    else if (modelo.IdAnticipo != 0)
                    {
                        var anticipos = await _context.Anticipos.Where(a => a.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                        anticipo1 = anticipos;
                        //pago.MontoRef = anticipos.Saldo;
                        anticipo1.Activo = false;

                        if (factura.Abonado == 0)
                        {
                            if ((pago.Monto + anticipo1.Saldo) < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + anticipo1.Saldo;

                            }
                            else if ((pago.Monto + anticipo1.Saldo) == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + anticipo1.Saldo;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";

                            }
                            else
                            {
                                return "El monto más el anticipo es mayor al total de la Factura!";
                            }
                        }
                        else
                        {
                            if ((pago.Monto + factura.Abonado + anticipo1.Saldo) < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + factura.Abonado + anticipo1.Saldo;

                            }
                            else if ((pago.Monto + factura.Abonado + anticipo1.Saldo) == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + factura.Abonado + anticipo1.Saldo;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";

                            }
                            else
                            {
                                return "El monto más el anticipo más lo abonado es mayor al total de la Factura!";
                            }
                        }
                    }

                    factura.MontoTotal = itemLibroCompra.BaseImponible + itemLibroCompra.Iva;

                    // resgistrar transaccion
                    // armar transaccion

                    var transaccion = new Transaccion
                    {
                        TipoTransaccion = false,
                        IdCodCuenta = factura.IdCodCuenta,
                        Descripcion = modelo.Concepto,
                        MontoTotal = factura.MontoTotal,
                        Documento = factura.NumFactura.ToString(),
                        Cancelado = factura.MontoTotal,
                        SimboloMoneda = pago.SimboloMoneda,
                        SimboloRef = pago.SimboloRef,
                        ValorDolar = pago.ValorDolar,
                        MontoRef = montoReferencia
                    };

                    using (var _dbContext = new NuevaAppContext())
                    {

                        _dbContext.Add(pago);
                        _dbContext.Add(transaccion);
                        _dbContext.Update(monedaCuenta);
                        _dbContext.Update(factura);
                        _dbContext.Update(itemCuentasPagar);
                        if (modelo.IdAnticipo != 0)
                        {
                            _dbContext.Update(anticipo1);
                        }
                        _dbContext.SaveChanges();
                    }

                    PagoFactura pagoFactura = new PagoFactura
                    {
                        IdPagoEmitido = pago.IdPagoEmitido,
                        IdFactura = modelo.IdFactura,
                        IdAnticipo = anticipo1.IdAnticipo > 0 ? anticipo1.IdAnticipo : null
                    };

                    // registrar comprobantes
                    if (modelo.retencionesIva && !modelo.retencionesIslr)
                    {
                        var compRetencionUltimo = from c in _context.CompRetIvas
                                                         join p in _context.Proveedors
                                                         on c.IdProveedor equals p.IdProveedor
                                                         where p.IdCondominio == modelo.IdCondominio
                                                         orderby c.IdComprobanteIva
                                                         select c;

                        var numRet = 1;

                        if (compRetencionUltimo.Any())
                        {
                            numRet = compRetencionUltimo.Last().NumComprobante + 1;
                        }

                        var stringNumRetFecha = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                        var diferenciaCeros = 14 - (numRet.ToString().Length + stringNumRetFecha.Length);

                        var ceros = new string('0', diferenciaCeros);

                        // comprobante de retencion de iva
                        var comp = new CompRetIva
                        {
                            IdFactura = factura.IdFactura,
                            IdProveedor = factura.IdProveedor,
                            FechaEmision = DateTime.Now,
                            TipoTransaccion = false,
                            NumFacturaAfectada = factura.NumFactura.ToString(),
                            TotalCompraIva = factura.MontoTotal,
                            BaseImponible = factura.Subtotal,
                            Alicuota = 16,
                            ImpIva = factura.Iva,
                            IvaRetenido = itemLibroCompra.RetIva,
                            NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + ceros + numRet.ToString(),
                            NumComprobante = numRet
                        };

                        // actualizar itemLibroCompra
                        itemLibroCompra.NumComprobanteRet = comp.NumComprobante;
                        itemLibroCompra.FechaComprobanteRet = comp.FechaEmision;

                        _context.CompRetIvas.Add(comp);
                        _context.LibroCompras.Update(itemLibroCompra);

                        await _context.SaveChangesAsync();

                    }
                    else if (!modelo.retencionesIva && modelo.retencionesIslr)
                    {
                        var islr = await _context.Islrs.Where(c => c.Id == proveedor.IdRetencionIslr).FirstOrDefaultAsync();

                        if (islr != null)
                        {
                            // comprobante de retencion de islr
                            var compRetIslrUltimo = from c in _context.ComprobanteRetencions
                                                           join p in _context.Proveedors
                                                           on c.IdProveedor equals p.IdProveedor
                                                           where p.IdCondominio == modelo.IdCondominio
                                                           orderby c.IdComprobante
                                                           select c;

                            var numRet = 1;

                            if (compRetIslrUltimo.Any())
                            {
                                numRet = compRetIslrUltimo.Last().NumComprobante + 1;
                            }

                            var stringNumRetFecha = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                            var diferenciaCeros = 14 - (numRet.ToString().Length + stringNumRetFecha.Length);

                            var ceros = new string('0', diferenciaCeros);

                            var compIslr = new ComprobanteRetencion
                            {
                                IdFactura = factura.IdFactura,
                                IdProveedor = factura.IdProveedor,
                                Descripcion = islr.Concepto,
                                Retencion = islr.Tarifa,
                                TotalFactura = itemLibroCompra.Monto,
                                BaseImponible = itemLibroCompra.BaseImponible,
                                Sustraendo = islr.Sustraendo,
                                ValorRetencion = itemLibroCompra.RetIslr,
                                TotalImpuesto = itemLibroCompra.RetIslr,
                                NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + ceros + numRet.ToString(),
                                NumComprobante = numRet,
                                FechaEmision = DateTime.Now
                            };

                            _context.ComprobanteRetencions.Add(compIslr);
                            await _context.SaveChangesAsync();
                        }

                    }
                    else if (modelo.retencionesIva && modelo.retencionesIslr)
                    {
                        // comprobante de retencion de iva
                        var compRetencionUltimo = from c in _context.CompRetIvas
                                                         join p in _context.Proveedors
                                                         on c.IdProveedor equals p.IdProveedor
                                                         where p.IdCondominio == modelo.IdCondominio
                                                         orderby c.IdComprobanteIva
                                                         select c;

                        var numRet = 1;

                        if (compRetencionUltimo.Any())
                        {
                            numRet = compRetencionUltimo.Last().NumComprobante + 1;
                        }

                        var stringNumRetFecha = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                        var diferenciaCeros = 14 - (numRet.ToString().Length + stringNumRetFecha.Length);

                        var ceros = new string('0', diferenciaCeros);

                        // comprobante de retencion de iva
                        var comp = new CompRetIva
                        {
                            IdFactura = factura.IdFactura,
                            IdProveedor = factura.IdProveedor,
                            FechaEmision = DateTime.Now,
                            TipoTransaccion = false,
                            NumFacturaAfectada = factura.NumFactura.ToString(),
                            TotalCompraIva = factura.MontoTotal,
                            BaseImponible = factura.Subtotal,
                            Alicuota = 16,
                            ImpIva = factura.Iva,
                            IvaRetenido = itemLibroCompra.RetIva,
                            NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + ceros + numRet.ToString(),
                            NumComprobante = numRet
                        };

                        // actualizar itemLibroCompra
                        itemLibroCompra.NumComprobanteRet = comp.NumComprobante;
                        itemLibroCompra.FechaComprobanteRet = comp.FechaEmision;

                        
                        
                        // comprobante de retencion de islr

                        var islr = await _context.Islrs.Where(c => c.Id == proveedor.IdRetencionIslr).FirstOrDefaultAsync();

                        if (islr != null)
                        {
                            // comprobante de retencion de islr
                            var compRetIslrUltimo = from c in _context.ComprobanteRetencions
                                                           join p in _context.Proveedors
                                                           on c.IdProveedor equals p.IdProveedor
                                                           where p.IdCondominio == modelo.IdCondominio
                                                           orderby c.IdComprobante
                                                           select c;

                            var numRetIslr = 1;

                            if (compRetIslrUltimo.Any())
                            {
                                numRetIslr = compRetIslrUltimo.Last().NumComprobante + 1;
                            }

                            var stringNumRetFechaIslr = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                            var diferenciaCerosIslr = 14 - (numRetIslr.ToString().Length + stringNumRetFechaIslr.Length);

                            var cerosIslr = new string('0', diferenciaCerosIslr);

                            var compIslr = new ComprobanteRetencion
                            {
                                IdFactura = factura.IdFactura,
                                IdProveedor = factura.IdProveedor,
                                Descripcion = islr.Concepto,
                                Retencion = islr.Tarifa,
                                TotalFactura = itemLibroCompra.Monto,
                                BaseImponible = itemLibroCompra.BaseImponible,
                                Sustraendo = islr.Sustraendo,
                                ValorRetencion = itemLibroCompra.RetIslr,
                                TotalImpuesto = itemLibroCompra.RetIslr,
                                NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + cerosIslr + numRetIslr.ToString(),
                                NumComprobante = numRetIslr,
                                FechaEmision = DateTime.Now
                            };

                            _context.CompRetIvas.Add(comp);
                            _context.LibroCompras.Update(itemLibroCompra);
                            _context.ComprobanteRetencions.Add(compIslr);
                            await _context.SaveChangesAsync();
                        }
                    }

                    //verficar si existe una provision sobre la sub cuenta
                    if (provisiones != null && provisiones.Any())
                    {
                        LdiarioGlobal asientoProvision = new LdiarioGlobal
                        {
                            IdCodCuenta = provisiones.First().IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = provisiones.First().Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo
                        };
                        LdiarioGlobal asientoProvisionCaja = new LdiarioGlobal
                        {
                            IdCodCuenta = idCaja.IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };
                        LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                        {
                            IdCodCuenta = provisiones.First().IdCodGasto,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto - provisiones.First().Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };
                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(asientoProvisionGasto);
                            _dbContext.Add(asientoProvision);
                            _dbContext.Add(asientoProvisionCaja);
                            _dbContext.SaveChanges();
                        }

                        //REGISTRAR ASIENTO EN LA TABLA GASTOS

                        //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                        Activo activoProvision = new Activo
                        {
                            IdAsiento = asientoProvisionCaja.IdAsiento
                        };
                        Pasivo pasivoProvision = new Pasivo
                        {
                            IdAsiento = asientoProvision.IdAsiento
                        };
                        //Gasto gastoProvision = new Gasto
                        //{
                        //    IdAsiento = asientoProvisionGasto.IdAsiento
                        //};

                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(activoProvision);
                            _dbContext.Add(pasivoProvision);
                            //_dbContext.Add(gastoProvision);
                            _dbContext.Add(pagoFactura);
                            _dbContext.SaveChanges();
                        }
                        resultado = "exito";
                    }
                    else
                    {
                        LdiarioGlobal asientoGasto = new LdiarioGlobal
                        {
                            IdCodCuenta = factura.IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };
                        LdiarioGlobal asientoCaja = new LdiarioGlobal
                        {
                            IdCodCuenta = idCaja.IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };

                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(asientoGasto);
                            _dbContext.Add(asientoCaja);
                            //_dbContext.Add(pagoFactura);
                            _dbContext.SaveChanges();
                        }

                        //REGISTRAR ASIENTO EN LA TABLA GASTOS
                        Gasto gasto = new Gasto
                        {
                            IdAsiento = asientoGasto.IdAsiento
                        };
                        //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                        Activo activo = new Activo
                        {
                            IdAsiento = asientoCaja.IdAsiento
                        };

                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(gasto);
                            _dbContext.Add(activo);
                            _dbContext.Add(pagoFactura);
                            _dbContext.SaveChanges();

                        }

                        resultado = "exito";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

            }
            else if (modelo.Pagoforma == FormaPago.Transferencia)
            {
                try
                {

                    var idBanco = (from c in _context.CodigoCuentasGlobals
                                   where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                   select c).First();

                    // buscar moneda asigna a la subcuenta
                    var moneda = from m in _context.MonedaConds
                                 join mc in _context.MonedaCuenta
                                 on m.IdMonedaCond equals mc.IdMoneda
                                 where mc.IdCodCuenta == idBanco.IdCodCuenta
                                 select m;

                    // si no es principal hacer el cambio
                    var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.IdCondominio);

                    // calcular monto referencia
                    if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                    {
                        return "No hay monedas registradas en el sistema!";
                    }
                    else if (moneda.First().Equals(monedaPrincipal.First()))
                    {
                        montoReferencia = modelo.Monto / monedaPrincipal.First().ValorDolar;
                    }
                    else if (!moneda.First().Equals(monedaPrincipal.First()))
                    {
                        montoReferencia = modelo.Monto / moneda.First().ValorDolar;

                        //montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                        //montoReferencia = montoDolares;
                    }

                    // disminuir saldo de la cuenta de CAJA
                    var monedaCuenta = (from m in _context.MonedaCuenta
                                        where m.IdCodCuenta == idBanco.IdCodCuenta
                                        select m).First();

                    monedaCuenta.SaldoFinal -= modelo.Monto;

                    // añadir al pago
                    //if (modelo.IdAnticipo != null && modelo.IdAnticipo != 0)
                    //{
                    //    var anticipos = await _context.Anticipos.Where(a => a.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                    //    anticipo1 = anticipos;
                    //    anticipo1.Activo = false;
                    //}
                    pago.MontoRef = montoReferencia;
                    pago.FormaPago = true;
                    pago.SimboloMoneda = moneda.First().Simbolo;
                    pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                    pago.MontoRef = montoReferencia;
                    pago.SimboloRef = "$";

                    // validar si existe anticipo
                    if (modelo.IdAnticipo == 0)
                    {
                        // valido si hay abonado en la factura
                        if (factura.Abonado == 0)
                        {
                            if (pago.Monto < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;

                            }
                            else if (pago.Monto == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";

                            }
                            else
                            {
                                return "El monto es mayor al total de la Factura!";
                            }
                        }
                        else
                        {
                            if ((pago.Monto + factura.Abonado) < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;

                            }
                            else if ((pago.Monto + factura.Abonado) == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";

                            }
                            else
                            {
                                return "El monto más lo abonado es mayor al total de la Factura!";
                            }
                        }
                    }
                    else if (modelo.IdAnticipo != 0)
                    {
                        var anticipos = await _context.Anticipos.Where(a => a.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                        anticipo1 = anticipos;
                        //pago.MontoRef = anticipos.Saldo;
                        anticipo1.Activo = false;

                        if (factura.Abonado == 0)
                        {
                            if ((pago.Monto + anticipo1.Saldo) < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + anticipo1.Saldo;

                            }
                            else if ((pago.Monto + anticipo1.Saldo) == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + anticipo1.Saldo;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";

                            }
                            else
                            {
                                return "El monto más el aniticipo es mayor al total de la Factura!";
                            }
                        }
                        else
                        {
                            if ((pago.Monto + factura.Abonado + anticipo1.Saldo) < factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + factura.Abonado + anticipo1.Saldo;

                            }
                            else if ((pago.Monto + factura.Abonado + anticipo1.Saldo) == factura.MontoTotal)
                            {
                                factura.Abonado += pago.Monto + factura.Abonado + anticipo1.Saldo;
                                factura.EnProceso = false;
                                factura.Pagada = true;
                                itemCuentasPagar.Status = "Cancelada";

                            }
                            else
                            {
                                return "El monto más el aniticipo más lo abonado es mayor al total de la Factura!";
                            }
                        }
                    }

                    factura.MontoTotal = itemLibroCompra.BaseImponible + itemLibroCompra.Iva;

                    using (var _dbContext = new NuevaAppContext())
                    {

                        _dbContext.Add(pago);
                        _dbContext.Update(monedaCuenta);
                        _dbContext.Update(factura);
                        _dbContext.Update(itemCuentasPagar);
                        if (modelo.IdAnticipo != 0)
                        {
                            _dbContext.Update(anticipo1);
                        }
                        _dbContext.SaveChanges();
                    }

                    PagoFactura pagoFactura = new PagoFactura
                    {
                        IdPagoEmitido = pago.IdPagoEmitido,
                        IdFactura = modelo.IdFactura,
                        IdAnticipo = anticipo1.IdAnticipo > 0 ? anticipo1.IdAnticipo : null
                    };

                    ReferenciasPe referencia = new ReferenciasPe
                    {
                        IdPagoEmitido = pago.IdPagoEmitido,
                        NumReferencia = modelo.NumReferencia,
                        Banco = modelo.IdCodigoCuentaBanco.ToString()
                    };

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(referencia);
                        _dbContext.SaveChanges();
                    }

                    // registrar comprobantes
                    if (modelo.retencionesIva && !modelo.retencionesIslr)
                    {
                        var compRetencionUltimo = await (from c in _context.CompRetIvas
                                                         join p in _context.Proveedors
                                                         on c.IdProveedor equals p.IdProveedor
                                                         where p.IdCondominio == modelo.IdCondominio
                                                         orderby c.IdComprobanteIva
                                                         select c).LastAsync();

                        var numRet = compRetencionUltimo.NumComprobante + 1;

                        var stringNumRetFecha = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                        var diferenciaCeros = 14 - (numRet.ToString().Length + stringNumRetFecha.Length);

                        var ceros = new string('0', diferenciaCeros);

                        // comprobante de retencion de iva
                        var comp = new CompRetIva
                        {
                            IdFactura = factura.IdFactura,
                            IdProveedor = factura.IdProveedor,
                            FechaEmision = DateTime.Now,
                            TipoTransaccion = false,
                            NumFacturaAfectada = factura.NumFactura.ToString(),
                            TotalCompraIva = factura.MontoTotal,
                            BaseImponible = factura.Subtotal,
                            Alicuota = 16,
                            ImpIva = factura.Iva,
                            IvaRetenido = itemLibroCompra.RetIva,
                            NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + ceros + numRet.ToString(),
                            NumComprobante = numRet
                        };

                        // actualizar itemLibroCompra
                        itemLibroCompra.NumComprobanteRet = comp.NumComprobante;
                        itemLibroCompra.FechaComprobanteRet = comp.FechaEmision;

                        _context.CompRetIvas.Add(comp);
                        _context.LibroCompras.Update(itemLibroCompra);

                        await _context.SaveChangesAsync();

                    }
                    else if (!modelo.retencionesIva && modelo.retencionesIslr)
                    {
                        var islr = await _context.Islrs.Where(c => c.Id == proveedor.IdRetencionIslr).FirstOrDefaultAsync();

                        if (islr != null)
                        {
                            // comprobante de retencion de islr
                            var compRetIslrUltimo = await (from c in _context.ComprobanteRetencions
                                                           join p in _context.Proveedors
                                                           on c.IdProveedor equals p.IdProveedor
                                                           where p.IdCondominio == modelo.IdCondominio
                                                           orderby c.IdComprobante
                                                           select c).LastAsync();

                            var numRet = compRetIslrUltimo.NumComprobante + 1;

                            var stringNumRetFecha = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                            var diferenciaCeros = 14 - (numRet.ToString().Length + stringNumRetFecha.Length);

                            var ceros = new string('0', diferenciaCeros);

                            var compIslr = new ComprobanteRetencion
                            {
                                IdFactura = factura.IdFactura,
                                IdProveedor = factura.IdProveedor,
                                Descripcion = islr.Concepto,
                                Retencion = islr.Tarifa,
                                TotalFactura = itemLibroCompra.Monto,
                                BaseImponible = itemLibroCompra.BaseImponible,
                                Sustraendo = islr.Sustraendo,
                                ValorRetencion = itemLibroCompra.RetIslr,
                                TotalImpuesto = itemLibroCompra.RetIslr,
                                NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + ceros + numRet.ToString(),
                                NumComprobante = numRet,
                                FechaEmision = DateTime.Now
                            };

                            _context.ComprobanteRetencions.Add(compIslr);
                            await _context.SaveChangesAsync();
                        }

                    }
                    else if (modelo.retencionesIva && modelo.retencionesIslr)
                    {
                        // comprobante de retencion de iva
                        var compRetencionUltimo = await (from c in _context.CompRetIvas
                                                         join p in _context.Proveedors
                                                         on c.IdProveedor equals p.IdProveedor
                                                         where p.IdCondominio == modelo.IdCondominio
                                                         orderby c.IdComprobanteIva
                                                         select c).LastAsync();

                        var numRet = compRetencionUltimo.NumComprobante + 1;

                        var stringNumRetFecha = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                        var diferenciaCeros = 14 - (numRet.ToString().Length + stringNumRetFecha.Length);

                        var ceros = new string('0', diferenciaCeros);

                        // comprobante de retencion de iva
                        var comp = new CompRetIva
                        {
                            IdFactura = factura.IdFactura,
                            IdProveedor = factura.IdProveedor,
                            FechaEmision = DateTime.Now,
                            TipoTransaccion = false,
                            NumFacturaAfectada = factura.NumFactura.ToString(),
                            TotalCompraIva = factura.MontoTotal,
                            BaseImponible = factura.Subtotal,
                            Alicuota = 16,
                            ImpIva = factura.Iva,
                            IvaRetenido = itemLibroCompra.RetIva,
                            NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + ceros + numRet.ToString(),
                            NumComprobante = numRet
                        };

                        // actualizar itemLibroCompra
                        itemLibroCompra.NumComprobanteRet = comp.NumComprobante;
                        itemLibroCompra.FechaComprobanteRet = comp.FechaEmision;



                        // comprobante de retencion de islr

                        var islr = await _context.Islrs.Where(c => c.Id == proveedor.IdRetencionIslr).FirstOrDefaultAsync();

                        if (islr != null)
                        {
                            // comprobante de retencion de islr
                            var compRetIslrUltimo = await (from c in _context.ComprobanteRetencions
                                                           join p in _context.Proveedors
                                                           on c.IdProveedor equals p.IdProveedor
                                                           where p.IdCondominio == modelo.IdCondominio
                                                           orderby c.IdComprobante
                                                           select c).LastAsync();

                            var numRetIslr = compRetIslrUltimo.NumComprobante + 1;

                            var stringNumRetFechaIslr = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString();

                            var diferenciaCerosIslr = 14 - (numRetIslr.ToString().Length + stringNumRetFechaIslr.Length);

                            var cerosIslr = new string('0', diferenciaCerosIslr);

                            var compIslr = new ComprobanteRetencion
                            {
                                IdFactura = factura.IdFactura,
                                IdProveedor = factura.IdProveedor,
                                Descripcion = islr.Concepto,
                                Retencion = islr.Tarifa,
                                TotalFactura = itemLibroCompra.Monto,
                                BaseImponible = itemLibroCompra.BaseImponible,
                                Sustraendo = islr.Sustraendo,
                                ValorRetencion = itemLibroCompra.RetIslr,
                                TotalImpuesto = itemLibroCompra.RetIslr,
                                NumCompRet = DateTime.Today.Year.ToString() + "-" + DateTime.Today.Month.ToString() + "-" + cerosIslr + numRetIslr.ToString(),
                                NumComprobante = numRetIslr,
                                FechaEmision = DateTime.Now
                            };

                            _context.CompRetIvas.Add(comp);
                            _context.LibroCompras.Update(itemLibroCompra);
                            _context.ComprobanteRetencions.Add(compIslr);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (provisiones != null && provisiones.Any())
                    {
                        LdiarioGlobal asientoProvision = new LdiarioGlobal
                        {
                            IdCodCuenta = provisiones.First().IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = provisiones.First().Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };
                        LdiarioGlobal asientoProvisionBanco = new LdiarioGlobal
                        {
                            IdCodCuenta = idBanco.IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };
                        LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                        {
                            IdCodCuenta = provisiones.First().IdCodGasto,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto - provisiones.First().Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };
                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(asientoProvisionGasto);
                            _dbContext.Add(asientoProvision);
                            _dbContext.Add(asientoProvisionBanco);
                            _dbContext.SaveChanges();
                        }

                        //REGISTRAR ASIENTO EN LA TABLA GASTOS

                        //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                        Activo activoProvision = new Activo
                        {
                            IdAsiento = asientoProvisionBanco.IdAsiento
                        };
                        Pasivo pasivoProvision = new Pasivo
                        {
                            IdAsiento = asientoProvision.IdAsiento
                        };
                        //Gasto gastoProvision = new Gasto
                        //{
                        //    IdAsiento = asientoProvisionGasto.IdAsiento
                        //};

                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(activoProvision);
                            _dbContext.Add(pasivoProvision);
                            // _dbContext.Add(gastoProvision);
                            _dbContext.SaveChanges();
                        }

                        return "exito";
                    }
                    else
                    {
                        //REGISTRAR ASIENTO EN EL DIARIO (idCC, fecha, descripcion, concepto, monto, tipoOperacion)
                        //buscar el id en codigo de cuentas global de la subcuenta seleccionada

                        LdiarioGlobal asientoGasto = new LdiarioGlobal
                        {
                            IdCodCuenta = factura.IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };
                        LdiarioGlobal asientoBanco = new LdiarioGlobal
                        {
                            IdCodCuenta = idBanco.IdCodCuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            MontoRef = montoReferencia,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            ValorDolar = monedaPrincipal.First().ValorDolar,
                            SimboloMoneda = moneda.First().Simbolo,
                            SimboloRef = monedaPrincipal.First().Simbolo

                        };

                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(asientoGasto);
                            _dbContext.Add(asientoBanco);
                            _dbContext.SaveChanges();
                        }

                        //REGISTRAR ASIENTO EN LA TABLA GASTOS
                        Gasto gasto = new Gasto
                        {
                            IdAsiento = asientoGasto.IdAsiento
                        };
                        //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                        Activo activo = new Activo
                        {
                            IdAsiento = asientoBanco.IdAsiento
                        };

                        using (var _dbContext = new NuevaAppContext())
                        {
                            _dbContext.Add(gasto);
                            _dbContext.Add(activo);
                            _dbContext.Add(pagoFactura);
                            _dbContext.SaveChanges();
                        }

                        return "exito";
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            return resultado;
        }

        public async Task<PagoNominaVM> FormRegistrarPagoNomina(int id)
        {
            var modelo = new PagoNominaVM();

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(id);
            var subcuentasCaja = await _repoCuentas.ObtenerCaja(id);
            var subcuentasModel = await _repoCuentas.ObtenerGastos(id);
            var empleados = from c in _context.CondominioNominas
                            join y in _context.Empleados
                            on c.IdEmpleado equals y.IdEmpleado
                            where c.IdCondominio == id
                            select y;

            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.Empleados = empleados.Select(c => new SelectListItem(c.Nombre, c.IdEmpleado.ToString())).ToList();

            return modelo;
        }

        public async Task<string> RegistrarPagoNomina(PagoNominaVM modelo)
        {
            using (var context = new NuevaAppContext())
            {
                var cc = context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).First();
                modelo.IdSubcuenta = cc.IdCodCuenta;

                var resultado = string.Empty;
                decimal montoReferencia = 0;
                decimal deducciones = 0;
                decimal percepciones = 0;

                // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
                // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
                PagoEmitido pago = new PagoEmitido
                {
                    IdCondominio = modelo.IdCondominio,
                    Fecha = modelo.Fecha,
                    Monto = modelo.Monto
                };

                var provisiones = from c in _context.Provisiones
                                  where c.IdCodGasto == modelo.IdSubcuenta
                                  select c;

                var diario = from l in _context.LdiarioGlobals
                             select l;

                int numAsiento = 0;

                var diarioCondominio = from a in _context.LdiarioGlobals
                                       join c in _context.CodigoCuentasGlobals
                                       on a.IdCodCuenta equals c.IdCodCuenta
                                       where c.IdCondominio == modelo.IdCondominio
                                       select a;

                if (diarioCondominio.Count() > 0)
                {
                    numAsiento = diarioCondominio.ToList().Last().NumAsiento;
                }
                if (modelo.Pagoforma == FormaPago.Efectivo)
                {
                    try
                    {
                        var idCaja = (from c in _context.CodigoCuentasGlobals
                                      where c.IdSubCuenta == modelo.IdCodigoCuentaCaja
                                      select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in _context.MonedaConds
                                     join mc in _context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idCaja.IdCodCuenta
                                     select m;

                        // si no es principal hacer el cambio
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.IdCondominio);

                        // calcular monto referencia
                        if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                        {
                            return "No hay monedas registradas en el sistema!";
                        }
                        else if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / monedaPrincipal.First().ValorDolar;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / moneda.First().ValorDolar;
                        }

                        // disminuir saldo de la cuenta de CAJA
                        var monedaCuenta = (from m in _context.MonedaCuenta
                                            where m.IdCodCuenta == idCaja.IdCodCuenta
                                            select m).First();

                        monedaCuenta.SaldoFinal -= modelo.Monto;
                        // añadir al pago

                        pago.FormaPago = false;
                        pago.SimboloMoneda = moneda.First().Simbolo;
                        pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                        pago.SimboloRef = "$";
                        pago.MontoRef = montoReferencia;

                        // verificar bono
                        if (modelo.Bonos)
                        {
                            var bonos = (await _context.Bonificaciones
                                .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                                .ToListAsync()).Sum(c => c.Monto);

                            pago.Monto += bonos;

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / moneda.First().ValorDolar;
                            }
                        }

                        // calcular el pago con las deducciones/percepciones o no
                        if (modelo.percepciones && !modelo.deducciones)
                        {
                            percepciones = (await _context.Percepciones
                            .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                            .ToListAsync()).Sum(c => c.Monto);

                            pago.Monto += percepciones;

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / moneda.First().ValorDolar;
                            }

                        }
                        else if (!modelo.percepciones && modelo.deducciones)
                        {
                            deducciones = (await _context.Deducciones
                            .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                            .ToListAsync()).Sum(c => c.Monto);

                            pago.Monto -= deducciones;

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / moneda.First().ValorDolar;
                            }
                        }
                        else if (modelo.percepciones && modelo.deducciones)
                        {
                            deducciones = (await _context.Deducciones
                            .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                            .ToListAsync()).Sum(c => c.Monto);

                            percepciones = (await _context.Percepciones
                                .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                                .ToListAsync()).Sum(c => c.Monto);

                            pago.Monto += percepciones - deducciones;

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / moneda.First().ValorDolar;
                            }
                        }
                        else
                        {
                            var empleado = await _context.Empleados.FindAsync(modelo.IdEmpleado);
                            if (empleado != null)
                            {
                                if (pago.Monto > empleado.BaseSueldo)
                                {
                                    return "El monto a pagar excede la base del sueldo de este empleado";
                                }
                            }
                        }

                        // armar Recibo Nomina
                        var reciboNomina = new ReciboNomina
                        {
                            IdEmpleado = modelo.IdEmpleado,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            PagoTotal = pago.Monto,
                            RefMonto = pago.MontoRef,
                            Entregado = true,
                            Periodo = true
                        };

                        // armar transaccion
                        var transaccion = new Transaccion
                        {
                            TipoTransaccion = false,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Descripcion = modelo.Concepto,
                            MontoTotal = pago.Monto,
                            Documento = "",
                            Cancelado = pago.Monto,
                            SimboloMoneda = pago.SimboloMoneda,
                            SimboloRef = pago.SimboloRef,
                            ValorDolar = pago.ValorDolar,
                            MontoRef = pago.MontoRef
                        };

                        // registrar pago emitido
                        // registrar recibo nomina
                        // registrar transaccion 

                        context.PagoEmitidos.Add(pago);
                        context.ReciboNominas.Add(reciboNomina);
                        context.Transaccions.Add(transaccion);
                        await context.SaveChangesAsync();


                        // registrar pago nomina

                        var pagoNomina = new PagosNomina
                        {
                            IdPagoEmitido = pago.IdPagoEmitido,
                            IdReciboNomina = reciboNomina.IdReciboNomina
                        };

                        context.PagosNominas.Add(pagoNomina);
                        await context.SaveChangesAsync();


                        // asientos contables

                        //verficar si existe una provision sobre la sub cuenta
                        if (provisiones != null && provisiones.Any())
                        {
                            LdiarioGlobal asientoProvision = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo
                            };
                            LdiarioGlobal asientoProvisionCaja = new LdiarioGlobal
                            {
                                IdCodCuenta = idCaja.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodGasto,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto - provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };

                            if (modelo.percepciones)
                            {
                                foreach (var percepcion in _context.Percepciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoPercepcion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)percepcion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = percepcion.Concepto,
                                        Monto = percepcion.Monto,
                                        MontoRef = percepcion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoPercepcion);
                                }
                            }

                            if (modelo.deducciones)
                            {
                                foreach (var deduccion in _context.Deducciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoDeduccion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)deduccion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = deduccion.Concepto,
                                        Monto = deduccion.Monto,
                                        MontoRef = deduccion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoDeduccion);
                                }
                            }

                            if (modelo.Bonos)
                            {
                                foreach (var bono in _context.Bonificaciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoBono = new LdiarioGlobal
                                    {
                                        IdCodCuenta = bono.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = bono.Concepto,
                                        Monto = bono.Monto,
                                        MontoRef = bono.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoBono);
                                }
                            }

                            context.Add(asientoProvisionGasto);
                            context.Add(asientoProvision);
                            context.Add(asientoProvisionCaja);
                            context.SaveChanges();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS

                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activoProvision = new Activo
                            {
                                IdAsiento = asientoProvisionCaja.IdAsiento
                            };
                            Pasivo pasivoProvision = new Pasivo
                            {
                                IdAsiento = asientoProvision.IdAsiento
                            };
                            //Gasto gastoProvision = new Gasto
                            //{
                            //    IdAsiento = asientoProvisionGasto.IdAsiento
                            //};


                            context.Add(activoProvision);
                            context.Add(pasivoProvision);
                            //_dbContext.Add(gastoProvision);
                            context.SaveChanges();

                            resultado = "exito";
                        }
                        else
                        {
                            // asientos para deducciones y percepciones
                            // calcular el pago con las deducciones/percepciones o no
                            if (modelo.percepciones)
                            {
                                foreach (var percepcion in _context.Percepciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoPercepcion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)percepcion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = percepcion.Concepto,
                                        Monto = percepcion.Monto,
                                        MontoRef = percepcion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoPercepcion);
                                }                                
                            }

                            if (modelo.deducciones)
                            {
                                foreach (var deduccion in _context.Deducciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoDeduccion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)deduccion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = deduccion.Concepto,
                                        Monto = deduccion.Monto,
                                        MontoRef = deduccion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoDeduccion);
                                }
                            }

                            if (modelo.Bonos)
                            {
                                foreach (var bono in _context.Bonificaciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoBono = new LdiarioGlobal
                                    {
                                        IdCodCuenta = bono.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = bono.Concepto,
                                        Monto = bono.Monto,
                                        MontoRef = bono.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoBono);
                                }
                            }

                            LdiarioGlobal asientoGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = modelo.IdSubcuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoCaja = new LdiarioGlobal
                            {
                                IdCodCuenta = idCaja.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };

                            context.Add(asientoGasto);
                            context.Add(asientoCaja);
                            //_dbContext.Add(pagoFactura);
                            context.SaveChanges();

                            

                            //REGISTRAR ASIENTO EN LA TABLA GASTOS
                            Gasto gasto = new Gasto
                            {
                                IdAsiento = asientoGasto.IdAsiento
                            };
                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activo = new Activo
                            {
                                IdAsiento = asientoCaja.IdAsiento
                            };


                            context.Add(gasto);
                            context.Add(activo);
                            await context.SaveChangesAsync();


                            resultado = "exito";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }

                }
                else if (modelo.Pagoforma == FormaPago.Transferencia)
                {
                    try
                    {

                        var idBanco = (from c in _context.CodigoCuentasGlobals
                                       where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                       select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in _context.MonedaConds
                                     join mc in _context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idBanco.IdCodCuenta
                                     select m;

                        // si no es principal hacer el cambio
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.IdCondominio);

                        // calcular monto referencia
                        if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                        {
                            return "No hay monedas registradas en el sistema!";
                        }
                        else if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / monedaPrincipal.First().ValorDolar;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / moneda.First().ValorDolar;

                            //montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                            //montoReferencia = montoDolares;
                        }

                        // disminuir saldo de la cuenta de CAJA
                        var monedaCuenta = (from m in _context.MonedaCuenta
                                            where m.IdCodCuenta == idBanco.IdCodCuenta
                                            select m).First();

                        monedaCuenta.SaldoFinal -= modelo.Monto;

                        // añadir al pago
                        //if (modelo.IdAnticipo != null && modelo.IdAnticipo != 0)
                        //{
                        //    var anticipos = await _context.Anticipos.Where(a => a.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                        //    anticipo1 = anticipos;
                        //    anticipo1.Activo = false;
                        //}
                        pago.MontoRef = montoReferencia;
                        pago.FormaPago = true;
                        pago.SimboloMoneda = moneda.First().Simbolo;
                        pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                        pago.MontoRef = montoReferencia;
                        pago.SimboloRef = "$";

                        // calcular el pago con las deducciones/percepciones o no
                        if (modelo.percepciones && !modelo.deducciones)
                        {
                            percepciones = (await _context.Percepciones
                            .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                            .ToListAsync()).Sum(c => c.Monto);

                            pago.Monto += percepciones;

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / moneda.First().ValorDolar;
                            }

                        }
                        else if (!modelo.percepciones && modelo.deducciones)
                        {
                            deducciones = (await _context.Deducciones
                            .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                            .ToListAsync()).Sum(c => c.Monto);

                            pago.Monto -= deducciones;

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / moneda.First().ValorDolar;
                            }
                        }
                        else
                        {
                            deducciones = (await _context.Deducciones
                            .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                            .ToListAsync()).Sum(c => c.Monto);

                            percepciones = (await _context.Percepciones
                                .Where(c => c.IdEmpleado == modelo.IdEmpleado)
                                .ToListAsync()).Sum(c => c.Monto);

                            pago.Monto += percepciones - deducciones;

                            if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                pago.MontoRef = pago.Monto / moneda.First().ValorDolar;
                            }
                        }

                        // armar Recibo Nomina
                        var reciboNomina = new ReciboNomina
                        {
                            IdEmpleado = modelo.IdEmpleado,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            PagoTotal = pago.Monto,
                            RefMonto = pago.MontoRef,
                            Entregado = true,
                            Periodo = true
                        };

                        // armar transaccion
                        var transaccion = new Transaccion
                        {
                            TipoTransaccion = false,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Descripcion = modelo.Concepto,
                            MontoTotal = pago.Monto,
                            Documento = "",
                            Cancelado = pago.Monto,
                            SimboloMoneda = pago.SimboloMoneda,
                            SimboloRef = pago.SimboloRef,
                            ValorDolar = pago.ValorDolar,
                            MontoRef = pago.MontoRef
                        };

                        // registrar pago emitido
                        // registrar recibo nomina
                        // registrar transaccion 

                        context.PagoEmitidos.Add(pago);
                        context.ReciboNominas.Add(reciboNomina);
                        context.Transaccions.Add(transaccion);
                        await context.SaveChangesAsync();


                        // registrar pago nomina

                        var pagoNomina = new PagosNomina
                        {
                            IdPagoEmitido = pago.IdPagoEmitido,
                            IdReciboNomina = reciboNomina.IdReciboNomina
                        };

                        context.PagosNominas.Add(pagoNomina);


                        ReferenciasPe referencia = new ReferenciasPe
                        {
                            IdPagoEmitido = pago.IdPagoEmitido,
                            NumReferencia = modelo.NumReferencia,
                            Banco = modelo.IdCodigoCuentaBanco.ToString()
                        };


                        context.Add(referencia);

                        if (provisiones != null && provisiones.Any())
                        {
                            LdiarioGlobal asientoProvision = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionBanco = new LdiarioGlobal
                            {
                                IdCodCuenta = idBanco.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodGasto,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto - provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };

                            if (modelo.percepciones)
                            {
                                foreach (var percepcion in _context.Percepciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoPercepcion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)percepcion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = percepcion.Concepto,
                                        Monto = percepcion.Monto,
                                        MontoRef = percepcion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoPercepcion);
                                }
                            }

                            if (modelo.deducciones)
                            {
                                foreach (var deduccion in _context.Deducciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoDeduccion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)deduccion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = deduccion.Concepto,
                                        Monto = deduccion.Monto,
                                        MontoRef = deduccion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoDeduccion);
                                }
                            }

                            if (modelo.Bonos)
                            {
                                foreach (var bono in _context.Bonificaciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoBono = new LdiarioGlobal
                                    {
                                        IdCodCuenta = bono.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = bono.Concepto,
                                        Monto = bono.Monto,
                                        MontoRef = bono.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoBono);
                                }
                            }

                            context.Add(asientoProvisionGasto);
                            context.Add(asientoProvision);
                            context.Add(asientoProvisionBanco);
                            await context.SaveChangesAsync();

                            //REGISTRAR ASIENTO EN LA TABLA GASTOS

                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activoProvision = new Activo
                            {
                                IdAsiento = asientoProvisionBanco.IdAsiento
                            };
                            Pasivo pasivoProvision = new Pasivo
                            {
                                IdAsiento = asientoProvision.IdAsiento
                            };
                            //Gasto gastoProvision = new Gasto
                            //{
                            //    IdAsiento = asientoProvisionGasto.IdAsiento
                            //};


                            context.Add(activoProvision);
                            context.Add(pasivoProvision);
                            // _dbContext.Add(gastoProvision);
                            await context.SaveChangesAsync();


                            return "exito";
                        }
                        else
                        {
                            if (modelo.percepciones)
                            {
                                foreach (var percepcion in _context.Percepciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoPercepcion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)percepcion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = percepcion.Concepto,
                                        Monto = percepcion.Monto,
                                        MontoRef = percepcion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoPercepcion);
                                }
                            }

                            if (modelo.deducciones)
                            {
                                foreach (var deduccion in _context.Deducciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoDeduccion = new LdiarioGlobal
                                    {
                                        IdCodCuenta = (int)deduccion.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = deduccion.Concepto,
                                        Monto = deduccion.Monto,
                                        MontoRef = deduccion.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoDeduccion);
                                }
                            }

                            if (modelo.Bonos)
                            {
                                foreach (var bono in _context.Bonificaciones.Where(c => c.IdEmpleado == modelo.IdEmpleado).ToList())
                                {
                                    LdiarioGlobal asientoBono = new LdiarioGlobal
                                    {
                                        IdCodCuenta = bono.IdCodCuenta,
                                        Fecha = modelo.Fecha,
                                        Concepto = bono.Concepto,
                                        Monto = bono.Monto,
                                        MontoRef = bono.RefMonto,
                                        TipoOperacion = true,
                                        NumAsiento = numAsiento + 1,
                                        ValorDolar = monedaPrincipal.First().ValorDolar,
                                        SimboloMoneda = moneda.First().Simbolo,
                                        SimboloRef = monedaPrincipal.First().Simbolo

                                    };

                                    context.Add(asientoBono);
                                }
                            }

                            //REGISTRAR ASIENTO EN EL DIARIO (idCC, fecha, descripcion, concepto, monto, tipoOperacion)
                            //buscar el id en codigo de cuentas global de la subcuenta seleccionada

                            LdiarioGlobal asientoGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = modelo.IdSubcuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoBanco = new LdiarioGlobal
                            {
                                IdCodCuenta = idBanco.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };


                            context.Add(asientoGasto);
                            context.Add(asientoBanco);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS
                            Gasto gasto = new Gasto
                            {
                                IdAsiento = asientoGasto.IdAsiento
                            };
                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activo = new Activo
                            {
                                IdAsiento = asientoBanco.IdAsiento
                            };


                            context.Add(gasto);
                            context.Add(activo);
                            //_dbContext.Add(pagoFactura);
                            await context.SaveChangesAsync();


                            return "exito";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }

                return resultado;

            }
        }

        public async Task<OrdenPagoVM> FormOrdenPago(int id)
        {
            var modelo = new OrdenPagoVM();

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(id);
            var subcuentasCaja = await _repoCuentas.ObtenerCaja(id);
            var subcuentasModel = await _repoCuentas.ObtenerGastos(id);

            var proveedores = await _repoCuentas.ObtenerProveedores(id);
            //var facturas = await _repoCuentas.ObtenerFacturas(proveedores);
            //var anticipos = await _repoCuentas.ObtenerAnticipos(proveedores); 
            //modelo.Facturas = new List<SelectListItem> { new SelectListItem("Seleccione una factura", "") };
            //modelo.Anticipos = new List<SelectListItem> { new SelectListItem("Seleccione un anticipo", "") };

            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

            modelo.Proveedor = proveedores.Select(c => new SelectListItem(c.Nombre, c.IdProveedor.ToString())).ToList();
            //modelo.Facturas = facturas.Select(c => new SelectListItem(c.Descripcion, c.IdFactura.ToString())).ToList();
            //modelo.Anticipos = anticipos.Select(c => new SelectListItem(c.Detalle, c.IdAnticipo.ToString())).ToList();
            // ENVIAR MODELO

            return modelo;
        }

        public async Task<string> RegistrarOrdenPago(OrdenPagoVM modelo)
        {

            using (var context = new NuevaAppContext())
            {
                string resultado = "";
                decimal montoReferencia = 0;
                var cc = context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).First();
                modelo.IdSubcuenta = cc.IdCodCuenta;


                // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
                // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
                PagoEmitido pago = new PagoEmitido
                {
                    IdCondominio = modelo.Pago.IdCondominio,
                    Fecha = modelo.Pago.Fecha,
                    Monto = modelo.Pago.Monto
                };

                var provisiones = from c in context.Provisiones
                                  where c.IdCodGasto == modelo.IdSubcuenta
                                  select c;

                var diario = from l in context.LdiarioGlobals
                             select l;

                int numAsiento = 0;

                var diarioCondominio = from a in context.LdiarioGlobals
                                       join c in context.CodigoCuentasGlobals
                                       on a.IdCodCuenta equals c.IdCodCuenta
                                       where c.IdCondominio == modelo.Pago.IdCondominio
                                       select a;

                if (diarioCondominio.Count() > 0)
                {
                    numAsiento = diarioCondominio.ToList().Last().NumAsiento;
                }

                if (modelo.Pagoforma == FormaPago.Efectivo)
                {
                    try
                    {
                        var idCaja = (from c in context.CodigoCuentasGlobals
                                      where c.IdSubCuenta == modelo.IdCodigoCuentaCaja
                                      select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in context.MonedaConds
                                     join mc in context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idCaja.IdCodCuenta
                                     select m;

                        // si no es principal hacer el cambio
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.Pago.IdCondominio);

                        // calcular monto referencia
                        if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                        {
                            return "No hay monedas registradas en el sistema!";
                        }
                        else if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Pago.Monto / monedaPrincipal.First().ValorDolar;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Pago.Monto / moneda.First().ValorDolar;
                        }

                        // disminuir saldo de la cuenta de CAJA
                        var monedaCuenta = (from m in context.MonedaCuenta
                                            where m.IdCodCuenta == idCaja.IdCodCuenta
                                            select m).First();

                        monedaCuenta.SaldoFinal -= modelo.Pago.Monto;

                        pago.FormaPago = false;
                        pago.SimboloMoneda = moneda.First().Simbolo;
                        pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                        pago.SimboloRef = "$";
                        pago.MontoRef = montoReferencia;

                        // resgistrar transaccion
                        // armar transaccion
                        var transaccion = new Transaccion
                        {
                            TipoTransaccion = false,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Descripcion = modelo.Concepto,
                            MontoTotal = pago.Monto,
                            Documento = "",
                            Cancelado = pago.Monto,
                            SimboloMoneda = pago.SimboloMoneda,
                            SimboloRef = pago.SimboloRef,
                            ValorDolar = pago.ValorDolar,
                            MontoRef = pago.MontoRef
                        };

                        // registrar pago
                        context.PagoEmitidos.Add(pago);
                        context.Transaccions.Add(transaccion);
                        await context.SaveChangesAsync();

                        // registrar la orden de pago
                        var ordenPago = new OrdenPago
                        {
                            IdPagoEmitido = pago.IdPagoEmitido,
                            IdProveedor = modelo.IdProveedor,
                            Fecha = pago.Fecha
                        };

                        context.OrdenPagos.Add(ordenPago);
                        await context.SaveChangesAsync();


                        //verficar si existe una provision sobre la sub cuenta
                        if (provisiones != null && provisiones.Any())
                        {
                            LdiarioGlobal asientoProvision = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodCuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo
                            };
                            LdiarioGlobal asientoProvisionCaja = new LdiarioGlobal
                            {
                                IdCodCuenta = idCaja.IdCodCuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodGasto,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto - provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };

                            context.Add(asientoProvisionGasto);
                            context.Add(asientoProvision);
                            context.Add(asientoProvisionCaja);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS

                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activoProvision = new Activo
                            {
                                IdAsiento = asientoProvisionCaja.IdAsiento
                            };
                            Pasivo pasivoProvision = new Pasivo
                            {
                                IdAsiento = asientoProvision.IdAsiento
                            };



                            context.Add(activoProvision);
                            context.Add(pasivoProvision);
                            await context.SaveChangesAsync();

                            resultado = "exito";
                        }
                        else
                        {
                            LdiarioGlobal asientoGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = modelo.IdSubcuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoCaja = new LdiarioGlobal
                            {
                                IdCodCuenta = idCaja.IdCodCuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };


                            context.Add(asientoGasto);
                            context.Add(asientoCaja);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS
                            Gasto gasto = new Gasto
                            {
                                IdAsiento = asientoGasto.IdAsiento
                            };
                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activo = new Activo
                            {
                                IdAsiento = asientoCaja.IdAsiento
                            };


                            context.Add(gasto);
                            context.Add(activo);
                            await context.SaveChangesAsync();


                            resultado = "exito";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }

                }
                else if (modelo.Pagoforma == FormaPago.Transferencia)
                {
                    try
                    {

                        var idBanco = (from c in _context.CodigoCuentasGlobals
                                       where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                       select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in _context.MonedaConds
                                     join mc in _context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idBanco.IdCodCuenta
                                     select m;

                        // si no es principal hacer el cambio
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.Pago.IdCondominio);

                        // calcular monto referencia
                        if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                        {
                            return "No hay monedas registradas en el sistema!";
                        }
                        else if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Pago.Monto / monedaPrincipal.First().ValorDolar;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Pago.Monto / moneda.First().ValorDolar;
                        }

                        // disminuir saldo de la cuenta de CAJA
                        var monedaCuenta = (from m in _context.MonedaCuenta
                                            where m.IdCodCuenta == idBanco.IdCodCuenta
                                            select m).First();

                        monedaCuenta.SaldoFinal -= modelo.Pago.Monto;


                        pago.MontoRef = montoReferencia;
                        pago.FormaPago = true;
                        pago.SimboloMoneda = moneda.First().Simbolo;
                        pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                        pago.MontoRef = montoReferencia;
                        pago.SimboloRef = "$";

                        // resgistrar transaccion
                        // armar transaccion
                        var transaccion = new Transaccion
                        {
                            TipoTransaccion = false,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Descripcion = modelo.Concepto,
                            MontoTotal = pago.Monto,
                            Documento = "",
                            Cancelado = pago.Monto,
                            SimboloMoneda = pago.SimboloMoneda,
                            SimboloRef = pago.SimboloRef,
                            ValorDolar = pago.ValorDolar,
                            MontoRef = pago.MontoRef
                        };

                        // registrar pago
                        context.PagoEmitidos.Add(pago);
                        await context.SaveChangesAsync();

                        // registrar la orden de pago
                        var ordenPago = new OrdenPago
                        {
                            IdPagoEmitido = pago.IdPagoEmitido,
                            IdProveedor = modelo.IdProveedor,
                            Fecha = pago.Fecha
                        };

                        ReferenciasPe referencia = new ReferenciasPe
                        {
                            IdPagoEmitido = pago.IdPagoEmitido,
                            NumReferencia = modelo.NumReferencia,
                            Banco = modelo.IdCodigoCuentaBanco.ToString()
                        };


                        context.Add(referencia);
                        context.OrdenPagos.Add(ordenPago);
                        context.Transaccions.Add(transaccion);

                        await context.SaveChangesAsync();


                        if (provisiones != null && provisiones.Any())
                        {
                            LdiarioGlobal asientoProvision = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodCuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionBanco = new LdiarioGlobal
                            {
                                IdCodCuenta = idBanco.IdCodCuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodGasto,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto - provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };

                            context.Add(asientoProvisionGasto);
                            context.Add(asientoProvision);
                            context.Add(asientoProvisionBanco);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS

                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activoProvision = new Activo
                            {
                                IdAsiento = asientoProvisionBanco.IdAsiento
                            };
                            Pasivo pasivoProvision = new Pasivo
                            {
                                IdAsiento = asientoProvision.IdAsiento
                            };


                            context.Add(activoProvision);
                            context.Add(pasivoProvision);
                            await context.SaveChangesAsync();


                            return "exito";
                        }
                        else
                        {
                            //REGISTRAR ASIENTO EN EL DIARIO (idCC, fecha, descripcion, concepto, monto, tipoOperacion)
                            //buscar el id en codigo de cuentas global de la subcuenta seleccionada

                            LdiarioGlobal asientoGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = modelo.IdSubcuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoBanco = new LdiarioGlobal
                            {
                                IdCodCuenta = idBanco.IdCodCuenta,
                                Fecha = modelo.Pago.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Pago.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };


                            context.Add(asientoGasto);
                            context.Add(asientoBanco);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS
                            Gasto gasto = new Gasto
                            {
                                IdAsiento = asientoGasto.IdAsiento
                            };
                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activo = new Activo
                            {
                                IdAsiento = asientoBanco.IdAsiento
                            };


                            context.Add(gasto);
                            context.Add(activo);
                            await context.SaveChangesAsync();

                            return "exito";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                return resultado;

            }

        }


        // form para pago de anticipo
        public async Task<PagoAnticipoVM> FormPagoAnticicipo(int id)
        {
            var modelo = new PagoAnticipoVM();

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(id);
            var subcuentasCaja = await _repoCuentas.ObtenerCaja(id);
            var subcuentasModel = await _repoCuentas.ObtenerGastos(id);

            var proveedores = await _repoCuentas.ObtenerProveedores(id);

            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.Proveedor = proveedores.Select(c => new SelectListItem(c.Nombre, c.IdProveedor.ToString())).ToList();
            // ENVIAR MODELO

            return modelo;
        }

        // regitrar pago de anticipo
        public async Task<string> RegistrarAnticipo(PagoAnticipoVM modelo)
        {
            using (var context = new NuevaAppContext())
            {
                string resultado = "";
                decimal montoReferencia = 0;
                var cc = context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).First();
                modelo.IdSubcuenta = cc.IdCodCuenta;


                // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
                // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
                PagoEmitido pago = new PagoEmitido
                {
                    IdCondominio = modelo.IdCondominio,
                    Fecha = modelo.Fecha,
                    Monto = modelo.Monto
                };

                var provisiones = from c in context.Provisiones
                                  where c.IdCodGasto == modelo.IdSubcuenta
                                  select c;

                var diario = from l in context.LdiarioGlobals
                             select l;

                int numAsiento = 0;

                var diarioCondominio = from a in context.LdiarioGlobals
                                       join c in context.CodigoCuentasGlobals
                                       on a.IdCodCuenta equals c.IdCodCuenta
                                       where c.IdCondominio == modelo.IdCondominio
                                       select a;

                if (diarioCondominio.Count() > 0)
                {
                    numAsiento = diarioCondominio.ToList().Last().NumAsiento;
                }

                if (modelo.Pagoforma == FormaPago.Efectivo)
                {
                    try
                    {
                        var idCaja = (from c in context.CodigoCuentasGlobals
                                      where c.IdSubCuenta == modelo.IdCodigoCuentaCaja
                                      select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in context.MonedaConds
                                     join mc in context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idCaja.IdCodCuenta
                                     select m;

                        // si no es principal hacer el cambio
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.IdCondominio);

                        // calcular monto referencia
                        if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                        {
                            return "No hay monedas registradas en el sistema!";
                        }
                        else if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / monedaPrincipal.First().ValorDolar;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / moneda.First().ValorDolar;
                        }

                        // disminuir saldo de la cuenta de CAJA
                        var monedaCuenta = (from m in context.MonedaCuenta
                                            where m.IdCodCuenta == idCaja.IdCodCuenta
                                            select m).First();

                        monedaCuenta.SaldoFinal -= modelo.Monto;

                        pago.FormaPago = false;
                        pago.SimboloMoneda = moneda.First().Simbolo;
                        pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                        pago.SimboloRef = "$";
                        pago.MontoRef = montoReferencia;

                        // resgistrar transaccion
                        // armar transaccion
                        var transaccion = new Transaccion
                        {
                            TipoTransaccion = false,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Descripcion = modelo.Concepto,
                            MontoTotal = pago.Monto,
                            Documento = "",
                            Cancelado = pago.Monto,
                            SimboloMoneda = pago.SimboloMoneda,
                            SimboloRef = pago.SimboloRef,
                            ValorDolar = pago.ValorDolar,
                            MontoRef = pago.MontoRef
                        };

                        // registrar pago
                        context.PagoEmitidos.Add(pago);
                        context.Transaccions.Add(transaccion);
                        await context.SaveChangesAsync();

                        // registrar la Anticipo

                        var anticipo = new Anticipo
                        {
                            Numero = pago.IdPagoEmitido,
                            Fecha = modelo.Fecha,
                            Saldo = pago.Monto,
                            Detalle = modelo.Concepto,
                            IdProveedor = modelo.IdProveedor,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Activo = true
                        };

                        context.Anticipos.Add(anticipo);
                        await context.SaveChangesAsync();


                        //verficar si existe una provision sobre la sub cuenta
                        if (provisiones != null && provisiones.Any())
                        {
                            LdiarioGlobal asientoProvision = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo
                            };
                            LdiarioGlobal asientoProvisionCaja = new LdiarioGlobal
                            {
                                IdCodCuenta = idCaja.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodGasto,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto - provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };

                            context.Add(asientoProvisionGasto);
                            context.Add(asientoProvision);
                            context.Add(asientoProvisionCaja);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS

                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activoProvision = new Activo
                            {
                                IdAsiento = asientoProvisionCaja.IdAsiento
                            };
                            Pasivo pasivoProvision = new Pasivo
                            {
                                IdAsiento = asientoProvision.IdAsiento
                            };



                            context.Add(activoProvision);
                            context.Add(pasivoProvision);
                            await context.SaveChangesAsync();

                            resultado = "exito";
                        }
                        else
                        {
                            LdiarioGlobal asientoGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = modelo.IdSubcuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoCaja = new LdiarioGlobal
                            {
                                IdCodCuenta = idCaja.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };


                            context.Add(asientoGasto);
                            context.Add(asientoCaja);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS
                            Gasto gasto = new Gasto
                            {
                                IdAsiento = asientoGasto.IdAsiento
                            };
                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activo = new Activo
                            {
                                IdAsiento = asientoCaja.IdAsiento
                            };


                            context.Add(gasto);
                            context.Add(activo);
                            await context.SaveChangesAsync();


                            resultado = "exito";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }

                }
                else if (modelo.Pagoforma == FormaPago.Transferencia)
                {
                    try
                    {

                        var idBanco = (from c in _context.CodigoCuentasGlobals
                                       where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                       select c).First();

                        // buscar moneda asigna a la subcuenta
                        var moneda = from m in _context.MonedaConds
                                     join mc in _context.MonedaCuenta
                                     on m.IdMonedaCond equals mc.IdMoneda
                                     where mc.IdCodCuenta == idBanco.IdCodCuenta
                                     select m;

                        // si no es principal hacer el cambio
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(modelo.IdCondominio);

                        // calcular monto referencia
                        if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                        {
                            return "No hay monedas registradas en el sistema!";
                        }
                        else if (moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / monedaPrincipal.First().ValorDolar;
                        }
                        else if (!moneda.First().Equals(monedaPrincipal.First()))
                        {
                            montoReferencia = modelo.Monto / moneda.First().ValorDolar;
                        }

                        // disminuir saldo de la cuenta de CAJA
                        var monedaCuenta = (from m in _context.MonedaCuenta
                                            where m.IdCodCuenta == idBanco.IdCodCuenta
                                            select m).First();

                        monedaCuenta.SaldoFinal -= modelo.Monto;


                        pago.MontoRef = montoReferencia;
                        pago.FormaPago = true;
                        pago.SimboloMoneda = moneda.First().Simbolo;
                        pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                        pago.MontoRef = montoReferencia;
                        pago.SimboloRef = "$";

                        // resgistrar transaccion
                        // armar transaccion
                        var transaccion = new Transaccion
                        {
                            TipoTransaccion = false,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Descripcion = modelo.Concepto,
                            MontoTotal = pago.Monto,
                            Documento = "",
                            Cancelado = pago.Monto,
                            SimboloMoneda = pago.SimboloMoneda,
                            SimboloRef = pago.SimboloRef,
                            ValorDolar = pago.ValorDolar,
                            MontoRef = pago.MontoRef
                        };

                        // registrar pago
                        context.PagoEmitidos.Add(pago);
                        await context.SaveChangesAsync();

                        // registrar la Anticipo

                        var anticipo = new Anticipo
                        {
                            Numero = pago.IdPagoEmitido,
                            Fecha = modelo.Fecha,
                            Saldo = pago.Monto,
                            Detalle = modelo.Concepto,
                            IdProveedor = modelo.IdProveedor,
                            IdCodCuenta = modelo.IdSubcuenta,
                            Activo = true
                        };

                        ReferenciasPe referencia = new ReferenciasPe
                        {
                            IdPagoEmitido = pago.IdPagoEmitido,
                            NumReferencia = modelo.NumReferencia,
                            Banco = modelo.IdCodigoCuentaBanco.ToString()
                        };


                        context.Add(referencia);
                        context.Anticipos.Add(anticipo);
                        context.Transaccions.Add(transaccion);

                        await context.SaveChangesAsync();


                        if (provisiones != null && provisiones.Any())
                        {
                            LdiarioGlobal asientoProvision = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionBanco = new LdiarioGlobal
                            {
                                IdCodCuenta = idBanco.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = provisiones.First().IdCodGasto,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto - provisiones.First().Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };

                            context.Add(asientoProvisionGasto);
                            context.Add(asientoProvision);
                            context.Add(asientoProvisionBanco);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS

                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activoProvision = new Activo
                            {
                                IdAsiento = asientoProvisionBanco.IdAsiento
                            };
                            Pasivo pasivoProvision = new Pasivo
                            {
                                IdAsiento = asientoProvision.IdAsiento
                            };


                            context.Add(activoProvision);
                            context.Add(pasivoProvision);
                            await context.SaveChangesAsync();

                            return "exito";
                        }
                        else
                        {
                            //REGISTRAR ASIENTO EN EL DIARIO (idCC, fecha, descripcion, concepto, monto, tipoOperacion)
                            //buscar el id en codigo de cuentas global de la subcuenta seleccionada

                            LdiarioGlobal asientoGasto = new LdiarioGlobal
                            {
                                IdCodCuenta = modelo.IdSubcuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };
                            LdiarioGlobal asientoBanco = new LdiarioGlobal
                            {
                                IdCodCuenta = idBanco.IdCodCuenta,
                                Fecha = modelo.Fecha,
                                Concepto = modelo.Concepto,
                                Monto = modelo.Monto,
                                MontoRef = montoReferencia,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloMoneda = moneda.First().Simbolo,
                                SimboloRef = monedaPrincipal.First().Simbolo

                            };


                            context.Add(asientoGasto);
                            context.Add(asientoBanco);
                            await context.SaveChangesAsync();


                            //REGISTRAR ASIENTO EN LA TABLA GASTOS
                            Gasto gasto = new Gasto
                            {
                                IdAsiento = asientoGasto.IdAsiento
                            };
                            //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                            Activo activo = new Activo
                            {
                                IdAsiento = asientoBanco.IdAsiento
                            };


                            context.Add(gasto);
                            context.Add(activo);
                            await context.SaveChangesAsync();

                            return "exito";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                return resultado;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool PagoEmitidoExists(int id)
        {
            return (_context.PagoEmitidos?.Any(e => e.IdPagoEmitido == id)).GetValueOrDefault();
        }
    }
}