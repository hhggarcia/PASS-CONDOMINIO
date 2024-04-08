using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.ViewModels;
using SQLitePCL;

namespace Prueba.Repositories
{
    public interface IPagosRecibidosRepository
    {
        Task<CobroTransitoVM> FormCobroTransito(int id);
        Task<PagoFacturaEmitidaVM> FormPagoFacturaEmitida(int id);
        Task<IndexPagoFacturaEmitidaVM> GetPagosFacturasEmitidas(int id);
        Task<string> RegistrarCobroTransito(CobroTransitoVM modelo);
        Task<string> RegistrarPago(PagoFacturaEmitidaVM modelo);
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
        public async Task<string> RegistrarPago(PagoFacturaEmitidaVM modelo)
        {
            var resultado = string.Empty;
            decimal montoReferencia = 0;

            //var idCodCuenta = await _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).ToListAsync();
            //var idCodCuenta = from c in _context.CodigoCuentasGlobals
            //                  where c.IdSubCuenta == modelo.IdSubcuenta
            //                  select c;

            //var cc = _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == modelo.IdSubcuenta).First();
            //modelo.IdSubcuenta = cc.IdCodCuenta;

            // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
            // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
            PagoRecibido pago = new PagoRecibido
            {
                IdCondominio = modelo.IdCondominio,
                Fecha = modelo.Fecha,
                Monto = modelo.Monto,
                Concepto = modelo.Concepto,
                Confirmado = true,
            };
            // Anticipo anticipo1 = new Anticipo();

            var factura = await _context.FacturaEmitida.Where(c => c.IdFacturaEmitida == modelo.IdFactura).FirstAsync();

            var itemLibroVenta = await _context.LibroVentas.Where(c => c.IdFactura == factura.IdFacturaEmitida).FirstOrDefaultAsync();

            var itemCuentaCobrar = await _context.CuentasCobrars.Where(c => c.IdFactura == factura.IdFacturaEmitida).FirstOrDefaultAsync();

            if (itemLibroVenta != null)
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
                            itemCuentaCobrar.Status = "Cancelada";
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
                            itemCuentaCobrar.Status = "Cancelada";
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

                        }
                        else if (pago.Monto == factura.MontoTotal)
                        {
                            factura.Abonado += pago.Monto;
                            factura.EnProceso = false;
                            factura.Pagada = true;
                            itemCuentaCobrar.Status = "Cancelada";
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
                            itemCuentaCobrar.Status = "Cancelada";
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
                        Asignado = false
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
                        Asignado = false
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
    }
}
