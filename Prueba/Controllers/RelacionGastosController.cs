using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class RelacionGastosController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IRelacionGastoRepository _repoRelacionGastos;
        private readonly PruebaContext _context;

        public RelacionGastosController(IUnitOfWork unitOfWork,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IRelacionGastoRepository repoRelacionGastos,
            PruebaContext context)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _repoRelacionGastos = repoRelacionGastos;
        }

        // GET: RelacionGastos
        public async Task<IActionResult> Index()
        {
            var pruebaContext = _context.RelacionGastos.Include(r => r.IdCondominioNavigation);
            return View(await pruebaContext.ToListAsync());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> RelaciondeGastos()
        {
            try
            {
                // CARGAR GASTOS REGISTRADOS

                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var modelo = await _repoRelacionGastos.LoadDataRelacionGastos(idCondominio);

                TempData.Keep();

                return View(modelo);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: RelacionGastos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RelacionGastos == null)
            {
                return NotFound();
            }

            var relacionGasto = await _context.RelacionGastos
                .Include(r => r.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdRgastos == id);
            if (relacionGasto == null)
            {
                return NotFound();
            }
            var modelo = await _repoRelacionGastos.LoadDataRelacionGastosMes(id);
            //return View(relacionGasto);
            return View(modelo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET: RelacionGastos/Create
        public IActionResult Create()
        {
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            return View();
        }

        // POST: RelacionGastos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relacionGasto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRgastos,SubTotal,TotalMensual,Fecha,IdCondominio")] RelacionGasto relacionGasto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(relacionGasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", relacionGasto.IdCondominio);
            return View(relacionGasto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: RelacionGastos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RelacionGastos == null)
            {
                return NotFound();
            }

            var relacionGasto = await _context.RelacionGastos.FindAsync(id);
            if (relacionGasto == null)
            {
                return NotFound();
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", relacionGasto.IdCondominio);
            return View(relacionGasto);
        }

        // POST: RelacionGastos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="relacionGasto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRgastos,SubTotal,TotalMensual,Fecha,IdCondominio")] RelacionGasto relacionGasto)
        {
            if (id != relacionGasto.IdRgastos)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(relacionGasto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_repoRelacionGastos.RelacionGastoExists(relacionGasto.IdRgastos))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", relacionGasto.IdCondominio);
            return View(relacionGasto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: RelacionGastos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RelacionGastos == null)
            {
                return NotFound();
            }

            var relacionGasto = await _context.RelacionGastos
                .Include(r => r.IdCondominioNavigation)
                .FirstOrDefaultAsync(m => m.IdRgastos == id);
            if (relacionGasto == null)
            {
                return NotFound();
            }

            return View(relacionGasto);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // POST: RelacionGastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RelacionGastos == null)
            {
                return Problem("Entity set 'PruebaContext.RelacionGastos'  is null.");
            }
            var relacionGasto = await _context.RelacionGastos.FindAsync(id);
            if (relacionGasto != null)
            {
                var result = await _repoRelacionGastos.DeleteRecibosCobroRG(id);
                if (result)
                {
                    _context.RelacionGastos.Remove(relacionGasto);
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CrearFondo()
        {
            try
            {
                var modelo = new CrearFondoVM();

                // BUSCAR LAS CUENTAS CONTABLES DEL CONDOMINIO
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                           where c.IdCondominio == idCondominio
                                           select c;

                IQueryable<Grupo> gruposPatrimonio = from c in _context.Grupos
                                                     where c.IdClase == 3
                                                     select c;
                IQueryable<Cuenta> cuentas = from c in _context.Cuenta
                                             where gruposPatrimonio.First().Id == c.IdGrupo
                                             select c;
                IQueryable<SubCuenta> subcuentas = from c in _context.SubCuenta
                                                   where cuentas.First().Id == c.IdCuenta
                                                   select c;

                // LLENAR SELECT CON LOS FONDOS REGISTRADOS
                modelo.Fondos = subcuentas.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

                TempData.Keep();

                return View(modelo);
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearFondo(CrearFondoVM modelo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var fondo = new Fondo
                    {
                        IdCodCuenta = modelo.IdFondo,
                        Porcentaje = modelo.Porcentaje
                    };

                    using (var db_context = new PruebaContext())
                    {
                        await db_context.AddAsync(fondo);
                        await db_context.SaveChangesAsync();
                    }

                    return RedirectToAction("RelaciondeGastos");
                }
                return View(modelo);
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CrearProvision()
        {
            try
            {
                var modelo = new CrearProvisionVM();

                // BUSCAR LAS CUENTAS CONTABLES DEL CONDOMINIO
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                           where c.IdCondominio == idCondominio
                                           select c;
                var gruposGastos = from c in _context.Grupos
                                   where c.IdClase == 5
                                   select c;

                var cuentaProvision = from c in _context.Cuenta
                                      where c.Descripcion.Trim().ToUpper() == "PROVISIONES"
                                      select c;

                var subcuentasProvisiones = from c in _context.SubCuenta
                                            where c.IdCuenta == cuentaProvision.FirstOrDefault().Id
                                            select c;

                var cuentas = from c in _context.Cuenta
                              select c;

                var subcuentas = from c in _context.SubCuenta
                                 select c;

                // CARGAR CUENTAS GASTOS DEL CONDOMINIO
                IList<Cuenta> cuentasGastos = new List<Cuenta>();
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

                modelo.Gastos = subcuentasGastos.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();
                modelo.Provisiones = subcuentasProvisiones.Select(c => new SelectListItem(c.Descricion, c.Id.ToString())).ToList();

                return View(modelo);
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelo"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearProvision(CrearProvisionVM modelo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // GUARDAR PREVISION EN BD
                    var provision = new Provision
                    {
                        IdCodGasto = modelo.IdGasto,
                        IdCodCuenta = modelo.IdcodCuenta,
                        Monto = modelo.Monto
                    };

                    // CREAR ASIENTO SOBRE PREVISION

                    var diario = from l in _context.LdiarioGlobals
                                 select l;

                    int numAsiento = 1;

                    if (diario.Count() > 0)
                    {
                        numAsiento = diario.ToList().LastOrDefault().NumAsiento + 1;
                    }

                    LdiarioGlobal asientoProvision = new LdiarioGlobal
                    {
                        IdCodCuenta = provision.IdCodCuenta,
                        Fecha = DateTime.Today,
                        Concepto = modelo.Concepto,
                        Monto = modelo.Monto,
                        TipoOperacion = false,
                        NumAsiento = numAsiento
                    };
                    LdiarioGlobal asientoGastoProvisionado = new LdiarioGlobal
                    {
                        IdCodCuenta = modelo.IdGasto,
                        Fecha = DateTime.Today,
                        Concepto = modelo.Concepto,
                        Monto = modelo.Monto,
                        TipoOperacion = true,
                        NumAsiento = numAsiento
                    };
                    using (var db_context = new PruebaContext())
                    {
                        await db_context.AddAsync(provision);
                        await db_context.AddAsync(asientoGastoProvisionado);
                        await db_context.AddAsync(asientoProvision);
                        await db_context.SaveChangesAsync();
                    }

                    return RedirectToAction("RelaciondeGastos");

                }


                return View(modelo);
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


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GenerarReciboCobro()
        {
            try
            {
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

                var relacionesDeGasto = from c in _context.RelacionGastos
                                        select c;
                var existeActualRG = await relacionesDeGasto.Where(c => c.Fecha.Month == DateTime.Today.Month).ToListAsync();

                if (!existeActualRG.Any())
                {
                    var modelo = await _repoRelacionGastos.LoadDataRelacionGastos(idCondominio);
                    // CARGAR PROPIEDADES DE CADA INMUEBLE DEL CONDOMINIO
                    var inmueblesCondominio = from c in _context.Inmuebles
                                              where c.IdCondominio == idCondominio
                                              select c;

                    var propiedades = from c in _context.Propiedads
                                      select c;
                    var propietarios = from c in _context.AspNetUsers
                                       select c;

                    // REGISTRAR EN BD LA RELACION DE GASTOS
                    var relacionGasto = new RelacionGasto
                    {
                        SubTotal = modelo.SubTotal,
                        TotalMensual = modelo.Total,
                        Fecha = DateTime.Today,
                        IdCondominio = idCondominio
                    };

                    using (var db_context = new PruebaContext())
                    {
                        await db_context.AddAsync(relacionGasto);
                        await db_context.SaveChangesAsync();
                    }

                    IList<Propiedad> listaPropiedadesCondominio = new List<Propiedad>();
                    if (inmueblesCondominio != null && inmueblesCondominio.Any() && propiedades != null && propiedades.Any())
                    {
                        foreach (var item in inmueblesCondominio)
                        {
                            var propiedadsCond = await propiedades.Where(c => c.IdInmueble == item.IdInmueble).ToListAsync();
                            var aux2 = listaPropiedadesCondominio.Concat(propiedadsCond).ToList();
                            listaPropiedadesCondominio = aux2;
                        }

                        // CALCULAR LOS PAGOS DE CADA PROPIEDAD POR SU ALICUOTA
                        // CAMBIAR SOLVENCIA, SALDO Y DEUDA DE LA PROPIEDAD
                        IList<ReciboCobro> recibosCobroCond = new List<ReciboCobro>();
                        foreach (var propiedad in listaPropiedadesCondominio)
                        {
                            // VERIFICAR SOLVENCIA
                            // SI ES TRUE (ESTA SOLVENTE, AL DIA) NO SE BUSCA EN LA DEUDA
                            // SI ES FALSO PARA EL MONTO TOTAL A PAGAR DEBE MOSTRARSELE LA DEUDA
                            // Y EL TOTAL DEL MES MAS LA DEUDA
                            if (propiedad.Solvencia)
                            {
                                propiedad.Saldo = relacionGasto.TotalMensual * propiedad.Alicuota / 100;
                                propiedad.Solvencia = false;
                            }
                            else
                            {
                                propiedad.Deuda += propiedad.Saldo;
                                propiedad.Saldo = relacionGasto.TotalMensual * propiedad.Alicuota / 100;
                            }

                            // INFO DEL RECIBO
                            var recibo = new ReciboCobro
                            {
                                IdPropiedad = propiedad.IdPropiedad,
                                IdRgastos = relacionGasto.IdRgastos,
                                Monto = relacionGasto.TotalMensual * propiedad.Alicuota / 100,
                                Fecha = DateTime.Today
                            };

                            recibosCobroCond.Add(recibo);
                        }

                        // REGISTRAR EN BD LOS RECIBOS DE COBRO PARA CADA PROPIEDAD
                        //  Y EDITAR LAS PROPIEDADES

                        using (var db_context = new PruebaContext())
                        {
                            foreach (var item in recibosCobroCond)
                            {
                                await db_context.AddAsync(item);
                            }
                            foreach (var propiedad in listaPropiedadesCondominio)
                            {
                                db_context.Update(propiedad);
                            }
                            await db_context.SaveChangesAsync();
                        }

                        // CREAR MODELO PARA NUEVA VISTA
                        var aux = new RecibosCreadosVM
                        {
                            Propiedades = listaPropiedadesCondominio,
                            Propietarios = propietarios.ToList(),
                            Recibos = recibosCobroCond,
                            Inmuebles = inmueblesCondominio.ToList(),
                            RelacionGastos = modelo
                        };

                        // REENVIAR A OTRA VISTA

                        ViewBag.Recibos = "Exitoso";

                        TempData.Keep();

                        return View(nameof(Index));
                    }
                }
                else
                {
                    var modeloError = new ErrorViewModel()
                    {
                        RequestId = "Ya existe una relación de gasto para este mes!"
                    };

                    return View("Error", modeloError);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Recibos = "Fallido";
                ViewBag.MsgError = "Error: " + ex.Message;
                var aux = new RecibosCreadosVM();
                TempData.Keep();
                return View(aux);
            }

            return View(nameof(Index));

        }

    }
}
