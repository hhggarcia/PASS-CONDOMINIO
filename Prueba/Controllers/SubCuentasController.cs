using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prueba.Context;
using Prueba.Models;
using Prueba.Repositories;
using Prueba.ViewModels;

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]

    public class SubCuentasController : Controller
    {
        private readonly ICuentasContablesRepository _repoCuentasContables;
        private readonly NuevaAppContext _context;

        public SubCuentasController(ICuentasContablesRepository repoCuentasContables,
            NuevaAppContext context)
        {
            _repoCuentasContables = repoCuentasContables;
            _context = context;
        }

        // GET: SubCuentas
        //public async Task<IActionResult> Index()
        //{
        //    var NuevaAppContext = _context.SubCuenta.Include(s => s.IdCuentaNavigation);
        //    return View(await NuevaAppContext.ToListAsync());
        //}

        // GET: SubCuentas/Details/5
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

        // GET: SubCuentas/Create
        public IActionResult Create()
        {
            int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

            var subcuentas = _context.CodigoCuentasGlobals.Where(c => c.IdCondominio == idCondominio);
            //var NuevaAppContext = _context.Estacionamientos.Include(e => e.IdInmuebleNavigation);
            if (subcuentas != null && subcuentas.Any())
            {
                var cuentas = from c in _context.SubCuenta
                              join sc in subcuentas
                              on c.Id equals sc.IdCodigo
                              select c;

                var modelListCuentas = from c in _context.Cuenta
                                       join sc in cuentas
                                       on c.Id equals sc.IdCuenta
                                       select sc;

                ViewData["IdCuenta"] = new SelectList(modelListCuentas, "Id", "Descripcion");
            }
            else
            {
                ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Descripcion");
            }

            TempData.Keep();
            return View();
        }

        // POST: SubCuentas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCuenta,Descricion,Codigo")] SubCuenta subCuenta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subCuenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", subCuenta.IdCuenta);
            return View(subCuenta);
        }

        // GET: SubCuentas/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.SubCuenta == null)
        //    {
        //        return NotFound();
        //    }

        //    var subCuenta = await _context.SubCuenta.FindAsync(id);
        //    if (subCuenta == null)
        //    {
        //        return NotFound();
        //    }
        //    int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());

        //    var subcuentas = _context.CodigoCuentasGlobals.Include(e => e.IdCondominioNavigation)
        //        .Where(c => c.IdCondominio == idCondominio);
        //    //var NuevaAppContext = _context.Estacionamientos.Include(e => e.IdInmuebleNavigation);
        //    if (subcuentas != null && subcuentas.Any())
        //    {
        //        var cuentas = from c in _context.SubCuenta
        //                      join sc in subcuentas
        //                      on c.Id equals sc.IdCodigo
        //                      select c;

        //        var modelListCuentas = (from c in _context.Cuenta
        //                                join sc in cuentas
        //                                on c.Id equals sc.IdCuenta
        //                                select c).Distinct();

        //        ViewData["IdCuenta"] = new SelectList(modelListCuentas, "Id", "Descripcion", subCuenta.IdCuenta);
        //    }
        //    else
        //    {
        //        ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Descripcion", subCuenta.IdCuenta);
        //    }

        //    TempData.Keep();
        //    return View(subCuenta);
        //}

        // POST: SubCuentas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCuenta,Descricion,Codigo")] SubCuenta subCuenta)
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
            //}
            //ViewData["IdCuenta"] = new SelectList(_context.Cuenta, "Id", "Id", subCuenta.IdCuenta);
            //return View(subCuenta);
        }

        // GET: SubCuentas/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: SubCuentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SubCuenta == null)
            {
                return Problem("Entity set 'NuevaAppContext.SubCuenta'  is null.");
            }

            var result = await _repoCuentasContables.EliminarSubCuenta(id);

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

        /// <summary>
        /// Busca las cuentas contables de un condominio
        /// </summary>
        /// <returns>Vista del Plan de Cuentas</returns>
        public IActionResult Index()
        {
            try
            {
                //HACER MODELO PARA CARGAR TODAS LAS CUENTAS A LA TABLA INDEX
                var modelo = new IndexCuentasContablesVM();

                // BUSCAR LAS CUENTAS CONTABLES DEL CONDOMINIO
                int idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
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

                IList<SubCuenta> subcuentasModel = new List<SubCuenta>();

                foreach (var item in cuentasContablesCond)
                {
                    foreach (var subcuenta in subcuentas)
                    {
                        if (item.IdCodigo == subcuenta.Id)
                        {
                            subcuentasModel.Add(subcuenta);
                        }
                    }
                }

                //PASAR MODELO
                modelo.Clases = clases.ToList();
                modelo.Grupos = grupos.ToList();
                modelo.Cuentas = cuentas.ToList();
                modelo.SubCuentas = subcuentasModel;
                //CREAR FOR PARA CREAR LAS FILAS CON LA INFO 

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
        /// Metodo get de la creación de una nueva subCuenta
        /// </summary>
        /// <returns>Retorna el formulario para la creación de una subCuenta</returns>
        public IActionResult CrearSubCuenta()
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

        /// <summary>
        /// Metodo Ajax para cargar los selects de Grupos y Cuentas
        /// </summary>
        /// <param name="tipo">ID de la etiqueta html</param>
        /// <param name="valor">Id de la Clase o grupo seleccionada</param>
        /// <returns>Modelo para los Selects en formato Json</returns>
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

        /// <summary>
        /// Metodo post para la creación de una subcuenta 
        /// </summary>
        /// <param name="modelo">Modelo con IdClase, IdGrupo, IdCuenta, Descripción, Código</param>
        /// <returns>Regresa al Plan de Cuentas</returns>
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
                var idCondominio = Convert.ToInt32(TempData.Peek("idCondominio").ToString());
                var nuevoCC = new CodigoCuentasGlobal
                {
                    IdCodigo = nuevaSubCuenta.Id,
                    IdCondominio = idCondominio
                };

                using (var _dContext = new NuevaAppContext())
                {
                    _dContext.Add(nuevoCC);
                    _dContext.SaveChanges();
                }

                return RedirectToAction("CuentasContables");

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

        private bool SubCuentaExists(int id)
        {
            return (_context.SubCuenta?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
