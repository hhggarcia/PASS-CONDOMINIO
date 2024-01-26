using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using Prueba.Context;
using Prueba.Models;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    public class CodigoCuentasGlobalsController : Controller
    {
        private readonly NuevaAppContext _context;

        public CodigoCuentasGlobalsController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: CodigoCuentasGlobals
        public async Task<IActionResult> Index()
        {
            var modelo = new IndexCuentasContablesVM();

            // BUSCAR LAS CUENTAS CONTABLES DEL CONDOMINIO
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
            //int idCondominio = 1;
            var cuentasContablesCond = from c in _context.CodigoCuentasGlobals
                                       where c.IdCondominio == idCondominio
                                       select c;
            //CONSULTAS A BD SOBRE CLASE - GRUPO - CUENTA - SUB CUENTA

            IQueryable<Clase> clases = from c in _context.Clases
                                       select c;
            IQueryable<Grupo> grupos = from c in _context.Grupos
                                       select c;
            IQueryable<Cuenta> cuentas = from c in _context.Cuenta
                                         select c;
            IQueryable<SubCuenta> subcuentas = from c in _context.SubCuenta
                                               select c;
            //IList<SubCuenta> subcuentasModel = new List<SubCuenta>();
            IList<CuentaGlobalSubCuentasVM> subcuentaSaldo = new List<CuentaGlobalSubCuentasVM>();
            CuentaGlobalSubCuentasVM cuentaGlobalSubCuentasVM = new CuentaGlobalSubCuentasVM();
            foreach (var item in cuentasContablesCond)
            {
                foreach (var subcuenta in subcuentas)
                {
                    if (item.IdSubCuenta == subcuenta.Id)
                    {
                        cuentaGlobalSubCuentasVM.SaldoInicial = item.SaldoInicial;
                        cuentaGlobalSubCuentasVM.Saldo = item.Saldo;
                        cuentaGlobalSubCuentasVM.SubCuentas = subcuenta;
                        subcuentaSaldo.Add(cuentaGlobalSubCuentasVM);
                        //subcuentasModel.Add(subcuenta);
                    }
                }
            }
            //PASAR MODELO
            if(clases.ToList().Count > 0)
            {
                modelo.Clases = clases.ToList();
            }
            if (grupos.ToList().Count > 0)
            {
                modelo.Grupos = grupos.ToList();
            }   
            if (cuentas.ToList().Count > 0)
            {
                modelo.Cuentas = cuentas.ToList();
            }
            //modelo.SubCuentas = subcuentasModel;
            modelo.SubCuentasSaldo = subcuentaSaldo;
            //CREAR FOR PARA CREAR LAS FILAS CON LA INFO 

            TempData.Keep();
            return View(modelo);
        }
        public async Task<IActionResult> CrearSubCuenta()
        {
            try
            {
                SubcuentaCascadingVM modelo = new SubcuentaCascadingVM();

                var clases = from c in _context.Clases
                             select c;

                var clasesModel = clases.Select(c => new SelectListItem { Text = c.Descripcion, Value = c.Id.ToString() });

                modelo.Clases = clasesModel.ToList();
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
        public async Task<IActionResult> CrearCodigoCuentasGlobal()
        {
            try
            {
                CrearCodigoCuentasGlobalVMs modelo = new CrearCodigoCuentasGlobalVMs();
                var clases = from c in _context.Clases
                             select c;

                var clasesModel = clases.Select(c => new SelectListItem { Text = c.Descripcion, Value = c.Id.ToString() });

                modelo.Clases = clasesModel.ToList();
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
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> AjaxMethod(string tipo, int valor)
        {

            SubcuentaCascadingVM model = new SubcuentaCascadingVM();
            switch (tipo)
            {
                case "IdClase":
                    var grupos = from c in _context.Grupos
                                 where c.IdClase == valor
                                 select c;

                    model.Grupos = await grupos.Select(c => new SelectListItem { Text = c.Descripcion, Value = c.Id.ToString() }).ToListAsync();
                    break;
                case "IdGrupo":
                    var cuentas = from c in _context.Cuenta
                                  where c.IdGrupo == valor
                                  select c;

                    model.Cuentas = await cuentas.Select(c => new SelectListItem { Text = c.Descripcion, Value = c.Id.ToString() }).ToListAsync();
                    break;
            }
            return Json(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearSubCuentaPost(SubcuentaCascadingVM modelo)
        {
            try
            {
                //if (ModelState.IsValid)
                //{
                // REGISTRAR SUB CUENTA CON IDCUENTA, DESCRIP Y CODIGO

                var nuevaSubCuenta = new SubCuenta
                {
                    IdCuenta = modelo.IdCuenta,
                    Descricion = modelo.Descripcion,
                    Codigo = modelo.Codigo
                };

                using (var _dbContext = new NuevaAppContext())
                {
                    _dbContext.Add(nuevaSubCuenta);
                    _dbContext.SaveChanges();
                }

                // REGISTRAR EN CUENTAS CONTABLES GLOBAL ID CONDOMINIO Y ID SUB CUENTA
                //recuperar el id del condominio
                //var idCondominio = 1;
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var nuevoCC = new CodigoCuentasGlobal
                {
                    IdSubCuenta = nuevaSubCuenta.Id,
                    IdCondominio = idCondominio,
                    IdClase = modelo.IdClase,
                    IdGrupo = modelo.IdGrupo,
                    IdCuenta = modelo.IdCuenta,
                    Saldo = modelo.Saldo,
                    SaldoInicial = modelo.SaldoInicial,   
                    Codigo = modelo.Codigo
                };

                using (var _dContext = new NuevaAppContext())
                {
                    _dContext.Add(nuevoCC);
                    _dContext.SaveChanges();
                }

                return RedirectToAction(nameof(Index));

                //}

                //return View(modelo);

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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SubCuenta == null)
            {
                return NotFound();
            }

            var subCuenta = await _context.SubCuenta
                .Include(s => s.IdCuentaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subCuenta == null)
            {
                return NotFound();
            }

            return View(subCuenta);
        }

        // GET: CodigoCuentasGlobals/Details/5
        public async Task<IActionResult> DetailsSubCuenta(int? id)
        {
            if (id == null || _context.SubCuenta == null)
            {
                return NotFound();
            }

            var subCuenta = await _context.SubCuenta
                .Include(s => s.IdCuentaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subCuenta == null)
            {
                return NotFound();
            }

            return View(subCuenta);
        }

        public async Task<IActionResult> EditSubCuenta(int? id)
        {
            if (id == null || _context.SubCuenta == null)
            {
                return NotFound();
            }

            var subCuenta = await _context.SubCuenta.FindAsync(id);
            if (subCuenta == null)
            {
                return NotFound();
            }
            //int idCondominio = 1;
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = _context.CodigoCuentasGlobals.Include(e => e.IdCondominioNavigation)
                .Where(c => c.IdCondominio == idCondominio);
            //var pruebaContext = _context.Estacionamientos.Include(e => e.IdInmuebleNavigation);
            if (subcuentas != null && subcuentas.Any())
            {
                var cuentas = from c in _context.SubCuenta
                              join sc in subcuentas
                              on c.Id equals sc.IdSubCuenta
                              select c;

                var modelListCuentas = (from c in _context.Cuenta
                                        join sc in cuentas
                                        on c.Id equals sc.IdCuenta
                                        select c).Distinct();

                ViewData["IdCuenta"] = new SelectList(modelListCuentas, "Id", "Descripcion", subCuenta.IdCuenta);
            }
            else
            {
                ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Descripcion", subCuenta.IdCuenta);
            }

            TempData.Keep();
            return View(subCuenta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubCuenta(int id, [Bind("Id,IdCuenta,Descricion,Codigo")] SubCuenta subCuenta)
        {
            if (id != subCuenta.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                _context.Update(subCuenta);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubCuentaExists(subCuenta.Id))
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
        private bool SubCuentaExists(int id)
        {
            return (_context.SubCuenta?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> DeleteSubCuenta(int? id)
        {
            if (id == null || _context.SubCuenta == null)
            {
                return NotFound();
            }

            var subCuenta = await _context.SubCuenta
                .Include(s => s.IdCuentaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subCuenta == null)
            {
                return NotFound();
            }

            return View(subCuenta);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedSubCuenta(int id)
        {
            if (_context.SubCuenta == null)
            {
                return Problem("Entity set 'PruebaContext.SubCuenta'  is null.");
            }

            var subCuenta = await _context.SubCuenta.FindAsync(id);
            var result = 1; 
            if (subCuenta != null)
            {
                // buscar codigo cuentas -> codigo
                var codigosCuentas = await _context.CodigoCuentasGlobals.Where(c => c.IdSubCuenta == subCuenta.Id).ToListAsync();
                // eliminar del condominio
                if (codigosCuentas != null && codigosCuentas.Any())
                {
                    foreach (var cuenta in codigosCuentas)
                    {
                        var asientos = await _context.LdiarioGlobals.Where(c => c.IdCodCuenta == cuenta.IdCodCuenta).ToListAsync();

                        if (asientos.Any())
                        {
                            result = 0;
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
            await _context.SaveChangesAsync();
            if (result == 0)
            {
                var modeloError = new ErrorViewModel()
                {
                    RequestId = "para eliminar esta Sub Cuenta debe eliminar antes los asientos del Libro Diario afectados!"
                };
                return View("Error", modeloError);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: CodigoCuentasGlobals/Create
        public async Task<IActionResult> Create()
        {
            var listCuentasGlobales = await _context.CodigoCuentasGlobals.Where(c=>c.IdCondominio ==1).ToListAsync();
            //traer la lista de las subcuentas que no están.
            //
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id");
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio");
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id");
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id");
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id");
             
            return View();
        }

        // POST: CodigoCuentasGlobals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCodCuenta,IdSubCuenta,IdCuenta,IdGrupo,IdClase,Codigo,Saldo,SaldoInicial,IdCondominio")] CodigoCuentasGlobal codigoCuentasGlobal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(codigoCuentasGlobal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id", codigoCuentasGlobal.IdClase);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", codigoCuentasGlobal.IdCondominio);
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", codigoCuentasGlobal.IdCuenta);
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id", codigoCuentasGlobal.IdGrupo);
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id", codigoCuentasGlobal.IdSubCuenta);
            return View(codigoCuentasGlobal);
        }

        // GET: CodigoCuentasGlobals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var codigoCuentasGlobal = await _context.CodigoCuentasGlobals.FindAsync(id);
            if (codigoCuentasGlobal == null)
            {
                return NotFound();
            }
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id", codigoCuentasGlobal.IdClase);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", codigoCuentasGlobal.IdCondominio);
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", codigoCuentasGlobal.IdCuenta);
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id", codigoCuentasGlobal.IdGrupo);
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id", codigoCuentasGlobal.IdSubCuenta);
            return View(codigoCuentasGlobal);
        }

        // POST: CodigoCuentasGlobals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCodCuenta,IdSubCuenta,IdCuenta,IdGrupo,IdClase,Codigo,Saldo,SaldoInicial,IdCondominio")] CodigoCuentasGlobal codigoCuentasGlobal)
        {
            if (id != codigoCuentasGlobal.IdCodCuenta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(codigoCuentasGlobal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CodigoCuentasGlobalExists(codigoCuentasGlobal.IdCodCuenta))
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
            ViewData["IdClase"] = new SelectList(_context.Clases, "Id", "Id", codigoCuentasGlobal.IdClase);
            ViewData["IdCondominio"] = new SelectList(_context.Condominios, "IdCondominio", "IdCondominio", codigoCuentasGlobal.IdCondominio);
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", codigoCuentasGlobal.IdCuenta);
            ViewData["IdGrupo"] = new SelectList(_context.Grupos, "Id", "Id", codigoCuentasGlobal.IdGrupo);
            ViewData["IdSubCuenta"] = new SelectList(_context.SubCuenta, "Id", "Id", codigoCuentasGlobal.IdSubCuenta);
            return View(codigoCuentasGlobal);
        }

        // GET: CodigoCuentasGlobals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var codigoCuentasGlobal = await _context.CodigoCuentasGlobals
                .Include(c => c.IdClaseNavigation)
                .Include(c => c.IdCondominioNavigation)
                .Include(c => c.IdCuentaNavigation)
                .Include(c => c.IdGrupoNavigation)
                .Include(c => c.IdSubCuentaNavigation)
                .FirstOrDefaultAsync(m => m.IdCodCuenta == id);
            if (codigoCuentasGlobal == null)
            {
                return NotFound();
            }

            return View(codigoCuentasGlobal);
        }

        // POST: CodigoCuentasGlobals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var codigoCuentasGlobal = await _context.CodigoCuentasGlobals.FindAsync(id);
            if (codigoCuentasGlobal != null)
            {
                _context.CodigoCuentasGlobals.Remove(codigoCuentasGlobal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CodigoCuentasGlobalExists(int id)
        {
            return _context.CodigoCuentasGlobals.Any(e => e.IdCodCuenta == id);
        }
    }
}
