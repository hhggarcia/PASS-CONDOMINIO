using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Org.BouncyCastle.Ocsp;
using Prueba.Context;
using Prueba.Models;
using Prueba.ViewModels;
using SQLitePCL;
using System.Runtime.InteropServices;
using System.Text;

namespace Prueba.Repositories
{
    public interface IPagosRecibidosRepository
    {
        Task<CobroTransitoVM> FormCobroTransito(int id);
        Task<PagoFacturaEmitidaVM> FormPagoFacturaEmitida(int id);
        Task<IndexPagoFacturaEmitidaVM> GetPagosFacturasEmitidas(int id);
        Task<string> RegistrarCobroTransito(CobroTransitoVM modelo);
        Task<string> RegistrarPago(PagoFacturaEmitidaVM modelo);
        Task<string> RegistrarPagoPropietario(PagoRecibidoVM modelo);
        Task<string> RegistrarPagoPropietarioAdmin(PagoRecibidoVM modelo);
    }
    public class PagosRecibidosRepository : IPagosRecibidosRepository
    {
        private readonly IMonedaRepository _repoMoneda;
        private readonly ICuentasContablesRepository _repoCuentas;
        private readonly NuevaAppContext _context;

        public PagosRecibidosRepository(IMonedaRepository repoMoneda,
            NuevaAppContext context,
            ICuentasContablesRepository repoCuentas)
        {
            _repoMoneda = repoMoneda;
            _repoCuentas = repoCuentas;
            _context = context;
        }

        public async Task<IndexPagoFacturaEmitidaVM> GetPagosFacturasEmitidas(int id)
        {
            var modelo = new IndexPagoFacturaEmitidaVM();

            var listaFacturas = from c in _context.Clientes
                                where c.IdCondominio == id
                                join f in _context.FacturaEmitida
                                on c.IdCliente equals f.IdCliente
                                select f;

            var listaPagos = from f in listaFacturas
                             join p in _context.PagoFacturaEmitida
                             on f.IdFacturaEmitida equals p.IdFactura
                             join pago in _context.PagoRecibidos
                             on p.IdPagoRecibido equals pago.IdPagoRecibido
                             select pago;

            var referencias = from p in listaPagos
                              join r in _context.ReferenciasPrs
                              on p.IdPagoRecibido equals r.IdPagoRecibido
                              select r;

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(id);


            modelo.PagosRecibidos = await listaPagos.ToListAsync();
            modelo.Referencias = await referencias.ToListAsync();
            modelo.BancosCondominio = subcuentasBancos.ToList();

            return modelo;
        }
        public async Task<PagoFacturaEmitidaVM> FormPagoFacturaEmitida(int id)
        {
            var modelo = new PagoFacturaEmitidaVM();

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(id);
            var subcuentasCaja = await _repoCuentas.ObtenerCaja(id);
            var subcuentasModel = await _repoCuentas.ObtenerIngresos(id);

            var clientes = await _repoCuentas.ObtenerClientes(id);

            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

            modelo.Clientes = clientes.Select(c => new SelectListItem(c.Nombre, c.IdCliente.ToString())).ToList();
            // ENVIAR MODELO

            return modelo;
        }

        /// <summary>
        /// Registrar pago recibido de los clientes
        /// a sus facturas de ventas
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        public async Task<string> RegistrarPago(PagoFacturaEmitidaVM modelo)
        {
            var resultado = string.Empty;
            decimal montoReferencia = 0;

            // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
            // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
            PagoRecibido pago = new PagoRecibido
            {
                IdCondominio = modelo.IdCondominio,
                Fecha = modelo.Fecha,
                Monto = modelo.Monto,
                Concepto = modelo.Concepto,
                Confirmado = true,
                Activo = true
            };
            // Anticipo anticipo1 = new Anticipo();

            var factura = await _context.FacturaEmitida.Where(c => c.IdFacturaEmitida == modelo.IdFactura).FirstAsync();

            var cliente = await _context.Clientes.FirstAsync(c => c.IdCliente == modelo.IdCliente);

            var itemLibroVenta = await _context.LibroVentas.Where(c => c.IdFactura == factura.IdFacturaEmitida).FirstOrDefaultAsync();

            var itemCuentaCobrar = await _context.CuentasCobrars.Where(c => c.IdFactura == factura.IdFacturaEmitida).FirstOrDefaultAsync();

            if (itemLibroVenta != null && cliente != null)
            {
                if (modelo.RetencionesIva && !modelo.RetencionesIslr)
                {
                    factura.MontoTotal -= itemLibroVenta.RetIva;
                    pago.Monto -= itemLibroVenta.RetIva;
                }
                else if (!modelo.RetencionesIva && modelo.RetencionesIslr)
                {
                    factura.MontoTotal -= itemLibroVenta.RetIslr;
                    pago.Monto -= itemLibroVenta.RetIslr;

                }
                else if (modelo.RetencionesIva && modelo.RetencionesIslr)
                {
                    factura.MontoTotal -= itemLibroVenta.RetIva + itemLibroVenta.RetIslr;
                    pago.Monto -= itemLibroVenta.RetIva + itemLibroVenta.RetIslr;
                }
            }

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

                    // valido si hay abonado en la factura
                    if (factura.Abonado == 0)
                    {
                        if (pago.Monto < factura.MontoTotal)
                        {
                            factura.Abonado += pago.Monto;
                            cliente.Deuda -= pago.Monto;
                        }
                        else if (pago.Monto == factura.MontoTotal)
                        {
                            factura.Abonado += pago.Monto;
                            factura.EnProceso = false;
                            factura.Pagada = true;
                            itemCuentaCobrar.Status = "Cancelada";
                            cliente.Deuda -= pago.Monto;

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
                            cliente.Deuda -= pago.Monto;
                        }
                        else if ((pago.Monto + factura.Abonado) == factura.MontoTotal)
                        {
                            factura.Abonado += pago.Monto;
                            factura.EnProceso = false;
                            factura.Pagada = true;
                            itemCuentaCobrar.Status = "Cancelada";
                            cliente.Deuda -= pago.Monto;
                        }
                        else
                        {
                            return "El monto más lo abonado en la factura excede el total de la Factura!";
                        }
                    }



                    factura.MontoTotal = itemLibroVenta.BaseImponible + itemLibroVenta.Iva;

                    // resgistrar transaccion
                    // armar transaccion
                    //var transaccion = new Transaccion
                    //{
                    //    TipoTransaccion = true,
                    //    IdCodCuenta = factura.IdCodCuenta,
                    //    Descripcion = modelo.Concepto,
                    //    MontoTotal = factura.MontoTotal,
                    //    Documento = factura.NumFactura.ToString(),
                    //    Cancelado = factura.MontoTotal,
                    //    SimboloMoneda = pago.SimboloMoneda,
                    //    SimboloRef = pago.SimboloRef,
                    //    ValorDolar = pago.ValorDolar,
                    //    MontoRef = montoReferencia,
                    //    Fecha = DateTime.Today
                    //};

                    // validar retenciones

                    if (modelo.RetencionesIva)
                    {
                        var retIva = new CompRetIvaCliente
                        {
                            IdFactura = modelo.IdFactura,
                            IdCliente = modelo.IdCliente,
                            FechaEmision = modelo.FechaEmisionRetIva,
                            TipoTransaccion = true,
                            NumFacturaAfectada = factura.NumFactura.ToString(),
                            TotalCompraIva = factura.MontoTotal,
                            CompraSinCreditoIva = 0,
                            BaseImponible = itemLibroVenta.BaseImponible,
                            Alicuota = 16,
                            ImpIva = itemLibroVenta.Iva,
                            IvaRetenido = itemLibroVenta.RetIva,
                            TotalCompraRetIva = factura.MontoTotal - itemLibroVenta.RetIva,
                            NumCompRet = modelo.NumComprobanteRetIva,
                            NumComprobante = 1
                        };

                        itemLibroVenta.ComprobanteRetencion = modelo.NumComprobanteRetIva;

                        _context.Update(itemLibroVenta);
                        _context.Add(retIva);
                    }

                    if (modelo.RetencionesIslr)
                    {
                        var ret = (from c in _context.Clientes
                                   join v in _context.Islrs
                                   on c.IdRetencionIslr equals v.Id
                                   select v).FirstOrDefault();
                        if (ret != null)
                        {
                            var retIslr = new ComprobanteRetencionCliente
                            {
                                IdCliente = modelo.IdCliente,
                                IdFactura = modelo.IdFactura,
                                FechaEmision = modelo.FechaEmisionIslr,
                                Description = ret.Concepto,
                                Retencion = ret.Tarifa,
                                Sustraendo = ret.Sustraendo,
                                ValorRetencion = itemLibroVenta.RetIslr,
                                TotalImpuesto = itemLibroVenta.RetIslr,
                                NumCompRet = modelo.NumComprobanteRetIslr,
                                NumComprobante = 1,
                                TotalFactura = factura.MontoTotal,
                                BaseImponible = itemLibroVenta.BaseImponible
                            };

                            itemLibroVenta.ComprobanteRetencion = modelo.NumComprobanteRetIslr;

                            _context.Update(itemLibroVenta);
                            _context.Add(retIslr);

                        }
                    }

                    using (var _dbContext = new NuevaAppContext())
                    {

                        _dbContext.Add(pago);
                        //_dbContext.Add(transaccion);
                        _dbContext.Update(monedaCuenta);
                        _dbContext.Update(factura);
                        _context.Update(itemCuentaCobrar);
                        _context.Update(cliente);

                        _dbContext.SaveChanges();
                    }

                    PagoFacturaEmitida pagoFactura = new PagoFacturaEmitida
                    {
                        IdPagoRecibido = pago.IdPagoRecibido,
                        IdFactura = modelo.IdFactura
                    };


                    LdiarioGlobal asientoIngreso = new LdiarioGlobal
                    {
                        IdCodCuenta = factura.IdCodCuenta,
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
                    LdiarioGlobal asientoCaja = new LdiarioGlobal
                    {
                        IdCodCuenta = idCaja.IdCodCuenta,
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

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(asientoCaja);
                        _dbContext.Add(asientoIngreso);

                        //_dbContext.Add(pagoFactura);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN LA TABLA Ingresos
                    Ingreso ingreso = new Ingreso
                    {
                        IdAsiento = asientoIngreso.IdAsiento
                    };
                    //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                    Activo activo = new Activo
                    {
                        IdAsiento = asientoCaja.IdAsiento
                    };

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(ingreso);
                        _dbContext.Add(activo);
                        _dbContext.Add(pagoFactura);
                        _dbContext.SaveChanges();

                    }
                    resultado = "exito";

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

                    // valido si hay abonado en la factura
                    if (factura.Abonado == 0)
                    {
                        if (pago.Monto < factura.MontoTotal)
                        {
                            factura.Abonado += pago.Monto;
                            cliente.Deuda -= pago.Monto;

                        }
                        else if (pago.Monto == factura.MontoTotal)
                        {
                            factura.Abonado += pago.Monto;
                            factura.EnProceso = false;
                            factura.Pagada = true;
                            itemCuentaCobrar.Status = "Cancelada";
                            cliente.Deuda -= pago.Monto;

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
                            cliente.Deuda -= pago.Monto;

                        }
                        else if ((pago.Monto + factura.Abonado) == factura.MontoTotal)
                        {
                            factura.Abonado += pago.Monto;
                            factura.EnProceso = false;
                            factura.Pagada = true;
                            itemCuentaCobrar.Status = "Cancelada";
                            cliente.Deuda -= pago.Monto;

                        }
                        else
                        {
                            return "El monto más lo abonado es mayor al total de la Factura!";
                        }
                    }

                    factura.MontoTotal = itemLibroVenta.BaseImponible + itemLibroVenta.Iva;

                    // validar retenciones

                    if (modelo.RetencionesIva)
                    {
                        var retIva = new CompRetIvaCliente
                        {
                            IdFactura = modelo.IdFactura,
                            IdCliente = modelo.IdCliente,
                            FechaEmision = modelo.FechaEmisionRetIva,
                            TipoTransaccion = true,
                            NumFacturaAfectada = factura.NumFactura.ToString(),
                            TotalCompraIva = factura.MontoTotal,
                            CompraSinCreditoIva = 0,
                            BaseImponible = itemLibroVenta.BaseImponible,
                            Alicuota = 16,
                            ImpIva = itemLibroVenta.Iva,
                            IvaRetenido = itemLibroVenta.RetIva,
                            TotalCompraRetIva = factura.MontoTotal - itemLibroVenta.RetIva,
                            NumCompRet = modelo.NumComprobanteRetIva,
                            NumComprobante = 1
                        };

                        itemLibroVenta.ComprobanteRetencion = modelo.NumComprobanteRetIva;

                        _context.Update(itemLibroVenta);
                        _context.Add(retIva);
                    }

                    if (modelo.RetencionesIslr)
                    {
                        var ret = (from c in _context.Clientes
                                   join v in _context.Islrs
                                   on c.IdRetencionIslr equals v.Id
                                   select v).FirstOrDefault();
                        if (ret != null)
                        {
                            var retIslr = new ComprobanteRetencionCliente
                            {
                                IdCliente = modelo.IdCliente,
                                IdFactura = modelo.IdFactura,
                                FechaEmision = modelo.FechaEmisionIslr,
                                Description = ret.Concepto,
                                Retencion = ret.Tarifa,
                                Sustraendo = ret.Sustraendo,
                                ValorRetencion = itemLibroVenta.RetIslr,
                                TotalImpuesto = itemLibroVenta.RetIslr,
                                NumCompRet = modelo.NumComprobanteRetIslr,
                                NumComprobante = 1,
                                TotalFactura = factura.MontoTotal,
                                BaseImponible = itemLibroVenta.BaseImponible
                            };

                            itemLibroVenta.ComprobanteRetencion = modelo.NumComprobanteRetIslr;

                            _context.Update(itemLibroVenta);
                            _context.Add(retIslr);

                        }
                    }

                    using (var _dbContext = new NuevaAppContext())
                    {

                        _dbContext.Add(pago);
                        _dbContext.Update(monedaCuenta);
                        _dbContext.Update(factura);
                        _context.Update(itemCuentaCobrar);
                        _context.Update(cliente);

                        _dbContext.SaveChanges();
                    }

                    PagoFacturaEmitida pagoFactura = new PagoFacturaEmitida
                    {
                        IdPagoRecibido = pago.IdPagoRecibido,
                        IdFactura = modelo.IdFactura
                    };

                    ReferenciasPr referencia = new ReferenciasPr
                    {
                        IdPagoRecibido = pago.IdPagoRecibido,
                        NumReferencia = modelo.NumReferencia,
                        Banco = modelo.IdCodigoCuentaBanco.ToString()
                    };

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(referencia);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN EL DIARIO (idCC, fecha, descripcion, concepto, monto, tipoOperacion)
                    //buscar el id en codigo de cuentas global de la subcuenta seleccionada

                    LdiarioGlobal asientoGasto = new LdiarioGlobal
                    {
                        IdCodCuenta = factura.IdCodCuenta,
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
                    LdiarioGlobal asientoBanco = new LdiarioGlobal
                    {
                        IdCodCuenta = idBanco.IdCodCuenta,
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

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(asientoBanco);
                        _dbContext.Add(asientoGasto);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN LA TABLA GASTOS
                    Ingreso ingreso = new Ingreso
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
                        _dbContext.Add(ingreso);
                        _dbContext.Add(activo);
                        _dbContext.Add(pagoFactura);
                        _dbContext.SaveChanges();
                    }

                    return "exito";

                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            return resultado;
        }

        public async Task<CobroTransitoVM> FormCobroTransito(int id)
        {
            var modelo = new CobroTransitoVM();

            var subcuentasBancos = await _repoCuentas.ObtenerBancos(id);
            var subcuentasCaja = await _repoCuentas.ObtenerCaja(id);
            var subcuentasModel = await _repoCuentas.ObtenerSubcuentas(id);

            //var clientes = await _repoCuentas.ObtenerClientes(id);

            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

            //modelo.Clientes = clientes.Select(c => new SelectListItem(c.Nombre, c.IdCliente.ToString())).ToList();
            // ENVIAR MODELO

            return modelo;
        }

        public async Task<string> RegistrarCobroTransito(CobroTransitoVM modelo)
        {
            var resultado = string.Empty;
            decimal montoReferencia = 0;

            //var idCodCuenta = await _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).ToListAsync();
            //var idCodCuenta = from c in _context.CodigoCuentasGlobals
            //                  where c.IdSubCuenta == modelo.IdSubcuenta
            //                  select c;

            var cc = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).First();
            modelo.IdSubcuenta = cc.IdCodCuenta;

            // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
            // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
            PagoRecibido pago = new PagoRecibido
            {
                IdCondominio = modelo.IdCondominio,
                Fecha = modelo.Fecha,
                Monto = modelo.Monto,
                Concepto = modelo.Concepto,
                Confirmado = true,
                Activo = true
            };

            //var provisiones = from c in _context.Provisiones
            //                  where c.IdCodGasto == modelo.IdSubcuenta
            //                  select c;

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

                    pago.FormaPago = false;
                    pago.SimboloMoneda = moneda.First().Simbolo;
                    pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                    pago.SimboloRef = "$";
                    pago.MontoRef = montoReferencia;


                    // registrar cobro en transito
                    var cobroTransito = new CobroTransito
                    {
                        IdCondominio = modelo.IdCondominio,
                        FormaPago = false,
                        Monto = pago.Monto,
                        Fecha = pago.Fecha,
                        Concepto = modelo.Concepto,
                        ValorDolar = pago.ValorDolar,
                        MontoRef = montoReferencia,
                        SimboloMoneda = pago.SimboloMoneda,
                        SimboloRef = pago.SimboloRef,
                        Asignado = false,
                        Activo = true
                    };

                    // registrar transaccion
                    // armar transaccion
                    //var transaccion = new Transaccion
                    //{
                    //    TipoTransaccion = true,
                    //    IdCodCuenta = modelo.IdSubcuenta,
                    //    Descripcion = modelo.Concepto,
                    //    MontoTotal = pago.Monto,
                    //    Documento = "",
                    //    Cancelado = pago.Monto,
                    //    SimboloMoneda = pago.SimboloMoneda,
                    //    SimboloRef = pago.SimboloRef,
                    //    ValorDolar = pago.ValorDolar,
                    //    MontoRef = pago.MontoRef,
                    //    Fecha = DateTime.Today
                    //};

                    using (var _dbContext = new NuevaAppContext())
                    {

                        _dbContext.Add(pago);
                        _dbContext.Add(cobroTransito);
                        //_dbContext.Add(transaccion);
                        _dbContext.Update(monedaCuenta);

                        _dbContext.SaveChanges();
                    }

                    // relacion cobro transito <-> pago recibido
                    var pagoCobrotransito = new PagoCobroTransito
                    {
                        IdCobroTransito = cobroTransito.IdCobroTransito,
                        IdPagoRecibido = pago.IdPagoRecibido
                    };

                    LdiarioGlobal asientoIngreso = new LdiarioGlobal
                    {
                        IdCodCuenta = modelo.IdSubcuenta,
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
                    LdiarioGlobal asientoCaja = new LdiarioGlobal
                    {
                        IdCodCuenta = idCaja.IdCodCuenta,
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

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(asientoCaja);
                        _dbContext.Add(asientoIngreso);

                        //_dbContext.Add(pagoFactura);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN LA TABLA Ingresos
                    Ingreso ingreso = new Ingreso
                    {
                        IdAsiento = asientoIngreso.IdAsiento
                    };
                    //REGISTRAR ASIENTO EN LA TABLA ACTIVO
                    Activo activo = new Activo
                    {
                        IdAsiento = asientoCaja.IdAsiento
                    };

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(ingreso);
                        _dbContext.Add(activo);
                        _dbContext.Add(pagoCobrotransito);
                        _dbContext.SaveChanges();

                    }
                    resultado = "exito";

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

                    // registrar cobro en transito
                    var cobroTransito = new CobroTransito
                    {
                        IdCondominio = modelo.IdCondominio,
                        FormaPago = true,
                        Monto = pago.Monto,
                        Fecha = pago.Fecha,
                        Concepto = modelo.Concepto,
                        ValorDolar = pago.ValorDolar,
                        MontoRef = montoReferencia,
                        SimboloMoneda = pago.SimboloMoneda,
                        SimboloRef = pago.SimboloRef,
                        Asignado = false,
                        Activo = true
                    };

                    // registrar transaccion
                    // armar transaccion
                    //var transaccion = new Transaccion
                    //{
                    //    TipoTransaccion = true,
                    //    IdCodCuenta = modelo.IdSubcuenta,
                    //    Descripcion = modelo.Concepto,
                    //    MontoTotal = pago.Monto,
                    //    Documento = "",
                    //    Cancelado = pago.Monto,
                    //    SimboloMoneda = pago.SimboloMoneda,
                    //    SimboloRef = pago.SimboloRef,
                    //    ValorDolar = pago.ValorDolar,
                    //    MontoRef = pago.MontoRef,
                    //    Fecha = DateTime.Today

                    //};

                    using (var _dbContext = new NuevaAppContext())
                    {

                        _dbContext.Add(pago);
                        _dbContext.Add(cobroTransito);
                        // _dbContext.Add(transaccion);
                        _dbContext.Update(monedaCuenta);

                        _dbContext.SaveChanges();
                    }

                    // relacion cobro transito <-> pago recibido
                    var pagoCobrotransito = new PagoCobroTransito
                    {
                        IdCobroTransito = cobroTransito.IdCobroTransito,
                        IdPagoRecibido = pago.IdPagoRecibido
                    };

                    ReferenciasPr referencia = new ReferenciasPr
                    {
                        IdPagoRecibido = pago.IdPagoRecibido,
                        NumReferencia = modelo.NumReferencia,
                        Banco = modelo.IdCodigoCuentaBanco.ToString()
                    };

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(referencia);
                        _dbContext.SaveChanges();
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
                        TipoOperacion = false,
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
                        TipoOperacion = true,
                        NumAsiento = numAsiento + 1,
                        ValorDolar = monedaPrincipal.First().ValorDolar,
                        SimboloMoneda = moneda.First().Simbolo,
                        SimboloRef = monedaPrincipal.First().Simbolo

                    };

                    using (var _dbContext = new NuevaAppContext())
                    {
                        _dbContext.Add(asientoBanco);
                        _dbContext.Add(asientoGasto);
                        _dbContext.SaveChanges();
                    }

                    //REGISTRAR ASIENTO EN LA TABLA GASTOS
                    Ingreso ingreso = new Ingreso
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
                        _dbContext.Add(ingreso);
                        _dbContext.Add(activo);
                        _dbContext.Add(pagoCobrotransito);
                        _dbContext.SaveChanges();
                    }

                    return "exito";

                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            return resultado;
        }

        public async Task<string> RegistrarPagoPropietario(PagoRecibidoVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0)
                {

                    //var ejemplo = await _context.Propiedads.FindAsync()
                    var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);
                    var recibo = await _context.ReciboCobros.FindAsync(modelo.IdRecibo);
                    if (propiedad != null && recibo != null)
                    {
                        var pago = new PagoRecibido()
                        {
                            //IdPropiedad = modelo.IdPropiedad,
                            IdCondominio = modelo.IdCondominio,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Confirmado = false,
                            Imagen = modelo.Imagen,
                            Monto = recibo.Monto,
                            Activo = true
                        };

                        // validar num referencia repetido
                        decimal montoReferencia = 0;
                        var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);

                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(propiedad.IdCondominio);

                        if (modelo.Pagoforma == FormaPago.Efectivo)
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

                            // calcular monto referencia
                            if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                            {
                                return "No hay monedas registradas en el sistema!";
                            }
                            else if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                montoReferencia = recibo.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                montoReferencia = recibo.Monto / moneda.First().ValorDolar;
                            }

                            // disminuir saldo de la cuenta de CAJA
                            var monedaCuenta = (from m in _context.MonedaCuenta
                                                where m.IdCodCuenta == idCaja.IdCodCuenta
                                                select m).First();

                            monedaCuenta.SaldoFinal -= recibo.Monto;
                            // añadir al pago

                            pago.FormaPago = false;
                            pago.SimboloMoneda = moneda.First().Simbolo;
                            pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                            pago.SimboloRef = "$";
                            pago.MontoRef = montoReferencia;

                            // registrar pago
                            // registrar pagoPropiedad

                            _context.PagoRecibidos.Add(pago);
                            var valor = await _context.SaveChangesAsync();

                            if (valor > 0)
                            {
                                var pagoPropiedad = new PagoPropiedad()
                                {
                                    IdPago = pago.IdPagoRecibido,
                                    IdPropiedad = propiedad.IdPropiedad,
                                    Confirmado = false,
                                    Rectificado = false,
                                    Activo = false
                                };

                                var pagoRecibo = new PagosRecibo()
                                {
                                    IdPago = pago.IdPagoRecibido,
                                    IdRecibo = modelo.IdRecibo
                                };

                                recibo.EnProceso = true;

                                _context.PagoPropiedads.Add(pagoPropiedad);
                                _context.PagosRecibos.Add(pagoRecibo);
                                _context.ReciboCobros.Update(recibo);

                                await _context.SaveChangesAsync();

                                return "exito";
                            }
                            else
                            {
                                return "Error al registrar su pago. Intente nuevamente!";
                            }

                        }
                        else if (modelo.Pagoforma == FormaPago.Transferencia)
                        {
                            var idBanco = (from c in _context.CodigoCuentasGlobals
                                           where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                           select c).First();

                            var banco = await _context.SubCuenta.FindAsync(modelo.IdCodigoCuentaBanco);
                            // buscar moneda asigna a la subcuenta
                            var moneda = from m in _context.MonedaConds
                                         join mc in _context.MonedaCuenta
                                         on m.IdMonedaCond equals mc.IdMoneda
                                         where mc.IdCodCuenta == idBanco.IdCodCuenta
                                         select m;

                            // calcular monto referencia
                            if (moneda == null || monedaPrincipal == null || !monedaPrincipal.Any())
                            {
                                return "No hay monedas registradas en el sistema!";
                            }
                            else if (moneda.First().Equals(monedaPrincipal.First()))
                            {
                                montoReferencia = recibo.Monto / monedaPrincipal.First().ValorDolar;
                            }
                            else if (!moneda.First().Equals(monedaPrincipal.First()))
                            {
                                montoReferencia = recibo.Monto / moneda.First().ValorDolar;
                            }

                            pago.FormaPago = true;
                            pago.SimboloMoneda = moneda.First().Simbolo;
                            pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                            pago.SimboloRef = "$";
                            pago.MontoRef = montoReferencia;

                            // registrar pago
                            // registrar pagoPropiedad

                            _context.PagoRecibidos.Add(pago);
                            var valor = await _context.SaveChangesAsync();

                            if (valor > 0)
                            {
                                var pagoPropiedad = new PagoPropiedad()
                                {
                                    IdPago = pago.IdPagoRecibido,
                                    IdPropiedad = propiedad.IdPropiedad,
                                    Confirmado = false,
                                    Rectificado = false,
                                    Activo = false
                                };

                                var referencia = new ReferenciasPr()
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    NumReferencia = modelo.NumReferencia,
                                    Banco = banco.Id.ToString()
                                };

                                var pagoRecibo = new PagosRecibo()
                                {
                                    IdPago = pago.IdPagoRecibido,
                                    IdRecibo = modelo.IdRecibo
                                };

                                recibo.EnProceso = true;

                                _context.PagoPropiedads.Add(pagoPropiedad);
                                _context.PagosRecibos.Add(pagoRecibo);
                                _context.ReciboCobros.Update(recibo);
                                _context.ReferenciasPrs.Add(referencia);

                                await _context.SaveChangesAsync();

                                return "exito";
                            }
                            else
                            {
                                return "Error al registrar su pago. Intente nuevamente!";
                            }
                        }
                    }

                    return "No existe esta una Propiedad! Comunicarse con la Administración!";
                }

                return "No ha seleccionado una forma de pago. Intentelo nuevamente!";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<string> RegistrarPagoPropietarioAdmin(PagoRecibidoVM modelo)
        {
            try
            {
                if (modelo.IdCodigoCuentaCaja != 0 || modelo.IdCodigoCuentaBanco != 0 || modelo.Pagoforma == FormaPago.NotaCredito)
                {
                    var propiedad = await _context.Propiedads.FindAsync(modelo.IdPropiedad);

                    if (propiedad != null)
                    {
                        var pago = new PagoRecibido()
                        {
                            IdCondominio = modelo.IdCondominio,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Confirmado = true,
                            Monto = modelo.Monto,
                            Activo = true
                        };

                        // validar num referencia repetido
                        decimal montoReferencia = 0;
                        var condominio = await _context.Condominios.FindAsync(propiedad.IdCondominio);
                        var monedaPrincipal = await _repoMoneda.MonedaPrincipal(propiedad.IdCondominio);

                        // recibos seleccionados

                        if (modelo.ListRecibosIDs == null || !modelo.ListRecibosIDs.Any())
                        {
                            return "Debe seleccionar al menos un recibo";
                        }

                        var recibos = from rs in modelo.ListRecibosIDs
                                      join r in _context.ReciboCobros.ToList()
                                      on rs equals r.IdReciboCobro
                                      where r.IdPropiedad == propiedad.IdPropiedad
                                      select r;

                        if (modelo.Pagoforma == FormaPago.Efectivo)
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

                            monedaCuenta.SaldoFinal += modelo.Monto;
                            // añadir al pago

                            pago.FormaPago = false;
                            pago.SimboloMoneda = moneda.First().Simbolo;
                            pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                            pago.SimboloRef = "$";
                            pago.MontoRef = montoReferencia;

                            // registrar pago
                            // registrar pagoPropiedad

                            _context.PagoRecibidos.Add(pago);
                            var valor = await _context.SaveChangesAsync();

                            if (valor > 0)
                            {
                                var pagoPropiedad = new PagoPropiedad()
                                {
                                    IdPago = pago.IdPagoRecibido,
                                    IdPropiedad = propiedad.IdPropiedad,
                                    Confirmado = true,
                                    Rectificado = false,
                                    Activo = true
                                };

                                _context.PagoPropiedads.Add(pagoPropiedad);
                                await _context.SaveChangesAsync();

                                #region PAGO RECIBIENDO CUALQUIER MONTO
                                // PROCESO DE CONFIRMAR PAGO
                                var montoPago = modelo.Monto; // auxiliar para recorrer los recibos con el monto del pago                         

                                if (recibos != null && recibos.Any())
                                {
                                    foreach (var recibo in recibos)
                                    {
                                        decimal pendientePago = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.TotalPagar;

                                        if (pendientePago != 0 && pendientePago > montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            montoPago = 0;
                                        }
                                        else if (pendientePago != 0 && pendientePago < montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            recibo.Pagado = true;
                                            montoPago -= pendientePago;
                                        }
                                        else if (pendientePago != 0 && pendientePago == montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            recibo.Pagado = true;
                                            montoPago = 0;
                                        }

                                        recibo.TotalPagar = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.Monto + recibo.MontoMora + recibo.MontoIndexacion - recibo.Abonado;
                                        recibo.TotalPagar = recibo.TotalPagar < 0 ? 0 : recibo.TotalPagar;

                                        _context.ReciboCobros.Update(recibo);
                                    }

                                    await _context.SaveChangesAsync();

                                    var recibosActualizados = await _context.ReciboCobros
                                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();

                                    propiedad.Deuda = recibosActualizados
                                        .Where(c => !c.Pagado && !c.ReciboActual)
                                        .Sum(c => c.TotalPagar);

                                    if (montoPago > 0)
                                    {
                                        propiedad.Creditos += montoPago;
                                    }

                                    //// VERIFICAR SOLVENCIA DE LA PROPIEDAD
                                    if (propiedad.Saldo == 0 && propiedad.Deuda == 0)
                                    {
                                        propiedad.Solvencia = true;
                                    }
                                    else
                                    {
                                        propiedad.Solvencia = false;
                                    }

                                    _context.Propiedads.Update(propiedad);

                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    return "Esta propiedad no tiene recibos pendiente!";
                                }
                                #endregion                                

                                // REGISTRAR ASIENTOS CONTABLES
                                int numAsiento = 1;

                                var diarioCondominio = from a in _context.LdiarioGlobals
                                                       join c in _context.CodigoCuentasGlobals
                                                       on a.IdCodCuenta equals c.IdCodCuenta
                                                       where c.IdCondominio == modelo.IdCondominio
                                                       select a;

                                if (diarioCondominio.Count() > 0)
                                {
                                    numAsiento = diarioCondominio.ToList().Last().NumAsiento + 1;
                                }

                                // libro diario cuenta afecada
                                // asiento con los ingresos (Condominio) aumentar por haber Concepto (pago condominio -propiedad- -mes-)
                                // -> contra subcuenta  banco o caja por el debe
                                LdiarioGlobal asientoBanco = new LdiarioGlobal
                                {
                                    IdCodCuenta = idCaja.IdCodCuenta,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = true,
                                    NumAsiento = numAsiento + 1,
                                    MontoRef = pago.MontoRef,
                                    ValorDolar = pago.ValorDolar,
                                    SimboloMoneda = pago.SimboloMoneda,
                                    SimboloRef = pago.SimboloRef
                                    //IdDolar = reciboActual.First().IdDolar
                                };

                                LdiarioGlobal asientoIngreso = new LdiarioGlobal
                                {
                                    IdCodCuenta = (int)condominio.IdCodCuenta,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = false,
                                    NumAsiento = numAsiento + 1,
                                    MontoRef = pago.MontoRef,
                                    ValorDolar = pago.ValorDolar,
                                    SimboloMoneda = pago.SimboloMoneda,
                                    SimboloRef = pago.SimboloRef
                                    //IdDolar = reciboActual.First().IdDolar
                                };

                                _context.Add(asientoIngreso);
                                _context.Add(asientoBanco);

                                _context.SaveChanges();

                                // registrar asientos en bd

                                var ingreso = new Ingreso
                                {
                                    IdAsiento = asientoIngreso.IdAsiento,
                                };

                                var activo = new Activo
                                {
                                    IdAsiento = asientoBanco.IdAsiento,
                                };

                                using (var db_context = new NuevaAppContext())
                                {
                                    db_context.Add(ingreso);
                                    db_context.Add(activo);

                                    db_context.SaveChanges();
                                }

                                return "exito";
                            }
                            else
                            {
                                return "Error al registrar su pago. Intente nuevamente!";
                            }

                        }
                        else if (modelo.Pagoforma == FormaPago.Transferencia)
                        {
                            var idBanco = (from c in _context.CodigoCuentasGlobals
                                           where c.IdSubCuenta == modelo.IdCodigoCuentaBanco
                                           select c).First();

                            var banco = await _context.SubCuenta.FindAsync(modelo.IdCodigoCuentaBanco);
                            // buscar moneda asigna a la subcuenta
                            var moneda = from m in _context.MonedaConds
                                         join mc in _context.MonedaCuenta
                                         on m.IdMonedaCond equals mc.IdMoneda
                                         where mc.IdCodCuenta == idBanco.IdCodCuenta
                                         select m;

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

                            pago.FormaPago = true;
                            pago.SimboloMoneda = moneda.First().Simbolo;
                            pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                            pago.SimboloRef = "$";
                            pago.MontoRef = montoReferencia;

                            // registrar pago
                            // registrar pagoPropiedad

                            _context.PagoRecibidos.Add(pago);
                            var valor = await _context.SaveChangesAsync();

                            if (valor > 0)
                            {
                                var pagoPropiedad = new PagoPropiedad()
                                {
                                    IdPago = pago.IdPagoRecibido,
                                    IdPropiedad = propiedad.IdPropiedad,
                                    Confirmado = true,
                                    Rectificado = false,
                                    Activo = true
                                };

                                var referencia = new ReferenciasPr()
                                {
                                    IdPagoRecibido = pago.IdPagoRecibido,
                                    NumReferencia = modelo.NumReferencia,
                                    Banco = banco.Id.ToString()
                                };

                                _context.PagoPropiedads.Add(pagoPropiedad);
                                _context.ReferenciasPrs.Add(referencia);

                                await _context.SaveChangesAsync();

                                #region PAGO RECIBIENDO CUALQUIER MONTO
                                // PROCESO DE CONFIRMAR PAGO
                                var montoPago = modelo.Monto; // auxiliar para recorrer los recibos

                                if (recibos != null && recibos.Any())
                                {
                                    foreach (var recibo in recibos)
                                    {
                                        decimal pendientePago = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.TotalPagar;

                                        if (pendientePago != 0 && pendientePago > montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            montoPago = 0;
                                        }
                                        else if (pendientePago != 0 && pendientePago < montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            recibo.Pagado = true;
                                            montoPago -= pendientePago;
                                        }
                                        else if (pendientePago != 0 && pendientePago == montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            recibo.Pagado = true;
                                            montoPago = 0;
                                        }

                                        recibo.TotalPagar = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.Monto + recibo.MontoMora + recibo.MontoIndexacion - recibo.Abonado;
                                        recibo.TotalPagar = recibo.TotalPagar < 0 ? 0 : recibo.TotalPagar;

                                        _context.ReciboCobros.Update(recibo);
                                    }

                                    await _context.SaveChangesAsync();

                                    var recibosActualizados = await _context.ReciboCobros
                                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();

                                    propiedad.Deuda = recibosActualizados
                                        .Where(c => !c.Pagado && !c.ReciboActual)
                                        .Sum(c => c.TotalPagar);

                                    propiedad.Saldo = recibosActualizados
                                        .Where(c => c.ReciboActual)
                                        .Sum(c => c.Monto - c.Abonado);

                                    propiedad.Saldo = propiedad.Saldo < 0 ? 0 : propiedad.Saldo;

                                    if (montoPago > 0)
                                    {
                                        propiedad.Creditos += montoPago;
                                    }

                                    //// VERIFICAR SOLVENCIA DE LA PROPIEDAD
                                    if (propiedad.Saldo == 0 && propiedad.Deuda == 0)
                                    {
                                        propiedad.Solvencia = true;
                                    }
                                    else
                                    {
                                        propiedad.Solvencia = false;
                                    }

                                    _context.Propiedads.Update(propiedad);

                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    return "Esta propiedad no tiene recibos pendiente!";
                                }
                                #endregion                                

                                // REGISTRAR ASIENTOS CONTABLES
                                int numAsiento = 1;

                                var diarioCondominio = from a in _context.LdiarioGlobals
                                                       join c in _context.CodigoCuentasGlobals
                                                       on a.IdCodCuenta equals c.IdCodCuenta
                                                       where c.IdCondominio == modelo.IdCondominio
                                                       select a;

                                if (diarioCondominio.Count() > 0)
                                {
                                    numAsiento = diarioCondominio.ToList().Last().NumAsiento + 1;
                                }

                                // libro diario cuenta afecada
                                // asiento con los ingresos (Condominio) aumentar por haber Concepto (pago condominio -propiedad- -mes-)
                                // -> contra subcuenta  banco o caja por el debe
                                LdiarioGlobal asientoBanco = new LdiarioGlobal
                                {
                                    IdCodCuenta = idBanco.IdCodCuenta,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = true,
                                    NumAsiento = numAsiento + 1,
                                    MontoRef = pago.MontoRef,
                                    ValorDolar = pago.ValorDolar,
                                    SimboloMoneda = pago.SimboloMoneda,
                                    SimboloRef = pago.SimboloRef
                                    //IdDolar = reciboActual.First().IdDolar
                                };

                                LdiarioGlobal asientoIngreso = new LdiarioGlobal
                                {
                                    IdCodCuenta = (int)condominio.IdCodCuenta,
                                    Fecha = DateTime.Today,
                                    Concepto = "Condominio Appt: " + propiedad.Codigo,
                                    Monto = pago.Monto,
                                    TipoOperacion = false,
                                    NumAsiento = numAsiento + 1,
                                    MontoRef = pago.MontoRef,
                                    ValorDolar = pago.ValorDolar,
                                    SimboloMoneda = pago.SimboloMoneda,
                                    SimboloRef = pago.SimboloRef
                                    //IdDolar = reciboActual.First().IdDolar
                                };

                                _context.Add(asientoIngreso);
                                _context.Add(asientoBanco);

                                _context.SaveChanges();

                                // registrar asientos en bd

                                var ingreso = new Ingreso
                                {
                                    IdAsiento = asientoIngreso.IdAsiento,
                                };

                                var activo = new Activo
                                {
                                    IdAsiento = asientoBanco.IdAsiento,
                                };

                                using (var db_context = new NuevaAppContext())
                                {
                                    db_context.Add(ingreso);
                                    db_context.Add(activo);

                                    db_context.SaveChanges();
                                }

                                return "exito";
                            }
                            else
                            {
                                return "Error al registrar su pago. Intente nuevamente!";
                            }
                        }
                        else if (modelo.Pagoforma == FormaPago.NotaCredito)
                        {
                            // REGISTRAR NOTA DE CREDITO
                            // NO  VA A BANCOS NI EFECTIVO
                            var nota = new NotaCredito
                            {
                                Concepto = modelo.Concepto + " - " + propiedad.Codigo,
                                Comprobante = "",
                                Fecha = modelo.Fecha,
                                Monto = modelo.Monto,
                                IdPropiedad = modelo.IdPropiedad
                            };

                            _context.NotaCreditos.Add(nota);
                            var registro = await _context.SaveChangesAsync();
                            if (registro > 0)
                            {
                                #region PAGO RECIBIENDO CUALQUIER MONTO
                                // PROCESO DE CONFIRMAR PAGO
                                var montoPago = modelo.Monto; // auxiliar para recorrer los recibos con el monto del pago                         

                                if (recibos != null && recibos.Any())
                                {
                                    foreach (var recibo in recibos)
                                    {
                                        decimal pendientePago = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.TotalPagar;

                                        if (pendientePago != 0 && pendientePago > montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            montoPago = 0;
                                        }
                                        else if (pendientePago != 0 && pendientePago < montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            recibo.Pagado = true;
                                            montoPago -= pendientePago;
                                        }
                                        else if (pendientePago != 0 && pendientePago == montoPago)
                                        {
                                            recibo.Abonado += montoPago;
                                            recibo.Pagado = true;
                                            montoPago = 0;
                                        }

                                        recibo.TotalPagar = recibo.ReciboActual ? recibo.Monto - recibo.Abonado : recibo.Monto + recibo.MontoMora + recibo.MontoIndexacion - recibo.Abonado;
                                        recibo.TotalPagar = recibo.TotalPagar < 0 ? 0 : recibo.TotalPagar;

                                        _context.ReciboCobros.Update(recibo);
                                    }

                                    await _context.SaveChangesAsync();

                                    var recibosActualizados = await _context.ReciboCobros
                                        .Where(c => c.IdPropiedad == propiedad.IdPropiedad).ToListAsync();

                                    propiedad.Deuda = recibosActualizados
                                        .Where(c => !c.Pagado && !c.ReciboActual)
                                        .Sum(c => c.TotalPagar);

                                    if (montoPago > 0)
                                    {
                                        propiedad.Creditos += montoPago;
                                    }

                                    //// VERIFICAR SOLVENCIA DE LA PROPIEDAD
                                    if (propiedad.Saldo == 0 && propiedad.Deuda == 0)
                                    {
                                        propiedad.Solvencia = true;
                                    }
                                    else
                                    {
                                        propiedad.Solvencia = false;
                                    }

                                    _context.Propiedads.Update(propiedad);

                                    await _context.SaveChangesAsync();

                                    return "exito";
                                }
                                else
                                {
                                    return "Esta propiedad no tiene recibos pendiente!";
                                }
                                #endregion
                            } else
                            {
                                return "Error al registrar Nota de Credito. Intentar nuevamente";
                            }
                        }
                    }

                    return "No existe esta Propiedad! Comunicarse con la Administración!";
                }

                return "No ha seleccionado una forma de pago. Intentelo nuevamente!";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
