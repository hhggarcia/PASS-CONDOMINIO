using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using Prueba.Context;
using Prueba.Models;
using SQLitePCL;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Prueba.Repositories
{
    public interface IPagosEmitidosRepository
    {
        Task<int> Delete(int id);
        RegistroPagoVM FormRegistrarPago(int id);
        Task<IndexPagosVM> GetPagosEmitidos(int id);
        bool PagoEmitidoExists(int id);
        bool RegistrarPago(RegistroPagoVM modelo);
    }
    public class PagosEmitidosRepository: IPagosEmitidosRepository
    {
        private readonly PruebaContext _context;

        public PagosEmitidosRepository(PruebaContext context)
        {
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

            var listaPagos = from c in _context.PagoEmitidos
                             where c.IdCondominio == idCondominio
                             select c;

            var referencias = from p in _context.PagoEmitidos
                              where p.IdCondominio == idCondominio
                              join r in _context.ReferenciasPes
                              on p.IdPagoEmitido equals r.IdPagoEmitido
                              select r;

            var referenciasDolar = from d in _context.ReferenciaDolars
                                   select d;

            modelo.PagosEmitidos = await listaPagos.ToListAsync();
            modelo.Referencias = await referencias.ToListAsync();
            modelo.ReferenciasDolar = await referenciasDolar.ToListAsync();

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
        public RegistroPagoVM FormRegistrarPago(int id)
        {
            var modelo = new RegistroPagoVM();
            //LLENAR SELECT DE SUBCUENTAS DE GASTOS

            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == id
                                       select c;

            //CONSULTAS A BD SOBRE CLASE - GRUPO - CUENTA - SUB CUENTA

            //CARGAR SELECT DE SUB CUENTAS DE GASTOS
            IQueryable<Grupo> gruposGastos = from c in _context.Grupos
                                             where c.IdClase == 5
                                             select c;

            IQueryable<Cuenta> cuentas = from c in _context.Cuenta                                         
                                         select c;

            IQueryable<SubCuenta> subcuentas = from c in _context.SubCuenta
                                               join d in cuentasContablesCond
                                                on c.Id equals d.IdCodCuenta
                                               select c;

            //CARGAR SELECT DE SUB CUENTAS DE BANCOS
            IQueryable<Cuenta> bancos = from c in _context.Cuenta
                                        where c.Descripcion.ToUpper().Trim() == "BANCO"
                                        select c;
            IQueryable<Cuenta> caja = from c in _context.Cuenta
                                      where c.Descripcion.ToUpper().Trim() == "CAJA"
                                      select c;

            IQueryable<SubCuenta> subcuentasBancos = from c in _context.SubCuenta
                                                     join d in cuentasContablesCond
                                                     on c.Id equals d.IdCodCuenta
                                                     where c.IdCuenta == bancos.First().Id
                                                     select c;
            IQueryable<SubCuenta> subcuentasCaja = from c in _context.SubCuenta
                                                   join d in cuentasContablesCond
                                                   on c.Id equals d.IdCodCuenta
                                                   where c.IdCuenta == caja.First().Id
                                                   select c;

             
            IList < Cuenta > cuentasGastos = new List<Cuenta>();
            foreach (var grupo in gruposGastos)
            {
                foreach (var cuenta in cuentas)
                {
                    if (cuenta.IdGrupo == grupo.Id)
                    {
                        cuentasGastos.Add(cuenta);
                    }
                    continue;
                }
            }

            IList<SubCuenta> subcuentasGastos = new List<SubCuenta>();
            foreach (var cuenta in cuentasGastos)
            {
                foreach (var subcuenta in subcuentas)
                {
                    if (subcuenta.IdCuenta == cuenta.Id)
                    {
                        subcuentasGastos.Add(subcuenta);
                    }
                    continue;
                }
            }

            IList<SubCuenta> subcuentasModel = new List<SubCuenta>();
            foreach (var condominioCC in cuentasContablesCond)
            {
                foreach (var subcuenta in subcuentasGastos)
                {
                    if (condominioCC.IdCodigo == subcuenta.Id)
                    {
                        subcuentasModel.Add(subcuenta);
                    }
                    continue;
                }
            }


            modelo.SubCuentasGastos = subcuentasModel.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasBancos = subcuentasBancos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.SubCuentasCaja = subcuentasCaja.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
            modelo.ReferenciasDolar = _context.ReferenciaDolars.Select(c => new SelectListItem(c.Valor.ToString(), c.IdReferencia.ToString())).ToList();
            // ENVIAR MODELO

            return modelo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        public bool RegistrarPago(RegistroPagoVM modelo)
        {
            bool resultado = false;

            // REGISTRAR PAGO EMITIDO (idCondominio, fecha, monto, forma de pago)
            // forma de pago 1 -> Registrar referencia de transferencia. 0 -> seguir
            PagoEmitido pago = new PagoEmitido
            {
                IdCondominio = modelo.IdCondominio,
                Fecha = modelo.Fecha,
                Monto = modelo.Monto,
                IdDolar = modelo.IdReferenciaDolar
            };

            var provisiones = from c in _context.Provisiones
                              where c.IdCodGasto == modelo.IdSubcuenta
                              select c;

            var diario = from l in _context.LdiarioGlobals
                         select l;

            int numAsiento = 1;

            if (diario.Count() > 0)
            {
                numAsiento = diario.ToList().Last().NumAsiento;
            }

            if (modelo.Pagoforma == FormaPago.Efectivo)
            {
                try
                {
                    pago.FormaPago = false;

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(pago);
                        _dbContext.SaveChanges();
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
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar
                        };
                        LdiarioGlobal asientoProvisionCaja = new LdiarioGlobal
                        {
                            IdCodCuenta = modelo.IdCodigoCuentaCaja,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };
                        LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                        {
                            IdCodCuenta = provisiones.First().IdCodGasto,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto - provisiones.First().Monto,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };
                        using (var _dbContext = new PruebaContext())
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

                        using (var _dbContext = new PruebaContext())
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
                            IdCodCuenta = modelo.IdSubcuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };
                        LdiarioGlobal asientoCaja = new LdiarioGlobal
                        {
                            IdCodCuenta = modelo.IdCodigoCuentaCaja,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };

                        using (var _dbContext = new PruebaContext())
                        {
                            _dbContext.Add(asientoGasto);
                            _dbContext.Add(asientoCaja);
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

                        using (var _dbContext = new PruebaContext())
                        {
                            _dbContext.Add(gasto);
                            _dbContext.Add(activo);
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
                    pago.FormaPago = true;

                    using (var _dbContext = new PruebaContext())
                    {
                        _dbContext.Add(pago);
                        _dbContext.SaveChanges();
                    }

                    ReferenciasPe referencia = new ReferenciasPe
                    {
                        IdPagoEmitido = pago.IdPagoEmitido,
                        NumReferencia = modelo.NumReferencia,
                        Banco = modelo.IdCodigoCuentaBanco.ToString()
                    };

                    using (var _dbContext = new PruebaContext())
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
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };
                        LdiarioGlobal asientoProvisionBanco = new LdiarioGlobal
                        {
                            IdCodCuenta = modelo.IdCodigoCuentaBanco,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };
                        LdiarioGlobal asientoProvisionGasto = new LdiarioGlobal
                        {
                            IdCodCuenta = provisiones.First().IdCodGasto,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto - provisiones.First().Monto,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };
                        using (var _dbContext = new PruebaContext())
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

                        using (var _dbContext = new PruebaContext())
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
                            IdCodCuenta = modelo.IdSubcuenta,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            TipoOperacion = true,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };
                        LdiarioGlobal asientoBanco = new LdiarioGlobal
                        {
                            IdCodCuenta = modelo.IdCodigoCuentaBanco,
                            Fecha = modelo.Fecha,
                            Concepto = modelo.Concepto,
                            Monto = modelo.Monto,
                            TipoOperacion = false,
                            NumAsiento = numAsiento + 1,
                            IdDolar = modelo.IdReferenciaDolar

                        };

                        using (var _dbContext = new PruebaContext())
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

                        using (var _dbContext = new PruebaContext())
                        {
                            _dbContext.Add(gasto);
                            _dbContext.Add(activo);
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
