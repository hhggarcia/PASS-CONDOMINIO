using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
    }
}
