﻿using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using Org.BouncyCastle.Utilities;
using Prueba.Areas.Identity.Data;
using Prueba.Context;
using Prueba.Core.Repositories;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.Services;
using Prueba.Utils;
using Prueba.Validates;
using Prueba.ViewModels;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    public class CuotasEspecialesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IEmailService _serviceEmail;
        private readonly IPDFServices _servicePDF;
        private readonly IManageExcel _manageExcel;
        private readonly IReportesRepository _repoReportes;
        private readonly IRelacionGastoRepository _repoRelacionGasto;
        private readonly IMonedaRepository _repoMoneda;
        private readonly PruebaContext _context;

        public CuotasEspecialesController(IUnitOfWork unitOfWork, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore,
            IEmailService serviceEmail, IPDFServices servicePDF, IManageExcel manageExcel,
            IReportesRepository repoReportes, IRelacionGastoRepository repoRelacionGasto,
             IMonedaRepository repoMoneda, PruebaContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _serviceEmail = serviceEmail;
            _servicePDF = servicePDF;
            _manageExcel = manageExcel;
            _repoReportes = repoReportes;
            _repoRelacionGasto = repoRelacionGasto;
            _repoMoneda = repoMoneda;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var idAdministrador = TempData.Peek("idUserLog").ToString();
                var idCondominio = _context.Condominios.Where(c => c.IdAdministrador == idAdministrador).Select(c => c.IdCondominio).FirstOrDefault();
                var cuotasCondominio = _context.CuotasEspeciales.Where(c => c.IdCondominio == idCondominio);
                var condominiosModel = await cuotasCondominio.ToListAsync();

                TempData.Keep();

                return View(cuotasCondominio);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            try
            {
                //ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
                return View();
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CuotasEspeciale modelo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Crear Cuotas Especiales
                    modelo.IdCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                    var revisionCuotas = from c in _context.CuotasEspeciales
                                         where c.Descripcion.Trim().ToLower() == modelo.Descripcion.Trim().ToLower()
                                         select c.Descripcion;
                    if(revisionCuotas == null)
                    {
                        var modeloError = new ErrorViewModel()
                        {
                            RequestId = "Ya existe una cuota especial con la misma descripción."
                        };
                        return View("Error", modeloError);
                    }
                    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                    var idInmueble = from c in _context.Inmuebles
                                     where c.IdCondominio == idCondominio
                                     select c.IdInmueble;

                    var listaPropiedades =
                        (from c in _context.Propiedads
                         where c.IdInmueble == idInmueble.First()
                         select c).ToList();

                    modelo.SubCuotas = Math.Round((decimal)(modelo.MontoTotal / listaPropiedades.Count()), 2);
                    modelo.MontoMensual = modelo.MontoTotal / modelo.CantidadCuotas;
                    DateTime fechacreacion = (DateTime)modelo.FechaInicio;
                    modelo.FechaFin = fechacreacion.AddMonths((int)modelo.CantidadCuotas);
                    var monedaPrincipal = await _repoMoneda.MonedaPrincipal(idCondominio);
                    modelo.ValorDolar = monedaPrincipal.First().ValorDolar;
                    modelo.SimboloMoneda = monedaPrincipal.First().Simbolo;
                    modelo.SimboloRef = "$";
                    using (var dbContext = new PruebaContext())
                    {
                        await dbContext.AddAsync(modelo);
                        await dbContext.SaveChangesAsync();
                        var idCuotaEspecial = modelo.IdCuotaEspecial;
                        foreach (var c in listaPropiedades)
                        {
                            var nuevoReciboCuota = new ReciboCuota
                            {
                                IdPropiedad = c.IdPropiedad,
                                IdCuotaEspecial = idCuotaEspecial,
                                SubCuotas = modelo.SubCuotas,
                                Fecha = modelo.FechaInicio,
                                EnProceso = false,
                                Confirmado = false,
                                CuotasFaltantes = modelo.CantidadCuotas,
                                Abonado = 0,
                                CuotasPagadas = 0,
                                SimboloMoneda = monedaPrincipal.First().Simbolo,
                                ValorDolar = monedaPrincipal.First().ValorDolar,
                                SimboloRef = "$"
                            };
                            await dbContext.AddAsync(nuevoReciboCuota);

                        }
                        await dbContext.SaveChangesAsync();
                        TempData.Keep();
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View();
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalles(int? id)
        {
            try
            {
                if (id == null || _context.CuotasEspeciales == null)
                {
                    return NotFound();
                }
                var cuotaDetalles =  _context.CuotasEspeciales.Where(c=>c.IdCuotaEspecial == id).First();
                DetalleCuotasVM detalleCuotasVM = new DetalleCuotasVM();
                detalleCuotasVM.CuotasEspeciale = cuotaDetalles;
                var cantidadPropiedades = _context.ReciboCuotas.Where(c=> c.IdCuotaEspecial==id).ToList().Count();
                detalleCuotasVM.TotalPropiedadesMensual = (decimal)(cuotaDetalles.SubCuotas / cantidadPropiedades);
                return View(detalleCuotasVM);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                return View("Error", modeloError);
            }
        }
        public async Task<IActionResult> Editar(int? id){
            try
            {
                if (id == null || _context.CuotasEspeciales == null)
                {
                    return NotFound();
                }
                var cuotaDetalles = _context.CuotasEspeciales.Where(c => c.IdCuotaEspecial == id).First();
                return View(cuotaDetalles);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                return View("Error", modeloError);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CuotasEspeciale modelo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (id != modelo.IdCuotaEspecial)
                    {
                        return NotFound();
                    }
                    var listaRecibos = (from c in _context.ReciboCuotas
                                        where c.IdCuotaEspecial == modelo.IdCuotaEspecial
                                        select c).ToList();

                    DateTime fechacreacion = (DateTime)modelo.FechaInicio;
                    modelo.FechaFin = fechacreacion.AddMonths((int)modelo.CantidadCuotas);
                    modelo.SubCuotas = Math.Round((decimal)(modelo.MontoTotal / listaRecibos.Count()), 2);
                    modelo.MontoMensual = modelo.MontoTotal / modelo.CantidadCuotas;
                    _context.Update(modelo);
                    _context.SaveChanges();
          
                    using (var dbContext = new PruebaContext())
                    {
                        foreach (var c in listaRecibos)
                        {
                            var reciboCuota = dbContext.ReciboCuotas.Find(c.IdReciboCuotas);
                            reciboCuota.SubCuotas = modelo.SubCuotas;
                            reciboCuota.Fecha = modelo.FechaInicio;
                            reciboCuota.CuotasFaltantes = modelo.CantidadCuotas;
                            dbContext.Update(reciboCuota);
                        }
                        await dbContext.SaveChangesAsync();
                        TempData.Keep();
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View();
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

        }
         public async Task<IActionResult> Borrar(int? id)
        {
            try
            {
                if (id == null || _context.CuotasEspeciales == null)
                {
                    return NotFound();
                }
                var cuotaDetalles = _context.CuotasEspeciales.Where(c => c.IdCuotaEspecial == id).First();
                return View(cuotaDetalles);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                return View("Error", modeloError);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int? id, CuotasEspeciale modelo)
        {
            try
            {
                if (id != modelo.IdCuotaEspecial)
                {
                    return NotFound();
                }
                     var cuotaEspecial = await _context.CuotasEspeciales.FindAsync(id);
                    if (cuotaEspecial == null)
                    {
                        return NotFound();
                    }

                    _context.Remove(cuotaEspecial);

                    var recibos = await _context.ReciboCuotas.Where(c => c.IdCuotaEspecial == id).ToListAsync();
                    _context.RemoveRange(recibos);

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
               
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }

        public async Task<IActionResult> Recibos()
        {
            try
            {
              var recibosDelAdmin = from recibo in _context.ReciboCuotas
                                      join cuota in _context.CuotasEspeciales
                                      on recibo.IdCuotaEspecial equals cuota.IdCuotaEspecial
                                      where cuota.IdCuotaEspecial == recibo.IdCuotaEspecial
                                      select new ReciboCuotaVM
                                      {
                                          Nombre = cuota.Descripcion,
                                          ReciboCuota = recibo
                                      }; ;
                List<ReciboCuotaVM> listaRecibosDelAdmin = recibosDelAdmin.ToList();
                return View(listaRecibosDelAdmin);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                return View("Error", modeloError);
            }
        }

        public async Task<IActionResult> Cobrar()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                // propietarios
                IList<ApplicationUser> listaPropietarios = await _signInManager.UserManager.Users.ToListAsync();
                var listaPropiedades = await _context.Propiedads.ToListAsync();
                var resultado = (from a in _context.AspNetUsers
                                 join p in _context.Propiedads on a.Id equals p.IdUsuario
                                 orderby p.IdPropiedad
                                 select new
                                 {
                                     IdPropiedad = p.IdPropiedad,
                                     NombreCompleto = a.FirstName + ' ' + a.LastName,
                                     Codigo = p.Codigo
                                 }).ToList();
                var pagoRecibosCuotas = await _context.PagoReciboCuota.ToListAsync();
                var cuotasEspeciales = await _context.CuotasEspeciales.ToListAsync();
                var datosCobro = new List<CobrarCuotasVM>();
                foreach (var cuota in cuotasEspeciales)
                {
                   foreach(var pago  in pagoRecibosCuotas)
                    {
                      
                        if (cuota.IdCuotaEspecial == pago.IdCuota)
                        {
                            foreach (var usuario in resultado)
                            {
                                if(usuario.IdPropiedad == pago.IdPropiedad)
                                {
                                   if(pago.Confirmado != true)
                                    {
                                        var cobrarCuotasVM = new CobrarCuotasVM
                                        {
                                            NombreUsuario = usuario.NombreCompleto,
                                            CodigoPropiedad = usuario.Codigo,
                                            PagoReciboCuota = pago,
                                            CuotasEspeciale = cuota
                                        };
                                        datosCobro.Add(cobrarCuotasVM);
                                    }
                                }
                            }
                        }
                    } 
                }

                return View(datosCobro);
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                return View("Error", modeloError);
            }
        }
        [HttpGet]
        public IActionResult ConfirmarPagoRecibido(int id)
        {
            try
            {
                TempData["idPagoConfirmar"] = id.ToString();
                return View();
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

        }
        [HttpPost]
        public async Task<IActionResult> ConfirmarPagoRecibidoPost()
        {
            try
            {
                int id = Convert.ToInt32(TempData.Peek("idPagoConfirmar").ToString());

                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                // buscar pago
                var pago = await _context.PagoReciboCuota.FindAsync(id);
                var cuotaEspecial = await _context.CuotasEspeciales.FindAsync(pago.IdCuota);
                if (pago != null)
                {
                    var relacionPagosRecibos = await _context.PagosCuotasRecibidos.FindAsync(pago.IdPagoRecibido);
                    var reciboActual = await _context.ReciboCuotas.FindAsync(relacionPagosRecibos.IdRecibido);
                    // buscar subcuenta contable donde esta el pago del condominio
                    var cuentaCondominio = from s in _context.SubCuenta
                                           join cc in _context.CodigoCuentasGlobals
                                           on s.Id equals cc.IdCodigo
                                           where cc.IdCondominio == idCondominio
                                           where s.IdCuenta == 15 && s.Codigo == "01"
                                           select s;
                    // buscar referencia si tiene
                    var referencias = new List<ReferenciasPr>();
                    if ((bool)pago.FormaPago)
                    {
                        referencias = await _context.ReferenciasPrs.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                    }
                    if (reciboActual == null)
                    {
                        _context.PagoReciboCuota.Remove(pago);
                        await _context.SaveChangesAsync();

                        var modeloError = new ErrorViewModel()
                        {
                            RequestId = "El Recibo asignado a este Pago fue eliminado! Este pago será eliminado"
                        };

                        return View("Error", modeloError);
                    }
                    try
                    {
                        // se pago solo el recibo de la cuota del mes actual
                        using (var dbContext = new PruebaContext())
                        {
                            var pagoMensual = cuotaEspecial.SubCuotas / (reciboActual.CuotasFaltantes + reciboActual.CuotasPagadas);
                            // ADD PAGOS ABONADOS SOBRE LOS RECIBOS
                            if (pago.MontoRef == pagoMensual)
                            {
                                reciboActual.SubCuotas = reciboActual.SubCuotas-pago.MontoRef;
                                reciboActual.CuotasPagadas = reciboActual.CuotasPagadas +1;
                                reciboActual.CuotasFaltantes = reciboActual.CuotasFaltantes - 1;
                                reciboActual.Abonado = reciboActual.Abonado - pago.MontoRef;
                                reciboActual.EnProceso = false;
                            }
                            else if(pago.MontoRef < pagoMensual) 
                            {
                                reciboActual.Abonado = reciboActual.Abonado - pago.MontoRef;
                            }else if (pago.MontoRef >pagoMensual)
                            {
                                reciboActual.EnProceso = false;
                                reciboActual.SubCuotas = reciboActual.SubCuotas - pagoMensual;
                                reciboActual.CuotasPagadas = reciboActual.CuotasPagadas + 1;
                                reciboActual.CuotasFaltantes = reciboActual.CuotasFaltantes - 1;
                                reciboActual.Abonado = reciboActual.Abonado +(pago.MontoRef- pagoMensual);
                            }
                            pago.Confirmado = true;
                            dbContext.Update(reciboActual);
                            dbContext.Update(pago);
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
                            LdiarioGlobal asientoBanco = new LdiarioGlobal
                            {
                                IdCodCuenta = (int)pago.IdSubCuenta,
                                Fecha = DateTime.Today,
                                Concepto = "Condominio Appt: " + reciboActual.IdPropiedad,
                                Monto = (decimal)pago.Monto,
                                TipoOperacion = true,
                                NumAsiento = numAsiento + 1,
                                MontoRef = (decimal)pago.MontoRef,
                                ValorDolar = (decimal)pago.ValorDolar,
                                SimboloMoneda = pago.SimboloMoneda,
                                SimboloRef = pago.SimboloRef
                                //IdDolar = reciboActual.First().IdDolar
                            };

                            LdiarioGlobal asientoIngreso = new LdiarioGlobal
                            {
                                IdCodCuenta = cuentaCondominio.First().Id,
                                Fecha = DateTime.Today,
                                Concepto = "Condominio Appt: " + reciboActual.IdPropiedad,
                                Monto = (decimal)pago.Monto,
                                TipoOperacion = false,
                                NumAsiento = numAsiento + 1,
                                MontoRef = (decimal)pago.MontoRef,
                                ValorDolar = (decimal)pago.ValorDolar,
                                SimboloMoneda = pago.SimboloMoneda,
                                SimboloRef = pago.SimboloRef
                                //IdDolar = reciboActual.First().IdDolar
                            };
                            dbContext.Add(asientoIngreso);
                            dbContext.Add(asientoBanco);

                            dbContext.SaveChanges();
                            var ingreso = new Ingreso
                            {
                                IdAsiento = asientoIngreso.IdAsiento,
                            };

                            var activo = new Activo
                            {
                                IdAsiento = asientoBanco.IdAsiento,
                            };

                            using (var db_context = new PruebaContext())
                            {
                                db_context.Add(ingreso);
                                db_context.Add(activo);

                                db_context.SaveChanges();
                            }
                        }
                        TempData.Keep();
                        return RedirectToAction("Cobrar");

                    }
                    catch (Exception ex)
                    {
                        var error = new ErrorViewModel()
                        {
                            RequestId = ex.Message
                        };

                        return View("Error", error);
                    }

                }
                TempData.Keep();
                return RedirectToAction("PagosRecibidos");
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };
                return View("Error", modeloError);
            }
        }

        [HttpGet]
        public IActionResult RectificarPago(int id)
        {
            try
            {
                TempData["idPagoConfirmar"] = id.ToString();
                return View();
            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }

        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarRectificarPago()
        {
            try
            {
                int id = Convert.ToInt32(TempData.Peek("idPagoConfirmar").ToString());
                var pago = await _context.PagoReciboCuota.FindAsync(id);

                if (pago != null)
                {
                    var relacionPagosRecibos = await _context.PagosCuotasRecibidos.FindAsync(pago.IdPagoRecibido);
                    var reciboActual = await _context.ReciboCuotas.FindAsync(relacionPagosRecibos.IdRecibido);
                    reciboActual.Abonado = reciboActual.Abonado- pago.MontoRef;
                    if (reciboActual == null)
                    {
                        _context.PagoReciboCuota.Remove(pago);
                        await _context.SaveChangesAsync();

                        var modeloError = new ErrorViewModel()
                        {
                            RequestId = "El Recibo asignado a este Pago fue eliminado! Este pago será eliminado"
                        };

                        return View("Error", modeloError);
                    }

                     if ((bool)pago.FormaPago)
                        {
                            var referencia = await _context.ReferenciasPrs.Where(c => c.IdPagoRecibido == pago.IdPagoRecibido).ToListAsync();
                            _context.ReferenciasPrs.RemoveRange(referencia);
                            _context.PagoReciboCuota.Remove(pago);
                     }else{
                           _context.PagoReciboCuota.Remove(pago);
                     }
                    var relacionPagosCuotas = await _context.PagosCuotasRecibidos.Where(c => c.IdRecibido == reciboActual.IdReciboCuotas).FirstAsync();
                    _context.RemoveRange(relacionPagosCuotas);
                    var propiedad = await _context.Propiedads.FindAsync(pago.IdPropiedad);
                    reciboActual.EnProceso = false;
                    _context.ReciboCuotas.Update(reciboActual);
                    await _context.SaveChangesAsync();
                }
                TempData.Keep();
                return RedirectToAction("Cobrar");

            }
            catch (Exception ex)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = ex.Message
                };

                return View("Error", modeloError);
            }
        }
    }
}