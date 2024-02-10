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
        Task<RegistroPagoVM> FormRegistrarPago(int id);
        Task<IndexPagosVM> GetPagosEmitidos(int id);
        bool PagoEmitidoExists(int id);
        Task<bool> RegistrarPago(RegistroPagoVM modelo);
    }
    public class PagosEmitidosRepository: IPagosEmitidosRepository
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
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
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
        public async Task<bool> RegistrarPago(RegistroPagoVM modelo)
        {
            bool resultado = false;
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
            

            var provisiones = from c in _context.Provisiones
                              where c.IdCodGasto == modelo.IdFactura
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
                        return resultado;
                    }
                    else if (moneda.First().Equals(monedaPrincipal.First()))
                    {
                        montoReferencia = modelo.Monto;
                    }
                    else if (!moneda.First().Equals(monedaPrincipal.First()))
                    {
                        var montoDolares = modelo.Monto * moneda.First().ValorDolar;

                        montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                        //montoReferencia = montoDolares;
                    }

                    // disminuir saldo de la cuenta de CAJA
                    var monedaCuenta = (from m in _context.MonedaCuenta
                                        where m.IdCodCuenta == idCaja.IdCodCuenta
                                        select m).First();

                    monedaCuenta.SaldoFinal -= modelo.Monto;
                    // añadir al pago

                    if (modelo.IdAnticipo != null && modelo.IdAnticipo !=0 )
                    {
                        var anticipos =await _context.Anticipos.Where(a=>a.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                        anticipo1 = anticipos;
                        pago.MontoRef = anticipos.Saldo;
                        anticipo1.Activo = false;
                    }
                    else
                    {
                        pago.MontoRef = montoReferencia;

                    }
                    pago.FormaPago = false;
                    pago.SimboloMoneda = moneda.First().Simbolo;
                    pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                    pago.SimboloRef = monedaPrincipal.First().Simbolo;

                    if(factura.Abonado == 0)
                    {
                        if (pago.MontoRef == factura.Subtotal)
                        {
                            factura.Abonado = pago.MontoRef;
                            factura.Pagada = true;
                            factura.EnProceso = false;
                            //factura.MontoTotal = 0;
                        }
                        else if (pago.MontoRef < factura.Subtotal)
                        {
                            factura.Abonado = pago.MontoRef + factura.Abonado;
                            factura.Pagada = false;
                            factura.EnProceso = true;
                        }
                    }
                    else
                    {
                        var montototal = factura.Abonado + pago.MontoRef;
                        if (montototal == factura.Subtotal)
                        {
                            factura.Abonado = montototal;
                            factura.Pagada = true;
                            factura.EnProceso = false;
                        }
                        else if (montototal < factura.Subtotal)
                        {
                            factura.Abonado = pago.MontoRef + factura.Abonado;
                            factura.Pagada = false;
                            factura.EnProceso = true;
                        }
                        else
                        {
                            factura.Abonado = montototal;
                            factura.Pagada = true;
                            factura.EnProceso = false;
                        }
                    }
                  
                    using (var _dbContext = new NuevaAppContext())
                    {
                       
                        _dbContext.Add(pago);
                        _dbContext.Update(monedaCuenta);
                        _dbContext.Update(factura);
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
                        IdAnticipo = anticipo1.IdAnticipo
                    };

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
                            _dbContext.SaveChanges();
                        }
                        resultado = true;
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
                        resultado = true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
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
                        return resultado;
                    }
                    else if (moneda.First().Equals(monedaPrincipal.First()))
                    {
                        montoReferencia = modelo.Monto;
                    }
                    else if (!moneda.First().Equals(monedaPrincipal.First()))
                    {
                        var montoDolares = modelo.Monto * moneda.First().ValorDolar;

                        montoReferencia = montoDolares * monedaPrincipal.First().ValorDolar;
                    }

                    // disminuir saldo de la cuenta de CAJA
                    var monedaCuenta = (from m in _context.MonedaCuenta
                                        where m.IdCodCuenta == idBanco.IdCodCuenta
                                        select m).First();

                    monedaCuenta.SaldoFinal -= modelo.Monto;

                    // añadir al pago
                    if (modelo.IdAnticipo != null && modelo.IdAnticipo != 0)
                    {
                        var anticipos = await _context.Anticipos.Where(a => a.IdAnticipo == modelo.IdAnticipo).FirstAsync();
                        anticipo1 = anticipos;
                        pago.MontoRef = anticipos.Saldo;
                        anticipo1.Activo = false;
                    }
                    else
                    {
                        pago.MontoRef = montoReferencia;

                    }
                    pago.FormaPago = true;
                    pago.SimboloMoneda = moneda.First().Simbolo;
                    pago.ValorDolar = monedaPrincipal.First().ValorDolar;
                    pago.MontoRef = montoReferencia;
                    pago.SimboloRef = monedaPrincipal.First().Simbolo;
                    if (pago.MontoRef > factura.MontoTotal)
                    {
                        factura.Pagada = true;
                        factura.EnProceso = false;
                        factura.Abonado = pago.MontoRef - factura.MontoTotal;
                        factura.Subtotal = 0;
                        factura.MontoTotal = 0;
                    }
                    else if (pago.MontoRef < factura.MontoTotal)
                    {
                        factura.Pagada = false;
                        factura.EnProceso = true;
                        factura.Abonado = pago.MontoRef;
                    }
                    else
                    {
                        factura.Pagada = true;
                        factura.EnProceso = false;
                        factura.Subtotal = 0;
                        factura.MontoTotal = 0;
                        factura.Abonado = 0;
                    }
            
                    using (var _dbContext = new NuevaAppContext())
                    {
 
                        _dbContext.Add(pago);
                        _dbContext.Update(monedaCuenta);
                        _dbContext.Update(factura);
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
                        IdAnticipo = anticipo1.IdAnticipo
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

                        return true;
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

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return resultado;
                }
            }

            return resultado;
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
