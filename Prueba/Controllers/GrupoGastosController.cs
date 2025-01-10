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

namespace Prueba.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    public class GrupoGastosController : Controller
    {
        private readonly NuevaAppContext _context;

        public GrupoGastosController(NuevaAppContext context)
        {
            _context = context;
        }

        // GET: GrupoGastos
        public async Task<IActionResult> Index()
        {
            return View(await _context.GrupoGastos.ToListAsync());
        }

        // GET: GrupoGastos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoGasto = await _context.GrupoGastos
                .FirstOrDefaultAsync(m => m.IdGrupoGasto == id);
            if (grupoGasto == null)
            {
                return NotFound();
            }

            return View(grupoGasto);
        }

        // GET: GrupoGastos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GrupoGastos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdGrupoGasto,NumGrupo,NombreGrupo")] GrupoGasto grupoGasto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grupoGasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grupoGasto);
        }

        // GET: GrupoGastos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoGasto = await _context.GrupoGastos.FindAsync(id);
            if (grupoGasto == null)
            {
                return NotFound();
            }
            return View(grupoGasto);
        }

        // POST: GrupoGastos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdGrupoGasto,NumGrupo,NombreGrupo")] GrupoGasto grupoGasto)
        {
            if (id != grupoGasto.IdGrupoGasto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grupoGasto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrupoGastoExists(grupoGasto.IdGrupoGasto))
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
            return View(grupoGasto);
        }

        // GET: GrupoGastos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoGasto = await _context.GrupoGastos
                .FirstOrDefaultAsync(m => m.IdGrupoGasto == id);
            if (grupoGasto == null)
            {
                return NotFound();
            }

            return View(grupoGasto);
        }

        // POST: GrupoGastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grupoGasto = await _context.GrupoGastos.FindAsync(id);
            if (grupoGasto != null)
            {
                IList<CuentasGrupo> cuentasGrupos = await _context.CuentasGrupos.Where(c => c.IdGrupoGasto == id).ToListAsync();
                IList<PropiedadesGrupo> propiedadesGrupos = await _context.PropiedadesGrupos.Where(c => c.IdGrupoGasto == id).ToListAsync();

                if (cuentasGrupos.Any())
                {
                    _context.CuentasGrupos.RemoveRange(cuentasGrupos);
                }

                if (propiedadesGrupos.Any())
                {
                    _context.PropiedadesGrupos.RemoveRange(propiedadesGrupos);
                }

                _context.GrupoGastos.Remove(grupoGasto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Accion para llevar a la vista index de cuentas en un grupo
        /// </summary>
        /// <param name="id">id del grupo para buscar sus cuentas</param>
        /// <returns></returns>
        public async Task<IActionResult> VerCuentas(int id)
        {
            // buscar cuentas del grupo
            var cuentasGrupos = await (from g in _context.GrupoGastos
                                join cg in _context.CuentasGrupos
                                on g.IdGrupoGasto equals cg.IdGrupoGasto
                                join cc in _context.CodigoCuentasGlobals
                                on cg.IdCodCuenta equals cc.IdCodCuenta
                                join sc in _context.SubCuenta
                                on cc.IdSubCuenta equals sc.Id
                                where g.IdGrupoGasto == id
                                select sc).ToListAsync();

            TempData["IDGrupo"] = id.ToString();
            //ViewData["NombreGrupo"] = id;

            return View(cuentasGrupos);
        }

        public IActionResult AgregarCuenta()
        {
            return RedirectToAction("Create", "CuentasGrupos");
        }

        private bool GrupoGastoExists(int id)
        {
            return _context.GrupoGastos.Any(e => e.IdGrupoGasto == id);
        }

        public async Task<IActionResult> EliminarCuentaGrupo(int? id)
        {
            var idGrupo = Convert.ToInt32(TempData.Peek("IDGrupo").ToString());

            if (id == null)
            {
                return NotFound();
            }

            var subcuenta = await _context.SubCuenta
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subcuenta == null)
            {
                return NotFound();
            }

            TempData.Keep();

            return View(subcuenta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ID de la subcuenta a eliminar del grupo</param>
        /// <returns></returns>
        [HttpPost, ActionName("EliminarCuentaGrupo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCuentaGrupoConfirmed(int id)
        {
            var idGrupo = Convert.ToInt32(TempData.Peek("IDGrupo").ToString());

            var idCC = from c in _context.CodigoCuentasGlobals
                       where c.IdSubCuenta == id
                       select c;

            if (!idCC.Any())
            {
                return NotFound();
            }

            var cuentaGrupo = (from c in _context.CuentasGrupos
                              where c.IdGrupoGasto == idGrupo && c.IdCodCuenta == idCC.First().IdCodCuenta
                              select c).First();

            if (cuentaGrupo == null)
            {
                return NotFound();
            }

            _context.CuentasGrupos.Remove(cuentaGrupo);
            await _context.SaveChangesAsync();

            return RedirectToAction("VerCuentas", new { id = idGrupo });

        }
    }
}
